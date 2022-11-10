using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RollicGames.Advertisements;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityExtensions;
public class AdsM : MonoSingleton<AdsM>
{
    
    [Serializable]
    public class AdButtonsClass
    {
        public Button buttonObject;
        public GameObject rayImage;
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
                AcceptReward();
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

    void AcceptReward()
    {
        Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
        currentRewarded();
        currentRewarded = null;
        StopCoroutine("InterRoutine");
        StartCoroutine("InterRoutine");
    }
    
    public void Add3CarButton()
    {
        Analytics.Instance.RewardedTapped("Add3Car");
        ProcessAds(Add3Car);
    }
    void Add3Car()
    {
        PM.Instance.StartCoroutine("Add3CarRoutine");
    }

    public void IncreaseSizeButton()
    {        
        Analytics.Instance.RewardedTapped("IncreaseSize");
        ProcessAds(IncreaseSize);
    }
    void IncreaseSize()
    {
        PM.Instance.IncreaseSize();
    }
    public void IncreaseGatesButton()
    {        
        Analytics.Instance.RewardedTapped("IncreaseGates");
        ProcessAds(IncreaseGates);
    }
    void IncreaseGates()
    {
        PM.Instance.IncreaseGates();
    }
    public void MergeButton()
    {        
        Analytics.Instance.RewardedTapped("Merge");
        ProcessAds(Merge);
    }
    void Merge()
    {
        PM.Instance.Merge();
        UIM.Instance.merge.state = "CanBuy";
    }
    
    public void AddLastCarButton()
    {      
        adDetails[5].buttonObject.transform.SizeUpAnimation();  
        Analytics.Instance.RewardedTapped("AddLastCar");
        ProcessAds(AddLastCar);
    }
    void AddLastCar()
    {
        PM.Instance.AddLastCar();
    }
    
    
    public void SpeedUpButton()
    {     adDetails[2].buttonObject.transform.SizeUpAnimation();     
        Analytics.Instance.RewardedTapped("SpeedUp");
        ProcessAds(SpeedUp);
    }
    void SpeedUp()
    {
        PM.Instance.StartCoroutine("SpeedUpRoutine");
    }
    
    
    public void AddIncomeButton()
    {      adDetails[3].buttonObject.transform.SizeUpAnimation();    
        Analytics.Instance.RewardedTapped("AddIncome");
        ProcessAds(AddIncome);
    }
    void AddIncome()
    {
        PM.Instance.StartCoroutine("AddIncomeRoutine");
    }

    
    public void AutoTapButton()
    {      adDetails[1].buttonObject.transform.SizeUpAnimation();    
        Analytics.Instance.RewardedTapped("AutoTapButton");
        ProcessAds(AutoTap);
    }
    
    void AutoTap()
    {
        PM.Instance.StartCoroutine("AutoTapRoutine");
    }
    
    
    public void EvolveCarsButton()
    {      adDetails[4].buttonObject.transform.SizeUpAnimation();    
        Analytics.Instance.RewardedTapped("EvolveCars");
        ProcessAds(EvolveCars);
    }
    void EvolveCars()
    {
        PM.Instance.StartCoroutine("EvolveCarsRoutine");
    }
    
    
    public void FeverCarButton()
    {adDetails[0].buttonObject.transform.SizeUpAnimation();  
        Analytics.Instance.RewardedTapped("FeverCar");
        ProcessAds(FeverCar);
    } 
    void FeverCar()
    {
        PM.Instance.StartCoroutine("FeverCarRoutine");
    }

    public void ProcessAds(Action action)
    {
        if (Application.isEditor)
        {
            if (adReady)
            {
                currentRewarded = action;
                AcceptReward();
            }
            else
                NoAds();
        }
        else if (!RewardedCanBeShown())
            NoAds();
        else
        {
            currentRewarded = action;
            Analytics.Instance.RewardedImpression(currentRewarded.Method.Name);
            RLAdvertisementManager.Instance.showRewardedVideo();
        }  
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