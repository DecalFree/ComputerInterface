using ComputerInterface.Enumerations;

namespace ComputerInterface.Extensions;

public static class CustomButtonExtensions {
    extension(EKeyboardButton keyboardButton) {
        public bool IsFunctionKey() {
            uint index = (uint)keyboardButton;
            return index is > 35 and < 47;
        }

        public bool IsNumberKey() {
            uint index = (uint)keyboardButton;
            return index <= 9;
        }

        public bool TryParseNumber(out int num) {
            if (keyboardButton.IsNumberKey()) {
                num = (int)keyboardButton;
                return true;
            }

            num = 0;
            return false;
        }

        public bool InRange(char from, char to) {
            char chr = keyboardButton.ToString().ToLower()[0];
            return chr >= from && chr <= to;
        }
    }
}