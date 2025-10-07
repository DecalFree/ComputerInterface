using System;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;
using ComputerInterface.Queues;

namespace ComputerInterface.Views.GameSettings;

internal class QueueView : ComputerView {
    private readonly List<IQueueInfo> _queues = [
        new DefaultQueue(),
        new CompetitiveQueue(),
        new MinigamesQueue()
    ];
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    public QueueView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = _queues.Count;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);

        var prefsQueue = BaseGameInterface.GetQueue();

        var queue = _queues.FirstOrDefault(q => string.Equals(q.DisplayName, prefsQueue, StringComparison.CurrentCultureIgnoreCase)) ?? _queues.FirstOrDefault(q => q.DisplayName == "Default");

        _selectionHandler.CurrentSelectionIndex = _queues.IndexOf(queue);
        if (!BaseGameInterface.IsInTroop())
            BaseGameInterface.SetQueue(_queues[_selectionHandler.CurrentSelectionIndex]);

        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Queue Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        for (var i = 0; i < _queues.Count; i++) {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(i, _queues[i].DisplayName));
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append(_queues[_selectionHandler.CurrentSelectionIndex].Description);

        SetText(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (!BaseGameInterface.IsInTroop() && _selectionHandler.HandleKeypress(key)) {
                    BaseGameInterface.SetQueue(_queues[_selectionHandler.CurrentSelectionIndex]);
                    Redraw();
                }
                break;
        }
    }
}