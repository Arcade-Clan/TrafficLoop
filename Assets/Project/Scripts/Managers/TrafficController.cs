using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;
using Random = UnityEngine.Random;

public class TrafficController : MonoBehaviour
{

    public Path[] paths;
    public TrafficLight[] trafficLights;
    [HideInInspector]
    public List<int> carProductionIndex = new List<int>();
    
    public void ProcessProductionIndex()
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

    public IEnumerator StartTrafficRoutine()
    {
        yield return null;
        paths = FindObjectsOfType<Path>();
        StartCoroutine("CreateCarRoutine");
        StartCoroutine("GreenlightPathRoutine");
    }

    
    
    public void AddCar(int index)
    {
        List<Path> randomPaths = new List<Path>(paths);
        randomPaths.Shuffle();

        for (int c = 3; c >= 0; c--)
        {
            foreach (var path in randomPaths)
            {
                for (int b = 0; b < 10; b++)
                {
                    float randomPosition = Random.Range(0.2f, 0.8f);
                    if (Physics.SphereCast(path.tween.PathGetPoint(randomPosition) + Vector3.up * 15, c,
                            Vector3.down, out RaycastHit hit, 20, LayerMask.GetMask("Car")))
                        continue;
                    Vector3 newPosition = path.tween.PathGetPoint(randomPosition);
                    Car newCar = Instantiate(GM.Instance.carPrefab, newPosition, 
                        Quaternion.LookRotation(path.tween.PathGetPoint(randomPosition+0.01f) - newPosition));
                    GM.Instance.cars[index].cars.Add(newCar);
                    newCar.MoveCar(index,path,randomPosition,true);
                    return;
                }
            }
        }
    }
    
    public void RecalculateTrafficElements()
    {
        for (int a = 0; a < GM.Instance.cars.Length; a++)
        {
            for (int b = 0; b < GM.Instance.cars[a].cars.Count; b++)
            {
                if (!GM.Instance.cars[a].cars[b].path.gameObject.activeInHierarchy)
                {
                    GM.Instance.cars[a].cars[b].GetComponentInChildren<Collider>().enabled = false;
                    GM.Instance.cars[a].cars[b].transform.DOScale(0, 10f).SetEase(Ease.Linear);
                    GM.Instance.cars[a].cars[b].collidedCar = null;
                    GM.Instance.cars[a].cars[b].trafficLight = null;
                   
                }
            }
        }
        
        paths = FindObjectsOfType<Path>();
        StopCoroutine("GreenlightPathRoutine");
        StartCoroutine("GreenlightPathRoutine");
    }
    
    IEnumerator GreenlightPathRoutine()
    {
        int trafficIndex = 0;
        float timer = 0;
        trafficLights = FindObjectsOfType<TrafficLight>();
        while (true)
        {
            bool checkForLights = false;
            for (int a = 0; a < trafficLights.Length; a++)
            {
                if(trafficLights[a].trafficIndex ==trafficIndex)
                {
                    trafficLights[a].Pass(true);
                    timer = trafficLights[a].timer;
                    checkForLights = true;
                }
                else
                    trafficLights[a].Pass(false);
            }

            if (!checkForLights)
            {
                trafficIndex = 0;
                for (int a = 0; a < trafficLights.Length; a++)
                {
                    if (trafficLights[a].trafficIndex == trafficIndex)
                    {
                        trafficLights[a].Pass(true);
                        timer = trafficLights[a].timer;
                    }
                    else
                        trafficLights[a].Pass(false);
                }
            }

            yield return new WaitForSeconds(timer);
            trafficIndex += 1;  
        }
    }

    IEnumerator CreateCarRoutine()
    {
        while (true)
        {
            
            CreateCar(carProductionIndex.First()+AdsM.Instance.upgradeAllCarLevel);
            yield return new WaitForSeconds(GM.Instance.baseSecondCreation/GM.Instance.baseSecondCreationSpeedUp / PM.Instance.TotalCarCount());
            while (GM.Instance.stopCarCreationOnTrafficDensity < GM.Instance.trafficDensity)
                yield return null;
        }
    }
    
    public void CreateCar(int index)
    {
        carProductionIndex.RemoveAt(0);
        if (carProductionIndex.Count == 0)
            ProcessProductionIndex();
        Path selectedPath = null;
        List<Path> loopingPaths = new List<Path>(paths);
        loopingPaths.Shuffle();
        for (int a = 0; a < loopingPaths.Count; a++)
        {
            if (Physics.CheckSphere(loopingPaths[a].tween.PathGetPoint(0), 2.5f, LayerMask.GetMask("Car")))
                continue;
            selectedPath = loopingPaths[a];  
        }
        if (!selectedPath)
            return;
        Vector3 newPosition = selectedPath.tween.PathGetPoint(0);
        
        Car newCar = Instantiate(GM.Instance.carPrefab, newPosition,
            Quaternion.LookRotation(selectedPath.tween.PathGetPoint(0.01f) - newPosition));
        GM.Instance.cars[index].cars.Add(newCar);
        newCar.MoveCar(index,selectedPath,0,false);
        UIM.Instance.UpdateEconomyUI();
    }
    
    void Update()
    {
        for (int a = 0; a <10 ; a++)
        {
            if (Input.GetKeyDown(""+a))
                CreateCar(a);
        }

        if (Input.GetKeyDown(KeyCode.T))
            CreateCar(10);
    }
}