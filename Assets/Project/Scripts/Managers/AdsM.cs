using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using ElephantSDK;
using RollicGames.Advertisements;
using TMPro;
using UnityEngine;

#if UNITY_IOS
using UnityEngine.iOS;
#endif
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
    public GameObject add3CarOffer;
    
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
    bool specialBuild;
    void Awake()
    {
        if (FindObjectOfType<SpecialBuild>())
            specialBuild = true;
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
        StartCoroutine("Timer");
        PrepareAds();
    }
    
    void OnSdkInit()
    {
        if (!specialBuild)
            RLAdvertisementManager.Instance.loadBanner();
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
        StopCoroutine("ShowPopUpsRoutine");
        StartCoroutine("ShowPopUpsRoutine");
        int interInit = RemoteConfig.GetInstance().GetInt("inter_init", 120);
        
        if (!PlayerPrefs.HasKey("FirstInter"))
        {
            yield return StartCoroutine("Waiter", interInit - 15);
            Debug.Log("RateUs");
#if UNITY_IOS
            Device.RequestStoreReview();
#elif UNITY_ANDROID || UNITY_EDITOR
            UIM.Instance.rateUsPanel.Show();
#endif
            yield return StartCoroutine("Waiter", 15);
            PlayerPrefs.SetInt("FirstInter", 1);
        }
        else
            yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("inter_freq", 90));

        while (true)
        {
            while (!RLAdvertisementManager.Instance.isInterstitialReady() && !Application.isEditor)
                yield return null;
            Analytics.Instance.InterstitialShown();
            if (!specialBuild)
                RLAdvertisementManager.Instance.showInterstitial();
            Debug.Log("InterShown");
            yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("inter_freq", 90));
        }
    }

   public void AndroidRate()
   {
       Application.OpenURL("market://details?id=" + Application.identifier);
   }
   
   IEnumerator Timer()
   {
       
       while (true)
       {
           yield return StartCoroutine("Waiter", 1);
           PlayerPrefs.SetInt("Timer",PlayerPrefs.GetInt("Timer")+1);
           //Debug.Log(PlayerPrefs.GetInt("Timer"));
       }
   }
   
   public int adIndex = 0;
   
   IEnumerator ShowPopUpsRoutine()
   {
       yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("popup_freq", 90) / 2);
       while (true)
       {
           while(PlayerPrefs.GetInt("Timer")< RemoteConfig.GetInstance().GetInt("popup_loop", 555))
               yield return null;
           //print("Timer");
           OpenPopUp(adIndex + 2);
           adIndex = (adIndex + 1) % 3;
           yield return StartCoroutine("Waiter", RemoteConfig.GetInstance().GetInt("popup_freq", 90));
       }
   }
   

   
    IEnumerator Waiter(float value)
    {
        while (value >= 0)
        {
            value -= Time.fixedDeltaTime/Time.timeScale;
            //print("A"+ Time.deltaTime);
            //print("B" + Time.unscaledDeltaTime);
            //print("C" + Time.fixedUnscaledDeltaTime);
            //print("A"+Time.timeScale);
            //print("D" + Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
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
        if (Application.isEditor|| specialBuild)
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
        //Debug.Log("No Ads");
    }
    
    void RewardedAdResultCallback(RLRewardedAdResult result)
    {
        switch (result)
        {
            case RLRewardedAdResult.Finished:
            {
                //Debug.Log("XXXFinished"+ currentRewarded.Method.Name);
                AcceptReward();
                break;
            }
            case RLRewardedAdResult.Skipped:
            {
                //Debug.Log("XXXSkipped" + currentRewarded.Method.Name);
                currentRewarded = null;
                break;
            }
            case RLRewardedAdResult.Failed:
            {
                //Debug.Log("XXXFailed" + currentRewarded.Method.Name);
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
        if (PlayerPrefs.HasKey("ClosestCarOffer" + PlayerPrefs.GetInt("LastCarLevel", 1)))
            yield break;
        PlayerPrefs.SetInt("ClosestCarOffer" + PlayerPrefs.GetInt("LastCarLevel", 1), 1);
        OpenPopUpAdd3Car();
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
        newCarImage.sprite = newCarSprites[PlayerPrefs.GetInt("LastCarLevel", 1)];
        newCarImagePopUp.sprite = newCarSprites[PlayerPrefs.GetInt("LastCarLevel", 1)];
        newCarButtonImage.sprite = newCarSprites[PlayerPrefs.GetInt("LastCarLevel", 1)-1];
    }

    public void FeverCarPopUp()
    {
        if (!PlayerPrefs.HasKey(adDetails[0].name+ "AdOpened") && PlayerPrefs.GetInt("Timer") >
            RemoteConfig.GetInstance().GetInt("fever_time", feverTimer))
        {
            PlayerPrefs.SetInt(adDetails[0].name + "AdOpened", 1);
            OpenPopUp(0);
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
            //print("SpeedUpPopUp");
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


    

    public int LastCarLevel()
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
        if (PlayerPrefs.GetInt("LastCarLevel",1)<LastCarLevel())
        {
            PlayerPrefs.SetInt("LastCarLevel", PlayerPrefs.GetInt("LastCarLevel",1)+1);
            PlayerPrefs.SetInt(adDetails[5].name + "AdOpened", 1);
            adDetails[5].buttonObject.Show();
            OpenPopUp(5);
        }
    }

    public void OpenPopUp(int index)
    {
        //print("OpenPopUp");
        Analytics.Instance.PopUpShown(adDetails[index].name);
        for (int a = 0; a < adDetails.Length; a++)
            adDetails[a].popUpPanel.Hide();
        add3CarOffer.Hide();
        adDetails[index].popUpPanel.transform.parent.parent.Show();
        adDetails[index].popUpPanel.Show();
    }

    public void OpenPopUpAdd3Car()
    {
        Analytics.Instance.PopUpShown("Add3Car");
        for (int a = 0; a < adDetails.Length; a++)
            adDetails[a].popUpPanel.Hide();
        add3CarOffer.transform.parent.parent.Show();
        add3CarOffer.Show();
    }
    
#endregion
    
}