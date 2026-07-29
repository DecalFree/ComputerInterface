using System.Text;
using ComputerInterface.Enumerations;
using UnityEngine;

namespace ComputerInterface.Behaviors.UI;

public class UIPageHandler {
    public int CurrentPage { get; set; }

    /// <summary>
    /// Last Page (0 indexed)
    /// </summary>
    public int MaxPage { get; protected set; }

    /// <summary>
    /// How many lines are allowed per page
    /// </summary>
    public int EntriesPerPage { get; set; }

    /// <summary>
    /// How many elements are on the current page
    /// </summary>
    public int ItemsOnScreen { get; protected set; }

    /// <summary>
    /// 0 = left mark (<!--<-->)
    /// 1 = right mark (>)
    /// 2 = current page
    /// 3 = max page
    /// </summary>
    public string Footer = "{0} {2}/{3} {1}";

    public string PrevMark = "<";
    public string NextMark = ">";

    private readonly bool _useButtons;
    private readonly EKeyboardButton _previousButton;
    private readonly EKeyboardButton _nextButton;


    public UIPageHandler(EKeyboardButton previousButton, EKeyboardButton nextButton) {
        _previousButton = previousButton;
        _nextButton = nextButton;
        _useButtons = true;
    }

    public UIPageHandler() {
    }

    public bool HandleButtonPress(EKeyboardButton keyboardButton) {
        if (!_useButtons)
            return false;

        if (keyboardButton == _previousButton) {
            PreviousPage();
            return true;
        }

        if (keyboardButton == _nextButton) {
            NextPage();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Goes to the next page
    /// </summary>
    /// <returns></returns>
    public void NextPage() {
        if (CurrentPage < MaxPage)
            CurrentPage++;
    }

    /// <summary>
    /// Goes to the previous page
    /// </summary>
    /// <returns></returns>
    public void PreviousPage() {
        if (CurrentPage > 0)
            CurrentPage--;
    }

    /// <summary>
    /// Advances the page to the specidied line
    /// </summary>
    /// <param name="index"></param>
    /// <returns>line number relative to the page</returns>
    public int MovePageToIndex(int index) {
        int page = Mathf.FloorToInt((float)index / EntriesPerPage);
        CurrentPage = page;
        return index % EntriesPerPage;
    }

    /// <summary>
    /// Given the index of an item relative to the page
    /// returns the absolute index
    /// </summary>
    /// <param name="page"></param>
    /// <param name="itemIndex"></param>
    /// <returns></returns>
    public int GetAbsoluteIndex(int page, int itemIndex) => page * EntriesPerPage + itemIndex;

    /// <summary>
    /// Given the index of an item relative to the page
    /// returns the absolute index
    /// </summary>
    /// <param name="itemIndex"></param>
    /// <returns></returns>
    public int GetAbsoluteIndex(int itemIndex) => GetAbsoluteIndex(CurrentPage, itemIndex);

    public void AppendFooter(StringBuilder stringBuilder) {
        for (int i = 0; i < EntriesPerPage - ItemsOnScreen; i++)
            stringBuilder.AppendLine();

        stringBuilder.Append(GetFooter());
    }

    private string GetFooter() => string.Format(Footer, CurrentPage > 0 ? PrevMark : " ", CurrentPage < MaxPage ? NextMark : " ", CurrentPage + 1, MaxPage + 1);
}