using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    IEnumerator Start()
    {
        yield return null;
        StartCoroutine("CreateCarRoutine");
        StartCoroutine("GreenlightCarRoutine");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            roads.GetRandom().startPath.CreateCar();
        if (Input.GetMouseButtonDown(1))
            SendARandomCar();
    }

    IEnumerator CreateCarRoutine()
    {
        while (true)
        {
            roads.GetRandom().startPath.CreateCar();
            yield return new WaitForSeconds(GameManager.Instance.carProduction);
        }
    }

    IEnumerator GreenlightCarRoutine()
    {
        while (true)
        {
            SendARandomCar();
            yield return new WaitForSeconds(GameManager.Instance.greenlightPermission);
        }
    }
    
    void SendARandomCar()
    {
        int randomStart = Random.Range(0,roads.Count);
        RoadClass selectedRoad = null;
        for (int a = randomStart; a < randomStart+roads.Count; a++)
        {
            if (roads[randomStart].startPath.cars.Count != 0 && roads[randomStart].startPath.cars.First().arrived)
            {
                selectedRoad = roads[randomStart];
                break;  
            }
        }

        if (selectedRoad==null)
            return;
        Car firstCar = selectedRoad.startPath.cars.First();
        selectedRoad.startPath.RemoveCar(firstCar);
            selectedRoad.followPaths.GetRandom().AddCar(firstCar);
    }
}