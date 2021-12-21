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
        FindObjectOfType<OptionsMenu>().BackgroundChangeAction += OnChangeBackgroundImage;
    }

    void OnChangeBackgroundImage()
    {
        currentBackground.sprite = backgroundImages[(int)userSettings.backgroundImage];
    }
}