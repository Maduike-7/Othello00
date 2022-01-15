using UnityEngine;

public class MainMenu : Menu
{
    [SerializeField] GameObject menuDisc;

    protected override void Awake()
    {
        base.Awake();
        backgroundTransition = FindObjectOfType<BackgroundTransition>();
    }

    protected override void Update()
    {
        base.Update();
        menuDisc.transform.Rotate(Vector3.up);
    }

    public override void Open()
    {
        base.Open();
        menuDisc.SetActive(true);
    }

    public override void Close()
    {
        base.Close();
        menuDisc.SetActive(false);
    }

#if UNITY_ANDROID
    public override void HandleBackButtonInput()
    {
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        androidJavaObject.Call<bool>("moveTaskToBack", true);
    }
#endif
}