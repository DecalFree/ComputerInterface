using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

public class ItemSettingsView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    private float _insVolumeFloat = 0.10f;

    public ItemSettingsView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = 1;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        UpdateState();
        Redraw();
    }

    private void UpdateState() {
        _selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetItemMode() ? 1 : 0;
        _insVolumeFloat = BaseGameInterface.GetInstrumentVolume();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Item Tab").AppendLine();
        stringBuilder.AppendClr("0 - 9 to set Instrument Volume", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.Append("Instrument Volume: ").Append(Mathf.CeilToInt(_insVolumeFloat * 50f));
        stringBuilder.AppendLines(3);

        stringBuilder.Append("Item Particles:").AppendLine();
        stringBuilder.Append(_selectionHandler.GetIndicatedText(0, "Enabled")).AppendLine();
        stringBuilder.Append(_selectionHandler.GetIndicatedText(1, "Disabled")).AppendLine();

        SetText(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            BaseGameInterface.SetItemMode(_selectionHandler.CurrentSelectionIndex == 1);
            Redraw();
            return;
        }

        if (key.TryParseNumber(out var num)) {
            BaseGameInterface.SetInstrumentVolume(num);
            UpdateState();
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}