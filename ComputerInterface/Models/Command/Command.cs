using System;

namespace ComputerInterface.Models.Command;

public class Command(string name, Type[] argumentTypes, Func<object[], string> callback) {
    public readonly string Name = name;
    public readonly Type[] ArgumentTypes = argumentTypes;
    public readonly Func<object[], string> Callback = callback;

    public int ArgumentCount => ArgumentTypes?.Length ?? 0;
}