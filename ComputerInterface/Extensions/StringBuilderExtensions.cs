using System.Text;

namespace ComputerInterface.Extensions;

public static class StringBuilderExtensions {
    extension(StringBuilder stringBuilder) {
        public StringBuilder AppendClr(string text, string color) => stringBuilder.BeginColor(color).Append(text).EndColor();

        /// <summary>
        /// Writes a string with the specified color
        /// </summary>
        /// <param name="stringBuilder">the string to print</param>
        /// <param name="color">the hex color (doesn't have to start with '#')</param>
        /// <returns></returns>
        public StringBuilder BeginColor(string color) {
            if (color[0] != '#')
                color = "#" + color;
            return stringBuilder.Append($"<color={color}>");
        }

        public StringBuilder BeginColor(UnityEngine.Color color) => stringBuilder.BeginColor(UnityEngine.ColorUtility.ToHtmlStringRGB(color));

        public StringBuilder EndColor() => stringBuilder.Append("</color>");

        public StringBuilder BeginAlign(string align) => stringBuilder.Append($"<align=\"{align}\">");

        public StringBuilder EndAlign() => stringBuilder.Append("</align>");

        public StringBuilder BeginCenter() => stringBuilder.BeginAlign("center");

        public StringBuilder Repeat(string toRepeat, int repeatNum) {
            for (int i = 0; i < repeatNum; i++)
                stringBuilder.Append(toRepeat);

            return stringBuilder;
        }

        public StringBuilder AppendLines(int numOfLines) {
            stringBuilder.Repeat("\n", numOfLines);
            return stringBuilder;
        }

        public StringBuilder BeginMono(int spacing = 58) {
            stringBuilder.Append("<mspace=58>");
            return stringBuilder;
        }

        public StringBuilder EndMono() {
            stringBuilder.Append("</mspace>");
            return stringBuilder;
        }

        public StringBuilder AppendMono(string text, int spacing = 58) {
            stringBuilder.BeginMono(spacing).Append(text).EndMono();
            return stringBuilder;
        }

        public StringBuilder AppendSize(string text, int size) {
            stringBuilder.Append($"<size={size}%>").Append(text).Append("</size>");
            return stringBuilder;
        }

        public StringBuilder BeginVOffset(float offset) {
            stringBuilder.Append($"<voffset={offset}em>");
            return stringBuilder;
        }

        public StringBuilder EndVOffset() {
            stringBuilder.Append("</voffset>");
            return stringBuilder;
        }

        public StringBuilder MakeBar(char chr, int length, float offset, string color = null) {
            stringBuilder.BeginVOffset(offset);
            if (color != null)
                stringBuilder.BeginColor(color);
            stringBuilder.Repeat(chr.ToString(), length);
            if (color != null)
                stringBuilder.EndColor();
            stringBuilder.EndVOffset();
            return stringBuilder;
        }
    }
}