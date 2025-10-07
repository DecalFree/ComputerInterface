using System;

namespace ComputerInterface.Models;

public class ComputerViewSwitchEventArgs(Type sourceType, Type destinationType, object[] args) {
    public readonly Type SourceType = sourceType;
    public readonly Type DestinationType = destinationType;
    public readonly object[] Args = args;

    public delegate void ComputerViewSwitchEventHandler(ComputerViewSwitchEventArgs args);
}