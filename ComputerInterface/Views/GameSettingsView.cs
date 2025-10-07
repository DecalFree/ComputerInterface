using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Views.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

public class GameSettingsEntry : IComputerModEntry {
    public string EntryName => "Game Settings";
    public Type EntryViewType => typeof(GameSettingsView);
}

public class GameSettingsView : ComputerView {
    private readonly UIElementPageHandler<Tuple<string, Type>> _pageHandler = new(EKeyboardKey.Left, EKeyboardKey.Right) {
        Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>",
        NextMark = "▼",
        PrevMark = "▲",
        EntriesPerPage = 11
    };
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);

    private readonly List<Tuple<string, Type>> _gameSettingsViews = [
        new("Room   ", typeof(RoomView)),
        new("Name   ", typeof(NameSettingView)),
        new("Color  ", typeof(ColorSettingView)),
        new("Turn   ", typeof(TurnSettingView)),
        new("Mic    ", typeof(MicSettingsView)),
        new("Queue  ", typeof(QueueView)),
        new("Troop  ", typeof(TroopView)),
        new("Group  ", typeof(GroupView)),
        new("Voice  ", typeof(VoiceSettingsView)),
        new("Automod", typeof(AutomodView)),
        new("Items  ", typeof(ItemSettingsView)),
        new("Redeem ", typeof(RedemptionView)),
        new("Credits", typeof(CreditsView)),
        new("Support", typeof(SupportView))
    ];

    public GameSettingsView() {
        _pageHandler.SetElements(_gameSettingsViews.ToArray());

        _selectionHandler.OnSelected += ItemSelected;
        _selectionHandler.MaxIdx = _gameSettingsViews.Count - 1;
        _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().AppendClr("== ", "ffffff50").Append("Game Settings").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

        var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((entry, idx) => {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.Item1));
            stringBuilder.AppendLine();
        });

        for (var i = 0; i < _pageHandler.EntriesPerPage - _pageHandler.ItemsOnScreen; i++)
            stringBuilder.AppendLine();
        stringBuilder.Append($"<color=#ffffff50><align=\"center\"><  {_pageHandler.CurrentPage + 1}/{_pageHandler.MaxPage + 1}  ></align></color>");

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

    private void ItemSelected(int idx) =>
        ShowView(_gameSettingsViews[_selectionHandler.CurrentSelectionIndex].Item2);
}