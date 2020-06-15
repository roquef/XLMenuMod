﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityModManagerNet;
using XLMenuMod.Levels.Interfaces;

namespace XLMenuMod.Levels
{
    public class CustomFolderInfo : LevelInfo, ICustomLevelInfo
    {
        [JsonIgnore]
        public ICustomLevelInfo Parent { get; set; }

        [JsonIgnore]
        public List<ICustomLevelInfo> Children { get; set; }

        /// <summary>
        /// Will be the sum of the directory's contents.
        /// </summary>
        [JsonIgnore]
        public long Size { get; set; }

        [JsonIgnore]
        public int PlayCount { get; set; }

        [JsonIgnore]
        public DateTime ModifiedDate { get; set; }

        [JsonIgnore]
        public bool IsFavorite { get; set; }

        public string GetName() { return name; }
        public long GetSize() { return Size; }


        public CustomFolderInfo(string name, string path, ICustomLevelInfo parent)
        {
            this.name = name;
            this.path = path;
            Parent = parent;
            Children = new List<ICustomLevelInfo>();

            if (GetName() != "..\\" && !string.IsNullOrEmpty(path))
                Size = GetDirectorySize(path);
        }

        private long GetDirectorySize(string directory)
        {
            var files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).Where(x => Path.GetExtension(x).ToLower() != "*.dll");

            long directorySize = 0;
            foreach (var file in files)
            {
                directorySize += new FileInfo(file).Length;
            }

            return directorySize;
        }
    }
}
