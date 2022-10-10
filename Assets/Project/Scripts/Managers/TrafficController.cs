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

    public List<Path> paths;


    void Awake()
    {
        AddRoads();
    }

    void AddRoads()
    {
        GameObject[] newPaths = GameObject.FindGameObjectsWithTag("StartPath");
        for (int a = 0; a < newPaths.Length; a++)
        {
            paths.Add(newPaths[a].GetComponent<Path>());
        }

    }
    
    IEnumerator StartTrafficRoutine()
    {
        yield return null;
        StartCoroutine("CreateCarRoutine");
        if(GameManager.Instance.freeTraffic)
        {
            foreach (Path t in paths)
                t.Pass();
        }
        else
            StartCoroutine("GreenlightPathRoutine");
    }

    IEnumerator GreenlightPathRoutine()
    {
        int pathIndex = 0;
        bool checkForLights = false;
        float timer = 0;
        while (true)
        {
            checkForLights = false;
            for (int a = 0; a < paths.Count; a++)
            {
                if(paths[a].trafficIndex ==pathIndex)
                {
                    paths[a].Pass();
                    timer = paths[a].lightTimer;
                    checkForLights = true;
                }
                else
                    paths[a].Stop();
            }

            if (!checkForLights)
            {
                pathIndex = 0;
                for (int a = 0; a < paths.Count; a++)
                {
                    if (paths[a].trafficIndex == pathIndex)
                    {
                        paths[a].Pass();
                        timer = paths[a].lightTimer;
                    }
                    else
                        paths[a].Stop();
                }
            }

            yield return new WaitForSeconds(timer);
            pathIndex += 1;  
        }
    }
    
    IEnumerator CreateCarRoutine()
    {
        while (true)
        {
            CreateCar();
            yield return new WaitForSeconds(60f/ GameManager.Instance.upgrades[0].Value());
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