using ComputerInterface.Interfaces;
using System;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Tools;

namespace ComputerInterface.Behaviours;

internal class ComputerViewController {
    public event Action<string> OnTextChanged;
    public event Action<IMonitor> OnMonitorChanged;
    public event Action<ComputerViewSwitchEventArgs> OnSwitchView;
    public event Action<ComputerViewChangeBackgroundEventArgs> OnSetBackground;

    public IComputerView CurrentComputerView { get; private set; }

    private readonly PropUpdateBinder _propUpdateBinder;

    public ComputerViewController() {
        _propUpdateBinder = new PropUpdateBinder();
        _propUpdateBinder.Bind("Text", RaiseOnTextChanged);
    }

    public void SetMonitor(IMonitor monitor) {
        OnMonitorChanged?.Invoke(monitor);
        SetView(CurrentComputerView, null);
    }

    public void SetView(IComputerView computerView, object[] args) {
        UnregisterView(CurrentComputerView);
        RegisterView(computerView);

        CurrentComputerView = computerView;
        try {
            CurrentComputerView.OnShow(args);
        }
        catch (Exception exception) {
            Logging.Error($"Error while showing roomView {computerView.GetType().Name}: {exception}");
        }
    }

    public void NotifyOfKeyPress(EKeyboardKey key) {
        try {
            CurrentComputerView?.OnKeyPressed(key);
        }
        catch (Exception exception) {
            Logging.Error($"Error in OnKeyPressed for key {key} in roomView {CurrentComputerView?.GetType().Name}: {exception}");
        }
    }

    private void RegisterView(IComputerView view) {
        if (view == null)
            return;

        view.PropertyChanged += _propUpdateBinder.PropertyChanged;
        view.OnViewSwitchRequest += OnSwitchViewRequest;
        view.OnChangeBackgroundRequest += OnChangeBackgroundRequest;
    }

    private void UnregisterView(IComputerView view) {
        if (view == null)
            return;

        view.PropertyChanged -= _propUpdateBinder.PropertyChanged;
        view.OnViewSwitchRequest -= OnSwitchViewRequest;
        view.OnChangeBackgroundRequest -= OnChangeBackgroundRequest;

        OnChangeBackgroundRequest(null);
    }

    private void OnSwitchViewRequest(ComputerViewSwitchEventArgs args) =>
        OnSwitchView?.Invoke(args);

    private void OnChangeBackgroundRequest(ComputerViewChangeBackgroundEventArgs args) =>
        OnSetBackground?.Invoke(args);

    private void RaiseOnTextChanged() {
        if (CurrentComputerView == null)
            return;

        OnTextChanged?.Invoke(CurrentComputerView.Text);
    }
}