#if BEPINEX
using System.Text;
using BepInEx;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class ModView : ComputerView {
    private ModListView.ModListItem _plugin;

    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down, EKeyboardButton.Enter) {
        MaxIndex = 1
    };

    public ModView() => _selectionHandler.OnSelected += OnOptionSelected;

    private void OnOptionSelected(int index) {
        if (!_plugin.Supported)
            return;

        switch (index) {
            case 0:
                // Enable was pressed
                _plugin.PluginInfo.Instance.enabled = true;
                PluginCore.CIConfig.RemoveDisabledMod(_plugin.PluginInfo.Metadata.GUID);
                return;
            case 1:
                // Disable was pressed
                _plugin.PluginInfo.Instance.enabled = false;
                PluginCore.CIConfig.AddDisabledMod(_plugin.PluginInfo.Metadata.GUID);
                break;
        }

        UpdateViewScreen();
    }

    private string GetSelectionString(int index, string character) => _selectionHandler.CurrentSelectionIndex == index ? "<color=#ed6540>" + character + "</color>" : " ";

    public override void OnViewShown(object[] arguments) {
        if (arguments == null || arguments.Length == 0)
            return;

        _plugin = (ModListView.ModListItem)arguments[0];
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        PluginInfo pluginInfo = _plugin.PluginInfo;
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append($"{pluginInfo.Metadata.Name} ({(_plugin.PluginInfo.Instance.enabled ? "<color=#00ff00>Enabled</color>" : "<color=#ff0000>Disabled</color>")})").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append($"<size=40>{pluginInfo.Metadata.GUID}, v{pluginInfo.Metadata.Version}</size>").AppendLines(2);

        stringBuilder.AppendLine();
        stringBuilder.Append(GetSelectionString(0, "[")).Append("<color=#7Cff7C>Enabled</color>").Append(GetSelectionString(0, "]")).AppendLine();
        stringBuilder.Append(GetSelectionString(1, "[")).Append("<color=#ff7C7C>Disabled</color>").Append(GetSelectionString(1, "]")).AppendLine();
        stringBuilder.AppendLine().AppendLine();

        if (!_plugin.Supported) {
            stringBuilder.BeginCenter().AppendClr("This mod doesn't support toggling between Enabled/Disabled states.", "ff505050").EndAlign();
            return stringBuilder.ToString();
        }

        stringBuilder.Append("1. Select an option, either Enable or Disable").AppendLines(2);
        stringBuilder.Append("2. Press Enter, the mod will be toggled accordingly");

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ReturnToPreviousView();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}
#endif