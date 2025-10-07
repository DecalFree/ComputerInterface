using System.Text;

namespace ComputerInterface.Extensions;

/// <summary>
/// Bunch of extension methods for the <see cref="StringBuilder"/>
/// </summary>
public static class StringBuilderEx {
    public static StringBuilder AppendClr(this StringBuilder stringBuilder, string text, string color) =>
        stringBuilder.BeginColor(color).Append(text).EndColor();

    /// <summary>
    /// Writes a string with the specified color
    /// </summary>
    /// <param name="stringBuilder">the string to print</param>
    /// <param name="color">the hex color (doesn't have to start with '#')</param>
    /// <returns></returns>
    public static StringBuilder BeginColor(this StringBuilder stringBuilder, string color) {
        if (color[0] != '#')
            color = "#" + color;
        return stringBuilder.Append($"<color={color}>");
    }

    public static StringBuilder BeginColor(this StringBuilder stringBuilder, UnityEngine.Color color) =>
        stringBuilder.BeginColor(UnityEngine.ColorUtility.ToHtmlStringRGB(color));

    public static StringBuilder EndColor(this StringBuilder stringBuilder) =>
        stringBuilder.Append("</color>");

    public static StringBuilder BeginAlign(this StringBuilder stringBuilder, string align) =>
        stringBuilder.Append($"<align=\"{align}\">");

    public static StringBuilder EndAlign(this StringBuilder stringBuilder) =>
        stringBuilder.Append("</align>");

    public static StringBuilder BeginCenter(this StringBuilder stringBuilder) =>
        stringBuilder.BeginAlign("center");

    public static StringBuilder Repeat(this StringBuilder stringBuilder, string toRepeat, int repeatNum) {
        for (var i = 0; i < repeatNum; i++)
            stringBuilder.Append(toRepeat);

        return stringBuilder;
    }

    public static StringBuilder AppendLines(this StringBuilder stringBuilder, int numOfLines) {
        stringBuilder.Repeat("\n", numOfLines);
        return stringBuilder;
    }

    public static StringBuilder BeginMono(this StringBuilder stringBuilder, int spacing = 58) {
        stringBuilder.Append("<mspace=58>");
        return stringBuilder;
    }

    public static StringBuilder EndMono(this StringBuilder stringBuilder) {
        stringBuilder.Append("</mspace>");
        return stringBuilder;
    }

    public static StringBuilder AppendMono(this StringBuilder stringBuilder, string text, int spacing = 58) {
        stringBuilder.BeginMono(spacing).Append(text).EndMono();
        return stringBuilder;
    }

    public static StringBuilder AppendSize(this StringBuilder stringBuilder, string text, int size) {
        stringBuilder.Append($"<size={size}%>").Append(text).Append("</size>");
        return stringBuilder;
    }

    public static StringBuilder BeginVOffset(this StringBuilder stringBuilder, float offset) {
        stringBuilder.Append($"<voffset={offset}em>");
        return stringBuilder;
    }

    public static StringBuilder EndVOffset(this StringBuilder stringBuilder) {
        stringBuilder.Append("</voffset>");
        return stringBuilder;
    }

    public static StringBuilder MakeBar(this StringBuilder stringBuilder, char chr, int length, float offset, string color = null) {
        stringBuilder.BeginVOffset(offset);
        if (color != null)
            stringBuilder.BeginColor(color);
        stringBuilder.Repeat(chr.ToString(), length);
        if (color != null)
            stringBuilder.EndColor();
        stringBuilder.EndVOffset();
        return stringBuilder;
    }

    public static string Clamp(this string str, int length) {
        if (str.Length > length) {
            var newStr = str[..(length - 3)];
            return newStr + "...";
        }

        return str;
    }
}