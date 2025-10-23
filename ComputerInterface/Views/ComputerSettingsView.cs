using System;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

public class ComputerSettingsEntry : IComputerModEntry {
    public string EntryName => "Computer Settings";
    
    public Type EntryViewType => typeof(ComputerSettingsView);
}

public class ComputerSettingsView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);

    private readonly MonitorController _monitorController = new();
    private readonly CIConfig _config = Plugin.CIConfig;

    public ComputerSettingsView() {
        _selectionHandler.OnSelected += SetNewMonitor;
        _selectionHandler.MaxIdx = 1;
        _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        _selectionHandler.CurrentSelectionIndex = (int)_config.CurrentMonitorType.Value;
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.BeginCenter().AppendClr("== ", "ffffff50").Append("Computer Settings").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

        stringBuilder.Append("Monitor Type:").AppendLine();
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "Classic"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Modern"));
        
        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            Redraw();
            return;
        }
        
        switch (key) {
            case EKeyboardKey.Back:
                ReturnToMainMenu();
                break;
        }
    }

    private void SetNewMonitor(int idx) {
        if (_config.CurrentMonitorType.Value == _monitorController.Monitors[_selectionHandler.CurrentSelectionIndex].MonitorType)
            return;
        
        _monitorController.SetMonitor(_monitorController.Monitors[_selectionHandler.CurrentSelectionIndex].MonitorType);
    }
}