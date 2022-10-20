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
    public List<Path> loopingPaths;
    
    
    IEnumerator StartTrafficRoutine()
    {
        yield return null;
        paths = FindObjectsOfType<Path>();
        StartCoroutine("CreateCarRoutine");
        StartCoroutine("GreenlightPathRoutine");
    }

    public IEnumerator RecalculateTrafficElements()
    {
        for (int a = 0; a < GameManager.Instance.cars.Length; a++)
        {
            for (int b = 0; b < GameManager.Instance.cars[a].cars.Count; b++)
            {
                if (!GameManager.Instance.cars[a].cars[b].path.gameObject.activeInHierarchy)
                {
                    GameManager.Instance.cars[a].cars[b].GetComponentInChildren<Collider>().enabled = false;
                    GameManager.Instance.cars[a].cars[b].collidedCar = null;
                    GameManager.Instance.cars[a].cars[b].trafficLight = null;
                    //GameManager.Instance.cars[a].cars[b].transform.DOScale(0,3f).SetEase(Ease.Linear);
                }
            }
        }

        yield return null;
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
            CreateCar();
            yield return new WaitForSeconds(GameManager.Instance.baseSecondCreation / GameManager.Instance.TotalCarCount());
            while (GameManager.Instance.stopCarCreationOnTrafficDensity < GameManager.Instance.trafficDensity)
                yield return null;
        }
    }

    int carCounter = 0;
    
    void CreateCar()
    {

        if (loopingPaths.Count == 0)
        {
            loopingPaths = new List<Path>(paths);
            loopingPaths.Shuffle();
        }
        Path selectedPath = null;
        int pathIndex = 0;
        do
        {
           
            if (!Physics.CheckSphere(loopingPaths[pathIndex].tween.PathGetPoint(0), 5, LayerMask.GetMask("Car")))
                selectedPath = loopingPaths[pathIndex];
            pathIndex ++;
        }
        while (!selectedPath && pathIndex<loopingPaths.Count);

        if (!selectedPath)
            return;
        loopingPaths.Remove(selectedPath);
        Vector3 newPosition = selectedPath.tween.PathGetPoint(0);
        
        carCounter = (carCounter + 1) % GameManager.Instance.carProductionIndex.Count;
        int carIndex = GameManager.Instance.carProductionIndex[carCounter];
        Car newCar = Instantiate(GameManager.Instance.carPrefab, newPosition,
            Quaternion.LookRotation(selectedPath.tween.PathGetPoint(0.01f) - newPosition));
        GameManager.Instance.cars[carIndex].cars.Add(newCar);
        newCar.MoveCar(carIndex,selectedPath);
        UIManager.Instance.UpdateEconomyUI();
    }
}