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

    [Serializable]
    public class RoadClass
    {
        public Path startPath;
        public List<Path> followPaths;
    }
    public List <RoadClass> roads = new ();

    void Awake()
    {
        AddRoads();
    }

    void AddRoads()
    {
        GameObject[] startPaths = GameObject.FindGameObjectsWithTag("StartPath");
        for (int a = 0; a < startPaths.Length; a++)
        {
            roads.Add(new RoadClass());
            RoadClass currentRoad = roads.Last();
            currentRoad.startPath = startPaths[a].GetComponent<Path>();
            currentRoad.followPaths = currentRoad.startPath.GetComponentsInChildren<Path>().ToList();
            currentRoad.followPaths.Remove(currentRoad.startPath);
        }
    }
    
    IEnumerator StartTrafficRoutine()
    {
        yield return null;
        StartCoroutine("CreateCarRoutine");
        if(GameManager.Instance.freeTraffic)
        {
            foreach (RoadClass t in roads)
                t.startPath.Pass();
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
            for (int a = 0; a < roads.Count; a++)
            {
                if(roads[a].startPath.trafficIndex ==pathIndex)
                {
                    roads[a].startPath.Pass();
                    timer = roads[a].startPath.lightTimer;
                    checkForLights = true;
                }
                else
                    roads[a].startPath.Stop();
            }

            if (!checkForLights)
            {
                pathIndex = 0;
                for (int a = 0; a < roads.Count; a++)
                {
                    if (roads[a].startPath.trafficIndex == pathIndex)
                    {
                        roads[a].startPath.Pass();
                        timer = roads[a].startPath.lightTimer;
                    }
                    else
                        roads[a].startPath.Stop();
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
        RoadClass randomRoad = roads.GetRandom();
        Vector3 newPosition = randomRoad.startPath.tween.PathGetPoint(0);
        Car newCar = Instantiate(GameManager.Instance.cars[randomIndex].carPrefab, newPosition,
            Quaternion.LookRotation(randomRoad.startPath.tween.PathGetPoint(0.01f) - newPosition));
        GameManager.Instance.cars[randomIndex].cars.Add(newCar);
        newCar.MoveCar(randomRoad);
        UIManager.Instance.UpdateEconomyUI();
    }
}