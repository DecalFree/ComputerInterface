using System;
using System.ComponentModel;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;

namespace ComputerInterface.Interfaces;

public interface IComputerView : INotifyPropertyChanged {
    string Text { get; set; }

    Type CallerViewType { get; set; }

    void OnKeyPressed(EKeyboardKey key);

    void OnShow(object[] args);

    event ComputerViewSwitchEventArgs.ComputerViewSwitchEventHandler OnViewSwitchRequest;

    event ComputerViewChangeBackgroundEventArgs.ComputerViewChangeBackgroundEventHandler OnChangeBackgroundRequest;
}