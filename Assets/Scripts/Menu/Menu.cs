using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class Menu : MonoBehaviour
{
    protected Canvas thisMenu;

    protected virtual void Awake()
    {
        thisMenu = GetComponent<Canvas>();
    }

    public void Open()
    {
        thisMenu.enabled = true;

        if (thisMenu.TryGetComponent(out Menu m))
            m.SetEnabled(true);
    }

    public void Close()
    {
        thisMenu.enabled = false;
        SetEnabled(false);
    }

    public virtual void SetEnabled(bool state)
    {
        if (TryGetComponent(out CanvasGroup cg))
        {
            cg.interactable = state;
            cg.blocksRaycasts = state;
        }

        enabled = state;
    }

    public void LoadScene(int sceneIndex)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}