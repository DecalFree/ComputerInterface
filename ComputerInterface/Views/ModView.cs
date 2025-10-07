using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

internal class ModView : ComputerView {
    private ModListView.ModListItem _plugin;

    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);

    public ModView() {
        _selectionHandler.OnSelected += OnOptionSelected;
        _selectionHandler.MaxIdx = 1;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        if (args == null || args.Length == 0)
            return;

        _plugin = (ModListView.ModListItem)args[0];
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        RedrawHeader(stringBuilder);
        RedrawSelection(stringBuilder);
        DrawNotice(stringBuilder);

        Text = stringBuilder.ToString();
    }

    private void RedrawHeader(StringBuilder stringBuilder) {
        var pluginInfo = _plugin.PluginInfo;
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append($"{pluginInfo.Metadata.Name} ({(_plugin.PluginInfo.Instance.enabled ? "<color=#00ff00>Enabled</color>" : "<color=#ff0000>Disabled</color>")})").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append($"<size=40>{pluginInfo.Metadata.GUID}, v{pluginInfo.Metadata.Version}</size>").AppendLines(2);
    }

    private void RedrawSelection(StringBuilder stringBuilder) {
        stringBuilder.AppendLine();
        stringBuilder.Append(GetSelectionString(0, "[")).Append("<color=#7Cff7C>Enabled</color>").Append(GetSelectionString(0, "]")).AppendLine();
        stringBuilder.Append(GetSelectionString(1, "[")).Append("<color=#ff7C7C>Disabled</color>").Append(GetSelectionString(1, "]")).AppendLine();
        stringBuilder.AppendLine().AppendLine();
    }

    private void DrawNotice(StringBuilder stringBuilder) {
        if (!_plugin.Supported) {
            stringBuilder.BeginCenter().AppendClr("This mod doesn't support toggling between Enabled/Disabled states.", "ff505050").EndAlign();
            return;
        }

        stringBuilder.Append("1. Select an option, either Enable or Disable").AppendLines(2);
        stringBuilder.Append("2. Press Enter, the mod will be toggled accordingly");
    }

    private string GetSelectionString(int idx, string character) =>
        _selectionHandler.CurrentSelectionIndex == idx ? "<color=#ed6540>" + character + "</color>" : " ";

    private void OnOptionSelected(int idx) {
        if (idx == 0) {
            // Enable was pressed
            _plugin.PluginInfo.Instance.enabled = true;
            Plugin.CIConfig.RemoveDisabledMod(_plugin.PluginInfo.Metadata.GUID);
            return;
        }

        if (idx == 1) {
            // Disable was pressed
            _plugin.PluginInfo.Instance.enabled = false;
            Plugin.CIConfig.AddDisabledMod(_plugin.PluginInfo.Metadata.GUID);
        }

        Redraw();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            Redraw();
            return;
        }

        if (key == EKeyboardKey.Back)
            ReturnView();
    }
}