using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollision : MonoBehaviour
{

    public float forwardForce = 100;
    
    void OnTriggerEnter(Collider other)
    {
        Car car = other.GetComponentInParent<Car>();
        if (!car.GetComponent<Rigidbody>().isKinematic)
            return;
        car.StopAllCoroutines();
        Rigidbody rb = car.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        car.gameObject.layer = LayerMask.NameToLayer("CrashLayer");
        rb.AddForce(car.transform.forward* forwardForce);
    }
}