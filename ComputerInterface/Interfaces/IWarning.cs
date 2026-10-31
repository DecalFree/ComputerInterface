namespace ComputerInterface.Interfaces;

internal interface IWarning {
    string WarningMessage { get; }

    bool CanBeIgnored { get; }
}