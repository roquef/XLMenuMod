﻿using Newtonsoft.Json;
using System.Linq;
using XLMenuMod.Interfaces;

namespace XLMenuMod.Levels
{
    public class CustomLevelFolderInfo : LevelInfo, ICustomFolderInfo
    {
        [JsonIgnore]
        public CustomFolderInfo FolderInfo { get; set; }

        public CustomLevelFolderInfo(string name, string path, CustomFolderInfo parent)
        {
            this.name = name;
            this.path = path;

            FolderInfo = new CustomFolderInfo(name, path, parent) { ParentObject = this };

            if (name != "..\\")
            {
                var backFolder = FolderInfo.Children.First();
                backFolder.ParentObject = new CustomLevelFolderInfo("..\\", backFolder.GetPath(), parent);
            }
        }
    }
}
