using System;
using ComputerInterface.Enumerations;

namespace ComputerInterface.Behaviors.UI;

public class UISelectionHandler {
    public event Action<int> OnSelected;

    public int CurrentSelectionIndex;

    /// <summary>
    /// Min 0 indexed item
    /// This can stay on 0
    /// </summary>
    public int Min = 0;

    /// <summary>
    /// Max 0 indexed item
    /// e.g. If you have two items this should be 1
    /// </summary>
    public int MaxIndex { get; set; }

    private readonly EKeyboardButton _upButton;
    private readonly EKeyboardButton _downButton;
    private readonly EKeyboardButton _selectButton;
    private readonly bool _canSelect;

    private string _startSelected;
    private string _endSelected;
    private string _startNormal;
    private string _endNormal;

    public UISelectionHandler(EKeyboardButton upButton, EKeyboardButton downButton, EKeyboardButton selectButton) {
        _upButton = upButton;
        _downButton = downButton;
        _selectButton = selectButton;
        _canSelect = true;
    }

    public UISelectionHandler(EKeyboardButton upButton, EKeyboardButton downButton) {
        _upButton = upButton;
        _downButton = downButton;
    }

    public bool HandleButtonPress(EKeyboardButton keyboardButton) {
        if (keyboardButton == _upButton) {
            MoveSelectionUp();
            return true;
        }

        if (keyboardButton == _downButton) {
            MoveSelectionDown();
            return true;
        }

        if (_canSelect && keyboardButton == _selectButton) {
            OnSelected?.Invoke(CurrentSelectionIndex);
            return true;
        }

        return false;
    }

    public void MoveSelectionUp() {
        CurrentSelectionIndex--;
        ClampSelection();
    }

    public void MoveSelectionDown() {
        CurrentSelectionIndex++;
        ClampSelection();
    }

    public void ConfigureSelectionIndicator(string startSelected, string endSelected, string startNormal, string endNormal) {
        _startSelected = startSelected;
        _endSelected = endSelected;
        _startNormal = startNormal;
        _endNormal = endNormal;
    }

    public string GetIndicatedText(int index, int current, string text) {
        if (index == current)
            return _startSelected + text + _endSelected;

        return _startNormal + text + _endNormal;
    }

    public string GetIndicatedText(int index, string text) => GetIndicatedText(index, CurrentSelectionIndex, text);

    private void ClampSelection() {
        if (CurrentSelectionIndex > MaxIndex) {
            CurrentSelectionIndex = MaxIndex;
            return;
        }

        if (CurrentSelectionIndex < Min)
            CurrentSelectionIndex = Min;
    }
}