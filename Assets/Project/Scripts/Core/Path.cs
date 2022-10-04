using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;

public class Path : MonoBehaviour
{
    public DOTweenPath path;
    public float pathLength;
    public List<Car> cars = new ();
    public Tween tween;
    public bool endPath;
    void Start()
    {
        tween = path.GetTween();
        pathLength = tween.PathLength();
    }
    
    void SendCars()
    {
        float position = pathLength;
        for (int a = 0; a < cars.Count; a++)
        {
            if(a!=0)
                position -= cars[a-1].size;
            cars[a].Move(this, position,a);
        }
    }
    
    public void CreateCar()
    {
        Vector3 newPosition = path.tween.PathGetPoint(0);
        Car newCar = Instantiate(GameManager.Instance.carPrefabs.GetRandom(), newPosition, Quaternion.LookRotation(path.tween.PathGetPoint(0.01f) - newPosition));
        cars.Add(newCar);
        SendCars();
    }

    public void RemoveCar(Car car)
    {
        cars.Remove(car);
        SendCars();
    }

    public void DestroyCar(Car car)
    {
        cars.Remove(car);
        Destroy(car.gameObject);
        SendCars();
    }
    
    public void AddCar(Car car)
    {
        car.place = 0;
        car.arrived = false;
        cars.Add(car);
        SendCars();
    }
}