﻿using Newtonsoft.Json;
using System.Linq;
using XLMenuMod.Gear.Interfaces;
using XLMenuMod.Interfaces;

namespace XLMenuMod.Gear
{
    public class CustomGearFolderInfo : ICustomGearInfo, ICustomFolderInfo
    {
        [JsonIgnore]
        public ICustomInfo Info { get; set; }

        public CustomFolderInfo FolderInfo
        {
            get { return Info as CustomFolderInfo; }
            set { Info = value; }
        }

        public string GetName() { return Info?.GetName(); }

        public CustomGearFolderInfo(string name, string path, CustomFolderInfo parent)
        {
            Info = new CustomFolderInfo(name, path, parent) { ParentObject = this };

            if (name != "..\\")
            {
                var backFolder = FolderInfo.Children.First();
                backFolder.ParentObject = new CustomGearFolderInfo("..\\", backFolder.GetPath(), parent);
            }
        }
    }
}
