using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Models;

internal class CIConfig {
    public readonly ConfigEntry<Color> ScreenBackgroundColor;
    public readonly ConfigEntry<string> ScreenBackgroundPath;
    public Texture BackgroundTexture;

    private readonly ConfigEntry<string> _disabledMods;
    private List<string> _disabledModsList;

    public readonly ConfigEntry<bool> AcknowledgedSafetyWarning;

    public CIConfig(ConfigFile config) {
        ScreenBackgroundColor = config.Bind("Appearance", "ScreenBackgroundColor", new Color(0.05f, 0.05f, 0.05f), "The background color of the monitor screen.");
        ScreenBackgroundPath = config.Bind("Appearance", "ScreenBackgroundPath", "BepInEx/plugins/ComputerInterface/background.png", "The background image of the monitor screen.");

        _disabledMods = config.Bind("Data", "DisabledMods", "", "The list of mods disabled by the Computer Interface mod.");

        AcknowledgedSafetyWarning = config.Bind("Safety", "AcknowledgedSafetyWarning", false, "Indicates if the safety warning has been acknowledged by the user.");

        BackgroundTexture = GetTexture(ScreenBackgroundPath.Value);
        DeserializeDisabledMods();
    }

    public void AddDisabledMod(string guid) {
        if (!_disabledModsList.Contains(guid))
            _disabledModsList.Add(guid);

        SerializeDisabledMods();
    }

    public void RemoveDisabledMod(string guid) {
        _disabledModsList.Remove(guid);
        SerializeDisabledMods();
    }

    public bool IsModDisabled(string guid) => _disabledModsList.Contains(guid);

    private void DeserializeDisabledMods() {
        _disabledModsList = [];
        string modString = _disabledMods.Value;
        if (modString.StartsWith(";"))
            modString = modString[1..];

        foreach (string guid in modString.Split(';'))
            _disabledModsList.Add(guid);
    }

    private void SerializeDisabledMods() => _disabledMods.Value = string.Join(";", _disabledModsList);

    public Texture GetTexture(string path) {
        try {
            if (path.IsNullOrWhiteSpace())
                return null;
            FileInfo fileInfo = new(path);
            if (!fileInfo.Exists)
                return null;
            Texture2D texture = new(2, 2);
            texture.LoadImage(File.ReadAllBytes(fileInfo.FullName));
            return texture;
        }
        catch (Exception) {
            Logging.Error("Couldn't load one of Computer Interface's textures");
            return null;
        }
    }
}