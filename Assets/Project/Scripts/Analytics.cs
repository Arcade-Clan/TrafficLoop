using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
//using GameAnalyticsSDK;
using UnityEngine;
//using Facebook.Unity;
using UnityExtensions;

public class Analytics : MonoSingleton<Analytics>
{
    
    public void SendCarBought()
    {
        Elephant.Event("BuyCarCount", 1);
    }

    public void SendMergeLevel(int level)
    {
        Elephant.Event("MergeLevel", level);
    }

    public void SendSizeUp()
    {
        Elephant.Event("SizeUpCount", 1);
    }
    
    public void SendIncomeClicked()
    {
        Elephant.Event("IncomeClicked", 1);
    }

    public void SendSpeedUp()
    {
        Elephant.Event("SpeedUpClicked", 1);
    }

}