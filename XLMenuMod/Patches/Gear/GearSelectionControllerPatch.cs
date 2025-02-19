﻿using HarmonyLib;
using ModIO;
using Rewired;
using SkaterXL.Data;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityModManagerNet;
using XLMenuMod.Utilities;
using XLMenuMod.Utilities.Gear;
using XLMenuMod.Utilities.UserInterface;

namespace XLMenuMod.Patches.Gear
{
    public static class GearSelectionControllerPatch
	{
		[HarmonyPatch(typeof(GearSelectionController), nameof(GearSelectionController.ConfigureHeaderView))]
		public static class ConfigureHeaderViewPatch
		{
			static void Postfix(GearSelectionController __instance, IndexPath index, MVCListHeaderView itemView)
			{
				if (index.depth < 2) return;

				CustomGearManager.Instance.SortLabel.gameObject.SetActive(__instance.listView.currentIndexPath[1] >= 10);
				UserInterfaceHelper.Instance.UpdateLabelColor(CustomGearManager.Instance.SortLabel, Main.Settings.EnableDarkMode ? UserInterfaceHelper.DarkModeText : UserInterfaceHelper.DefaultText);

				var officialGear = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();
				// return out if it's not one of the tabs XLMenuMod is aware of.
				if (index[1] < 0 || index[1] > (officialGear[index[0]].Length * 2) - 1) return;

                if (CustomGearManager.Instance.CurrentFolder == null) return;
                if (index[0] < 0) return;

                bool isCustom = index[1] >= officialGear[index[0]].Length;

                itemView.Label.fontSize = UserInterfaceHelper.Instance.GetFontSize(Main.Settings.FontSize);

                if (isCustom || CategoryHelper.IsTypeOf(index, GearCategory.Hair))
                {
                    if (SpriteHelper.MenuIcons != null)
                    {
                        itemView.Label.spriteAsset = SpriteHelper.MenuIcons;

                        string newText = "<sprite name=\"folder_outline\">";

                        if (CustomGearManager.Instance.CurrentFolder.GetName().Equals("\\mod.io"))
                        {
                            itemView.Label.spriteAsset = SpriteHelper.BrandIcons;
                            newText = "<sprite name=\"mod.io\">";
                        }
                        itemView.SetText(CustomGearManager.Instance.CurrentFolder.GetName().Replace("\\", newText));
                    }
                }
                else
                {
                    itemView.SetBrandSprite(CustomGearManager.Instance.CurrentFolder.GetParentObject() as CustomGearFolderInfo);
                }
            }
		}

		[HarmonyPatch(typeof(GearSelectionController), nameof(GearSelectionController.ConfigureListItemView))]
		public static class ConfigureListItemViewPatch
		{
			static void Postfix(GearSelectionController __instance, IndexPath index, ref MVCListItemView itemView)
			{
				__instance.normalColor = Main.Settings.EnableDarkMode ? UserInterfaceHelper.DarkModeText.normalColor : UserInterfaceHelper.DefaultText.normalColor;

				if (index[1] < 0) return;

				itemView.Label.richText = true;

				var officialGear = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();
				// return out if it's not one of the tabs XLMenuMod is aware of.
				if (index[1] < 0 || index[1] > (officialGear[index[0]].Length * 2) - 1) return;

				bool isCustom = index[1] >= Enum.GetValues(typeof(GearCategory)).Length;

				if (SpriteHelper.MenuIcons != null) 
					itemView.Label.spriteAsset = SpriteHelper.MenuIcons;

                if (index.depth < 3) return;

                GearInfo gearAtIndex = GearDatabase.Instance.GetGearAtIndex(index, out bool _);

                if (gearAtIndex == null)
                {
                    itemView.SetText("NOT FOUND", false);
                    Traverse.Create(GearSelectionController.Instance).Method("SetIsEquippedIndicators", itemView, false).GetValue();
                }
                else
                {
                    // To ensure the items have the proper font and weight.
                    itemView.Label.font = FontDatabase.bookOblique;
                    itemView.Label.fontStyle = FontStyles.Normal;

                    itemView.Label.fontSize = UserInterfaceHelper.Instance.GetFontSize(Main.Settings.FontSize);

					if (gearAtIndex.name.StartsWith("\\"))
                    {
                        if (isCustom || CategoryHelper.IsTypeOf(index, GearCategory.Hair))
                        {
                            var newText = "<space=18px><sprite name=\"folder_outline\" tint=1>";

                            if (gearAtIndex is CustomGearFolderInfo folder && folder.CustomSprite != null)
                            {
                                itemView.Label.spriteAsset = folder.CustomSprite;
                                newText = "<space=18px><sprite=0 tint=1>";
                            }

                            if (gearAtIndex.name.Equals("\\mod.io"))
                            {
                                itemView.Label.spriteAsset = SpriteHelper.BrandIcons;
                                newText = "<space=18px><sprite name=\"mod.io\" tint=1>";
                            }
								
                            itemView.SetText(gearAtIndex.name.Replace("\\", newText), true);
                        }
                        else
                        {
                            itemView.SetBrandSprite(gearAtIndex);
                        }
                    }
                    else if (gearAtIndex.name.Equals("..\\"))
                    {
                        itemView.SetText(gearAtIndex.name.Replace("..\\", "<space=18px><sprite name=\"folder\" tint=1>Go Back"), true);
                    }
                    else
                    {
                        itemView.SetText(gearAtIndex.name, true);
                    }

                    Traverse.Create(__instance).Method("SetIsEquippedIndicators", itemView, __instance.previewCustomizer.HasEquipped(gearAtIndex)).GetValue();
                }
            }
		}

		[HarmonyPatch(typeof(GearSelectionController), nameof(GearSelectionController.GetNumberOfItems))]
		public static class GetNumberOfItemsPatch
		{
			static void Postfix(ref int __result, IndexPath index)
			{
				var officialGear = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();

				if (index[0] < 0) return;

				// return out if it's not one of the tabs XLMenuMod is aware of.
				if (index[1] < 0 || index[1] > (officialGear[index[0]].Length * 2) - 1) return;

				bool isCustom = index[0] < officialGear.Length && index[1] >= officialGear[index[0]].Length;

				if (isCustom && index.depth >= 3)
				{
					__result = CustomGearManager.Instance.CurrentFolder.HasChildren() ? CustomGearManager.Instance.CurrentFolder.Children.Count : CustomGearManager.Instance.NestedItems.Count;
				}
				else if (!isCustom && index.depth >= 3)
				{
					__result = CustomGearManager.Instance.CurrentFolder.HasChildren() ? CustomGearManager.Instance.CurrentFolder.Children.Count : CustomGearManager.Instance.NestedOfficialItems.Count;
				}

				if (Main.Settings.HideOfficialGear && !isCustom && index.depth >= 2 && index[1] != 0 && index[1] != 1)
					__result = 0;
			}
		}

		[HarmonyPatch(typeof(GearSelectionController), "ListView_OnItemSelectedEvent")]
		public static class ListView_OnItemSelectedEventPatch
		{
			static bool Prefix(GearSelectionController __instance, IndexPath index)
			{
				var officialGear = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();
				if (index[0] < 0) return true;

				// return out if it's not one of the tabs XLMenuMod is aware of.
				if (index[1] < 0 || index[1] > (officialGear[index[0]].Length * 2) - 1) return true;

				var gear = GearDatabase.Instance.GetGearAtIndex(index);

				if (gear is CustomGearFolderInfo selectedFolder)
				{
					selectedFolder.FolderInfo.Children = CustomGearManager.Instance.SortList(selectedFolder.FolderInfo.Children);

					var currentIndexPath = Traverse.Create(__instance.listView).Property<IndexPath>("currentIndexPath");

					if (selectedFolder.FolderInfo.GetName() == "..\\")
					{
						CustomGearManager.Instance.CurrentFolder = selectedFolder.FolderInfo.Parent;
						currentIndexPath.Value = __instance.listView.currentIndexPath.Up();

						__instance.listView.UpdateList();
						__instance.listView.SetHighlighted(Traverse.Create(__instance.listView).Property<IndexPath>("currentIndexPath").Value, true);
					}
					else
					{
						CustomGearManager.Instance.CurrentFolder = selectedFolder.FolderInfo;

						if (CustomGearManager.Instance.CurrentFolder.Parent != null)
						{
							currentIndexPath.Value = __instance.listView.currentIndexPath.Sub(CustomGearManager.Instance.CurrentFolder.Parent.Children.IndexOf(CustomGearManager.Instance.CurrentFolder));
						}
						else
						{
							var gearList = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();

							bool isCustom = index[1] >= gearList[index[0]].Length;

							if (isCustom)
							{
								currentIndexPath.Value = __instance.listView.currentIndexPath.Sub(CustomGearManager.Instance.NestedItems.IndexOf(CustomGearManager.Instance.CurrentFolder));
							}
							else
							{
								currentIndexPath.Value = __instance.listView.currentIndexPath.Sub(CustomGearManager.Instance.NestedOfficialItems.IndexOf(CustomGearManager.Instance.CurrentFolder));
							}
						}

						EventSystem.current.SetSelectedGameObject(null);
						__instance.listView.UpdateList();
					}

					return false;
				}

				if (index.depth >= 3)
				{
					if (__instance.previewCustomizer.HasEquipped(gear))
						return false;
					try
					{
						__instance.previewCustomizer.EquipGear(gear);
						__instance.previewCustomizer.OnlyShowEquippedGear();
						Traverse.Create(__instance).Field<bool>("didChangeGear").Value = true;
					}
					catch (Exception ex)
					{
						UnityModManager.Logger.LogException("XLMenuMod", ex);
					}
					__instance.Save();
					__instance.listView.UpdateList();

					return false;
				}

				CustomGearManager.Instance.CurrentFolder = null;
				return true;
			}
		}

		[HarmonyPatch(typeof(GearSelectionController), "ListView_OnItemHighlightedEvent")]
		public static class ListView_OnItemHighlightedEventPatch
		{
			/// <summary>
			/// Most of this code comes directly from the default GearSelectionController, we just allow you do exceed an index.depth of 3.
			/// </summary>
			/// <param name="__instance"></param>
			/// <param name="index"></param>
			static void Postfix(GearSelectionController __instance, IndexPath index)
			{
				var officialGear = Traverse.Create(GearDatabase.Instance).Field("gearListSource").GetValue<GearInfo[][][]>();
				if (index[0] < 0) return;

				// return out if it's not one of the tabs XLMenuMod is aware of.
				if (index[1] < 0 || index[1] > (officialGear[index[0]].Length * 2) - 1) return;

				if (index.depth >= 3)
				{
					PreviewGear(__instance, index);
				}
			}

			private static void PreviewGear(GearSelectionController __instance, IndexPath index)
			{
				GearInfo gearAtIndex1 = GearDatabase.Instance.GetGearAtIndex(index);
				if (gearAtIndex1 == (GearInfo)null)
					return;
				List<GearInfo> toBeCachedGear = new List<GearInfo>();
				for (int steps = -__instance.preloadedItemsPerSide; steps <= __instance.preloadedItemsPerSide; ++steps)
				{
					GearInfo gearAtIndex2 = GearDatabase.Instance.GetGearAtIndex(index.Horizontal(steps));
					if (gearAtIndex2 != (GearInfo)null)
						toBeCachedGear.Add(gearAtIndex2);
				}

				if (gearAtIndex1 is CustomGearFolderInfo)
				{
					__instance.previewCustomizer.PreviewItem(null, toBeCachedGear);
				}
				else
				{
					__instance.previewCustomizer.PreviewItem(gearAtIndex1, toBeCachedGear);
				}
			}
		}

		[HarmonyPatch(typeof(GearSelectionController), "Awake")]
		public static class AwakePatch
		{
			static void Postfix(GearSelectionController __instance)
			{
				CustomGearManager.Instance.SortLabel = UserInterfaceHelper.Instance.CreateSortLabel(Main.Settings.EnableDarkMode, __instance.listView.HeaderView.Label, __instance.listView.HeaderView.transform, ((GearSortMethod)CustomGearManager.Instance.CurrentSort).ToString(), -60);
				UserInterfaceHelper.Instance.ToggleDarkMode(__instance.gameObject, Main.Settings.EnableDarkMode);

				ModManager.QueryInstalledMods(null, x =>
				{
					foreach (var kvp in x)
					{
						ModManager.GetModProfile(kvp.Key.modId, y =>
						{
							CustomGearManager.Instance.InstalledGearMods.Add(new KeyValuePair<string, string>(kvp.Value, y.name));
						}, null);
					}
				});
			}
		}

		[HarmonyPatch(typeof(GearSelectionController), "Update")]
		public static class UpdatePatch
		{
			static bool Prefix(GearSelectionController __instance)
			{
				var player = Traverse.Create(__instance).Field("player").GetValue<Player>();
				if (player.GetButtonDown("Y"))
				{
					UISounds.Instance?.PlayOneShotSelectionChange();

					CustomGearManager.Instance.OnNextSort<GearSortMethod>();
					return false;
				}

				if (__instance.listView.currentIndexPath.depth >= 3)
				{
					if (CustomGearManager.Instance.CurrentFolder == null) return true;
					if (!PlayerController.Instance.inputController.player.GetButtonDown("B")) return true;

					if (!Main.Settings.DisableBToMoveUpDirectory)
					{
						UISounds.Instance?.PlayOneShotSelectMajor();

						CustomGearManager.Instance.CurrentFolder = CustomGearManager.Instance.CurrentFolder.Parent;
						Traverse.Create(__instance.listView).Property<IndexPath>("currentIndexPath").Value = __instance.listView.currentIndexPath.Up();

						__instance.listView.UpdateList();
						__instance.listView.SetHighlighted(Traverse.Create(__instance.listView).Property<IndexPath>("currentIndexPath").Value, true);

						return false;
					}
				}

				return true;
			}
		}
	}
}
