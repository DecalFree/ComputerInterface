using ComputerInterface.Enumerations;

namespace ComputerInterface.Extensions;

public static class CustomKeyEx {
    public static bool IsFunctionKey(this EKeyboardKey key) {
        var idx = (uint)key;
        return idx is > 35 and < 47;
    }

    public static bool IsNumberKey(this EKeyboardKey key) {
        var idx = (uint)key;
        return idx <= 9;
    }

    public static bool TryParseNumber(this EKeyboardKey key, out int num) {
        if (key.IsNumberKey()) {
            num = (int)key;
            return true;
        }

        num = 0;
        return false;
    }

    public static bool InRange(this EKeyboardKey key, char from, char to) {
        var chr = key.ToString().ToLower()[0];
        return chr >= from && chr <= to;
    }
}