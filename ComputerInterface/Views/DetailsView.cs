using System;
using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using GorillaNetworking;

namespace ComputerInterface.Views;

internal class DetailsEntry : IComputerViewEntry {
    public string EntryName => "Details";

    public Type EntryComputerView => typeof(DetailsView);
}

internal class DetailsView : ComputerView {
    private string _name;
    private string _sessionName;

    private int _playerCount;
    private int _playersBanned;

    public override void OnViewShown(object[] arguments) {
        _name = GameInterfaceService.GetPeerName();
        _sessionName = GameInterfaceService.GetSessionName();
        _playerCount = NetworkSystem.Instance.GlobalPlayerCount();
        _playersBanned = GorillaComputer.instance.GetField<int>("usersBanned");
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Details").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append("<size=40>Press any key to update page</size>").AppendLines(2);

        stringBuilder.BeginColor("ffffff50").Append("Name: ").EndColor();
        stringBuilder.Append($"<size=50>{_name}</size>").AppendLine();
        stringBuilder.BeginColor("ffffff50").Append("Display Name: ").EndColor();
        stringBuilder.Append($"<size=50>{GorillaTagger.Instance.offlineVRRig.NormalizeName(true, _name).ToUpper()}</size>").AppendLines(3);

        stringBuilder.BeginColor("ffffff50").Append("Players Online: ").EndColor();
        stringBuilder.Append($"<size=50>{_playerCount}</size>").AppendLine();
        stringBuilder.BeginColor("ffffff50").Append("Users Banned: ").EndColor();
        stringBuilder.Append($"<size=50>{_playersBanned} (Yesterday)</size>").AppendLines(3);

        stringBuilder.BeginColor("ffffff50").Append("Current Room: ").EndColor();
        stringBuilder.Append($"<size=50>{(_sessionName.IsNullOrWhiteSpace() ? "-None-" : _sessionName)}</size>").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ReturnToMainMenu();
                break;
            default:
                UpdateViewScreen();
                break;
        }
    }
}