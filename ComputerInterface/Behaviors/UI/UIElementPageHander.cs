using System;
using ComputerInterface.Enumerations;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Behaviors.UI;

public class UIElementPageHandler<T> : UIPageHandler {
    private T[] _elements;

    public UIElementPageHandler(EKeyboardButton previousButton, EKeyboardButton nextButton) : base(previousButton, nextButton) {
    }

    public UIElementPageHandler() {
    }

    /// <summary>
    /// Sets the elements for the pages
    /// </summary>
    /// <param name="elements"></param>
    public void SetElements(T[] elements) {
        _elements = elements;
        MaxPage = Mathf.CeilToInt((float)elements.Length / EntriesPerPage) - 1;
        CurrentPage = 0;
        ItemsOnScreen = Math.Min(EntriesPerPage, _elements.Length);
    }

    /// <summary>
    /// iterates through the elements of the given page
    /// and returns them with the callback
    /// </summary>
    /// <param name="page"></param>
    /// <param name="elementCallback">Callback with (Element T, Index i)</param>
    public void EnumerateElements(int page, Action<T, int> elementCallback) {
        if (elementCallback == null)
            return;

        T[] elements = GetElementsForPage(page);
        for (int i = 0; i < elements.Length; i++)
            elementCallback(elements[i], i);
    }

    /// <summary>
    /// iterates through the elements of the current page
    /// and returns them with the callback
    /// </summary>
    /// <param name="elementCallback">Callback with (Element T, Index i)</param>
    public void EnumerateElements(Action<T, int> elementCallback) {
        if (elementCallback == null)
            return;

        T[] elements = GetElementsForPage(CurrentPage);
        for (int i = 0; i < elements.Length; i++)
            elementCallback(elements[i], i);
    }

    /// <summary>
    /// Gets the elements for the given page
    /// </summary>
    /// <param name="page"></param>
    /// <returns></returns>
    public T[] GetElementsForPage(int page) {
        if (_elements == null) {
            Logging.Error("Elements are not set yet\nPlease set the lines first");
            return null;
        }

        int startIndex = EntriesPerPage * page;
        ItemsOnScreen = Math.Min(EntriesPerPage, _elements.Length - startIndex);
        T[] pageElements = new T[ItemsOnScreen];
        for (int i = 0; i < ItemsOnScreen; i++)
            pageElements[i] = _elements[startIndex + i];

        return pageElements;
    }
}