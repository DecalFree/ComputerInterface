using ComputerInterface.Interfaces;
using ComputerInterface.Views;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using UnityEngine;

namespace ComputerInterface.Models;

public class ComputerView : IComputerView {
    /// <summary>
    /// How many characters fit in the x-axis of the screen
    /// </summary>
    public static int ScreenWidth = 52;

    /// <summary>
    /// How many characters fit in the y-axis of the screen
    /// </summary>
    public static int ScreenHeight = 12;

    public string PrimaryColor = "ed6540";

    /// <summary>
    /// Text that is shown on screen
    /// assigning to it automatically updates the text
    /// </summary>
    public string Text {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    protected string _text;

    public Type CallerViewType { get; set; }

    /// <summary>
    /// Set text from a <see cref="StringBuilder"/>
    /// </summary>
    /// <param name="stringBuilder"></param>
    public virtual void SetText(StringBuilder stringBuilder) =>
        Text = stringBuilder.ToString();

    /// <summary>
    /// Set text from a <see cref="StringBuilder"/> the the callback is providing
    /// </summary>
    /// <param name="builderCallback"></param>
    public virtual void SetText(Action<StringBuilder> builderCallback) {
        StringBuilder stringBuilder = new();
        builderCallback(stringBuilder);
        SetText(stringBuilder);
    }

    /// <summary>
    /// Gets called when a key is pressed on the keyboard
    /// </summary>
    /// <param name="key"></param>
    public virtual void OnKeyPressed(EKeyboardKey key) {
    }

    /// <summary>
    /// Gets called when the roomView is shown
    /// call the base OnShow when overriding
    /// to display the current text on the computer
    /// </summary>
    public virtual void OnShow(object[] args) =>
        RaisePropertyChanged(nameof(Text));

    /// <summary>
    /// Switch to another roomView
    /// </summary>
    public void ShowView<T>(params object[] args) =>
        ShowView(typeof(T), args);

    /// <summary>
    /// Switch to another roomView
    /// </summary>
    public void ShowView(Type type, params object[] args) =>
        OnViewSwitchRequest?.Invoke(new ComputerViewSwitchEventArgs(GetType(), type, args));

    /// <summary>
    /// Return to previous roomView
    /// </summary>
    public void ReturnView() =>
        ShowView(CallerViewType);

    /// <summary>
    /// Shows the main menu roomView
    /// </summary>
    public void ReturnToMainMenu() =>
        ShowView<MainMenuView>();

    public void SetBackground(Texture texture, Color? color = null) {
        ComputerViewChangeBackgroundEventArgs args = new(texture, color);
        OnChangeBackgroundRequest?.Invoke(args);
    }

    public void RevertBackground() =>
        OnChangeBackgroundRequest?.Invoke(null);

    public async Task ShowSplashForDuration(Texture texture, int milliseconds) {
        var text = Text;
        Text = "";
        SetBackground(texture);
        await Task.Delay(milliseconds);
        RevertBackground();
        Text = text;
    }

    public event ComputerViewSwitchEventArgs.ComputerViewSwitchEventHandler OnViewSwitchRequest;
    public event ComputerViewChangeBackgroundEventArgs.ComputerViewChangeBackgroundEventHandler OnChangeBackgroundRequest;

    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        storage = value;
        RaisePropertyChanged(propertyName);
        return true;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}