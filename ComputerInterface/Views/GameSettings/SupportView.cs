using ComputerInterface.Extensions;
using GorillaNetworking;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class SupportView : ComputerView {
    public override void OnShow(object[] args) {
        base.OnShow(args);
        BaseGameInterface.InitSupportMode();

        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        DrawHeader(stringBuilder);
        DrawOptions(stringBuilder);

        SetText(stringBuilder);
    }

    private void DrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Support Tab").AppendLine();
        stringBuilder.AppendClr("Only show this to AA support", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndColor().EndAlign().AppendLines(2);
    }

    private void DrawOptions(StringBuilder stringBuilder) {
        if (!BaseGameInterface.DisplaySupportTab) {
            stringBuilder.AppendLine("To roomView support and account inforamtion, press the Option 1 key.").AppendLines(2);
            stringBuilder.AppendClr("Only show this information to Another Axiom support.", ColorUtility.ToHtmlStringRGB(Color.red));
            SetText(stringBuilder);
            return;
        }

        stringBuilder.Append("Player ID: ").Append(PlayFabAuthenticator.instance.GetPlayFabPlayerId()).AppendLine();
        stringBuilder.Append("Platform: ").Append("Steam").AppendLines(2);
        stringBuilder.Append("Version: ").Append(GorillaComputer.instance.GetField<string>("version")).AppendLine();
        stringBuilder.Append("Build Date: ").Append(GorillaComputer.instance.GetField<string>("buildDate")).AppendLine();
        SetText(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Option1:
                BaseGameInterface.DisplaySupportTab = true;
                Redraw();
                break;
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}