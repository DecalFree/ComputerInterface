using System;

namespace ComputerInterface.Interfaces;

public interface IComputerViewEntry {
    string EntryName { get; }

    Type EntryComputerView { get; }
}