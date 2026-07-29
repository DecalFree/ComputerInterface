using System;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;

namespace ComputerInterface.Behaviors.UI;

public class UITextInputHandler {
    public string Text;

    public bool IsValid => Validator != null && Validator.Invoke(Text);

    public Func<string, bool> Validator;

    public bool HandleButtonPress(EKeyboardButton keyboardButton) {
        if (keyboardButton == EKeyboardButton.Delete) {
            DeleteChar();
            return true;
        }

        if (keyboardButton == EKeyboardButton.Space) {
            AddSpace();
            return true;
        }

        if (keyboardButton.IsFunctionKey())
            return false;

        TypeChar(keyboardButton);
        return true;
    }

    private void TypeChar(EKeyboardButton keyboardButton) {
        if (keyboardButton.TryParseNumber(out int num)) {
            Text += num;
            return;
        }

        Text += keyboardButton;
    }

    public void AddSpace() => Text += " ";

    public void DeleteChar() {
        if (Text.Length == 0)
            return;

        Text = Text[..^1];
    }
}