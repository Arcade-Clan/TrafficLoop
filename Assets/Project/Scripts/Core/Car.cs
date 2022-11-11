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
    [FormerlySerializedAs("path")] public Road road;
    Vector3 wheelRotation;
    public Car collidedCar;
    public List<Car> ignoredCars;
    public Car lastCar;
    public int priority;
    float speed;
    void Awake()
    {
        priority = Random.Range(-int.MaxValue, int.MaxValue);

    }
    
    public void MoveCar(int newCarIndex,Road newRoad,float position,bool withAnimation)
    {
        
        carIndex = newCarIndex;
        for (int a = 0; a < cars.Length; a++)
            cars[a].gameObject.SetActive(carIndex == a);
        road = newRoad;
        place = position * road.pathLength;
        speed = Random.Range(0.9f, 1.1f) * cars[carIndex].carSpeed;
        currentSpeed = speed;
        StartCoroutine("MoveRoutine");
        if (withAnimation)
            Appear();
    }

    void Appear()
    {
        CarModel model = cars[carIndex];
        model.transform.localPosition+=Vector3.up*5;
        float scale = model.transform.localScale.x;
        model.transform.localScale = Vector3.zero;
        model.transform.DOScale(scale,1.5f).SetEase(Ease.OutElastic);
        model.transform.DOLocalMoveY(0, 1).SetEase(Ease.OutBounce);
    }
    
    IEnumerator MoveRoutine()
    {
        TextMeshPro text = GetComponentInChildren<TextMeshPro>();
        while (true)
        {
            collidedCar = CheckRay();
            
            if (collidedCar && place < collidedCar.place &&  
                (Vector3.Distance(road.transform.position, collidedCar.road.transform.position) <1f || 
                 Vector3.Distance(road.path.wps.Last(), collidedCar.road.path.wps.Last()) < 1f))
            {
                text.text = "Same Path\n"+collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GM.Instance.slowStrength);
            }
            else if (collidedCar && (!collidedCar.collidedCar || !collidedCar.collidedCar.collidedCar||priority<collidedCar.priority))
            {
                CheckLastCar(collidedCar);
                text.text = "Crash\n" + collidedCar.gameObject.name;
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GM.Instance.slowStrength);
            }
            else if (trafficLight&& !cars[carIndex].feverCar)
            {
                text.text = "Light";
                currentSpeed = Mathf.Lerp(currentSpeed, 0, GM.Instance.slowStrength);
            }
            else
            {
                text.text = "Go";
                currentSpeed = Mathf.Lerp(currentSpeed, speed, GM.Instance.startMoveStrength);
            }
            Vector3 rotation = road.tween.PathGetPoint((place + currentSpeed / 60) / road.pathLength);
            float angle = Vector3.Angle((rotation - transform.position).normalized, transform.forward);
            place += currentSpeed * (1 - angle / 28f) / 60;
            Vector3 newPosition = road.tween.PathGetPoint(place / road.pathLength);
            Rotate(newPosition);
            transform.position = newPosition;
            if (place>=road.pathLength)
            {
                GM.Instance.cars[carIndex].cars.Remove(this);
                UIM.Instance.UpdateEconomyUI();
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
            if(timer>GM.Instance.ignoreCarWaiter)
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
        if (cars[carIndex].feverCar)
            return null;
        float ray = GM.Instance.rayDistance;

        for (int a = 0; a <= 10; a++)
        {

            Vector3 position = road.tween.PathGetPoint((place + ray * a / 10f) / road.pathLength);
            
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
        GetComponentInChildren<Emoji>(true).Show();
    }

    public void UpgradeCar()
    {
        cars[carIndex].Hide();
        carIndex += 1;
        cars[carIndex].Show();
        ScaleCar();
    }
    
    void ScaleCar()
    {
        float carScale = transform.localScale.x;
        transform.DOScale(carScale * 2, 0.25f).SetEase(Ease.OutSine)
            .OnComplete(
                () =>
                {
                    transform.DOScale(carScale, 0.25f).SetEase(Ease.InSine);
                });
    }

    public void AllCarUpgrade()
    {
        if (carIndex >= 10)
            return;
        GM.Instance.cars[carIndex].cars.Remove(this);
        UpgradeCar();
        GM.Instance.cars[carIndex].cars.Add(this);
    }
}