using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class QueueView : ComputerView {
    private static readonly List<Tuple<string, string, string>> Queues = [
        new("Default", "DEFAULT", "Default is for anyone to play normally."),
        new("Competitive", "COMPETITIVE", "Competitive is for players who want to play the game, while trying as hard as they can."),
        new("Minigames", "MINIGAMES", "Minigames is for people looking to play with their own set of rules.")
    ];

    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = Queues.Count
    };

    public QueueView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnViewShown(object[] arguments) {
        string prefsQueue = GameInterfaceService.Queue;

        Tuple<string, string, string> queue = Queues.FirstOrDefault(q => string.Equals(q.Item1, prefsQueue, StringComparison.CurrentCultureIgnoreCase)) ?? Queues.FirstOrDefault(q => q.Item1 == "Default");

        _selectionHandler.CurrentSelectionIndex = Queues.IndexOf(queue);
        if (!GameInterfaceService.IsInTroop)
            GameInterfaceService.Queue = Queues[_selectionHandler.CurrentSelectionIndex].Item2;
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Queue Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        for (int i = 0; i < Queues.Count; i++) {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(i, Queues[i].Item1));
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLines(3).AppendClr($"* {Queues[_selectionHandler.CurrentSelectionIndex].Item3}", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (!GameInterfaceService.IsInTroop && _selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.Queue = Queues[_selectionHandler.CurrentSelectionIndex].Item2;
                    UpdateViewScreen();
                }
                break;
        }
    }
}