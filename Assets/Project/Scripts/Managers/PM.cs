using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;
public class PM : MonoSingleton<PM>
{
    
    [HideInInspector]
    public List<int> carProductionIndex = new List<int>();
    
   public IEnumerator GetStatsRoutine()
    {
        while(true)
        {
            List<Car> calculatedCars = new();
            for (int a = 0; a < GM.Instance.cars.Length; a++)
                calculatedCars.AddRange(GM.Instance.cars[a].cars);
            UIM.Instance.carAmount.text = "" + calculatedCars.Count;
            UIM.Instance.carAmountPerMinute.text = "" + GM.Instance.upgrades[0].Value();
            UIM.Instance.incomePerMinute.text = "" + GM.Instance.upgrades[0].Value() * GM.Instance.upgrades[2].Value();

            float density = 0;
            for (int a = 0; a < calculatedCars.Count; a++)
            {
                if (calculatedCars[a].collidedCar)
                    density += 1;
            }

            if (calculatedCars.Count > 0)
            {
                GM.Instance.trafficDensity = density / calculatedCars.Count;
                UIM.Instance.trafficDensity.fillAmount = Mathf.Lerp(UIM.Instance.trafficDensity.fillAmount, GM.Instance.trafficDensity, 0.1f);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public int TotalCarCount()
    {
        int carCount = 0;
        for (int a = 0; a < GM.Instance.cars.Length; a++)
        {
            carCount += GM.Instance.cars[a].carLevel;
        }

        return carCount;
    }
    
#region SpeedUp

    public void SpeedUp()
    {
        UIM.Instance.IncreaseTutorialProgression(1);
        Analytics.Instance.SendSpeedUp();
        Taptic.Light();
        StopCoroutine("SpeedUpCoolDown");
        GM.Instance.tapSpeed = GM.Instance.tapSpeedUpMultiplier;
        StartCoroutine("SpeedUpCoolDown");
    }

    IEnumerator SpeedUpCoolDown()
    {
        yield return new WaitForSecondsRealtime(GM.Instance.tapSpeedUpTimer);
        GM.Instance.tapSpeed = 1;
    }
    
    public IEnumerator TimeCalculationRoutine()
    {
        while (true)
        {
            if(Mathf.Max(GM.Instance.tapSpeed, AdsM.Instance.autoTapSpeedUp)>1)
                GM.Instance.baseSecondCreationSpeedUp = GM.Instance.baseSecondCreationSpeedUpMultiplier;
            else
                GM.Instance.baseSecondCreationSpeedUp = 1;
            GM.Instance.simulationSpeed = Mathf.Max(GM.Instance.tapSpeed, AdsM.Instance.autoTapSpeedUp) * AdsM.Instance.speedOfferSpeedUp;
            Time.timeScale = Mathf.Lerp(Time.timeScale, GM.Instance.simulationSpeed, 0.2f);
            ProcessTrails();
            yield return null;
        }
    }
    void ProcessTrails()
    {
        for (int a = 0; a < GM.Instance.cars.Length; a++)
        {
            for (int b = 0; b < GM.Instance.cars[a].cars.Count; b++)
                GM.Instance.cars[a].cars[b].cars[GM.Instance.cars[a].cars[b].carIndex].trail.
                    GetComponentsInChildren<TrailRenderer>().ForEach(t => t.time = GM.Instance.simulationSpeed * 0.5f);
        } 
    }
#endregion


    public void IncreaseCarCount()
    {
        //PlaySound(1);
        if (!UIM.Instance.IncreaseTutorialProgression(0))
            return;
        GM.Instance.PlaySound(0);
        Analytics.Instance.SendCarBought();
        Taptic.Medium();
        GM.Instance.gold -= GM.Instance.upgrades[0].Cost();
        UIM.Instance.UpdateGold();
        GM.Instance.upgrades[0].upgradeLevel += 1;
        GM.Instance.trafficController.AddCar(AdsM.Instance.upgradeAllCarLevel);
        PlayerPrefs.SetInt(GM.Instance.upgrades[0].upgradeName, GM.Instance.upgrades[0].upgradeLevel);
        GM.Instance.cars[0].carLevel += 1; 
        PlayerPrefs.SetInt(GM.Instance.cars[0].carName, GM.Instance.cars[0].carLevel);
        CreateProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }

    public void IncreaseSize()
    {
        //PlaySound(1);
        if (!UIM.Instance.IncreaseTutorialProgression(4))
            return;
        GM.Instance.PlaySound(0);
        Analytics.Instance.SendSizeUp();
        Taptic.Medium();
        GM.Instance.gold -= GM.Instance.upgrades[1].Cost();
        UIM.Instance.UpdateGold();
        GM.Instance.upgrades[1].upgradeLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.upgrades[1].upgradeName, GM.Instance.upgrades[1].upgradeLevel);
        LM.Instance.SwitchLevel();
        UIM.Instance.UpdateEconomyUI();
    }

    public void IncreaseIncome()
    {
        if (!UIM.Instance.IncreaseTutorialProgression(2))
            return;
        GM.Instance.PlaySound(0);
        Analytics.Instance.SendIncomeClicked();
        Taptic.Medium();
        GM.Instance.gold -= GM.Instance.upgrades[2].Cost();
        UIM.Instance.UpdateGold();
        GM.Instance.upgrades[2].upgradeLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.upgrades[2].upgradeName, GM.Instance.upgrades[2].upgradeLevel);
        UIM.Instance.UpdateEconomyUI();

    }

    public void Merge()
    {

        if (!UIM.Instance.IncreaseTutorialProgression(3))
            return;
        GM.Instance.PlaySound(0);
        Taptic.Medium();
        GM.Instance.gold -= GM.Instance.merge.Cost();
        UIM.Instance.UpdateGold();
        GM.Instance.merge.mergeLevel += 1;
        Analytics.Instance.SendMergeLevel(GM.Instance.merge.mergeLevel);
        PlayerPrefs.SetInt(GM.Instance.merge.mergeName, GM.Instance.merge.mergeLevel);
        MergeCars();
    }

    void MergeCars()
    {
        int carIndex = 0;
        for (int a = 0; a < GM.Instance.cars.Length - 1; a++)
        {
            if (GM.Instance.cars[a].cars.Count >= 3&&!GM.Instance.cars[a].specialCar)
            {
                carIndex = a;
                break;
            }
        }
        Car[] closestCars = GM.Instance.cars[carIndex].cars.OrderBy(p => Vector3.Distance(p.transform.position,Vector3.zero)).ToArray();
        GM.Instance.cars[carIndex].carLevel = Mathf.Max(GM.Instance.cars[carIndex].carLevel-3,0);
        GM.Instance.cars[carIndex+1].carLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex].carName, GM.Instance.cars[carIndex].carLevel);
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex+1].carName, GM.Instance.cars[carIndex+1].carLevel);
        CreateProductionIndex();
        StartCoroutine(MergeCarsRoutine(closestCars));
    }

    IEnumerator MergeCarsRoutine(Car[] cars)
    {
        cars[0].StopCoroutine("MoveRoutine");
        GM.Instance.cars[cars[0].carIndex].cars.Remove(cars[0]);
        GM.Instance.cars[cars[1].carIndex].cars.Remove(cars[1]);
        GM.Instance.cars[cars[2].carIndex].cars.Remove(cars[2]);
        UIM.Instance.UpdateEconomyUI();
        for (int a = 1; a < 3; a++)
        {
            cars[a].StopAllCoroutines();
            cars[a].GetComponentInChildren<Collider>().enabled = false;
            cars[a].transform.SetParent(cars[0].transform);
            cars[a].transform.DOLocalRotate(Vector3.zero, 0.66f);
            cars[a].transform.DOLocalMoveX(0, 0.66f);
            cars[a].transform.DOLocalMoveZ(0, 0.66f);
            cars[a].transform.DOLocalMoveY(7, 0.33f).SetEase(Ease.OutSine);
        }
        yield return new WaitForSeconds(0.33f);

        cars[1].transform.DOLocalMoveY(0, 0.33f).SetEase(Ease.InSine);
        yield return cars[2].transform.DOLocalMoveY(0, 0.33f).SetEase(Ease.InSine).WaitForCompletion();
        Destroy(cars[1].gameObject);
        Destroy(cars[2].gameObject);
        cars[0].UpgradeCar();
        cars[0].StartCoroutine("MoveRoutine");
        GM.Instance.cars[cars[0].carIndex].cars.Add(cars[0]);
        Taptic.Medium();
        UIM.Instance.UpdateEconomyUI();
    }
    
    public bool CanMerge()
    {
        for (int a = 0; a < GM.Instance.cars.Length-1; a++)
        {
            if (GM.Instance.cars[a].cars.Count >= 3 && !GM.Instance.cars[a].specialCar)
                return true;
        }

        return false;
    }
    
    public void CreateProductionIndex()
    {
        carProductionIndex.Clear();
        for (int a = 0; a < GM.Instance.cars.Length; a++)
        {
            for (int b = 0; b < GM.Instance.cars[a].carLevel; b++)
                carProductionIndex.Add(a);
        }
        if(carProductionIndex.Count * GM.Instance.specialCarRandomChance > Random.Range(0f,1f))
            carProductionIndex.Add(Random.Range(10,12));
        carProductionIndex.Shuffle();
    }

    public IEnumerator Add3CarRoutine()
    {
        for (int a = 0; a < 3; a++)
        {
            IncreaseCarCount();
            yield return new WaitForSeconds(0.5f);
        }
    }
}