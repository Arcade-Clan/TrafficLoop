using System.Collections;
using System.Collections.Generic;
using com.adjust.sdk;
using ElephantSDK;
//using GameAnalyticsSDK;
using UnityEngine;
//using Facebook.Unity;
using UnityExtensions;

public class Analytics : MonoSingleton<Analytics>
{
    public float multiplier = 1;
    
    void Start()
    {
        multiplier = RemoteConfig.GetInstance().GetFloat("Economy", 1);
        Elephant.Event("level_started", GM.Instance.upgrades[1].upgradeLevel);
    }


    public void InterstitialShown()
    {
        Elephant.Event("interstitial_shown",1);
    }

    public void RewardedFailed(string value)
    {
        Elephant.Event("rw_failed", 1, Params.New().Set("activity_name", value));
    }
    
    public void RewardedImpression(string value)
    {
        Elephant.Event("rw_impression", 1,Params.New().Set("activity_name", value));
    }
    
    public void RewardedCompleted(string value)
    {
        Elephant.Event("rw_completed", 1,Params.New().Set("activity_name", value));
    }
       
    public void RewardedTapped(string value)
    {
        Elephant.Event("rw_tapped", 1,Params.New().Set("activity_name", value));
    }

    public void ButtonTapped(string value)
    {
        Elephant.Event("rw_ad_tapped", 1, Params.New().Set("button_name", value).Set("minute", Time.realtimeSinceStartup));
    }
    
    public void SendCarBought()
    {
        Elephant.Event("BuyCarCount", 1);
        Elephant.Event("upgrade", 1, Params.New().Set("source", "AddCar"));
        Elephant.Transaction("cash_spent",1, 0, GM.Instance.gold,"BuyCar");
        Elephant.Event("money", GM.Instance.gold);
    }

    public void SendMergeLevel(int level)
    {
        Elephant.Event("MergeLevel", level);
        Elephant.Event("upgrade", 1, Params.New().Set("source", "Merge"));
        Elephant.Transaction("cash_spent",1, 0, GM.Instance.gold,"Merge");
        Elephant.Event("money", GM.Instance.gold);
    }

    public void BoosterUsed(string value)
    {
        Elephant.Event("booster_used", 1, Params.New().Set("type", value));
    }
    
    
    public void EarnedMoney(int value)
    {
        Elephant.Transaction("cash_earned",1, 0,value,"Gate");
        Elephant.Event("money", GM.Instance.gold);
    }

    public void PopUpShown(string value)
    {
        Elephant.Event("popup_shown", 1, Params.New().Set("type", value));
    }
    
    public void SendSizeUp()
    {
        Elephant.Event("upgrade", 1,Params.New().Set("source", "SizeUp"));
        Elephant.LevelCompleted(GM.Instance.upgrades[1].upgradeLevel);
        Elephant.LevelStarted(GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Event("level_started", GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Event("level_completed", GM.Instance.upgrades[1].upgradeLevel);
        Elephant.Event("area_unlocked", GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Transaction("cash_spent",1, 0, GM.Instance.gold,"SizeUp");
        Elephant.Event("money", GM.Instance.gold);
        if(GM.Instance.upgrades[1].upgradeLevel==1)
        {
            AdjustEvent adjustEvent = new ("yg1mza");
            Adjust.trackEvent(adjustEvent);
        }
        else if (GM.Instance.upgrades[1].upgradeLevel == 2)
        {
            AdjustEvent adjustEvent = new("wqwslw");
            Adjust.trackEvent(adjustEvent);
        }
        else if (GM.Instance.upgrades[1].upgradeLevel == 3)
        {
            AdjustEvent adjustEvent = new("5y5fr8");
            Adjust.trackEvent(adjustEvent);
        }
        else if (GM.Instance.upgrades[1].upgradeLevel == 4)
        {
            AdjustEvent adjustEvent = new("ftcypk");
            Adjust.trackEvent(adjustEvent);
        }
        else if (GM.Instance.upgrades[1].upgradeLevel == 5)
        {
            AdjustEvent adjustEvent = new("8fu53b");
            Adjust.trackEvent(adjustEvent);
        }
    }
    
    public void SendIncomeClicked()
    {
        Elephant.Event("upgrade", 1,Params.New().Set("source", "Income"));
        Elephant.Transaction("cash_spent",1, 0,GM.Instance.gold,"Income");
        Elephant.Event("money", GM.Instance.gold);
    }

    public void SendSpeedUp()
    {
        Elephant.Event("SpeedUpClicked", 1);
    }
    
}