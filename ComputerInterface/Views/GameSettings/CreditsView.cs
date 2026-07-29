using System.Linq;
using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class CreditsView : ComputerView {
    private int _currentPage;

    private int MaxPage => (int)GameInterfaceService.TotalCreditsPages.GetValue(GameInterfaceService.CreditsView);

    public override void OnViewShown(object[] arguments) => GameInterfaceService.CreditsView.pageSize = ScreenHeight - 2;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.Append(GetPage(_currentPage)).Append($"<color=#ffffff50><align=\"center\"><  {_currentPage + 1}/{MaxPage}  ></align></color>");

        return stringBuilder.ToString();
    }

    private string GetPage(int page) {
        string text = GameInterfaceService.CreditsGetPage.Invoke(GameInterfaceService.CreditsView, [ page ]) as string;
        string[] lines = text?.Split('\n');
        return string.Join("\n", lines!.Take(lines!.Length - 2));
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Left:
                _currentPage--;
                if (_currentPage == -1)
                    _currentPage = MaxPage - 1; // C# modulus is wrong: -1 % 5 = -1 -Graic

                UpdateViewScreen();
                break;
            case EKeyboardButton.Right:
                _currentPage++;
                _currentPage %= MaxPage;
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}