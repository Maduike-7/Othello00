using UnityEngine;

public class InstructionsMenu : Menu
{
    [SerializeField] RectTransform[] instructionPages;
    [SerializeField] GameObject previousPageButton, nextPageButton;

    int currentPage;

    int CurrentPage
    {
        get => currentPage;
        set
        {
            currentPage = value;
            ChangePageTo(currentPage);
        }
    }

    public override void Open()
    {
        base.Open();
        CurrentPage = 0;
    }

    public void OnChangePage(int direction)
    {
        CurrentPage = Mathf.Clamp(CurrentPage + direction, 0, instructionPages.Length - 1);
    }

    void ChangePageTo(int pageNumber)
    {
        for (int i = 0; i < instructionPages.Length; i++)
        {
            instructionPages[i].gameObject.SetActive(i == pageNumber);
        }

        previousPageButton.SetActive(pageNumber != 0);
        nextPageButton.SetActive(pageNumber != instructionPages.Length - 1);
    }
}