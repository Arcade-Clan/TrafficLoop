﻿using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityExtensions;

public class GameManager : MonoSingleton<GameManager>
{

    public int gold;
    public List<Level> levels;
    public float slowStrength = 0.1f;
    public float carSpeed;
    [ReadOnly]
    public float simulationSpeed =1;
    float speedUp = 1;
    public float speedUpTimer = 0.5f;
    public float speedUpMultiplier = 2f;
    public float rayDistance = 2;

    Camera cam;
    [HideInInspector]
    public TrafficController trafficController;

    public bool freeTraffic;
    [Serializable]
    public class UpgradeClass
    {
        public string upgradeName;
        public int upgradeLevel;
        public int maxLevel = -1;
        public float baseValue;
        public float increment;
        public float expoRatio;
        public float startValue;
        public float incrementValue;
        public int Cost(int value)
        {
            if (value == 0)
                return Mathf.RoundToInt(baseValue + increment * upgradeLevel + upgradeLevel * ((upgradeLevel + 1) / 2) * expoRatio);
            return Mathf.RoundToInt(baseValue + increment * upgradeLevel * expoRatio);
        }
        public float Value() { return startValue + incrementValue * upgradeLevel; }
        public bool Max() { return maxLevel == upgradeLevel; }
    }
    public UpgradeClass[] upgrades;

    [Serializable]
    public class CarClass
    {
        public string carName;
        public Car carPrefab;
        public int carLevel;
        public int carValue;
        public List<Car> cars =new List<Car>();
    }
    public CarClass[] cars;

    [Serializable]
    public class MergeClass
    {
        public string mergeName;
        public int mergeLevel;
        public float baseValue;
        public float increment;
        public float expoRatio;
        public int Cost() { return Mathf.RoundToInt(baseValue + increment * mergeLevel + mergeLevel * ((mergeLevel + 1) / 2) * expoRatio); }
    }
    public MergeClass merge;
    
    
    void Awake()
    {
        Application.targetFrameRate = 60;
        SetObjects();
        GetSaves();
        CalculateMerge();
    }

    
    
    void SetObjects()
    {
        if (!FindObjectOfType<Level>())
            Instantiate(levels[PlayerPrefs.GetInt("Level") % levels.Count]);
        trafficController = FindObjectOfType<TrafficController>();
        
    }

    public void GetSaves()
    {
        gold = PlayerPrefs.GetInt("Gold");
        UIManager.Instance.goldText.text = "" + gold;
        for (int a = 0; a < upgrades.Length; a++)
            upgrades[a].upgradeLevel = PlayerPrefs.GetInt(upgrades[a].upgradeName);
        merge.mergeLevel = PlayerPrefs.GetInt(merge.mergeName);
        UIManager.Instance.UpdateEconomyUI();
    }

    void CalculateMerge()
    {
        cars[0].carLevel = 100;

        for (int a = 1; a < cars.Length; a++)
        {
            cars[a].carLevel = 0;
        }
        int carIndex = 0;
        for (int b = 0; b < merge.mergeLevel; b++)
        {
            cars[carIndex].carLevel -= 1;
            cars[carIndex+1].carLevel += 1;
            if (cars[carIndex].carLevel <= 0)
                carIndex += 1;
        }
    }
    
    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine("SpeedUpRoutine");
        //Analytics.Instance.SendLevelStart((PlayerPrefs.GetInt("Level") + 1));
        UIManager.Instance.panels[3].Show();
        UIManager.Instance.panels[4].Hide();
        Destroy(FindObjectOfType<InputPanel>().GetComponent<EventTrigger>());
        InputPanel.Instance.OnPointerDownEvent.AddListener(SpeedUp);
        trafficController.StartCoroutine("StartTrafficRoutine");
    }

#region SpeedUp

    public void SpeedUp()
    {
        Taptic.Light();
        StopCoroutine("SpeedUpCoolDown");
        simulationSpeed = speedUpMultiplier * speedUp;
        Time.timeScale = simulationSpeed;
        StartCoroutine("SpeedUpCoolDown");
    }

    IEnumerator SpeedUpCoolDown()
    {
        yield return new WaitForSeconds(speedUpTimer);
        simulationSpeed = speedUp;
    }

    public IEnumerator SpeedUpRoutine()
    {
        while (true)
        {
            Time.timeScale = Mathf.Lerp(Time.timeScale, simulationSpeed, 0.05f);
            yield return null;
        }
    }
    
#endregion

#region HelperUpdate

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            gold += 1000;
            PlayerPrefs.SetInt("Gold", gold);
            UIManager.Instance.goldText.text = "" + gold;
            UIManager.Instance.UpdateEconomyUI();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Application.LoadLevel(1);
        }

        if (Input.GetKeyDown(KeyCode.S))
            simulationSpeed = 10;
        else if (Input.GetKeyUp(KeyCode.S))
            simulationSpeed = 1;
    }

#endregion

#region Sounds

    /*
      [Serializable]
      public class SoundClass
      {
  
          public AudioClip sound;
          [Range(0f, 1f)] public float volume = 1;
  
      }
  
      public SoundClass[] sounds;
  
      public void PlaySound(int value)
      {
          if (sounds.Length > 0)
              GetComponent<AudioSource>().PlayOneShot(sounds[value].sound, sounds[value].volume);
      }
      */  

#endregion
    
    public void Win()
    {
        //Analytics.Instance.SendLevelComplete((PlayerPrefs.GetInt("Level") + 1));
        UIManager.Instance.panels[1].Show();
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
    }

    public void IncreaseMoney(Car car,Vector3 position)
    {
        int value = cars[car.carIndex].carValue * (int)upgrades[2].Value();
        gold += Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        UIManager.Instance.CreateText(value, position);
        UIManager.Instance.UpdateEconomyUI();
    }

    public void IncreaseCarCount()
    {
        //PlaySound(1);
        Taptic.Medium();
        gold -= upgrades[0].Cost(0);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[0].upgradeLevel += 1;
        PlayerPrefs.SetInt(upgrades[0].upgradeName, upgrades[0].upgradeLevel);
        UIManager.Instance.UpdateEconomyUI();
    }

    public void IncreaseSize()
    {
        //PlaySound(1);
        Taptic.Medium();
        gold -= upgrades[1].Cost(1);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[1].upgradeLevel += 1;
        PlayerPrefs.SetInt(upgrades[1].upgradeName, upgrades[1].upgradeLevel);
        UIManager.Instance.UpdateEconomyUI();
        //ChangeRoads();
    }

    public void IncreaseIncome()
    {
        //PlaySound(1);
        Taptic.Medium();
        gold -= upgrades[2].Cost(2);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[2].upgradeLevel += 1;
        PlayerPrefs.SetInt(upgrades[2].upgradeName, upgrades[2].upgradeLevel);
        UIManager.Instance.UpdateEconomyUI();

    }

    public void Merge()
    {
        //PlaySound(1);
        Taptic.Medium();
        gold -= merge.Cost();
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        merge.mergeLevel += 1;
        PlayerPrefs.SetInt(merge.mergeName, merge.mergeLevel);
        CalculateMerge();
        UIManager.Instance.UpdateEconomyUI();
    }

    public bool CanMerge()
    {
        for (int a = 0; a < cars.Length-1; a++)
        {
            if (cars[a].cars.Count >= 3)
                return true;
        }

        return false;
    }
    
   
}