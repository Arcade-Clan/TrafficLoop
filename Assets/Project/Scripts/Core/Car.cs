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
    public CarModel[] cars;
    public Path path;
    Vector3 wheelRotation;
    public Car collidedCar;
    public List<Car> ignoredCars;
    public Car lastCar;
    public float ignoreCarWaiter = 5;
    public int priority;

    void Awake()
    {
        priority = Random.Range(-int.MaxValue, int.MaxValue);
    }
    
    public void MoveCar(int newCarIndex,Path newPath)
    {
        carIndex = newCarIndex;
        for (int a = 0; a < cars.Length; a++)
            cars[a].gameObject.SetActive(carIndex == a);
        path = newPath;
        StartCoroutine("MoveRoutine");
    }

    
    IEnumerator MoveRoutine()
    {
        float speed  = Random.Range(0.9f, 1.1f) * GameManager.Instance.carSpeed;
        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        while (true)
        {
            collidedCar = CheckRay();
            
            if (collidedCar && place < collidedCar.place &&  
                (Vector3.Distance(path.transform.position, collidedCar.path.transform.position) <1f || 
                 Vector3.Distance(path.path.wps.Last(), collidedCar.path.path.wps.Last()) < 1f))
            {
                text.text = "Same Path\n"+collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            }
            else if (collidedCar && (!collidedCar.collidedCar || !collidedCar.collidedCar.collidedCar||priority<collidedCar.priority))
            {
                CheckLastCar(collidedCar);
                text.text = "Crash\n" + collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            }
            else if (trafficLight)
            {
                text.text = "Light";
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GameManager.Instance.slowStrength);
            }
            else
            {
                text.text = "Go";
                currentSpeed = Mathf.Lerp(currentSpeed, speed, GameManager.Instance.startMoveStrength);
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

    float timer;

    void CheckLastCar(Car newCar)
    {
        if (lastCar == newCar)
        {
            timer += Time.fixedDeltaTime;
            if(timer>ignoreCarWaiter)
            {
                ignoredCars.Add(lastCar);
                EmojiTrigger();
            }
        }
        else
        {
            timer = 0;
            lastCar = newCar;
        }
    }

    
    Car CheckRay()
    {
        float ray = GameManager.Instance.rayDistance;

        for (int a = 0; a <= 10; a++)
        {

            Vector3 position = path.tween.PathGetPoint((place + ray * a / 10f) / path.pathLength);
            
            Collider[] hits = Physics.OverlapSphere(position, 0.75f,LayerMask.GetMask("Car"));
            for (int b = 0; b < hits.Length; b++)
            {
                Debug.DrawLine(position + Vector3.up * 5f, position, Color.red);
                Car checkingCar = hits[b].transform.GetComponentInParent<Car>();
                if (!checkingCar)
                    continue;
                if (checkingCar == this)
                    continue;
                if (ignoredCars.Contains(checkingCar))
                    continue;
                return hits[b].transform.GetComponentInParent<Car>();
            }

            //openValue = a;
            Debug.DrawLine(position + Vector3.up * 5f, position, Color.green);
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

    public void EmojiTrigger()
    {
        Emoji emoji = GetComponentInChildren<Emoji>(true);
        emoji.Show();
        emoji.transform.LookAt(GameManager.Instance.cam.transform);
    }

    public void UpgradeCar()
    {
        cars[carIndex].Hide();
        carIndex += 1;
        cars[carIndex].Show();
    }
    
}