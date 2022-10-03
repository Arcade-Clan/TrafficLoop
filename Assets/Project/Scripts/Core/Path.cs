using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Path : MonoBehaviour
{
    public DOTweenPath path;
    public float pathLength;
    public List<Car> cars = new ();
    public Tween tween;
    public Car carPrefab;
    
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
            position -= cars[a].size;
            cars[a].UpdateCar(this, position);
        }
    }
    
    public void CreateCar()
    {
        Vector3 newPosition = path.tween.PathGetPoint(0);
        Car newCar = Instantiate(carPrefab, newPosition, Quaternion.LookRotation(path.tween.PathGetPoint(0.01f) - newPosition));
        cars.Add(newCar);
        SendCars();
    }

    public void RemoveCar(Car car)
    {
        cars.Remove(car);
        SendCars();
    }

    public void AddCar(Car car)
    {
        car.position = 0;
        cars.Add(car);
        SendCars();
    }
}