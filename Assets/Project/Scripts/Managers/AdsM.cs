using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExtensions;
public class AdsM : MonoSingleton<AdsM>
{
    public int speedUpMultiplier;
    public int speedUpTimer;
    [ReadOnly] public float speedOfferSpeedUp = 1;
    [ReadOnly] public float autoTapSpeedUp = 1;
    public int autoTapTimer = 60;
    public int upgradeAllCarTimer = 15;
    [ReadOnly] public int upgradeAllCarLevel = 0;
    void Start()
    {
        UIM.Instance.addIncomeText.text = "" + Mathf.RoundToInt(GM.Instance.merge.Cost() * 2f);
    }
    
    public void Add3Car()
    {
        PM.Instance.StartCoroutine("Add3CarRoutine");
    }

    public void AddNewCar()
    {
        int carIndex = 0;
        for (int a = GM.Instance.cars.Length - 2; a >= 0; a--)
        {
            if (GM.Instance.cars[a].carLevel <= 0)
                continue;
            carIndex = a;
            break;
        }
        GM.Instance.trafficController.AddCar(carIndex);
    }
    
    public void SpeedUp()
    {
        StartCoroutine("SpeedUpRoutine");
    }

    IEnumerator SpeedUpRoutine()
    {
        UIM.Instance.speedUpButton.Hide();
        speedOfferSpeedUp = speedUpMultiplier;
        float timer = Time.realtimeSinceStartup;
        while (timer + speedUpTimer > Time.realtimeSinceStartup)
        {
            UIM.Instance.speedUpFiller.fillAmount = 1 - (Time.realtimeSinceStartup - timer) / speedUpTimer;
            yield return null;
        }
        

        speedOfferSpeedUp = 1;
        UIM.Instance.speedUpButton.Show();
    }

    public void AddIncome()
    {
        GM.Instance.gold += Mathf.RoundToInt(GM.Instance.merge.Cost() * 2f);
        UIM.Instance.UpdateGold();
        UIM.Instance.UpdateEconomyUI();
    }

    public void AutoTap()
    {
        StartCoroutine("AutoTapRoutine");
    }
    

    IEnumerator AutoTapRoutine()
    {
        UIM.Instance.autoTapButton.Hide();
        autoTapSpeedUp = GM.Instance.tapSpeedUpMultiplier;
        float timer = Time.realtimeSinceStartup;
        while (timer + autoTapTimer > Time.realtimeSinceStartup)
        {
            UIM.Instance.autoTapFiller.fillAmount = 1 - (Time.realtimeSinceStartup - timer) / autoTapTimer;
            yield return null;
        }
        autoTapSpeedUp = 1;
        UIM.Instance.autoTapButton.Show();
    }
    
    public void UpgradeAllCars()
    {
        StartCoroutine("UpgradeAllCarsRoutine");
    }
    
    IEnumerator UpgradeAllCarsRoutine()
    {
        for (int a = GM.Instance.cars.Length - 1; a >= 0; a--)
        {
            for (int b = GM.Instance.cars[a].cars.Count - 1; b >= 0; b--)
                GM.Instance.cars[a].cars[b].AllCarUpgrade();
        }
        UIM.Instance.upgradeAllCarButton.Hide();
        upgradeAllCarLevel = 1;
        float timer = Time.realtimeSinceStartup;
        while (timer + upgradeAllCarTimer > Time.realtimeSinceStartup)
        {
            UIM.Instance.upgradeAllCarFiller.fillAmount = 1 - (Time.realtimeSinceStartup - timer) / upgradeAllCarTimer;
            yield return null;
        }
        upgradeAllCarLevel = 0;
        UIM.Instance.upgradeAllCarButton.Show();
    }


    public void FeverCar()
    {
        GM.Instance.trafficController.CreateCar(11);
    }
    
    
}