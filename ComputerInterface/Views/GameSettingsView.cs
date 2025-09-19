using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views.GameSettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace ComputerInterface.Views
{
    public class GameSettingsEntry : IComputerModEntry
    {
        public string EntryName => "Game Settings";
        public Type EntryViewType => typeof(GameSettingsView);
    }

    public class GameSettingsView : ComputerView
    {
        private readonly UIElementPageHandler<Tuple<string, Type>> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        private readonly List<Tuple<string, Type>> _gameSettingsViews;

        public GameSettingsView()
        {
            _gameSettingsViews = new List<Tuple<string, Type>>
            {
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
                new("Support", typeof(SupportView)),
            };

            _pageHandler = new UIElementPageHandler<Tuple<string, Type>>(EKeyboardKey.Left, EKeyboardKey.Right)
            {
                Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>page {2}/{3}</margin></align></color>",
                NextMark = "▼",
                PrevMark = "▲",
                EntriesPerPage = 11
            };
            _pageHandler.SetElements(_gameSettingsViews.ToArray());

            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ItemSelected;
            _selectionHandler.MaxIdx = _gameSettingsViews.Count - 1;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.BeginCenter().AppendClr("== ", "ffffff50").Append("Game Settings").AppendClr(" ==", "ffffff50").EndAlign().AppendLines(2);

            int lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((entry, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.Item1));
                str.AppendLine();
            });

            for (int i = 0; i < _pageHandler.EntriesPerPage - _pageHandler.ItemsOnScreen; i++)
            {
                str.AppendLine();
            }
            str.Append($"<color=#ffffff50><align=\"center\"><  {_pageHandler.CurrentPage + 1}/{_pageHandler.MaxPage + 1}  ></align></color>");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private void ItemSelected(int idx)
        {
            ShowView(_gameSettingsViews[_selectionHandler.CurrentSelectionIndex].Item2);
        }
    }
}
