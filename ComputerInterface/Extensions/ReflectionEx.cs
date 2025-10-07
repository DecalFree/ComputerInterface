using HarmonyLib;

namespace ComputerInterface.Extensions;

internal static class ReflectionEx {
    public static void InvokeMethod(this object obj, string name, params object[] parameters) {
        var methodInfo = AccessTools.Method(obj.GetType(), name);
        methodInfo.Invoke(obj, parameters);
    }

    public static void SetField(this object obj, string name, object value) {
        var fieldInfo = AccessTools.Field(obj.GetType(), name);
        fieldInfo.SetValue(obj, value);
    }

    public static T GetField<T>(this object obj, string name) {
        var fieldInfo = AccessTools.Field(obj.GetType(), name);
        return (T)fieldInfo.GetValue(obj);
    }
}