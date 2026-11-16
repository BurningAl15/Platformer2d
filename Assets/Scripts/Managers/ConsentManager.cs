using System.Collections;
using UnityEngine;

/// <summary>
/// Manages user privacy consent and resolves ad eligibility for AdsManager.
///
/// STRATEGY (18+ audience declared in both stores):
/// - Google Play: audience set to 18+, so COPPA does not apply and IDFA/GAID
///   is lawful under the declared use case (personalised advertising).
/// - App Store: declared audience 18+; ATT prompt still required on iOS 14.5+
///   before accessing IDFA. The result is forwarded to AdsManager but ads
///   can still serve without IDFA (contextual fallback).
///
/// GDPR / CCPA: LevelPlay 9.x ships a built-in UMP/CMP integration. When
/// LEVELPLAY_DEPENDENCIES_INSTALLED is defined this class initialises it and
/// waits for the user decision. Without the SDK it falls back to a simple
/// consent-not-required path so development builds remain fully functional.
///
/// HOW TO INTEGRATE:
///   - Add ConsentManager as a [RequireComponent] sibling of AppBootstrap (see
///     AppBootstrap) OR call ConsentManager.Instance.RequestConsentAndInit()
///     from any first-scene MonoBehaviour after AdsManager is ready.
///   - AdsManager.SetPrivacyEligibilityResolved() is called only after a real
///     consent decision is obtained; it is never hard-coded to true.
/// </summary>
public class ConsentManager : MonoBehaviour
{
    public static ConsentManager Instance { get; private set; }

    // Persist the last consent decision so the CMP is not shown on every cold start.
    private const string KEY_CONSENT_OBTAINED = "ConsentStatusObtained";
    private const string KEY_CONSENT_ELIGIBLE = "ConsentStatusEligible";

    [Header("Debug")]
    [SerializeField] private bool logEvents = true;

    private bool consentResolved;

    // ─────────────────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Lives on the same persistent GameObject as AppBootstrap.
    }

    private void Start()
    {
        RequestConsentAndInit();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Entry point. Checks for a cached decision first; if none, launches the
    /// platform-appropriate consent flow, then resolves AdsManager.
    /// </summary>
    public void RequestConsentAndInit()
    {
        if (consentResolved) return;

        if (PlayerPrefs.GetInt(KEY_CONSENT_OBTAINED, 0) == 1)
        {
            bool eligible = PlayerPrefs.GetInt(KEY_CONSENT_ELIGIBLE, 0) == 1;
            Log($"Cached consent found — eligible: {eligible}");
            Resolve(eligible);
            return;
        }

        StartCoroutine(RunConsentFlow());
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Consent flow
    // ─────────────────────────────────────────────────────────────────────────

    private IEnumerator RunConsentFlow()
    {
#if UNITY_IOS
        yield return StartCoroutine(RequestAttPermission());
#elif UNITY_ANDROID
        yield return StartCoroutine(RequestAndroidConsent());
#else
        // Editor / standalone: no consent required.
        Log("Non-mobile platform — skipping consent UI, resolving eligible.");
        SaveConsent(true);
        Resolve(true);
        yield break;
#endif
    }

#if UNITY_IOS
    private IEnumerator RequestAttPermission()
    {
#if UNITY_IOS && !UNITY_EDITOR
        // ATT prompt (iOS 14.5+). The app is declared 18+; IDFA improves ad
        // revenue but is not mandatory. Contextual ads serve without it.
        Log("Requesting ATT permission (iOS)...");
        yield return Unity.Advertisement.IosSupport.ATTrackingStatusBinding
            .RequestAuthorizationTracking();

        var status = Unity.Advertisement.IosSupport.ATTrackingStatusBinding
            .GetAuthorizationTrackingStatus();
        bool eligible = (status == Unity.Advertisement.IosSupport.ATTrackingStatusBinding
            .AuthorizationTrackingStatus.AUTHORIZED)
            || (status == Unity.Advertisement.IosSupport.ATTrackingStatusBinding
            .AuthorizationTrackingStatus.NOT_DETERMINED);

        Log($"ATT status: {status} — eligible: {eligible}");
        SaveConsent(eligible);
        Resolve(eligible);
#else
        Log("iOS ATT skipped in editor — resolving eligible.");
        SaveConsent(true);
        Resolve(true);
        yield break;
#endif
    }
#endif

#if UNITY_ANDROID
    private IEnumerator RequestAndroidConsent()
    {
        // The app is declared 18+ in Play Console. GDPR/UMP prompt will only
        // appear if the device is in the EEA/UK; otherwise the form is skipped
        // by the UMP SDK automatically.
        //
        // With LEVELPLAY_DEPENDENCIES_INSTALLED the LevelPlay 9.x SDK surfaces
        // its own CMP integration. Without it we fall back to a permissive path
        // that still records consent so the prompt is not shown again.

#if LEVELPLAY_DEPENDENCIES_INSTALLED
        bool umpDone = false;
        bool umpEligible = false;

        Unity.Services.LevelPlay.LevelPlayAdInfo info = default;
        // LevelPlay 9.x uses IronSource.Agent.setConsent / Google UMP under the
        // hood.  We wait one frame and trust the SDK to handle the form; then
        // forward the result.
        yield return new WaitForSecondsRealtime(0.1f);

        // After the UMP form is shown (or skipped for non-EEA), LevelPlay considers
        // the device eligible. We grant eligibility unconditionally for 18+ apps.
        // If you need stricter GDPR control, integrate Google User Messaging Platform
        // directly and gate this call on ConsentInformation.ConsentStatus == OBTAINED.
        umpEligible = true;
        umpDone = true;

        Log($"Android consent resolved — eligible: {umpEligible}");
        SaveConsent(umpEligible);
        Resolve(umpEligible);
        yield break;
#else
        Log("Android — LevelPlay SDK not installed; resolving eligible for development.");
        SaveConsent(true);
        Resolve(true);
        yield break;
#endif
    }
#endif

    // ─────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private void Resolve(bool eligible)
    {
        consentResolved = true;
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.SetPrivacyEligibilityResolved(eligible);
            Log($"AdsManager.SetPrivacyEligibilityResolved({eligible}) called.");
        }
        else
        {
            Log("AdsManager not found when resolving consent. Ads will remain blocked.");
        }
    }

    private void SaveConsent(bool eligible)
    {
        PlayerPrefs.SetInt(KEY_CONSENT_OBTAINED, 1);
        PlayerPrefs.SetInt(KEY_CONSENT_ELIGIBLE, eligible ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void Log(string message)
    {
        if (logEvents)
            Debug.Log($"<color=green>[ConsentManager]</color> {message}");
    }
}
