using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener, IUnityAdsInitializationListener
{
    private static AdManager _instance;
    public bool _testMode = false;
    private string _gameId;
    private string _adUnitId;

    public static AdManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var adManagerObj = new GameObject("AdManager");
                _instance = adManagerObj.AddComponent<AdManager>();
                DontDestroyOnLoad(adManagerObj);
            }
            return _instance;
        }
    }

    public void Init()
    {
        #if UNITY_ANDROID
            _gameId = "5462539";
            _adUnitId = "Rewarded_Android";
        #elif UNITY_IOS
            _gameId = "5462538";
            _adUnitId = "Rewarded_iOS";
        #else
            _gameId = "5462539";
            _adUnitId = "Rewarded_Android";
        #endif

        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void LoadAd()
    {
        Advertisement.Load(_adUnitId, this);
    }

    public static void ShowAd()
    {
        if (Advertisement.isInitialized)
        {
            Advertisement.Show(Instance._adUnitId, Instance);
        }
        else
        {
            Debug.Log("Ad not ready");
        }
    }

    // IUnityAdsLoadListener implementation
    public void OnUnityAdsAdLoaded(string placementId)
    {
        // Optional actions to take when the ad is loaded
        //print("Ad loaded");
        //ShowAd();
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"Error loading Ad Unit: {placementId} - {error.ToString()} - {message}");
    }

    // IUnityAdsShowListener implementation
    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            // Optionally reward the user for watching the ad to completion.
            //print("Completed ad");
        }
        // Reload the ad
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"Error showing Ad Unit {placementId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string placementId) { }
    public void OnUnityAdsShowClick(string placementId) { }

    public void OnInitializationComplete()
    {
        //print("Ads inited");
        LoadAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        print("Ads failed init " + message);
    }
}

