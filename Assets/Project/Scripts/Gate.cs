using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.IncreaseMoney(other.transform.parent.GetComponent<Car>(),transform.position+Vector3.up);
    }

}