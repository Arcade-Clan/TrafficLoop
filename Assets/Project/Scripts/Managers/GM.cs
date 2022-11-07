using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityExtensions;

public class GM : MonoSingleton<GM>
{

    public int gold;
    [Header("Car Details")]
    public float slowStrength = 0.1f;
    public float startMoveStrength = 0.05f;
    public float rayDistance = 2;
    public Car carPrefab;

    [Header("Time Details")]
    public float carSpeed;
    [ReadOnly] public float simulationSpeed = 1;
    public float tapSpeedUpTimer = 0.5f;
    public float tapSpeedUpMultiplier = 2f;
    [HideInInspector]
    public float tapSpeed = 1f;
    
    [Header("Traffic Details")]
    public float baseSecondCreation = 60;
    [ReadOnly] public float baseSecondCreationSpeedUp = 2;
    public float baseSecondCreationSpeedUpMultiplier = 2;
    [ReadOnly] public float trafficDensity;
    public float stopCarCreationOnTrafficDensity = 0.5f;
    public int fireTruckComesAfterAmount = 10;
    [HideInInspector] public TrafficController trafficController;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public Camera cam;



    
    [Serializable]
    public class UpgradeClass
    {
        public string upgradeName;
        public int upgradeLevel;
        public float baseValue;
        public float increment;
        public float expoRatio;
        public float startValue;
        public float incrementValue;
        public float[] upgradeValues;
        public int Cost(int value)
        {
            if (value == 0)
                return Mathf.RoundToInt(baseValue + increment * upgradeLevel + upgradeLevel * ((upgradeLevel + 1) / 2) * expoRatio);
            if(value==1&&(upgradeLevel>0 && upgradeLevel<5))
                return Mathf.RoundToInt(upgradeValues[upgradeLevel]*Analytics.Instance.multiplier);
            return Mathf.RoundToInt(upgradeValues[upgradeLevel]);
        }

        public float Value() { return startValue + incrementValue * upgradeLevel; }

        public bool Max(int value)
        {
            if (value == 1)
                return upgradeLevel == LM.Instance.level.sections.Length-1;
            return false;
        }

    }

    public UpgradeClass[] upgrades;

    [Serializable]
    public class CarClass
    {
        public string carName;
        public int carLevel;
        public int carValue;
        public List<Car> cars = new List<Car>();
    }

    public CarClass[] cars;
    public CarClass[] specialCars;
    [Serializable]
    public class MergeClass
    {
        public string mergeName;
        public int mergeLevel;
        public float baseValue;
        public float increment;
        public float expoRatio;

        public int Cost()
        {
            return Mathf.RoundToInt(baseValue + increment * mergeLevel + mergeLevel * ((mergeLevel + 1) / 2) * expoRatio);
        }
    }

    public MergeClass merge;
    //public GameObject crashSmoke;
    
   

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
    
    void Awake()
    {
        Application.targetFrameRate = 60;
        SetObjects();
        GetSaves();
    }

    void Start()
    {
        LM.Instance.CreateLevel();
        PM.Instance.CreateProductionIndex();
        UIM.Instance.UpdateEconomyUI();
        PM.Instance.StartCoroutine("GetStatsRoutine");
        StartGame();
    }
    
    void SetObjects()
    {
        cam = FindObjectOfType<Camera>();
        canvas = FindObjectOfType<Canvas>();
        trafficController = FindObjectOfType<TrafficController>();
    }

    public void GetSaves()
    {
        gold = PlayerPrefs.GetInt("Gold",3);
        UIM.Instance.UpdateGold();
        for (int a = 0; a < upgrades.Length; a++)
            upgrades[a].upgradeLevel = PlayerPrefs.GetInt(upgrades[a].upgradeName);
        merge.mergeLevel = PlayerPrefs.GetInt(merge.mergeName);
        cars[0].carLevel = PlayerPrefs.GetInt(cars[0].carName, Mathf.RoundToInt(upgrades[0].Value()));
        for (int a = 1; a < cars.Length; a++)
            cars[a].carLevel = PlayerPrefs.GetInt(cars[a].carName);
    }

   
    


    public void StartGame()
    {
        PM.Instance.StartCoroutine("TimeCalculationRoutine");
        Destroy(FindObjectOfType<InputPanel>().GetComponent<EventTrigger>());
        InputPanel.Instance.OnPointerDownEvent.AddListener(PM.Instance.SpeedUp);
        trafficController.StartCoroutine("StartTrafficRoutine");
    }
    
    public void SolveTraffic()
    {
        Application.LoadLevel(1);
    }
    
#region HelperUpdate

    void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetKey(KeyCode.M) || Input.GetMouseButton(1))
            {
                gold += 1000;
                UIM.Instance.UpdateGold();
                UIM.Instance.UpdateEconomyUI();
            }

            if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(3))
            {
                PlayerPrefs.DeleteAll();
                Application.LoadLevel(1);
            }

            if (Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(2))
                simulationSpeed = 10;
            else if (Input.GetKeyUp(KeyCode.S) || Input.GetMouseButtonUp(2))
                simulationSpeed = 1;
        }
    }

#endregion

public void IncreaseMoney(Car car,Vector3 position)
{
    if (UIM.Instance.tutorialInProgress)
        return;
    int value = cars[car.carIndex].carValue * (int)upgrades[2].Value();
    gold += Mathf.RoundToInt(value);
    UIM.Instance.UpdateGold();
    UIM.Instance.CreateText(value, position);
    UIM.Instance.UpdateEconomyUI();
}

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
    
}