using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace ArchitectIcons
{
    [StaticConstructorOnStartup]
    public static class Resources
    {
        public static readonly Dictionary<String, Texture2D> iconsCache = new Dictionary<string, Texture2D>();
        public static readonly Texture2D MissingTexture;
        static Resources()
        {
            //MISSING
            MissingTexture = ContentFinder<Texture2D>.Get("UI/ArchitectIcons/Default/wrongsign");
        }

        public static string GetSettingsPath(string fileName)
        {
            string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "ArchitectIcons");
            var directoryInfo = new DirectoryInfo(path);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            return Path.Combine(path, fileName);
        }

        private static Texture2D LoadCustomIcon(String categoryDefName)
        {
            String path = GetSettingsPath(categoryDefName + ".png");
            if (File.Exists(path))
            {
                Texture2D texture = new Texture2D(16, 16);
                texture.LoadImage(File.ReadAllBytes(path));
                return texture;
            }

            return null;
        }


        public static Texture2D FindArchitectTabCategoryIcon(String categoryDefName)
        {
            if (iconsCache.ContainsKey(categoryDefName))
            {
                return iconsCache[categoryDefName];
            }
            else
            {
                Texture2D icon = null;
                //custom icon
                try
                {
                    icon = LoadCustomIcon(categoryDefName);
                }catch(Exception ex)
                {
                    Logger.Error(ex.Message);
                    Logger.Trace(ex.StackTrace);
                }
                //Search for mod support
                if(!icon)
                    icon = ContentFinder<Texture2D>.Get("UI/ArchitectIcons/" + categoryDefName, false);
                // if not supported
                
                if (!icon)
                {
                    //Check if  ArchitectIcons adds support. If not, set missing texture.
                    icon = ContentFinder<Texture2D>.Get("UI/ArchitectIcons/Default/" + categoryDefName, false) ?? MissingTexture;
                }
                iconsCache.Add(categoryDefName, icon);
                return icon;
            }
        }
    }
}