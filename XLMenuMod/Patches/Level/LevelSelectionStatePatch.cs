﻿using GameManagement;
using HarmonyLib;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityModManagerNet;
using XLMenuMod.Levels;

namespace XLMenuMod.Patches.Level
{
    static class LevelSelectionStatePatch
    {
        [HarmonyPatch(typeof(LevelSelectionState), nameof(LevelSelectionState.OnUpdate))]
        static class OnUpdatePatch
        {
            static bool Prefix(LevelSelectionState __instance)
            {
                if (PlayerController.Instance.inputController.player.GetButtonDown("Y"))
                {
                    UISounds.Instance?.PlayOneShotSelectionChange();

                    CustomLevelManager.OnNextSort();
                    return false;
                }

                if (CustomLevelManager.CurrentFolder == null) return true;
                if (!PlayerController.Instance.inputController.player.GetButtonDown("B")) return true;

                if (!Main.Settings.DisableBToMoveUpDirectory)
                {
                    UISounds.Instance?.PlayOneShotSelectMajor();

                    CustomLevelManager.CurrentFolder = CustomLevelManager.CurrentFolder.FolderInfo.Parent as CustomLevelFolderInfo;

                    EventSystem.current.SetSelectedGameObject(null);
                    UnityEngine.Object.FindObjectOfType<LevelSelectionController>()?.UpdateList();
                    CustomLevelManager.UpdateLabel();

                    return false;
                }

                CustomLevelManager.CurrentFolder = null;
                return true;
            }
        }

        [HarmonyPatch(typeof(LevelSelectionState), nameof(LevelSelectionState.OnEnter))]
        static class OnEnterPatch
        {
            static void Postfix()
            {
                CustomLevelManager.LoadNestedLevels();
            }
        }

        [HarmonyPatch(typeof(LevelSelectionState), nameof(LevelSelectionState.OnExit))]
        static class OnExitPatch
        {
            static void Postfix()
            {
                CustomLevelManager.CurrentFolder = null;
            }
        }
    }
}
