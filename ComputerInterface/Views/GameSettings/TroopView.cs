using System.Text;
using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

public class TroopView : ComputerView
{
    private readonly UITextInputHandler _textInputHandler;
    private BaseGameInterface.WordCheckResult _wordCheckResult;
    
    public TroopView() => _textInputHandler = new UITextInputHandler();
    
    public override void OnShow(object[] args)
    {
        base.OnShow(args);
        
        Redraw();
    }

    private void Redraw()
    {
        BaseGameInterface.CheckForComputer(out var computer);
        
        StringBuilder str = new();
        
        str.Repeat("=", SCREEN_WIDTH).AppendLine();
        str.BeginCenter().Append("Troop Tab").AppendLine();

        bool showState = !BaseGameInterface.IsInTroop();
        
        if (showState)
        {
            switch (_wordCheckResult)
            {
                case BaseGameInterface.WordCheckResult.Allowed:
                    str.AppendClr("Ready - Enter to Join or Create Troop", "ffffff50").EndAlign().AppendLine();
                    break;
                case BaseGameInterface.WordCheckResult.Blank:
                    str.AppendClr("Error - Troop is Blank", "ffffff50").EndAlign().AppendLine();
                    break;
                case BaseGameInterface.WordCheckResult.Empty:
                    str.AppendClr("Error - Troop is Empty", "ffffff50").EndAlign().AppendLine();
                    break;
                case BaseGameInterface.WordCheckResult.TooLong:
                    str.AppendClr("Error - Troop Exceeds Character Limit", "ffffff50").EndAlign().AppendLine();
                    break;
                case BaseGameInterface.WordCheckResult.NotAllowed:
                    str.AppendClr("Error - Troop Inappropriate", "ffffff50").EndAlign().AppendLine();
                    break;
            }
        }

        str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLine();
        str.AppendLine();

        if (BaseGameInterface.IsValidTroopName(computer.troopName))
        {
            str.AppendLine($"Current Troop: {BaseGameInterface.GetCurrentTroop()}");
            str.AppendLine($"Players In Troop: {Mathf.Max(1, computer.GetCurrentTroopPopulation())}");
            
            str.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append(computer.troopQueueActive ? "Press Option 2 for default queue." : "Press Option 1 for troop queue.");
            str.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Press Option 3 to leave your troop.");
        }
        else
        {
            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
            
            str.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to join or create a troop.");
        }

        Text = str.ToString();
    }
    
    public override void OnKeyPressed(EKeyboardKey key)
    {
        if (!BaseGameInterface.IsInTroop() && _textInputHandler.HandleKey(key))
        {
            if (_textInputHandler.Text.Length > BaseGameInterface.MAX_TROOP_LENGTH)
                _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MAX_TROOP_LENGTH];

            Redraw();
            return;
        }
        
        switch (key)
        {
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