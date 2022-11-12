using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ElephantSDK;
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
    public Image newCarImage;
    public Image newCarImagePopUp;
    public Image newCarButtonImage;
    public Sprite[] newCarSprites;
    
#if UNITY_IOS
    private string _appKey = "1728094ad";
#elif UNITY_ANDROID || UNITY_EDITOR
    private string _appKey = "172814145";
#endif

    public bool adReady = false;
    public Action currentRewarded;

    public int speedUpTimer = 120;
    public int incomeTimer = 180;
    public int feverTimer = 300;
    public int evolveTimer = 420;
    void Awake() 
    {
        RLAdvertisementManager.Instance.init(_appKey);
        RLAdvertisementManager.Instance.rewardedAdResultCallback = RewardedAdResultCallback;
        RLAdvertisementManager.OnRollicAdsSdkInitializedEvent += OnSdkInit;
        RLAdvertisementManager.OnRollicAdsAdFailedEvent += OnBannerFailed;
        if(Application.isEditor)
            PrepareAds();
    }

    void Start()
    {
        adDetails[1].timerValue = RemoteConfig.GetInstance().GetFloat("autoclick_duration", adDetails[1].timerValue);
        StartCoroutine("PopUpTimer");
    }
    
    void OnSdkInit() 
    {
        RLAdvertisementManager.Instance.loadBanner();
        PrepareAds();
    }

    void PrepareAds()
    {
        StartCoroutine("InterRoutine");
        for (int a = 0; a < adDetails.Length; a++)
        {
            if (PlayerPrefs.HasKey(adDetails[a].name + "AdOpened"))
                adDetails[a].buttonObject.Show();
            else
                adDetails[a].buttonObject.Hide();
        }
        
    }
    

#region AdEngine

   IEnumerator InterRoutine()
    {
        ResetAutoTapTime();
        if (!PlayerPrefs.HasKey("FirstInter"))
        {
            yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("inter_init", 120));
            PlayerPrefs.SetInt("FirstInter", 1);
        }
        else
            yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("inter_freq", 90));

        while (true)
        {
            while (!RLAdvertisementManager.Instance.isInterstitialReady() && !Application.isEditor)
                yield return null;
            Analytics.Instance.InterstitialShown();
            RLAdvertisementManager.Instance.showInterstitial();
            print("InterShown");
            yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("inter_freq", 90));
        }
    }


   IEnumerator PopUpTimer()
   {
       StartCoroutine("ShowPopUpsRoutine");
       while (true)
       {
           yield return StartCoroutine("Waiter", 1);
           PlayerPrefs.SetInt("Timer",PlayerPrefs.GetInt("Timer")+1);
       }
   }
   
   public int adIndex = 0;
   
   IEnumerator ShowPopUpsRoutine()
   {
       while (true)
       {
           if(PlayerPrefs.GetInt("Timer")< RemoteConfig.GetInstance().GetInt("popup_loop", 600))
               yield return null;
           yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("popup_freq", 120));
           OpenPopUp(adIndex + 2);
           adIndex = (adIndex + 1) % 3;
       }
   }
   

   
    IEnumerator Waiter(float value)
    {
        while (value > 0)
        {
            value -= 1 / 60f;
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
        noAdsText.DOFade(0, 0.5f).SetDelay(0.5f).OnComplete(() => noAdsText.enabled = false);
        Debug.Log("No Ads");
    }
    
    void RewardedAdResultCallback(RLRewardedAdResult result)
    {
        switch (result)
        {
            case RLRewardedAdResult.Finished:
            {
                Debug.Log("XXXFinished"+ currentRewarded.Method.Name);
                AcceptReward();
                break;
            }
            case RLRewardedAdResult.Skipped:
            {
                Debug.Log("XXXSkipped" + currentRewarded.Method.Name);
                currentRewarded = null;
                break;
            }
            case RLRewardedAdResult.Failed:
            {
                Debug.Log("XXXFailed" + currentRewarded.Method.Name);
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
    
#endregion
    
#region Buttons

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
        StartCoroutine("WaitFor3CarOffer");
    }

    IEnumerator WaitFor3CarOffer()
    {
        yield return StartCoroutine("Waiter", 15);
        OpenPopUp(0);
    }

    public void AddClosest3CarButton()
    {
        adDetails[5].buttonObject.transform.SizeUpAnimation("AddClosest3Car");
        Analytics.Instance.RewardedTapped("AddClosest3Car");
        ProcessAds(AddClosest3Car);
    }

    void AddClosest3Car()
    {
        PM.Instance.StartCoroutine("AddClosest3CarRoutine");
    }
    
#endregion
    
#region PopUp

    void Update()
    {
        FeverCarPopUp();
        ShowSpeedUpPopUp();
        AddIncomePopUp();
        EvolveCarsPopUp();
        newCarImage.sprite = newCarSprites[PlayerPrefs.GetInt("CarLevel", 1)];
        newCarImagePopUp.sprite = newCarSprites[PlayerPrefs.GetInt("CarLevel", 1)];
        newCarButtonImage.sprite = newCarSprites[PlayerPrefs.GetInt("CarLevel", 1)-1];
    }

    public void FeverCarPopUp()
    {
        if (!PlayerPrefs.HasKey(adDetails[0].name+ "AdOpened") && PlayerPrefs.GetInt("Timer") >
            RemoteConfig.GetInstance().GetInt("fever_time", feverTimer))
        {
            PlayerPrefs.SetInt(adDetails[0].name + "AdOpened", 1);
            adDetails[0].buttonObject.Show();
        }
    }

#region AutoTap

    public void ResetAutoTapTime()
    {
        StopCoroutine("ResetAutoTapRoutine");
        StartCoroutine("ResetAutoTapRoutine");
    }

    IEnumerator ResetAutoTapRoutine()
    {
        yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("autoclick_time", 60));
        AutoTapPopUp();
    }

    public void AutoTapPopUp()
    {
        PlayerPrefs.SetInt(adDetails[1].name + "AdOpened", 1);
        OpenPopUp(1);
        adDetails[1].buttonObject.Show();
    }

#endregion
    
    public void ShowSpeedUpPopUp()
    {
        if (!PlayerPrefs.HasKey(adDetails[2].name + "AdOpened") && PlayerPrefs.GetInt("Timer") >
            RemoteConfig.GetInstance().GetInt("x2speed_time", speedUpTimer))
        {
            PlayerPrefs.SetInt(adDetails[2].name + "AdOpened", 1);
            OpenPopUp(2);
            adDetails[2].buttonObject.Show();
        }
    }

    public void AddIncomePopUp()
    {
        if (!PlayerPrefs.HasKey(adDetails[3].name + "AdOpened") && PlayerPrefs.GetInt("Timer") >
            RemoteConfig.GetInstance().GetInt("x2money_time", incomeTimer))
        {
            PlayerPrefs.SetInt(adDetails[3].name + "AdOpened", 1);
            OpenPopUp(3);
            adDetails[3].buttonObject.Show();
        }
    }
    
    public void EvolveCarsPopUp()
    {
        if (!PlayerPrefs.HasKey(adDetails[4].name + "AdOpened") && PlayerPrefs.GetInt("Timer") >
            RemoteConfig.GetInstance().GetInt("evolvecars_time", evolveTimer))
        {
            PlayerPrefs.SetInt(adDetails[4].name + "AdOpened", 1);
            OpenPopUp(4);
            adDetails[4].buttonObject.Show();
        }
    }


    

    public int CarLevel()
    {
        for (int a = 9; a >= 0; a--)
        {
            if (GM.Instance.cars[a].carLevel <= 0)
                continue;
            return a;
        }
        return 0;
    }
    
    public void GetNewCarPopUp()
    {
        if (PlayerPrefs.GetInt("CarLevel",1)<CarLevel())
        {
            PlayerPrefs.SetInt("CarLevel", PlayerPrefs.GetInt("CarLevel",1)+1);
            PlayerPrefs.SetInt(adDetails[5].name + "AdOpened", 1);
            adDetails[5].buttonObject.Show();
            OpenPopUp(5);
        }
    }

    public void OpenPopUp(int index)
    {
        Analytics.Instance.PopUpShown(adDetails[index].name);
        for (int a = 0; a < adDetails.Length; a++)
            adDetails[a].popUpPanel.Hide();
        print(adDetails[index].popUpPanel.transform.parent.parent.name);
        print(adDetails[index].popUpPanel.gameObject.name);
        adDetails[index].popUpPanel.transform.parent.parent.Show();
        adDetails[index].popUpPanel.Show();
    }

#endregion
    
}