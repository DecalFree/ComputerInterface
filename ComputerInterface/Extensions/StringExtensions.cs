namespace ComputerInterface.Extensions;

public static class StringExtensions {
    extension(string str) {
        public string Clamp(int length) {
            if (str.Length > length) {
                string newStr = str[..(length - 3)];
                return newStr + "...";
            }

            return str;
        }
    }
}