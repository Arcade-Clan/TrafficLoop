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

public class GameManager : MonoSingleton<GameManager>
{

    public int gold;

    public float slowStrength = 0.1f;

    public float startMoveStrength = 0.05f;
    //public float rotationSlowDownStrength = 5;
    public float carSpeed;
    [ReadOnly] public float simulationSpeed = 1;
    float speedUp = 1;
    public float speedUpTimer = 0.5f;
    public float speedUpMultiplier = 2f;
    public float rayDistance = 2;

    [ReadOnly] public float trafficDensity;

    public float stopCarCreationOnTrafficDensity = 0.5f;
    [HideInInspector] public Canvas canvas;
    [HideInInspector] public Camera cam;
    [HideInInspector] public TrafficController trafficController;

    public float baseSecondCreation = 60;
    
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
                return upgradeLevel == LevelManager.Instance.level.sections.Length-1;
            return false;
        }

    }

    public UpgradeClass[] upgrades;

    [Serializable]
    public class CarClass
    {
        public string carName;
        [ReadOnly]
        public int carLevel;
        public int carValue;
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
    public GameObject crashSmoke;
    public List<int> carProductionIndex = new List<int>();
    public Car carPrefab;
    public int fireTruckComesAfterAmount = 10;

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
    }

    void Start()
    {
        SetObjects();
        GetSaves();
        LevelManager.Instance.CreateLevel();
        CreateProductionIndex();
        UIManager.Instance.UpdateEconomyUI();
        StartCoroutine("GetStatsRoutine");
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
        UIManager.Instance.goldText.text = "" + gold;
        for (int a = 0; a < upgrades.Length; a++)
            upgrades[a].upgradeLevel = PlayerPrefs.GetInt(upgrades[a].upgradeName);
        merge.mergeLevel = PlayerPrefs.GetInt(merge.mergeName);
        cars[0].carLevel = PlayerPrefs.GetInt(cars[0].carName, Mathf.RoundToInt(upgrades[0].Value()));
        for (int a = 1; a < cars.Length; a++)
            cars[a].carLevel = PlayerPrefs.GetInt(cars[a].carName);
    }

    void CreateProductionIndex()
    {
        carProductionIndex.Clear();
        for (int a = 0; a < cars.Length; a++)
        {
            for (int b = 0; b < cars[a].carLevel; b++)
                carProductionIndex.Add(a);
        }
        if(carProductionIndex.Count > fireTruckComesAfterAmount)
            carProductionIndex.Add(10);
        carProductionIndex.Shuffle();
    }
    


    public void StartGame()
    {
        StartCoroutine("SpeedUpRoutine");
        //Analytics.Instance.SendLevelStart((PlayerPrefs.GetInt("Level") + 1));
        //UIManager.Instance.panels[3].Show();
        //UIManager.Instance.panels[4].Hide();
        Destroy(FindObjectOfType<InputPanel>().GetComponent<EventTrigger>());
        InputPanel.Instance.OnPointerDownEvent.AddListener(SpeedUp);
        trafficController.StartCoroutine("StartTrafficRoutine");
    }


    
    public void Win()
    {
        //Analytics.Instance.SendLevelComplete((PlayerPrefs.GetInt("Level") + 1));
        //UIManager.Instance.panels[1].Show();
        PlayerPrefs.SetInt("Level", PlayerPrefs.GetInt("Level") + 1);
    }

    public void IncreaseMoney(Car car,Vector3 position)
    {
        if (UIManager.Instance.tutorialInProgress)
            return;
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
        if (!UIManager.Instance.IncreaseTutorialProgression(0))
            return;
        PlaySound(0);
        UIManager.Instance.upgrades[0].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.4f, 0.2f)
            .SetEase(Ease.OutSine).OnComplete(() =>
                UIManager.Instance.upgrades[0].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.15f, 0.2f)
                    .SetEase(Ease.InSine));
        Analytics.Instance.SendCarBought();
        Taptic.Medium();
        gold -= upgrades[0].Cost(0);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[0].upgradeLevel += 1;
		trafficController.AddCar();
        PlayerPrefs.SetInt(upgrades[0].upgradeName, upgrades[0].upgradeLevel);
        cars[0].carLevel += 1; 
        PlayerPrefs.SetInt(cars[0].carName, cars[0].carLevel);
        CreateProductionIndex();
        UIManager.Instance.UpdateEconomyUI();
    }

    public void IncreaseSize()
    {
        //PlaySound(1);
        if (!UIManager.Instance.IncreaseTutorialProgression(4))
            return;
        PlaySound(0);
        UIManager.Instance.upgrades[1].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.4f, 0.2f)
            .SetEase(Ease.OutSine).OnComplete(() =>
                UIManager.Instance.upgrades[1].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.15f, 0.2f)
                    .SetEase(Ease.InSine));
        Analytics.Instance.SendSizeUp();
        Taptic.Medium();
        gold -= upgrades[1].Cost(1);
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;
        upgrades[1].upgradeLevel += 1;
        PlayerPrefs.SetInt(upgrades[1].upgradeName, upgrades[1].upgradeLevel);
        LevelManager.Instance.SwitchLevel();
        UIManager.Instance.UpdateEconomyUI();
    }

    public void IncreaseIncome()
    {
        if (!UIManager.Instance.IncreaseTutorialProgression(2))
            return;
        PlaySound(0);
        UIManager.Instance.upgrades[2].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.4f, 0.2f)
            .SetEase(Ease.OutSine).OnComplete(() =>
                UIManager.Instance.upgrades[2].coverImage.transform.parent.GetComponent<Button>().transform.DOScale(1.15f, 0.2f)
                    .SetEase(Ease.InSine));
        Analytics.Instance.SendIncomeClicked();
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

        if (!UIManager.Instance.IncreaseTutorialProgression(3))
            return;
        PlaySound(0);
        Taptic.Medium();
        gold -= merge.Cost();
        PlayerPrefs.SetInt("Gold", gold);
        UIManager.Instance.goldText.text = "" + gold;

        merge.mergeLevel += 1;
        Analytics.Instance.SendMergeLevel(merge.mergeLevel);
        PlayerPrefs.SetInt(merge.mergeName, merge.mergeLevel);
        MergeCars();
    }

    void MergeCars()
    {
        int carIndex = 0;
        for (int a = 0; a < cars.Length - 1; a++)
        {
            if (cars[a].carLevel >= 3 && cars[a].cars.Count >= 3)
            {
                carIndex = a;
                break;
            }
        }
        Car[] closestCars = cars[carIndex].cars.OrderBy(p => Vector3.Distance(p.transform.position,Vector3.zero)).ToArray();
        cars[carIndex].carLevel -= 3;
        cars[carIndex+1].carLevel += 1;
        PlayerPrefs.SetInt(cars[carIndex].carName, cars[carIndex].carLevel);
        PlayerPrefs.SetInt(cars[carIndex+1].carName, cars[carIndex+1].carLevel);
        CreateProductionIndex();
        StartCoroutine(MergeCarsRoutine(closestCars));
    }

    IEnumerator MergeCarsRoutine(Car[] closestCars)
    {
        
        cars[closestCars[0].carIndex].cars.Remove(closestCars[0]);
        cars[closestCars[1].carIndex].cars.Remove(closestCars[1]);
        cars[closestCars[2].carIndex].cars.Remove(closestCars[2]);
        UIManager.Instance.UpdateEconomyUI();
        closestCars[1].StopAllCoroutines();
        closestCars[2].StopAllCoroutines();
        closestCars[1].GetComponentInChildren<Collider>().enabled = false;
        closestCars[2].GetComponentInChildren<Collider>().enabled = false;
        closestCars[1].transform.SetParent(closestCars[0].transform);
        closestCars[2].transform.SetParent(closestCars[0].transform);
        closestCars[1].transform.DOLocalRotate(Vector3.zero, 0.66f);
        closestCars[2].transform.DOLocalRotate(Vector3.zero, 0.66f);
        closestCars[1].transform.DOLocalMoveX(0, 0.66f);
        closestCars[1].transform.DOLocalMoveZ(0, 0.66f);
        closestCars[2].transform.DOLocalMoveX(0, 0.66f);
        closestCars[2].transform.DOLocalMoveZ(0, 0.66f);
        closestCars[1].transform.DOLocalMoveY(7, 0.33f).SetEase(Ease.OutSine);
        yield return closestCars[2].transform.DOLocalMoveY(7, 0.33f).SetEase(Ease.OutSine).WaitForCompletion();
        closestCars[1].transform.DOLocalMoveY(0, 0.33f).SetEase(Ease.InSine);
        yield return closestCars[2].transform.DOLocalMoveY(0, 0.33f).SetEase(Ease.InSine).WaitForCompletion();
        Destroy(closestCars[1].gameObject);
        Destroy(closestCars[2].gameObject);
        closestCars[0].UpgradeCar();
        cars[closestCars[0].carIndex].cars.Add(closestCars[0]);
        Taptic.Medium();
        float carScale = closestCars[0].cars[closestCars[0].carIndex].transform.localScale.x;
        closestCars[0].cars[closestCars[0].carIndex].transform.DOScale(carScale*2, 0.25f).SetEase(Ease.OutSine).OnComplete(()=>
            closestCars[0].cars[closestCars[0].carIndex].transform.DOScale(carScale, 0.25f).SetEase(Ease.InSine));
        UIManager.Instance.UpdateEconomyUI();
    }
    
    public bool CanMerge()
    {
        for (int a = 0; a < cars.Length-1; a++)
        {
            if (cars[a].carLevel>=3 &&cars[a].cars.Count >= 3)
                return true;
        }

        return false;
    }

    IEnumerator GetStatsRoutine()
    {
        while(true)
        {
            List<Car> calculatedCars = new();
            for (int a = 0; a < cars.Length; a++)
                calculatedCars.AddRange(cars[a].cars);
            UIManager.Instance.carAmount.text = "" + calculatedCars.Count;
            UIManager.Instance.carAmountPerMinute.text = "" + upgrades[0].Value();
            UIManager.Instance.incomePerMinute.text = "" + upgrades[0].Value() * upgrades[2].Value();

            float density = 0;
            for (int a = 0; a < calculatedCars.Count; a++)
            {
                if (calculatedCars[a].collidedCar)
                    density += 1;
            }

            if (calculatedCars.Count > 0)
            {
                trafficDensity = density / calculatedCars.Count;
                UIManager.Instance.trafficDensity.fillAmount = Mathf.Lerp(UIManager.Instance.trafficDensity.fillAmount, trafficDensity, 0.1f);
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public int TotalCarCount()
    {
        int carCount = 0;
        for (int a = 0; a < cars.Length; a++)
        {
            carCount += cars[a].carLevel;
        }

        return carCount;
    }
    
#region SpeedUp

    public void SpeedUp()
    {
        UIManager.Instance.IncreaseTutorialProgression(1);
        Analytics.Instance.SendSpeedUp();
        Taptic.Light();
        StopCoroutine("SpeedUpCoolDown");
        simulationSpeed = speedUpMultiplier * speedUp;
        Time.timeScale = simulationSpeed;
        for (int a = 0; a < cars.Length; a++)
        {
            for (int b = 0; b < cars[a].cars.Count; b++)
                cars[a].cars[b].cars[cars[a].cars[b].carIndex].trail.
                    GetComponentsInChildren<TrailRenderer>().ForEach(t => t.time = 2);
        }
        StartCoroutine("SpeedUpCoolDown");
    }

    IEnumerator SpeedUpCoolDown()
    {
        yield return new WaitForSeconds(speedUpTimer);
        simulationSpeed = speedUp;
        for (int a = 0; a < cars.Length; a++)
        {
            for (int b = 0; b < cars[a].cars.Count; b++)
                cars[a].cars[b].cars[cars[a].cars[b].carIndex].trail.
                    GetComponentsInChildren<TrailRenderer>().ForEach(t => t.time = 0.5f);
        }
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
                PlayerPrefs.SetInt("Gold", gold);
                UIManager.Instance.goldText.text = "" + gold;
                UIManager.Instance.UpdateEconomyUI();
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