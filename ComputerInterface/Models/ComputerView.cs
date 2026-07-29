using System;
using ComputerInterface.Behaviors;
using ComputerInterface.Enumerations;
using ComputerInterface.Views;

namespace ComputerInterface.Models;

public abstract class ComputerView {
    public static event Action<string> OnTextUpdated;

    /// <summary>
    /// Amount of characters that fit in the x-axis of the screen.
    /// </summary>
    public static int ScreenWidth = 52;

    /// <summary>
    /// Amount of characters that fit in the y-axis of the screen.
    /// </summary>
    public static int ScreenHeight = 12;

    public string PrimaryColor = "ed6540";

    /// <summary>
    /// The text that is shown on the screen.
    /// </summary>
    public string Text { get; private set; }

    public Type CallerComputerView { get; set; }

    /// <summary>
    /// Gets called when a ComputerView is shown.
    /// </summary>
    public virtual void OnViewShown(object[] arguments) {
    }

    /// <summary>
    /// Tells the computer what text should appear on the screen.
    /// </summary>
    protected abstract string GetViewText();

    /// <summary>
    /// Gets called when a button is pressed on the keyboard.
    /// </summary>
    /// <param name="pressedButton">The pressed button on the keyboard.</param>
    public virtual void OnButtonPressed(EKeyboardButton pressedButton) {
    }

    /// <summary>
    /// Switch to another ComputerView.
    /// </summary>
    public void ShowView<T>(params object[] arguments) => ShowView(typeof(T), arguments);

    /// <summary>
    /// Switch to another ComputerView.
    /// </summary>
    public void ShowView(Type computerView, params object[] arguments) => Main.Singleton.SwitchComputerView(GetType(), computerView, arguments);

    /// <summary>
    /// Return to the previous ComputerView.
    /// </summary>
    public void ReturnToPreviousView() => ShowView(CallerComputerView);

    /// <summary>
    /// Shows the MainMenu ComputerView.
    /// </summary>
    public void ReturnToMainMenu() => ShowView<MainMenuView>();

    /// <summary>
    /// Update text on the computer's screen.
    /// </summary>
    public void UpdateViewScreen() {
        Text = GetViewText();
        OnTextUpdated?.Invoke(Text);
    }
}