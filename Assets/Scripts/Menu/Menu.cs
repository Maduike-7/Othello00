using UnityEngine;
using UnityEngine.UI;

public abstract class Menu : MonoBehaviour
{
    protected Canvas thisMenu;

    protected virtual void Awake()
    {
        thisMenu = GetComponent<Canvas>();
    }

    public virtual void Open()
    {
        thisMenu.enabled = true;

        if (thisMenu.TryGetComponent(out Menu m))
            m.SetEnabled(true);
    }

    public virtual void Close()
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

#if UNITY_ANDROID
    protected virtual void Update()
    {
        if (Input.GetButtonUp("Cancel"))
        {
            HandleBackButtonInput();
        }
    }

    public virtual void HandleBackButtonInput()
    {
        GetComponentInChildren<Button>().onClick.Invoke();
    }
#endif
}