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
        public string name;
        public Button buttonObject;
        public GameObject rayImage;
        public GameObject adImage;
        public TextMeshProUGUI text;
        public TextMeshProUGUI timer;
        public float timerValue;
        public float multiplierValue;
        public bool adsEnabled = true;
        public GameObject popUpPanel;
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
        if(Application.isEditor)
            StartCoroutine("InterRoutine");
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
            yield return StartCoroutine("Waiter",120);
            PlayerPrefs.SetInt("FirstInter", 1);
        }
        else
            yield return StartCoroutine("Waiter", 90);

        while (true)
        {
            while (!RLAdvertisementManager.Instance.isInterstitialReady() && !Application.isEditor)
                yield return null;
            Analytics.Instance.InterstitialShown();
            RLAdvertisementManager.Instance.showInterstitial();
            print("InterShown");
            yield return StartCoroutine("Waiter", 90);
        }
    }

    IEnumerator Waiter(float value)
    {
        while (value > 0)
        {
            value -= 0.0166f;
            yield return null;
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
            {
                Analytics.Instance.RewardedFailed(currentRewarded.Method.Name);
                currentRewarded = null;
                break;
            }
        }
    }

    void AcceptReward()
    {
        Analytics.Instance.RewardedCompleted(currentRewarded.Method.Name);
        Analytics.Instance.BoosterUsed(currentRewarded.Method.Name);
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

    public void FeverCarButton()
    {
        adDetails[0].buttonObject.transform.SizeUpAnimation("FeverCar");
        Analytics.Instance.RewardedTapped("FeverCar");
        ProcessAds(FeverCar);
    }
    void FeverCar()
    {
        PM.Instance.StartCoroutine("FeverCarRoutine");
    }

    
    public void AutoTapButton()
    {
        adDetails[1].buttonObject.transform.SizeUpAnimation("AutoTapButton");
        Analytics.Instance.RewardedTapped("AutoTapButton");
        ProcessAds(AutoTap);
    }

    void AutoTap()
    {
        PM.Instance.StartCoroutine("AutoTapRoutine");
    }
    
    
    public void SpeedUpButton()
    {     
        adDetails[2].buttonObject.transform.SizeUpAnimation("SpeedUp");     
        Analytics.Instance.RewardedTapped("SpeedUp");
        ProcessAds(SpeedUp);
    }
    
    void SpeedUp()
    {
        PM.Instance.StartCoroutine("SpeedUpRoutine");
    }
    
    
    public void AddIncomeButton()
    {      
        adDetails[3].buttonObject.transform.SizeUpAnimation("AddIncome");    
        Analytics.Instance.RewardedTapped("AddIncome");
        ProcessAds(AddIncome);
    }
    
    void AddIncome()
    {
        PM.Instance.StartCoroutine("AddIncomeRoutine");
    }

    
    public void EvolveCarsButton()
    {      
        adDetails[4].buttonObject.transform.SizeUpAnimation("EvolveCars");    
        Analytics.Instance.RewardedTapped("EvolveCars");
        ProcessAds(EvolveCars);
    }
    
    void EvolveCars()
    {
        PM.Instance.StartCoroutine("EvolveCarsRoutine");
    }

    
    public void AddLastCarButton()
    {
        adDetails[5].buttonObject.transform.SizeUpAnimation("AddLastCar");
        Analytics.Instance.RewardedTapped("AddLastCar");
        ProcessAds(AddLastCar);
    }

    void AddLastCar()
    {
        PM.Instance.AddLastCar();
    }

    void Update()
    {
        ShowSpeedUpPopUp();
        AddIncomePopUp();
        FeverCarPopUp();
        EvolveCarsPopUp();
        AutoTapPopUp();
        GetNewCarPopUp();
    }


    public void ShowSpeedUpPopUp()
    {
        if(!PlayerPrefs.HasKey("SpeedUpPopUp") && Time.realtimeSinceStartup>120)
        {
            PlayerPrefs.SetInt("SpeedUpPopUp", 1);
           OpenPopUp(2); 
        }
    }

    public void AddIncomePopUp()
    {
        if (!PlayerPrefs.HasKey("AddIncomePopUp") && Time.realtimeSinceStartup > 180)
        {
            PlayerPrefs.SetInt("AddIncomePopUp", 1);
            OpenPopUp(3);
        }
    }

    public void FeverCarPopUp()
    {
        if (!PlayerPrefs.HasKey("FeverCarPopUp") && Time.realtimeSinceStartup > 300)
        {
            PlayerPrefs.SetInt("FeverCarPopUp", 1);
            OpenPopUp(0);
        }
    }

    public void EvolveCarsPopUp()
    {
        if (!PlayerPrefs.HasKey("EvolveCarsPopUp") && Time.realtimeSinceStartup > 420)
        {
            PlayerPrefs.SetInt("EvolveCarsPopUp", 1);
            OpenPopUp(4);
        }
    }

    public void AutoTapPopUp()
    {
        if (!PlayerPrefs.HasKey("AutoTapPopUp") && Time.realtimeSinceStartup > 120)
        {
            PlayerPrefs.SetInt("AutoTapPopUp", 1);
            OpenPopUp(1);
        }
    }

    public void GetNewCarPopUp()
    {
        if (!PlayerPrefs.HasKey("GetNewCarPopUp") && Time.realtimeSinceStartup > 120)
        {
            PlayerPrefs.SetInt("GetNewCarPopUp", 1);
            OpenPopUp(0);
        }
    }

    public void OpenPopUp(int index)
    {
        Analytics.Instance.PopUpShown(adDetails[index].name);
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