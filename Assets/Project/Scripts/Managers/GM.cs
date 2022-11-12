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
    [ReadOnly] public float simulationSpeed = 1;
    public float tapSpeedUpTimer = 0.5f;
    public float tapSpeedUpMultiplier = 2f;    
    public float ignoreCarWaiter = 3;
    [HideInInspector]
    public float tapSpeed = 1f;
    
    [Header("Traffic Details")]
    public float baseSecondCreation = 60;
    [ReadOnly] public float baseSecondCreationSpeedUp = 2;
    public float baseSecondCreationSpeedUpMultiplier = 2;
    [ReadOnly] public float trafficDensity;
    public float stopCarCreationOnTrafficDensity = 0.5f;
    [FormerlySerializedAs("fireTruckComesAfterAmount")] public float specialCarRandomChance = 0.1f;
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
        public float[] upgradeValues;
        public int Cost()
        {
            if (upgradeName == "Car Amount")
                return Mathf.RoundToInt(baseValue + increment * upgradeLevel + upgradeLevel * ((upgradeLevel + 1) / 2) * expoRatio);
            if (upgradeName == "Income")
                return Mathf.RoundToInt(baseValue + increment * upgradeLevel + upgradeLevel * ((upgradeLevel + 1) / 2) * expoRatio);
            if (upgradeValues.Length > upgradeLevel)
                return Mathf.RoundToInt(upgradeValues[upgradeLevel]);
            return 100000000;
        }



        public bool Max(int value)
        {
            if (value == 1)
                return upgradeLevel >= LM.Instance.level.sections.Length-1;
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
        public bool specialCar;
        public List<Car> cars = new List<Car>();
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

    public bool tutorialOn = true;
    
    public void PlaySound(int value)
    {
        if (!Options.Instance.soundsOn)
            return;
        if (sounds.Length > 0)
            GetComponent<AudioSource>().PlayOneShot(sounds[value].sound, sounds[value].volume);
    }
    
    void Awake()
    {
        Application.targetFrameRate = 60;
        SetObjects();
        GetSaves();
        LM.Instance.CreateLevel();
        trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }
    
    void SetObjects()
    {
        cam = Camera.main;
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
        for (int a = 0; a < cars.Length; a++)
            cars[a].carLevel = PlayerPrefs.GetInt(cars[a].carName, cars[a].carLevel);
    }
    
    void Start()
    {
        StartGame();
    }
    
    public void StartGame()
    { 
        PM.Instance.StartCoroutine("GetStatsRoutine");
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
        if (Input.GetMouseButton(2))
        {
            PlayerPrefs.DeleteAll();
            Application.LoadLevel(1);
        }
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

    int value = cars[car.carIndex].carValue * Mathf.RoundToInt(AdsM.Instance.adDetails[3].multiplierValue);
    if(cars[car.carIndex].specialCar)
        value = cars[car.carIndex].carValue * merge.Cost() * Mathf.RoundToInt(AdsM.Instance.adDetails[3].multiplierValue);
    Analytics.Instance.EarnedMoney(value);
    gold += Mathf.RoundToInt(value);
    UIM.Instance.UpdateGold();
    UIM.Instance.CreateText(value, position);
    UIM.Instance.UpdateEconomyUI();
}



}