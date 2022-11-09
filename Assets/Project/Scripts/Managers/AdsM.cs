using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RollicGames.Advertisements;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExtensions;
public class AdsM : MonoSingleton<AdsM>
{
    
    [Serializable]
    public class AdButtonsClass
    {
        public GameObject buttonObject;
        public GameObject adImage;
        public TextMeshProUGUI text;
        public TextMeshProUGUI timer;
        public float timerValue;
        public float multiplierValue;
    }
    public AdButtonsClass[] adDetails;
    public TMP_Text noAdsText;
    
    
#if UNITY_IOS
    private string _appKey = "1728094ad";
#elif UNITY_ANDROID || UNITY_EDITOR
    private string _appKey = "172814145";
#endif

    public bool adReady = false;
    public Action currentRewarded;
    
    void Awake() 
    {
        RLAdvertisementManager.Instance.init(_appKey);
        RLAdvertisementManager.Instance.rewardedAdResultCallback = RewardedAdResultCallback;
        RLAdvertisementManager.OnRollicAdsSdkInitializedEvent += OnSdkInit;
        RLAdvertisementManager.OnRollicAdsAdFailedEvent += OnBannerFailed;
    }
    
    void OnSdkInit() 
    {
        RLAdvertisementManager.Instance.loadBanner();
        StartCoroutine("InterRoutine");
    }

    IEnumerator InterRoutine()
    {
        if (!PlayerPrefs.HasKey("FirstInter"))
        {
            yield return new WaitForSecondsRealtime(120);
            PlayerPrefs.SetInt("FirstInter", 1);
        }
        else
            yield return new WaitForSecondsRealtime(90);

        while (true)
        {
            while (!RLAdvertisementManager.Instance.isInterstitialReady())
                yield return null;
            Analytics.Instance.InterstitialShown();
            RLAdvertisementManager.Instance.showInterstitial();
            yield return new WaitForSecondsRealtime(90);
        }
    }
    
    void OnBannerFailed(string errorMessage) {
        // You can log the errorMessage and identify why banner is not showing
    }
    
    bool RewardedCanBeShown()
    {
        if (currentRewarded != null)
            return false;
        if (Application.isEditor)
            return adReady;
        if(RLAdvertisementManager.Instance.isRewardedVideoAvailable())
            return true;
        return false;
    }
    
    void RewardedAdResultCallback(RLRewardedAdResult result)
    {
        switch (result)
        {
            case RLRewardedAdResult.Finished:
            {
                Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
                currentRewarded();
                currentRewarded = null;
                StopCoroutine("InterRoutine");
                StartCoroutine("InterRoutine");
                break;
            }
            case RLRewardedAdResult.Skipped:
            {
                currentRewarded = null;
                break;
            }
            case RLRewardedAdResult.Failed:
                break;
            default:
                break;
        }
    }
    
    public void Add3CarButton()
    {        Analytics.Instance.RewardedTapped("Add3Car");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = Add3Car;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    void Add3Car()
    {
        PM.Instance.StartCoroutine("Add3CarRoutine");
    }

    
    public void AddLastCarButton()
    {        Analytics.Instance.RewardedTapped("AddLastCar");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = AddLastCar;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    void AddLastCar()
    {
        PM.Instance.AddLastCar();
    }
    
    
    public void SpeedUpButton()
    {        Analytics.Instance.RewardedTapped("SpeedUp");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = SpeedUp;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    void SpeedUp()
    {
        PM.Instance.StartCoroutine("SpeedUpRoutine");
    }
    
    
    public void AddIncomeButton()
    {        Analytics.Instance.RewardedTapped("AddIncome");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = AddIncome;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    void AddIncome()
    {
        PM.Instance.StartCoroutine("AddIncomeRoutine");
    }

    
    public void AutoTapButton()
    {        Analytics.Instance.RewardedTapped("AutoTapButton");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = AutoTapButton;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    
    void AutoTap()
    {
        PM.Instance.StartCoroutine("AutoTapRoutine");
    }
    
    
    public void EvolveCarsButton()
    {        Analytics.Instance.RewardedTapped("UpgradeAllCars");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = EvolveCars;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    
    void EvolveCars()
    {
        PM.Instance.StartCoroutine("EvolveCarsRoutine");
    }
    
    
    public void FeverCarButton()
    {
        Analytics.Instance.RewardedTapped("FeverCar");
        if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = FeverCar;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }
    }
    void FeverCar()
    {
        PM.Instance.FeverCar();
    }

    void NoAds()
    {
        DOTween.Kill(noAdsText);
        noAdsText.enabled = true;
        noAdsText.alpha = 1;
        noAdsText.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(()=>noAdsText.enabled=false);
        Debug.Log("No Ads");
    }
    
}