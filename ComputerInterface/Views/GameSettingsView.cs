using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using ComputerInterface.Views.GameSettings;

namespace ComputerInterface.Views;

public class GameSettingsEntry : IComputerViewEntry {
    public string EntryName => "Game Settings";

    public Type EntryComputerView => typeof(GameSettingsView);
}

public class GameSettingsView : ComputerView {
    private readonly UIElementPageHandler<Tuple<string, Type>> _pageHandler = new(EKeyboardButton.Left, EKeyboardButton.Right) {
        Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>",
        NextMark = "▼",
        PrevMark = "▲",
        EntriesPerPage = 11
    };
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down, EKeyboardButton.Enter);

    private readonly List<Tuple<string, Type>> _gameSettingViews = [
        new("Room      ", typeof(SessionView)),
        new("Name      ", typeof(NameSettingView)),
        // new("Language  ", typeof(LanguageView)),
        new("Color     ", typeof(ColorSettingView)),
        new("Turn      ", typeof(TurnSettingView)),
        new("Microphone", typeof(MicrophoneSettingView)),
        new("Queue     ", typeof(QueueView)),
        new("Troop     ", typeof(TroopView)),
        new("Group     ", typeof(GroupView)),
        new("Voice     ", typeof(VoiceSettingView)),
        new("AutoMute  ", typeof(AutoMuteView)),
        new("Items     ", typeof(ItemSettingView)),
        new("Redemption", typeof(RedemptionView)),
        new("Credits   ", typeof(CreditsView)),
        new("Support   ", typeof(SupportView))
    ];

    public GameSettingsView() {
        _pageHandler.SetElements([.. _gameSettingViews]);

        _selectionHandler.OnSelected += ItemSelected;
        _selectionHandler.MaxIndex = _gameSettingViews.Count - 1;
        _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
    }

    private void ItemSelected(int index) => ShowView(_gameSettingViews[_selectionHandler.CurrentSelectionIndex].Item2);

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().AppendClr("== ", "ffffff50").Append("Game Settings").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

        int lineIndex = _pageHandler.MovePageToIndex(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((entry, index) => {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(index, lineIndex, entry.Item1));
            stringBuilder.AppendLine();
        });

        for (int i = 0; i < _pageHandler.EntriesPerPage - _pageHandler.ItemsOnScreen; i++)
            stringBuilder.AppendLine();
        stringBuilder.Append($"<color=#ffffff50><align=\"center\"><  {_pageHandler.CurrentPage + 1}/{_pageHandler.MaxPage + 1}  ></align></color>");

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ReturnToMainMenu();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}