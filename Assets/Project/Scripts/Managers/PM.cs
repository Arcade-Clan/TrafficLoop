using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ElephantSDK;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;
public class PM : MonoSingleton<PM>
{
    
    
    
    public IEnumerator GetStatsRoutine()
    {
        while(true)
        {
            List<Car> calculatedCars = new();
            for (int a = 0; a < GM.Instance.cars.Length; a++)
                calculatedCars.AddRange(GM.Instance.cars[a].cars);
            UIM.Instance.carAmount.text = "" + calculatedCars.Count;
            
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
            yield return null;
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
        if(Options.Instance.vibrationsOn)
            Taptic.Light();
        AdsM.Instance.ResetAutoTapTime();
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
            if(Mathf.Max(GM.Instance.tapSpeed, AdsM.Instance.adDetails[1].multiplierValue) > 1)
                GM.Instance.baseSecondCreationSpeedUp = RemoteConfig.GetInstance().GetFloat("speed_up_creation", 1);
            else
                GM.Instance.baseSecondCreationSpeedUp = 1;
            GM.Instance.simulationSpeed = Mathf.Max(GM.Instance.tapSpeed, AdsM.Instance.adDetails[1].multiplierValue) * AdsM.Instance.adDetails[2].multiplierValue*
                                          GM.Instance.editorSpeedUp;
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


    public void IncreaseCarCountButton()
    {
        if (!UIM.Instance.IncreaseTutorialProgression(0))
            return;
        UIM.Instance.upgrades[0].button.transform.SizeUpAnimation("AddCar");
        if (UIM.Instance.upgrades[0].state == "CanBuy")
        {
            GM.Instance.gold -= GM.Instance.upgrades[0].Cost();
            UIM.Instance.UpdateGold();
            IncreaseCarCount();
        }
        else if (UIM.Instance.upgrades[0].state == "AdButton")
            AdsM.Instance.Add3CarButton();
    }
    
    public void IncreaseCarCount()
    {

        Analytics.Instance.SendCarBought();
        GM.Instance.trafficController.AddCar(Mathf.RoundToInt(AdsM.Instance.adDetails[4].multiplierValue));
        GM.Instance.cars[0].carLevel += 1; 
        PlayerPrefs.SetInt(GM.Instance.cars[0].carName, GM.Instance.cars[0].carLevel);
        Upgrade(0);
        GM.Instance.trafficController.ProcessProductionIndex();
    }

    
    public void IncreaseSizeButton()
    {
        if (!UIM.Instance.IncreaseTutorialProgression(4))
            return;
        UIM.Instance.upgrades[1].button.transform.SizeUpAnimation("IncreaseSize");
        if (UIM.Instance.upgrades[1].state == "CanBuy")
        {
            GM.Instance.gold -= GM.Instance.upgrades[1].Cost();
            UIM.Instance.UpdateGold();
            IncreaseSize();
        }
        else if (UIM.Instance.upgrades[1].state == "AdButton")
            AdsM.Instance.IncreaseSizeButton();
    }
    
    public void IncreaseSize()
    {

        Analytics.Instance.SendSizeUp();
        Upgrade(1);
        LM.Instance.SwitchLevel();
    }
    
    
    public void IncreaseGatesButton()
    {
        if (!UIM.Instance.IncreaseTutorialProgression(2))
            return;
        UIM.Instance.upgrades[2].button.transform.SizeUpAnimation("IncreaseIncome");
        if(UIM.Instance.upgrades[2].state=="CanBuy")
        {
            GM.Instance.gold -= GM.Instance.upgrades[2].Cost();
            UIM.Instance.UpdateGold();
            IncreaseGates();
        }
        else if (UIM.Instance.upgrades[2].state == "AdButton")
            AdsM.Instance.IncreaseGatesButton();
    }
    
    public void IncreaseGates()
    {
        Analytics.Instance.SendIncomeClicked();
        Upgrade(2);
        LM.Instance.UpdateGates();
    }
    
    void Upgrade(int value)
    {
        GM.Instance.upgrades[value].upgradeLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.upgrades[value].upgradeName, GM.Instance.upgrades[value].upgradeLevel);
        UIM.Instance.UpdateEconomyUI(); 
    }

    public void MergeButton()
    {
        if (!UIM.Instance.IncreaseTutorialProgression(3))
            return;
        UIM.Instance.merge.button.transform.SizeUpAnimation("Merge");
        if (UIM.Instance.merge.state == "CanBuy")
        {
            GM.Instance.gold -= GM.Instance.merge.Cost();
            UIM.Instance.UpdateGold();
            Merge();
        }
        else if (UIM.Instance.merge.state == "AdButton")
            AdsM.Instance.MergeButton();
    }
    
    public void Merge()
    {


        if (!CanMerge())
            return;
        GM.Instance.merge.mergeLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.merge.mergeName, GM.Instance.merge.mergeLevel);
        Analytics.Instance.SendMergeLevel(GM.Instance.merge.mergeLevel);
        MergeCars();
    }

    void MergeCars()
    {
        int carIndex = LastCarIndex();
        Car[] closestCars = GM.Instance.cars[carIndex].cars.OrderBy(p => Vector3.Distance(p.transform.position,Vector3.zero)).ToArray();
        GM.Instance.cars[carIndex].carLevel = Mathf.Max(GM.Instance.cars[carIndex].carLevel-3,0);
        GM.Instance.cars[carIndex+1].carLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex].carName, GM.Instance.cars[carIndex].carLevel);
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex+1].carName, GM.Instance.cars[carIndex+1].carLevel);
        GM.Instance.trafficController.ProcessProductionIndex();
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
        if(Options.Instance.vibrationsOn)
            Taptic.Medium();
        UIM.Instance.UpdateEconomyUI();
        AdsM.Instance.GetNewCarPopUp();
    }
    
    public bool CanMerge()
    {
        if(LastCarIndex()==-1)
            return false;
        return true;
    }

    public int LastCarIndex()
    {
        for (int a = 0; a < 9; a++)
        {
            if (GM.Instance.cars[a].carLevel >= 3 && GM.Instance.cars[a].cars.Count >= 3)
                    return a;
        }
        return -1;
    }

    #region AdProcesses
        
    public IEnumerator FeverCarRoutine()
    {
        GM.Instance.trafficController.AddCar(16);
        AdsM.Instance.adDetails[0].multiplierValue = 1;
        GM.Instance.trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
        yield return WaitForAddRoutine(0);
        AdsM.Instance.adDetails[0].multiplierValue = -1;
        UIM.Instance.UpdateEconomyUI();
    }
    public IEnumerator AutoTapRoutine()
    {
        AdsM.Instance.adDetails[1].multiplierValue = 2;
        yield return WaitForAddRoutine(1);
        AdsM.Instance.adDetails[1].multiplierValue = 1;
    }
    public IEnumerator SpeedUpRoutine()
    {
        AdsM.Instance.adDetails[2].multiplierValue = 3;
        yield return WaitForAddRoutine(2);
        AdsM.Instance.adDetails[2].multiplierValue = 1;
    }
    public IEnumerator AddIncomeRoutine()
    {
        AdsM.Instance.adDetails[3].multiplierValue = 3;
        UIM.Instance.UpdateEconomyUI();
        yield return WaitForAddRoutine(3);
        AdsM.Instance.adDetails[3].multiplierValue = 1;
        UIM.Instance.UpdateEconomyUI();
    }
    public IEnumerator EvolveCarsRoutine()
    {
        for (int a = GM.Instance.cars.Length - 1; a >= 0; a--)
        {
            for (int b = GM.Instance.cars[a].cars.Count - 1; b >= 0; b--)
                GM.Instance.cars[a].cars[b].AllCarUpgrade();
        }
        AdsM.Instance.adDetails[4].multiplierValue = 1;
        yield return WaitForAddRoutine(4);
        AdsM.Instance.adDetails[4].multiplierValue = 0;
    }

    IEnumerator WaitForAddRoutine(int index)
    {
        AdsM.Instance.adDetails[index].buttonObject.enabled=false;
        AdsM.Instance.adDetails[index].text.Hide();
        AdsM.Instance.adDetails[index].timer.Show();
        AdsM.Instance.adDetails[index].adImage.Hide();
        AdsM.Instance.adDetails[index].rayImage.Show();
        AdsM.Instance.adDetails[index].buttonObject.transform.DOScale(1.05f,0.5f).SetDelay(0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad).SetUpdate(UpdateType.Normal,true);
        float timer = AdsM.Instance.adDetails[index].timerValue;
        while (timer>0)
        {
            AdsM.Instance.adDetails[index].timer.text = "" + string.Format("{0:N1}", timer);
            timer -= 1/60f;
            yield return null;
        }
        AdsM.Instance.adDetails[index].buttonObject.transform.DOKill();
        AdsM.Instance.adDetails[index].buttonObject.transform.localScale = Vector3.one;
        AdsM.Instance.adDetails[index].text.Show();
        AdsM.Instance.adDetails[index].timer.Hide();
        AdsM.Instance.adDetails[index].adImage.Show();
        AdsM.Instance.adDetails[index].rayImage.Hide();
        AdsM.Instance.adDetails[index].buttonObject.enabled=true;
    }
    
    public void AddLastCar()
    {
        int carIndex = AdsM.Instance.LastCarLevel();
        GM.Instance.cars[carIndex].carLevel += 1; 
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex].carName, GM.Instance.cars[carIndex].carLevel);
        GM.Instance.trafficController.AddCar(carIndex);
        GM.Instance.PlaySound(0);
        GM.Instance.trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }
    
    public IEnumerator Add3CarRoutine()
    {
        for (int a = 0; a < 3; a++)
        {
            Add3Car();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator AddClosest3CarRoutine()
    {
        for (int a = 0; a < 3; a++)
        {
            AddClosest3Car();
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void AddClosest3Car()
    {
        int carIndex = AdsM.Instance.LastCarLevel() - 1;
        GM.Instance.trafficController.AddCar(carIndex);
        GM.Instance.cars[carIndex].carLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex].carName, GM.Instance.cars[carIndex].carLevel);
        GM.Instance.PlaySound(0);
        GM.Instance.trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }
    
    public void Add3Car()
    {
        GM.Instance.trafficController.AddCar(Mathf.RoundToInt(AdsM.Instance.adDetails[4].multiplierValue));
        GM.Instance.cars[0].carLevel += 1; 
        PlayerPrefs.SetInt(GM.Instance.cars[0].carName, GM.Instance.cars[0].carLevel);
        GM.Instance.PlaySound(0);
        GM.Instance.trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }
    
    #endregion



}
public static class SizeUpExtension
{
    public static void SizeUpAnimation(this Transform button,string name)
    {
        Analytics.Instance.ButtonTapped(name);
        GM.Instance.PlaySound(0);
        if(Options.Instance.vibrationsOn)
            Taptic.Medium();
        button.DOKill();
        button.DOScale(1.2f, 0.25f).SetUpdate(UpdateType.Normal,true).OnComplete(
            ()=>
            {
                button.localScale = Vector3.one*1.2f;
                button.DOScale(1f, 0.25f).SetEase(Ease.InSine).SetUpdate(UpdateType.Normal, true);
            });
    }  
}