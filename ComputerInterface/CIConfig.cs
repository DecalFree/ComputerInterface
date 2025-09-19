using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ComputerInterface
{
    internal class CIConfig {
        public ConfigEntry<Color> ScreenBackgroundColor;
        public ConfigEntry<string> ScreenBackgroundPath;
        public Texture BackgroundTexture;

        private readonly ConfigEntry<string> _disabledMods;
        private List<string> _disabledModsList;

        public CIConfig(ConfigFile config)
        {
            ScreenBackgroundColor = config.Bind("Appearance", "ScreenBackgroundColor", new Color(0.05f, 0.05f, 0.05f), "The background colour of the monitor screen");
            ScreenBackgroundPath = config.Bind("Appearance", "ScreenBackgroundPath", "BepInEx/plugins/ComputerInterface/background.png", "The background image of the monitor screen");
            _disabledMods = config.Bind("Data", "DisabledMods", "", "The list of mods disabled by the ComputerInterface mod");

            BackgroundTexture = GetTexture(ScreenBackgroundPath.Value);
            DeserializeDisabledMods();
        }

        public void AddDisabledMod(string guid)
        {
            if (!_disabledModsList.Contains(guid))
            {
                _disabledModsList.Add(guid);
            }
            SerializeDisabledMods();
        }

        public void RemoveDisabledMod(string guid)
        {
            _disabledModsList.Remove(guid);
            SerializeDisabledMods();
        }

        public bool IsModDisabled(string guid)
        {
            return _disabledModsList.Contains(guid);
        }

        private void DeserializeDisabledMods()
        {
            _disabledModsList = new List<string>();
            string modString = _disabledMods.Value;
            if (modString.StartsWith(";")) modString = modString.Substring(1);

            foreach (string guid in modString.Split(';'))
            {
                _disabledModsList.Add(guid);
            }
        }

        private void SerializeDisabledMods()
        {
            _disabledMods.Value = string.Join(";", _disabledModsList);
        }

        private Texture GetTexture(string path)
        {
            try
            {
                if (path.IsNullOrWhiteSpace()) return null;
                FileInfo file = new(path);
                if (!file.Exists) return null;
                Texture2D tex = new(2, 2);
                tex.LoadImage(File.ReadAllBytes(file.FullName));
                return tex;
            }
            catch (Exception)
            {
                Debug.LogError("Couldn't load CI background");
                return null;
            }
        }
    }
}