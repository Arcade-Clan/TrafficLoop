using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
//using GameAnalyticsSDK;
using UnityEngine;
//using Facebook.Unity;
using UnityExtensions;

public class Analytics : MonoSingleton<Analytics>
{


    public void SendLevelStart(int level)
    {
        Elephant.LevelStarted(level);

    }

    public void SendLevelComplete(string level, int score, bool levelComplete = true)
    {

    }

}