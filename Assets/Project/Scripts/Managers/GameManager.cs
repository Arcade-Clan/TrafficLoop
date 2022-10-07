using System;
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
    public float carProduction = 1f;
    public Car[] carPrefabs;
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
        public float[] upgradeValue;
        public int[] upgradeCost;
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI levelText;
        public GameObject upgradePanel;
        public Image coverImage;
        public int Cost() { return upgradeCost[upgradeLevel]; }
        public float Value() { return upgradeValue[upgradeLevel]; }
        public bool Max() { return upgradeValue.Length - 1 == upgradeLevel; }
    }
    public UpgradeClass[] upgrades;


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

    private void Awake()
    {
        Application.targetFrameRate = 60;
        SetObjects();
        GetSaves();
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
        UpdateEconomy();
    }


    private void Start()
    {
        StartGame();
        RefreshStats();
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
    
    void UpdateEconomy()
    {
        for (int a = 0; a < upgrades.Length; a++)
        {
            if (upgrades[a].Max())
            {
                upgrades[a].upgradePanel.Hide();
                upgrades[a].coverImage.Hide();
            }
            else
            {
                upgrades[a].goldText.text = "" + upgrades[a].Cost();
                upgrades[a].levelText.text = "LEVEL " + (upgrades[a].upgradeLevel + 1);
                upgrades[a].coverImage.gameObject.SetActive(upgrades[a].Cost() >= gold);
            }
        }
    }



    public void RefreshStats()
    { 
        //carSpeed = upgrades[0].Value();
    }

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
    



    public void Win()
    {
        //Analytics.Instance.SendLevelComplete((PlayerPrefs.GetInt("Level") + 1));
        UIManager.Instance.panels[1].Show();
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
    }

    public void IncreaseMoney(Car car,Vector3 position)
    {
        int value = car.value * (int)upgrades[2].Value();
        gold += Mathf.RoundToInt(value);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        UIManager.Instance.CreateText(value, position);
        UpdateEconomy();
    }

    public void IncreaseStat(int value)
    {
        //PlaySound(1);
        Taptic.Medium();
        gold -= upgrades[value].Cost();
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[value].upgradeLevel += 1;
        PlayerPrefs.SetInt(upgrades[value].upgradeName, upgrades[value].upgradeLevel);
        UpdateEconomy();
        RefreshStats();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            gold += 1000;
            PlayerPrefs.SetInt("Gold", gold);
            UIManager.Instance.goldText.text = "" + gold;
            UpdateEconomy();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            Application.LoadLevel(0);
        }

        if (Input.GetKeyDown(KeyCode.S))
            simulationSpeed = 10;
        else if (Input.GetKeyUp(KeyCode.S))
            simulationSpeed = 1;
    }
}