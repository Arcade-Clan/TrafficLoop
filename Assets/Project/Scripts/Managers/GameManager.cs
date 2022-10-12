using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityExtensions;

public class GameManager : MonoSingleton<GameManager>
{

    public int gold;

    public float slowStrength = 0.1f;
    public float rotationSlowDownStrength = 5;
    public float carSpeed;
    [ReadOnly]
    public float simulationSpeed =1;
    float speedUp = 1;
    public float speedUpTimer = 0.5f;
    public float speedUpMultiplier = 2f;
    public float rayDistance = 2;

    [ReadOnly]
    public float trafficDensity;

    public float stopCarCreationOnTrafficDensity = 0.5f;
    
    Camera cam;
    [HideInInspector]
    public TrafficController trafficController;
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
    public GameObject crashSmoke;

    void Awake()
    {
        Application.targetFrameRate = 60;
        SetObjects();
        GetSaves();
        LevelManager.Instance.CreateLevel();
        CalculateMerge();
        StartCoroutine("GetStatsRoutine");
    }


    void SetObjects()
    {
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
        for (int a = 0; a < cars.Length; a++)
        {
            for (int b = 0; b < cars[a].cars.Count; b++)
                cars[a].cars[b].trail.Show();
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
                cars[a].cars[b].trail.Hide();
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

#region HelperUpdate

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)||Input.GetMouseButtonDown(1))
        {
            gold += 1000;
            PlayerPrefs.SetInt("Gold", gold);
            UIManager.Instance.goldText.text = "" + gold;
            UIManager.Instance.UpdateEconomyUI();
        }

        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(3))
        {
            PlayerPrefs.DeleteAll();
            Application.LoadLevel(0);
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(2))
            simulationSpeed = 10;
        else if (Input.GetKeyUp(KeyCode.S) || Input.GetMouseButtonUp(2))
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
        LevelManager.Instance.SwitchLevel();
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
        MergeCars();
    }

    void MergeCars()
    {
        int carIndex = 0;
        for (int a = 0; a < cars.Length - 1; a++)
        {
            if (cars[a].cars.Count >= 3)
            {
                carIndex = a;
                break;
            }
        }
        Car[] closestCars = cars[carIndex].cars.OrderBy(p => Vector3.Distance(p.transform.position,Vector3.zero)).ToArray();
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
        closestCars[1].transform.DOLocalRotate(Vector3.zero, 0.5f);
        closestCars[2].transform.DOLocalRotate(Vector3.zero, 0.5f);
        closestCars[1].transform.DOLocalMoveX(0, 0.5f);
        closestCars[1].transform.DOLocalMoveZ(0, 0.5f);
        closestCars[2].transform.DOLocalMoveX(0, 0.5f);
        closestCars[2].transform.DOLocalMoveZ(0, 0.5f);
        closestCars[1].transform.DOLocalMoveY(3, 0.25f).SetEase(Ease.OutSine);
        yield return closestCars[2].transform.DOLocalMoveY(10, 0.25f).SetEase(Ease.OutSine).WaitForCompletion();
        closestCars[1].transform.DOLocalMoveY(0, 0.25f).SetEase(Ease.InSine);
        yield return closestCars[2].transform.DOLocalMoveY(0, 0.25f).SetEase(Ease.InSine).WaitForCompletion();
        Destroy(closestCars[1].gameObject);
        Destroy(closestCars[2].gameObject);
        closestCars[0].UpgradeCar();
        cars[closestCars[0].carIndex].cars.Add(closestCars[0]);
        Taptic.Medium();
        closestCars[0].cars[closestCars[0].carIndex].model.transform.DOScale(1.5f, 0.25f).SetEase(Ease.OutSine).OnComplete(()=>
            closestCars[0].cars[closestCars[0].carIndex].model.transform.DOScale(1f, 0.25f).SetEase(Ease.InSine));
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
                UIManager.Instance.trafficDensity.fillAmount =
                    Mathf.Lerp(UIManager.Instance.trafficDensity.fillAmount, trafficDensity, 0.1f);
            }
            yield return new WaitForFixedUpdate();
        }

    }
   
}