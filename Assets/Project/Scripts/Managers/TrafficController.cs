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
    public bool freeTraffic;
    
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
        if(freeTraffic)
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
        while (true)
        {
            pathIndex = (pathIndex + 1) % roads.Count;
            roads[pathIndex].startPath.Pass();
            yield return new WaitForSeconds(roads[pathIndex].startPath.lightTimer);
            roads[pathIndex].startPath.Stop();
        }
    }
    
    IEnumerator CreateCarRoutine()
    {
        while (true)
        {
            CreateCar();
            yield return new WaitForSeconds(GameManager.Instance.carProduction);
        }
    }

    void CreateCar()
    {
        RoadClass randomRoad = roads.GetRandom();
        Vector3 newPosition = randomRoad.startPath.tween.PathGetPoint(0);
        Car newCar = Instantiate(GameManager.Instance.carPrefabs.GetRandom(), newPosition,
            Quaternion.LookRotation(randomRoad.startPath.tween.PathGetPoint(0.01f) - newPosition));
        newCar.MoveCar(randomRoad);
    }
}