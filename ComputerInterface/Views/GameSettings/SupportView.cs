using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class SupportView : ComputerView {
    public override void OnViewShown(object[] arguments) => GameInterfaceService.DisplaySupportTab = false;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Support Tab").AppendLine();
        stringBuilder.AppendClr("Only show this to AA support", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        if (!GameInterfaceService.DisplaySupportTab) {
            stringBuilder.AppendLine("To view support and account information, press the Option 1 key.").AppendLines(2);
            stringBuilder.AppendClr("Only show this information to Another Axiom support.", ColorUtility.ToHtmlStringRGB(Color.red));
            return stringBuilder.ToString();
        }

        stringBuilder.Append("Player ID: ").Append(GameInterfaceService.PeerID).AppendLine();
        stringBuilder.Append("Platform: ").Append(GameInterfaceService.PeerPlatform).AppendLines(2);

        stringBuilder.Append("Version: ").Append(GameInterfaceService.GameVersion).AppendLine();
        stringBuilder.Append("Build Date: ").Append(GameInterfaceService.BuildDate).AppendLine();
        stringBuilder.Append("Session ID: ").Append(GameInterfaceService.PeerSessionID).AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Option1:
                GameInterfaceService.DisplaySupportTab = true;
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}