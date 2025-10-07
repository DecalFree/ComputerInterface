using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using GorillaNetworking;
using System;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class DetailsEntry : IComputerModEntry {
    public string EntryName => "Details";
    public Type EntryViewType => typeof(DetailsView);
}

internal class DetailsView : ComputerView {
    private string _name;
    private string _roomCode;
    private int _playerCount;
    private int _playerBans;

    public override void OnShow(object[] args) {
        base.OnShow(args);
        UpdateStats();
        Redraw();
    }

    private void UpdateStats() {
        _name = BaseGameInterface.GetName();
        _roomCode = BaseGameInterface.GetRoomCode();
        _playerCount = NetworkSystem.Instance.GlobalPlayerCount();
        _playerBans = GorillaComputer.instance.GetField<int>("usersBanned");
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

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
        stringBuilder.Append($"<size=50>{_playerBans} (Yesterday)</size>").AppendLines(3);

        stringBuilder.BeginColor("ffffff50").Append("Current Room: ").EndColor();
        stringBuilder.Append($"<size=50>{(_roomCode.IsNullOrWhiteSpace() ? "-None-" : _roomCode)}</size>").AppendLine();

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                ReturnToMainMenu();
                break;
            default:
                Redraw();
                break;
        }
    }
}