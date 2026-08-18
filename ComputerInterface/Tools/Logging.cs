namespace ComputerInterface.Tools;

internal static class Logging {
    public static void Info(object logContent) => PluginCore.CurrentModLoader.OnLogMessage(logContent);

    public static void Warning(object logContent) => PluginCore.CurrentModLoader.OnLogWarning(logContent);

    public static void Error(object logContent) => PluginCore.CurrentModLoader.OnLogError(logContent);
}