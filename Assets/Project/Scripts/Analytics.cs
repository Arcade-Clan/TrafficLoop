using System.Collections;
using System.Collections.Generic;
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
    public void RewardedImpression(string value)
    {
        Elephant.Event("rw_ad_impression", 1,Params.New().Set("area_name", value));
    }
    
    public void RewardedCompleted(string value)
    {
        Elephant.Event("rw_ad_completed", 1,Params.New().Set("area_name", value));
    }
       
    public void RewardedTapped(string value)
    {
        Elephant.Event("rw_ad_tapped", 1,Params.New().Set("area_name", value));
    }

    public void SendCarBought()
    {
        Elephant.Event("BuyCarCount", 1);
        Elephant.Transaction("cash_spent",1, 0,100,"BuyCar");
    }

    public void SendMergeLevel(int level)
    {
        Elephant.Event("MergeLevel", level);
        Elephant.Transaction("cash_spent",1, 0,100,"Merge");
    }

    public void EarnedMoney(int value)
    {
        Elephant.Transaction("cash_earned",1, 0,value,"Gate");
    }
    
    public void SendSizeUp()
    {
        Elephant.Event("upgrade", 1,Params.New().Set("source", "SizeUp"));
        Elephant.LevelCompleted(GM.Instance.upgrades[1].upgradeLevel);
        Elephant.LevelStarted(GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Event("level_started", GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Event("level_completed", GM.Instance.upgrades[1].upgradeLevel);
        Elephant.Event("area_unlocked", GM.Instance.upgrades[1].upgradeLevel+1);
        Elephant.Transaction("cash_spent",1, 0,100,"SizeUp");
    }
    
    public void SendIncomeClicked()
    {
        Elephant.Event("upgrade", 1,Params.New().Set("source", "Income"));
        Elephant.Transaction("cash_spent",1, 0,100,"Income");

    }

    public void SendSpeedUp()
    {
        Elephant.Event("SpeedUpClicked", 1);
    }
    
}