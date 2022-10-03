using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityExtensions;

public class TrafficController : MonoBehaviour
{

    [Serializable]
    public class RoadClass
    {
        public Path startPath;
        public List<Path> followPaths;
    }
    public RoadClass[] roads;

    void Start()
    {
        //StartCoroutine("CreateCarRoutine");
        //StartCoroutine("GreenlightCarRoutine");
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
        RoadClass road = roads.GetRandom();
        Car firstCar = road.startPath.cars.First();
        if (firstCar.arrived)
        {
            road.startPath.RemoveCar(firstCar);
            road.followPaths.GetRandom().AddCar(firstCar);
        }
    }
}