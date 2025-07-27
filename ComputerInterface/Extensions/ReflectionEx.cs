using HarmonyLib;
using System.Reflection;

namespace ComputerInterface.Extensions
{
    internal static class ReflectionEx
    {
        public static void InvokeMethod(this object obj, string name, params object[] parameters)
        {
            MethodInfo method = AccessTools.Method(obj.GetType(), name);
            method.Invoke(obj, parameters);
        }

        public static void SetField(this object obj, string name, object value)
        {
            FieldInfo field = AccessTools.Field(obj.GetType(), name);
            field.SetValue(obj, value);
        }

        public static T GetField<T>(this object obj, string name)
        {
            FieldInfo field = AccessTools.Field(obj.GetType(), name);
            return (T)field.GetValue(obj);
        }
    }
}