using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModel : MonoBehaviour
{

    public Transform[] wheels;
    public GameObject trail;
    public float carSpeed = 12;
    public bool feverCar;
    
    void Awake()
    {
        trail = GetComponentInChildren<TrailRenderer>(true).gameObject;
    }

}