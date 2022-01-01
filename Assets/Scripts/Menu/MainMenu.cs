using UnityEngine;

public class MainMenu : Menu
{
#if UNITY_ANDROID
    public override void HandleBackButtonInput()
    {
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        androidJavaObject.Call<bool>("moveTaskToBack", true);
    }
#endif
}