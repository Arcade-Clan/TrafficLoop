using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;
using Random = UnityEngine.Random;

public class Car : MonoBehaviour
{
    public float place;
    public float currentSpeed = 1;
    TrafficController.RoadClass road;
    public Transform rayPoint;
    public Transform[] wheels;
    Vector3 wheelRotation;
    public Path path;
    
    
    public void MoveCar(TrafficController.RoadClass newRoad)
    {
        road = newRoad;
        path = newRoad.startPath;
        StartCoroutine("MoveRoutine");
    }

    IEnumerator MoveRoutine()
    {
        while (true)
        {
            Car forwardCar = CheckRay();
            if (place >= path.pathLength && !path.canPass)
                currentSpeed = 0;
            else if (path.pathLength - place <= GameManager.Instance.rayDistance && !path.canPass)
                currentSpeed = Mathf.Lerp(currentSpeed, GameManager.Instance.carSpeed * 0.3f, GameManager.Instance.slowStrength);
            else if(forwardCar)
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            else
                currentSpeed = Mathf.Lerp(currentSpeed, GameManager.Instance.carSpeed, GameManager.Instance.slowStrength);
            place += currentSpeed / 60;
            Vector3 newPosition = path.tween.PathGetPoint(place / path.pathLength);
            Rotate(newPosition);
            transform.position = newPosition;
            if (place>=path.pathLength)
            {
                if (path.endPath)
                {
                    Destroy(gameObject);
                    yield break;
                }
                if(path.canPass)
                {
                    place = 0;
                    path = road.followPaths.GetRandom();
                }
            }

            yield return new WaitForFixedUpdate();
        }
    }

    Car CheckRay()
    {
        Physics.SphereCast(rayPoint.position,0.25f, rayPoint.forward, out RaycastHit hit, GameManager.Instance.rayDistance, LayerMask.GetMask("Car"));
        if (hit.transform)
        {
            Debug.DrawLine(rayPoint.position, rayPoint.position+rayPoint.forward* GameManager.Instance.rayDistance, Color.red);
            return hit.transform.GetComponent<Car>();
        }

        Debug.DrawLine(rayPoint.position, rayPoint.position + rayPoint.forward * GameManager.Instance.rayDistance, Color.green);
        return null;
    }
    
    public void Rotate(Vector3 newPosition)
    {
        wheelRotation += new Vector3(Vector3.Distance(newPosition,transform.position) * 150f, 0, 0);
        for (int a = 0; a < wheels.Length; a++)
            wheels[a].transform.localEulerAngles = wheelRotation;
        if (newPosition == transform.position)
            return;
        float angle = Vector3.SignedAngle((newPosition - transform.position).normalized, transform.forward, Vector3.up);
        wheels[0].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle*2, wheelRotation.z);
        wheels[1].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle*2, wheelRotation.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newPosition - transform.position), 0.25f);
    }
    
}