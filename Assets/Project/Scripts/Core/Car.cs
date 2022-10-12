using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExtensions;

public class Car : MonoBehaviour
{

    public int carIndex = 1;
    public float place;
    public float currentSpeed = 1;
    public TrafficLight trafficLight;
    public Transform rayPoint;
    public CarModel[] cars;
    public Path path;
    Vector3 wheelRotation;
    float rayPointDistance;
    public float minSpeed;

    void Start()
    {
        minSpeed = Random.Range(0, 0.01f);
        rayPointDistance = Vector3.Distance(transform.position, rayPoint.position);
    }
    
    public void MoveCar(Path newPath)
    {
        path = newPath;
        StartCoroutine("MoveRoutine");
    }
    
    [FormerlySerializedAs("colliderCar")] public Car collidedCar;
    
    IEnumerator MoveRoutine()
    {
        float speed  = Random.Range(0.9f, 1.1f) * GameManager.Instance.carSpeed;
        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        while (true)
        {
            collidedCar = CheckRay();
            if (collidedCar && (Vector3.Distance(path.transform.position, collidedCar.path.transform.position) <0.1f))
            {
                text.text = "Same Path\n"+collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, GameManager.Instance.slowStrength);
            }
            else if (collidedCar && currentSpeed < collidedCar.currentSpeed)
            {
                text.text = "Different Speed\n" + collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, GameManager.Instance.slowStrength);
            }
            else if (trafficLight)
            {
                text.text = "Light";
                currentSpeed = Mathf.Lerp(currentSpeed, minSpeed, GameManager.Instance.slowStrength);
            }
            else
            {
                text.text = "Go";
                currentSpeed = Mathf.Lerp(currentSpeed, speed, GameManager.Instance.slowStrength);
            }
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

   


    
    Car CheckRay()
    {
        float ray = GameManager.Instance.rayDistance;
        for (int a = 1; a <= 10; a++)
        {
            Vector3 startPosition = path.tween.PathGetPoint((place + ray * (a - 1) / 10f) / path.pathLength);
            Vector3 endPosition = path.tween.PathGetPoint((place + ray * a / 10f) / path.pathLength);
            Physics.SphereCast(startPosition+Vector3.up, 0.5f, endPosition - startPosition, out RaycastHit hit, ray/10f, LayerMask.GetMask("Car"));
            
            if (hit.transform)
            {
                Debug.DrawRay(startPosition + Vector3.up, endPosition - startPosition, Color.red);
                return hit.transform.GetComponentInParent<Car>();
            }
            Debug.DrawRay(startPosition + Vector3.up, endPosition - startPosition, Color.green);
        }
        return null;
    }
    
    public void Rotate(Vector3 newPosition)
    {
        wheelRotation += new Vector3(Vector3.Distance(newPosition,transform.position) * 150f, 0, 0);
        for (int a = 0; a < cars[carIndex].wheels.Length; a++)
            cars[carIndex].wheels[a].transform.localEulerAngles = wheelRotation;
        if (newPosition == transform.position)
            return;
        float angle = Vector3.SignedAngle((newPosition - transform.position).normalized, transform.forward, Vector3.up);
        cars[carIndex].wheels[0].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle*2, wheelRotation.z);
        cars[carIndex].wheels[1].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle*2, wheelRotation.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newPosition - transform.position), 0.25f);
    }
    
    void OnTriggerEnter(Collider other)
    {
        //Car otherCar = other.transform.GetComponentInParent<Car>();
        
        //if (otherCar)
            //Instantiate(GameManager.Instance.crashSmoke,
            //Vector3.Lerp(transform.position, otherCar.transform.position, 0.5f), Quaternion.identity);
    }
    

    public void UpgradeCar()
    {
        cars[carIndex].Hide();
        carIndex += 1;
        cars[carIndex].Show();
    }
    
}