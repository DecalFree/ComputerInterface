using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using ComputerInterface.Enumerations;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Models;

internal class CIConfig {
    public readonly ConfigEntry<Color> ScreenBackgroundColor;
    public readonly ConfigEntry<string> ScreenBackgroundPath;
    public Texture BackgroundTexture;
    
    public readonly ConfigEntry<EMonitorType> CurrentMonitorType;

    private readonly ConfigEntry<string> _disabledMods;
    private List<string> _disabledModsList;

    public CIConfig(ConfigFile config) {
        ScreenBackgroundColor = config.Bind("Appearance", "ScreenBackgroundColor", new Color(0.05f, 0.05f, 0.05f), "The background colour of the monitor screen");
        ScreenBackgroundPath = config.Bind("Appearance", "ScreenBackgroundPath", "BepInEx/plugins/ComputerInterface/background.png", "The background image of the monitor screen");
        CurrentMonitorType = config.Bind("Appearance", "Monitor Type", EMonitorType.Modern, "The monitor to use in-game.");
        _disabledMods = config.Bind("Data", "DisabledMods", "", "The list of mods disabled by the ComputerInterface mod");

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

    public bool IsModDisabled(string guid) =>
        _disabledModsList.Contains(guid);

    private void DeserializeDisabledMods() {
        _disabledModsList = [];
        var modString = _disabledMods.Value;
        if (modString.StartsWith(";"))
            modString = modString.Substring(1);

        foreach (var guid in modString.Split(';'))
            _disabledModsList.Add(guid);
    }

    private void SerializeDisabledMods() =>
        _disabledMods.Value = string.Join(";", _disabledModsList);

    public Texture GetTexture(string path) {
        try {
            if (path.IsNullOrWhiteSpace())
                return null;
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                return null;
            var texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(fileInfo.FullName));
            return texture;
        }
        catch (Exception) {
            Logging.Error("Couldn't load CI background");
            return null;
        }
    }
}