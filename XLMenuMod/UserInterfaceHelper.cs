﻿using System.Collections.Generic;
using Rewired;
using System.Linq;
using System.Reflection;
using GameManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityModManagerNet;

namespace XLMenuMod
{
	public class UserInterfaceHelper
    {
	    public AssetBundle Assets { get; private set; }
	    public List<TMP_SpriteAsset> Sprites { get; private set; }

	    public TMP_SpriteAsset WhiteSprites => Sprites?.ElementAt(2);

	    public AssetBundle BrandAssets { get; private set; }
	    public TMP_SpriteAsset BrandSprites { get; private set; }

		public Sprite OriginalBackgroundTexture { get; private set; }
	    public Sprite DarkModeBackground { get; private set; }

	    private static UserInterfaceHelper _instance;
	    public static UserInterfaceHelper Instance
	    {
		    get { return _instance ?? (_instance = new UserInterfaceHelper()); }
		    private set { _instance = value; }
	    }

		public UserInterfaceHelper()
		{
			Instance = this;
		}

		public void LoadAssets()
		{
			Assets = AssetBundle.LoadFromMemory(ExtractResource("XLMenuMod.Assets.xlmenumod"));
			Sprites = LoadSpriteSheet(Assets).ToList();
			Assets.Unload(false);

			BrandAssets = AssetBundle.LoadFromMemory(ExtractResource("XLMenuMod.Assets.spritesheets_brands"));
			BrandSprites = LoadSpriteSheet(BrandAssets).FirstOrDefault();
			BrandAssets.Unload(false);

			LoadBackgroundTexture();

			var spriteAssets = Resources.FindObjectsOfTypeAll<TMP_SpriteAsset>();
			DarkControllerIcons = spriteAssets.FirstOrDefault(x => x.name == "ControllerIcons_ReversedOut_Greyish");
			LightControllerIcons = spriteAssets.FirstOrDefault(x => x.name == "ControllerIcons_ReversedOut_White");
		}

		private static TMP_SpriteAsset DarkControllerIcons { get; set; }
		private static TMP_SpriteAsset LightControllerIcons { get; set; }

		public TMP_Text CreateSortLabel(TMP_Text sourceText, Transform parent, string sort, int yOffset = -50)
        {
            TMP_Text label = GameObject.Instantiate(sourceText, parent);
            label.transform.localScale = new Vector3(1, 1, 1);
            
            UpdateLabelColor(label, Main.Settings.EnableDarkMode ? DarkModeText : DefaultText);
            label.spriteAsset = Main.Settings.EnableDarkMode ? LightControllerIcons : DarkControllerIcons;

            label.gameObject.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
            label.gameObject.SetActive(false);

            SetSortLabelText(ref label, sort.Replace('_', ' '));

            label.transform.localPosition = Vector3.zero;
            label.transform.Translate(new Vector3(0, yOffset, 0));

            return label;
        }

        public void SetSortLabelText(ref TMP_Text label, string text)
        {
            var sortLabelText = $"<size=80%><sprite={GetSpriteIndex_YButton_Gray()}> <size=60%><b>Sort By:</b> " + text.Replace('_', ' ');
            //var defaultLabelText = $"<size=60%><voffset=0.25em><sprite={GetSpriteIndex_XButton()}></voffset> <b>Set Default</b>";

            label?.SetText(sortLabelText); //+ defaultLabelText);
        }

        public int GetSpriteIndex_YButton_Gray()
        {
	        ControllerIconSprite_Gray returnVal;

	        switch (Application.platform)
	        {
		        case RuntimePlatform.WindowsPlayer:
		        case RuntimePlatform.WindowsEditor:
			        string str = PlayerController.Instance.inputController.player.controllers.Joysticks.FirstOrDefault<Joystick>()?.name ?? "unknown";
			        if (str.Contains("Dual Shock") || str.Contains("DualShock"))
			        {
				        returnVal = ControllerIconSprite_Gray.PS4_Triangle_Button;
				        break;
			        }
			        returnVal = ControllerIconSprite_Gray.XB1_Y;
			        break;
		        case RuntimePlatform.PS4:
			        returnVal = ControllerIconSprite_Gray.PS4_Triangle_Button;
			        break;
		        case RuntimePlatform.Switch:
			        returnVal = ControllerIconSprite_Gray.SWITCH_X;
			        break;
		        case RuntimePlatform.XboxOne:
		        default:
			        returnVal = ControllerIconSprite_Gray.XB1_Y;
			        break;
	        }

	        return (int)returnVal;
        }

        public int GetSpriteIndex_YButton()
        {
	        ControllerIconSprite returnVal;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    string str = PlayerController.Instance.inputController.player.controllers.Joysticks.FirstOrDefault<Joystick>()?.name ?? "unknown";
                    if (str.Contains("Dual Shock") || str.Contains("DualShock"))
                    {
                        returnVal = ControllerIconSprite.PS4_Triangle_Button;
                        break;
                    }
                    returnVal = ControllerIconSprite.XB1_Y;
                    break;
                case RuntimePlatform.PS4:
	                returnVal = ControllerIconSprite.PS4_Triangle_Button;
	                break;
                case RuntimePlatform.Switch:
	                returnVal = ControllerIconSprite.SWITCH_X;
	                break;
                case RuntimePlatform.XboxOne:
                default:
	                returnVal = ControllerIconSprite.XB1_Y;
	                break;
            }

            return (int)returnVal;
        }

        public int GetSpriteIndex_XButton()
        {
	        ControllerIconSprite returnVal;

	        switch (Application.platform)
	        {
		        case RuntimePlatform.WindowsPlayer:
		        case RuntimePlatform.WindowsEditor:
			        string str = PlayerController.Instance.inputController.player.controllers.Joysticks.FirstOrDefault<Joystick>()?.name ?? "unknown";
			        if (str.Contains("Dual Shock") || str.Contains("DualShock"))
			        {
				        returnVal = ControllerIconSprite.PS4_Square_Button;
				        break;
			        }
			        returnVal = ControllerIconSprite.XB1_X;
			        break;
		        case RuntimePlatform.PS4:
			        returnVal = ControllerIconSprite.PS4_Square_Button;
			        break;
		        case RuntimePlatform.Switch:
			        returnVal = ControllerIconSprite.SWITCH_X;
			        break;
		        case RuntimePlatform.XboxOne:
		        default:
			        returnVal = ControllerIconSprite.XB1_X;
			        break;
	        }

	        return (int)returnVal;
        }

        private void LoadBackgroundTexture()
        {
	        var texture2d = new Texture2D(2, 2);
	        if (!texture2d.LoadImage(ExtractResource("XLMenuMod.Assets.darkmode.png"))) return;

	        Sprite sprite = Sprite.Create(texture2d, new Rect(0, 0, texture2d.width, texture2d.height), new Vector2(0.5f, 0.5f));
	        DarkModeBackground = sprite;
        }

        private List<TMP_SpriteAsset> LoadSpriteSheet(AssetBundle bundle)
        {
	        var spriteBrandAssets = bundle.LoadAllAssets<TMP_SpriteAsset>();
	        if (spriteBrandAssets != null)
	        {
		        return spriteBrandAssets.ToList();
	        }

	        return null;
        }

		public static Color32 DarkModeTextColor = new Color32(244, 245, 245, 255);

		public static ColorBlock DarkModeText = new ColorBlock
		{
			colorMultiplier = 1,
			disabledColor = DarkModeTextColor,
			fadeDuration = 0,
			highlightedColor = DarkModeTextColor,
			normalColor = DarkModeTextColor,
			pressedColor = DarkModeTextColor,
			selectedColor = DarkModeTextColor
		};

		public static ColorBlock DefaultText = new ColorBlock
		{
			colorMultiplier = 1,
			disabledColor = new Color(0.784f, 0.784f, 0.784f, .502f),
			fadeDuration = 0,
			highlightedColor = new Color(0.973f, 0.973f, 0.973f, 1.000f),
			normalColor = new Color(0.267f, 0.267f, 0.267f, 1.000f),
			pressedColor = new Color(0.784f, 0.784f, 0.784f, 1.000f),
			selectedColor = new Color(0.973f, 0.973f, 0.973f, 1.000f)
		};

		public void UpdateLabelColor(Selectable button, ColorBlock color)
        {
	        button.colors = color;
        }

		public void UpdateLabelColor(TMP_Text label, ColorBlock color)
		{
			label.color = color.normalColor;
		}

        public void UpdateFontSize(TMP_Text label)
        {
	        switch (Main.Settings.FontSize)
	        {
		        case FontSizePreset.Small:
			        label.fontSize = 30;
			        break;
		        case FontSizePreset.Smaller:
			        label.fontSize = 24;
			        break;
		        case FontSizePreset.Normal:
		        default:
			        label.fontSize = 36;
			        break;
	        }
        }

        public void ToggleDarkMode(bool enabled)
        {
			ToggleDarkMode(GameStateMachine.Instance.PauseObject, enabled);
			ToggleDarkMode(GameStateMachine.Instance.SettingsObject, enabled);

			ToggleDarkMode(GameStateMachine.Instance.TutorialMenuObject, enabled);
			ToggleDarkMode(GameStateMachine.Instance.FeetControlTutorialObject, enabled, true);
			ToggleDarkMode(GameStateMachine.Instance.TutorialFlowObject, enabled, true);

			ToggleDarkMode(GameStateMachine.Instance.ChallengeSummaryObject, enabled);
			ToggleDarkMode(GameStateMachine.Instance.ChallengePlayObject, enabled);
			ToggleDarkMode(GameStateMachine.Instance.SpotSelectionObject, enabled);

			ToggleDarkMode(GameStateMachine.Instance.LevelSelectionObject, enabled);

			ToggleDarkMode(GameStateMachine.Instance.ReplayMenuObject, enabled, true);
			ToggleDarkMode(GameStateMachine.Instance.ReplayDeleteDialog, enabled, true);
        }

        public void ToggleDarkMode(GameObject gameObject, bool enabled, bool hasStaticText = false)
        {
	        SetBackgroundTexture(gameObject);

	        if (hasStaticText)
	        {
		        var components = gameObject.GetComponentsInChildren<TMP_Text>();
		        if (components != null)
		        {
			        foreach (var text in components)
			        {
				        //UnityModManager.Logger.Log("XLMenuMod: Updating TMP_Text: " + text.text + ", dark mode enabled: " + enabled);
				        UpdateLabelColor(text, enabled ? DarkModeText : DefaultText);

				        if (text.text.Contains("<sprite"))
				        {
					        text.spriteAsset = enabled ? LightControllerIcons : DarkControllerIcons;
				        }
			        }
		        }
	        }

	        ToggleDarkMode<MenuButton>(gameObject, enabled);
			ToggleDarkMode<MenuSlider>(gameObject, enabled);
			ToggleDarkMode<MenuToggle>(gameObject, enabled);

			var listView = gameObject.GetComponentInChildren<MVCListView>();
			ToggleDarkMode(listView, enabled);
        }

        public void ToggleDarkMode(MVCListView listView, bool enabled)
        {
	        if (listView == null) return;

	        UpdateFontSize(listView.ItemPrefab.Label);

	        ToggleDarkMode(listView.ItemPrefab, enabled);
	        UpdateLabelColor(listView.HeaderView, enabled ? DarkModeText : DefaultText);

	        foreach (var item in listView.ItemViews)
	        {
		        ToggleDarkMode(item, enabled);
	        }
		}

        public void ToggleDarkMode(MVCListItemView listItemView, bool enabled)
        {
	        var textColor = enabled ? DarkModeText : DefaultText;

	        UpdateFontSize(listItemView.Label);
	        UpdateLabelColor(listItemView, textColor);
		}

        private void ToggleDarkMode<T>(GameObject gameObject, bool enabled) where T : Selectable
        {
	        if (gameObject == null) return;

	        var controls = gameObject.GetComponentsInChildren<T>();
	        foreach (var control in controls)
	        {
		        UpdateLabelColor(control, enabled ? DarkModeText : DefaultText);
	        }
		}

        private void SetBackgroundTexture(GameObject gameObject)
        {
	        if (gameObject == null) return;

	        var textures = gameObject.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "MenuPanelBackground");

	        if (textures != null)
	        {
		        if (OriginalBackgroundTexture == null)
		        {
			        OriginalBackgroundTexture = textures.sprite;
		        }

		        if (Main.Settings.EnableDarkMode && DarkModeBackground != null)
		        {
			        textures.sprite = DarkModeBackground;
		        }
		        else
		        {
			        textures.sprite = OriginalBackgroundTexture;
		        }
	        }
        }



		private byte[] ExtractResource(string filename)
        {
	        Assembly a = Assembly.GetExecutingAssembly();
	        using (var resFilestream = a.GetManifestResourceStream(filename))
	        {
		        if (resFilestream == null) return null;
		        byte[] ba = new byte[resFilestream.Length];
		        resFilestream.Read(ba, 0, ba.Length);
		        return ba;
	        }
        }
	}
}
