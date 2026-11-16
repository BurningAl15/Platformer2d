using UnityEngine;
#if FOX_GOOGLE_UMP && !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
using GoogleMobileAds.Ump.Api;
using System.Threading;
#endif

/// <summary>Refreshes ad eligibility for the current launch on supported mobile players.</summary>
public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance { get; private set; }
    [SerializeField] private bool logEvents = true;
#if FOX_GOOGLE_UMP && !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
    private bool requestPending;
    private SynchronizationContext unityContext;
#endif

    private void Awake() => Instance = this;
    private void Start() => RequestConsentAndInit();
    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RequestConsentAndInit()
    {
        if (!AdsManager.SupportsAds) return;
#if FOX_GOOGLE_UMP && !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        if (requestPending) return;
        requestPending = true;
        unityContext = SynchronizationContext.Current;
        ConsentInformation.Update(new ConsentRequestParameters(), error => unityContext.Post(_ =>
        {
            if (this == null) return;
            if (error != null) { Finish(false); return; }
            ConsentForm.LoadAndShowConsentFormIfRequired(formError => unityContext.Post(__ =>
            {
                if (this != null) Finish(formError == null && ConsentInformation.CanRequestAds());
            }, null));
        }, null));
#else
        AdsManager.Instance?.SetPrivacyEligibilityResolved(false);
        if (logEvents) Debug.Log("[Consent] Mobile ads are waiting for UMP configuration.");
#endif
    }

#if FOX_GOOGLE_UMP && !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
    private void Finish(bool eligible)
    {
        requestPending = false;
        AdsManager.Instance?.SetPrivacyEligibilityResolved(eligible);
    }
#endif
}
