using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] UserSettings userSettings;

    [Space]

    [SerializeField] Image currentBackground;
    [SerializeField] List<Sprite> backgroundImages;

    void Awake()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex == 0)
        {
            FindObjectOfType<OptionsMenu>().BackgroundChangeAction += OnChangeBackgroundImage;
        }

        OnChangeBackgroundImage();
    }

    void OnChangeBackgroundImage()
    {
        currentBackground.sprite = backgroundImages[(int)userSettings.backgroundImage];
    }
}