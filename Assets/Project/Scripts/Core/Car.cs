using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Car : MonoBehaviour
{

    public float size;
    public Transform[] wheels;
    public float place;
    Path path;
    bool moving = true;
    Vector3 wheelRotation;
    public bool arrived;
    public float targetPlace = -1;

    public void Move(Path newPath,float newPlace,float delay)
    {
        if (path == newPath && Mathf.Approximately(newPlace, targetPlace))
            return;
        if (moving|| path != newPath)
            delay = 0;
        path = newPath;
        targetPlace = newPlace;
        DOTween.Kill(gameObject.GetInstanceID());
        DOTween.To(Movement, place, targetPlace,GameManager.Instance.carSpeed)
            .SetDelay(delay * GameManager.Instance.reactionDelayPerCar/10).SetSpeedBased()
            .OnStart(() => moving = true).OnComplete(() => moving = false)
            .SetEase(GameManager.Instance.carMovementEase)
            .SetId(gameObject.GetInstanceID()).SetUpdate(UpdateType.Fixed);
    }

    
    public void Movement(float newPlace)
    {
        wheelRotation += new Vector3((newPlace - place) * 150f, 0, 0);
        for (int a = 0; a < wheels.Length; a++)
            wheels[a].transform.localEulerAngles = wheelRotation;
        
        place = newPlace;

        Vector3 newPosition = path.tween.PathGetPoint(newPlace / path.pathLength);
        Rotate(newPosition);
        transform.position = newPosition;
        if (Mathf.Approximately(newPlace,path.pathLength))
            arrived = true;
    }

    public void Rotate(Vector3 newPosition)
    {
        if (newPosition == transform.position)
            return;
        float angle = Vector3.SignedAngle((newPosition - transform.position).normalized, transform.forward, Vector3.up);
        wheels[0].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30, wheelRotation.z);
        wheels[1].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30, wheelRotation.z);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(newPosition - transform.position), 0.1f);
    }
    
}