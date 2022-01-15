using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public abstract class Menu : MonoBehaviour
{
    protected Canvas thisMenu;

    protected BackgroundTransition backgroundTransition;
    IEnumerator sceneTransitionCoroutine;

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
        if (sceneTransitionCoroutine == null)
        {
            sceneTransitionCoroutine = LoadSceneAfterDelay(sceneIndex);
            StartCoroutine(sceneTransitionCoroutine);
        }
    }

    IEnumerator LoadSceneAfterDelay(int sceneIndex)
    {
        yield return backgroundTransition.Fade(0f, 1f, 1f);
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