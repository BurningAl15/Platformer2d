using System.Collections;
using UnityEngine;

/// <summary>
/// Asks the store for a review with the official native flows. Stores never
/// report whether the player rated or closed the card and they cap how often it
/// appears, so the game simply asks again after every cleared level from
/// Level 2 on; the store decides whether anything shows. Never asked in the
/// tutorial or after a Game Over, and only on mobile builds.
/// </summary>
public static class StoreReview
{
    private const int FirstLevel = 2;
    private static bool pending;

    /// <summary>Marks a review request for the next calm moment (back on the map).</summary>
    public static void NotifyLevelCleared(int levelNumber)
    {
        if (levelNumber >= FirstLevel && PlatformFeatures.IsMobile) pending = true;
    }

    /// <summary>Called by the level selector once its arrival animations are over.</summary>
    public static void RequestIfPending(MonoBehaviour host, float delaySeconds)
    {
        if (!pending || host == null) return;
        pending = false;
        host.StartCoroutine(RequestAfter(delaySeconds));
    }

    private static IEnumerator RequestAfter(float delaySeconds)
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
#if UNITY_IOS && !UNITY_EDITOR
        UnityEngine.iOS.Device.RequestStoreReview();
#elif UNITY_ANDROID && !UNITY_EDITOR
        RequestOnAndroid();
#else
        Debug.Log("[StoreReview] Review requested (only shown by the stores on device).");
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    // Google Play In-App Review (com.google.android.play:review, added in mainTemplate.gradle).
    private static void RequestOnAndroid()
    {
        try
        {
            var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            var manager = new AndroidJavaClass("com.google.android.play.core.review.ReviewManagerFactory")
                .CallStatic<AndroidJavaObject>("create", activity);
            var request = manager.Call<AndroidJavaObject>("requestReviewFlow");
            request.Call<AndroidJavaObject>("addOnCompleteListener", new TaskListener(task =>
            {
                if (!task.Call<bool>("isSuccessful")) return;
                var info = task.Call<AndroidJavaObject>("getResult");
                manager.Call<AndroidJavaObject>("launchReviewFlow", activity, info);
            }));
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning("[StoreReview] Review flow unavailable: " + exception.Message);
        }
    }

    private sealed class TaskListener : AndroidJavaProxy
    {
        private readonly System.Action<AndroidJavaObject> callback;

        public TaskListener(System.Action<AndroidJavaObject> callback)
            : base("com.google.android.gms.tasks.OnCompleteListener") => this.callback = callback;

        // Invoked by the Java task on completion (the name must match the Java interface).
        public void onComplete(AndroidJavaObject task) => callback?.Invoke(task);
    }
#endif
}
