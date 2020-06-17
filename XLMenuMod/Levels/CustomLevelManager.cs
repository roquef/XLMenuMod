﻿using HarmonyLib;
using Rewired.UI.ControlMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using XLMenuMod.Levels.Interfaces;

namespace XLMenuMod.Levels
{
    public class CustomLevelManager : MonoBehaviour
    {
        public static CustomFolderInfo CurrentFolder { get; set; }
        public static List<ICustomLevelInfo> NestedCustomLevels { get; set; }
        public static float LastSelectedTime { get; set; }
        public static CategoryButton SortCategoryButton { get; set; }
        public static int CurrentLevelSort { get; set; }

        static CustomLevelManager()
        {
            CurrentFolder = null;
            NestedCustomLevels = new List<ICustomLevelInfo>();
        }

        public static List<string> LoadNestedLevelPaths(string directoryToSearch = null)
        {
            var nestedLevels = new List<string>();

            if (directoryToSearch == null) directoryToSearch = SaveManager.Instance.CustomLevelsDir;

            if (Directory.Exists(directoryToSearch))
            {
                foreach (var subDir in Directory.GetDirectories(directoryToSearch))
                {
                    var directoryName = Path.GetFileName(subDir);
                    if (directoryName.ToLower().Equals("dlls")) continue;

                    foreach (string file in Directory.GetFiles(subDir))
                    {
                        if (!file.ToLower().EndsWith(".dll"))
                        {
                            nestedLevels.Add(file);
                        }
                    }

                    nestedLevels.AddRange(LoadNestedLevelPaths(subDir));
                }
            }

            return nestedLevels;
        }

        public static void LoadNestedLevels()
        {
            foreach (var level in LevelManager.Instance.CustomLevels)
            {
                if (string.IsNullOrEmpty(level.path) || !level.path.StartsWith(SaveManager.Instance.CustomLevelsDir)) continue;

                var levelSubPath = level.path.Replace(SaveManager.Instance.CustomLevelsDir + '\\', string.Empty);

                if (string.IsNullOrEmpty(levelSubPath)) continue;

                var folders = levelSubPath.Split('\\').ToList();
                if (folders == null || !folders.Any()) continue;

                if (folders.Count == 1)
                {
                    // This level is at the root
                    AddLevel(level);
                    continue;
                }

                CustomFolderInfo parent = null;
                for (int i = 0; i < folders.Count; i++)
                {
                    var folder = folders.ElementAt(i);
                    if (folder == null) continue;

                    if (folder == folders.Last())
                    {
                        AddLevel(level, ref parent);
                    }
                    else
                    {
                        AddFolder(folder, level.path, ref parent);
                    }
                }
            }

            Traverse.Create(LevelManager.Instance).Method("InitializeCustomLevels").GetValue();
        }

        public static void AddLevel(LevelInfo level)
        {
            if (level is CustomLevelInfo)
            {
                var customLevel = level as CustomLevelInfo;
                CreateOrUpdateLevel(NestedCustomLevels, level, customLevel, null);
            }
        }

        public static void AddLevel(LevelInfo level, ref CustomFolderInfo parent)
        {
            if (level is CustomLevelInfo)
            {
                var customLevel = level as CustomLevelInfo;

                if (parent == null)
                {
                    CreateOrUpdateLevel(NestedCustomLevels, level, customLevel, parent);
                }
                else
                {
                    CreateOrUpdateLevel(parent.Children, level, customLevel, parent);
                }
            }
        }

        private static void CreateOrUpdateLevel(List<ICustomLevelInfo> sourceList, LevelInfo levelToAdd, CustomLevelInfo customLevelToAdd, CustomFolderInfo parent)
        {
            var existing = sourceList.FirstOrDefault(x => x.GetHash() == customLevelToAdd.GetHash() && x is CustomLevelInfo) as CustomLevelInfo;
            if (existing == null)
            {
                sourceList.Add(new CustomLevelInfo(levelToAdd, parent) { PlayCount = customLevelToAdd.PlayCount });
            }
            else
            {
                existing.PlayCount = customLevelToAdd.PlayCount;
            }
        }

        public static void AddFolder(string folder, string path, ref CustomFolderInfo parent)
        {
            var newFolder = new CustomFolderInfo($"\\{folder}", Path.GetDirectoryName(path), parent);
            newFolder.Children.Add(new CustomFolderInfo("..\\", parent == null ? string.Empty : Path.GetDirectoryName(parent.path), newFolder.Parent));

            if (parent != null)
            {
                var child = parent.Children.FirstOrDefault(x => x.GetName() == newFolder.GetName() && x is CustomFolderInfo) as CustomFolderInfo;
                if (child == null)
                {
                    parent.Children.Add(newFolder);
                    parent = newFolder;
                }
                else
                {
                    parent = child;
                }
            }
            else
            {
                var child = NestedCustomLevels.FirstOrDefault(x => x.GetName() == newFolder.GetName() && x is CustomFolderInfo) as CustomFolderInfo;
                if (child == null)
                {
                    NestedCustomLevels.Add(newFolder);
                    parent = newFolder;
                }
                else
                {
                    parent = child;
                }
            }
        }

        public static void UpdateLabel()
        {
            var levelSelector = FindObjectOfType<LevelSelectionController>();
            if (levelSelector == null) return;

            if (CurrentFolder == null)
            {
                levelSelector.LevelCategoryButton.label.text = levelSelector.showCustom ? "Custom Maps" : "Official Maps";
            }
            else
            {
                levelSelector.LevelCategoryButton.label.text = CurrentFolder.GetName();
            }
        }

        public static List<ICustomLevelInfo> SortList(List<ICustomLevelInfo> levels)
        {
            List<ICustomLevelInfo> sorted = null;

            switch (CurrentLevelSort)
            {
                case (int)LevelSortMethod.Least_Played:
                    sorted = levels.OrderBy(x => x.GetPlayCount()).ToList();
                    break;
                case (int)LevelSortMethod.Most_Played:
                    sorted = levels.OrderByDescending(x => x.GetPlayCount()).ToList();
                    break;
                case (int)LevelSortMethod.Newest:
                    sorted = levels.OrderByDescending(x => x.GetModifiedDate(false)).ToList();
                    break;
                case (int)LevelSortMethod.Oldest:
                    sorted = levels.OrderBy(x => x.GetModifiedDate(true)).ToList();
                    break;
                case (int)LevelSortMethod.Filesize_ASC:
                    sorted = levels.OrderBy(x => x.Size).ToList();
                    break;
                case (int)LevelSortMethod.Filesize_DESC:
                    sorted = levels.OrderByDescending(x => x.Size).ToList();
                    break;
                case (int)LevelSortMethod.Name_ASC:
                    sorted = levels.OrderBy(x => x.GetName()).ToList();
                    break;
                case (int)LevelSortMethod.Name_DESC:
                default:
                    sorted = levels.OrderByDescending(x => x.GetName()).ToList();
                    break;
            }

            return sorted;
        }   

        public static void CreateSortCategoryButton(LevelSelectionController __instance)
        {
            SortCategoryButton = Instantiate(__instance.LevelCategoryButton, __instance.LevelCategoryButton.transform.parent);
            //SortCategoryButton.transform.SetParent(__instance.LevelCategoryButton.transform, false);
            SortCategoryButton.transform.localScale = new Vector3(1, 1, 1);

            SortCategoryButton.OnNextCategory += new Action(OnNextSort);
            SortCategoryButton.OnPreviousCategory += new Action(OnPreviousSort);

            SortCategoryButton.gameObject.SetActive(false);

            SortCategoryButton.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 200);

            //Delete the divider line
            Destroy(SortCategoryButton.gameObject.GetComponentInChildren<Image>());

            Traverse.Create(SortCategoryButton).Method("SetText", ((LevelSortMethod)CurrentLevelSort).ToString().Replace('_', ' ')).GetValue();
            SortCategoryButton.label.fontSize = 20;

            SortCategoryButton.transform.Translate(new Vector3(0, 20, 0));
        }

        private static void OnPreviousSort()
        {
            //TODO: Handle double selects
            CurrentLevelSort--;

            if (CurrentLevelSort < 0)
                CurrentLevelSort = Enum.GetValues(typeof(LevelSortMethod)).Length - 1;

            SortCategoryButton.label.text = ((LevelSortMethod)CurrentLevelSort).ToString().Replace('_', ' ');

            if (CurrentFolder != null && CurrentFolder.Children != null && CurrentFolder.Children.Any())
            {
                CurrentFolder.Children = SortList(CurrentFolder.Children);
            }
            else
            {
                NestedCustomLevels = SortList(NestedCustomLevels);
            }

            var levelSelector = FindObjectOfType<LevelSelectionController>();

            if (levelSelector != null)
                levelSelector.UpdateList();
        }

        private static void OnNextSort()
        {
            //TODO: Handle double selects
            CurrentLevelSort++;

            if (CurrentLevelSort > Enum.GetValues(typeof(LevelSortMethod)).Length - 1)
                CurrentLevelSort = 0;

            SortCategoryButton.label.text = ((LevelSortMethod)CurrentLevelSort).ToString().Replace('_', ' ');


            if (CurrentFolder != null && CurrentFolder.Children != null && CurrentFolder.Children.Any())
            {
                CurrentFolder.Children = SortList(CurrentFolder.Children);
            }
            else
            {
                NestedCustomLevels = SortList(NestedCustomLevels);
            }

            var levelSelector = FindObjectOfType<LevelSelectionController>();

            if (levelSelector != null)
                levelSelector.UpdateList();
        }
    }
}
