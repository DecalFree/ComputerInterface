using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

public class TroopView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();
    private EWordCheckResult _wordCheckResult;

    public override void OnShow(object[] args) {
        base.OnShow(args);
        
        Redraw();
    }

    private void Redraw() {
        BaseGameInterface.CheckForComputer(out var computer);
        
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.BeginCenter().Append("Troop Tab").AppendLine();

        var showState = !BaseGameInterface.IsInTroop();
        
        if (showState) {
            switch (_wordCheckResult) {
                case EWordCheckResult.Allowed:
                    stringBuilder.AppendClr("Ready - Enter to Join or Create Troop", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.Blank:
                    stringBuilder.AppendClr("Error - Troop is Blank", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.Empty:
                    stringBuilder.AppendClr("Error - Troop is Empty", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.TooLong:
                    stringBuilder.AppendClr("Error - Troop Exceeds Character Limit", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.NotAllowed:
                    stringBuilder.AppendClr("Error - Troop Inappropriate", "ffffff50").EndAlign().AppendLine();
                    break;
            }
        }

        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLine();
        stringBuilder.AppendLine();

        if (BaseGameInterface.IsValidTroopName(computer.troopName)) {
            stringBuilder.AppendLine($"Current Troop: {BaseGameInterface.GetCurrentTroop()}");
            stringBuilder.AppendLine($"Players In Troop: {Mathf.Max(1, computer.GetCurrentTroopPopulation())}");
            
            stringBuilder.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append(computer.troopQueueActive ? "Press Option 2 for default queue." : "Press Option 1 for troop queue.");
            stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Press Option 3 to leave your troop.");
        }
        else {
            stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
            
            stringBuilder.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to join or create a troop.");
        }

        Text = stringBuilder.ToString();
    }
    
    public override void OnKeyPressed(EKeyboardKey key) {
        if (!BaseGameInterface.IsInTroop() && _textInputHandler.HandleKey(key)) {
            if (_textInputHandler.Text.Length > BaseGameInterface.MaxTroopLength)
                _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MaxTroopLength];

            Redraw();
            return;
        }
        
        switch (key) {
            case EKeyboardKey.Option1:
                BaseGameInterface.JoinTroopQueue();
                Redraw();
                break;
            case EKeyboardKey.Option2:
                BaseGameInterface.JoinDefaultQueue();
                Redraw();
                break;
            case EKeyboardKey.Option3:
                BaseGameInterface.LeaveTroop();
                Redraw();
                break;
            case EKeyboardKey.Enter:
                _wordCheckResult = BaseGameInterface.JoinTroop(_textInputHandler.Text);
                Redraw();
                break;
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}