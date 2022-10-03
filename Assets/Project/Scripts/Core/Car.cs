using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class Car : MonoBehaviour
{

    public float size;
    public Transform[] wheels;
    public float position;
    public Path path;
    bool moving = true;
    Vector3 wheelRotation;
    public bool arrived;
    public float targetPlace = -1;

    public bool move;
    public float currentSpeed;

    public float targetPosition;

    void Start()
    {
        StartCoroutine("MoveRoutine");
    }

    public void UpdateCar(Path newPath,float newPosition)
    {
        targetPosition = newPosition;
        path = newPath;
    }
    
    IEnumerator MoveRoutine()
    {
        arrived = true;
        while (true)
        {
            move = position < targetPosition;
            if (move)
                currentSpeed = Mathf.Lerp(currentSpeed, GameManager.Instance.carSpeed,0.1f);
            else
                currentSpeed = Mathf.Lerp(currentSpeed, 0, 0.1f);
            position += currentSpeed / 60;
            Vector3 newPosition = path.tween.PathGetPoint(position / path.pathLength);
            transform.position = newPosition;
            yield return new WaitForFixedUpdate();
        }
    }
    
    
    public void Move(Path newPath,float newPlace,float delay)
    {
        if (path == newPath && Mathf.Approximately(newPlace, targetPlace))
            return;
        if (moving|| path != newPath)
            delay = 0;
        path = newPath;
        targetPlace = newPlace;
        DOTween.Kill(gameObject.GetInstanceID());
        DOTween.To(Movement, position, targetPlace,GameManager.Instance.carSpeed)
            .SetDelay(delay * GameManager.Instance.reactionDelayPerCar/10).SetSpeedBased()
            .OnStart(() => moving = true).OnComplete(() => moving = false)
            .SetEase(GameManager.Instance.carMovementEase)
            .SetId(gameObject.GetInstanceID()).SetUpdate(UpdateType.Fixed);
    }

    
    public void Movement(float newPlace)
    {
        wheelRotation += new Vector3((newPlace - position) * 150f, 0, 0);
        for (int a = 0; a < wheels.Length; a++)
            wheels[a].transform.localEulerAngles = wheelRotation;
        
        position = newPlace;

        Vector3 newPosition = path.tween.PathGetPoint(newPlace / path.pathLength);
        if(newPosition != transform.position)
        {
            TurnWheels(newPosition);
            transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.LookRotation(newPosition - transform.position),0.1f);
        }
        transform.position = newPosition;
        if (Mathf.Approximately(newPlace,path.pathLength))
            arrived = true;
    }

    public void TurnWheels(Vector3 newPosition)
    {
        float angle = Vector3.SignedAngle((newPosition - transform.position).normalized, transform.forward, Vector3.up);
        wheels[0].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30, wheelRotation.z);
        wheels[1].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30, wheelRotation.z);
    }
    
}