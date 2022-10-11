using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


public class Car : MonoBehaviour
{
    public float place;
    public float currentSpeed = 1;
    Path path;
    public Transform rayPoint;
    public Transform[] wheels;
    Vector3 wheelRotation;
    public int carIndex = 1;
    public TrafficLight trafficLight;
    
    void Start()
    {
        rayPointDistance = Vector3.Distance(transform.position, rayPoint.position);
        StartCoroutine("ForwardCarTimer");
    }
    
    public void MoveCar(Path newPath)
    {
        path = newPath;
        StartCoroutine("MoveRoutine");
    }

    [HideInInspector]
    public Car forwardCar;
    
    IEnumerator MoveRoutine()
    {
        while (true)
        {
            forwardCar = CheckRay();
            if (forwardCar)
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            else if (trafficLight)
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            else
                currentSpeed = Mathf.Lerp(currentSpeed, GameManager.Instance.carSpeed, GameManager.Instance.slowStrength);
            
            Vector3 rotation = path.tween.PathGetPoint((place + currentSpeed / 60) / path.pathLength);
            float angle = Vector3.Angle((rotation - transform.position).normalized, transform.forward);
            place += currentSpeed * (1 - angle / 28f) / 60;
            Vector3 newPosition = path.tween.PathGetPoint(place / path.pathLength);
            Rotate(newPosition);
            transform.position = newPosition;
            if (place>=path.pathLength)
            {
                GameManager.Instance.cars[carIndex].cars.Remove(this);
                    UIManager.Instance.UpdateEconomyUI();
                    Destroy(gameObject);
                    yield break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

   

    float rayPointDistance;
    
    Car CheckRay()
    {
        float ray = GameManager.Instance.rayDistance + rayPointDistance;
        for (int a = 1; a <= 10; a++)
        {
            Vector3 startPosition = path.tween.PathGetPoint((place + ray * (a - 1) / 10f) / path.pathLength);
            Vector3 endPosition = path.tween.PathGetPoint((place + ray * a / 10f) / path.pathLength);
            Physics.SphereCast(startPosition+Vector3.up, 0.5f, endPosition - startPosition, out RaycastHit hit, ray/10f, LayerMask.GetMask("Car"));
            
            if (hit.transform)
            {
                for (int b = 0; b < ignoredCars.Count; b++)
                {
                   // if (ignoredCars[b] && hit.transform == ignoredCars[b].transform)
                   //     return null;
                }
                Debug.DrawRay(startPosition + Vector3.up, endPosition - startPosition, Color.red);
                return hit.transform.GetComponent<Car>();
            }
            Debug.DrawRay(startPosition + Vector3.up, endPosition - startPosition, Color.green);
        }
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


    public List<Car> ignoredCars;

    public IEnumerator ForwardCarTimer()
    {
        while (true)
        {
            if (!forwardCar || forwardCar.path == path)
                yield return null;
            yield return new WaitForSeconds(1f);
            if (forwardCar && forwardCar.path != path)
            {
                if (!ignoredCars.Contains(forwardCar))
                    ignoredCars.Add(forwardCar);
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<TrafficLight>())
            trafficLight = other.GetComponent<TrafficLight>();
    }


    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<TrafficLight>())
            trafficLight = null;
    }
    
}