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

    
    void Start()
    {
        
        paths = FindObjectsOfType<Path>();
    }
    
    IEnumerator StartTrafficRoutine()
    {
        yield return null;
        StartCoroutine("CreateCarRoutine");
        StartCoroutine("GreenlightPathRoutine");
    }

    public void RecalculateTrafficElements()
    {
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
            yield return new WaitForSeconds(60f/ GameManager.Instance.upgrades[0].Value());
            while (GameManager.Instance.stopCarCreationOnTrafficDensity < GameManager.Instance.trafficDensity)
                yield return null;
        }
    }

    void CreateCar()
    {
        int percentage = Random.Range(0, 101);
        int increment = 0;
        int randomIndex = 0;
        for (int a = 0; a < GameManager.Instance.cars.Length; a++)
        {
            if (percentage >= increment && percentage <= increment + GameManager.Instance.cars[a].carLevel)
            {
                randomIndex = a;
                break;
            }
            increment += GameManager.Instance.cars[a].carLevel;
        }
        Path randomPath = paths.GetRandom();
        Vector3 newPosition = randomPath.tween.PathGetPoint(0);
        Car newCar = Instantiate(GameManager.Instance.cars[randomIndex].carPrefab, newPosition,
            Quaternion.LookRotation(randomPath.tween.PathGetPoint(0.01f) - newPosition));
        GameManager.Instance.cars[randomIndex].cars.Add(newCar);
        newCar.MoveCar(randomPath);
        UIManager.Instance.UpdateEconomyUI();
    }
}