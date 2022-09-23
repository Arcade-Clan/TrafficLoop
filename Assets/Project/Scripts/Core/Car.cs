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
    bool moving;
    Vector3 wheelRotation;
    public bool arrived;
    public void Move(Path newPath,float newPlace,float delay)
    {
        path = newPath;
        if (moving)
            delay = 0;
        DOTween.Kill(gameObject.GetInstanceID());
        DOTween.To(Movement, place, newPlace,GameManager.Instance.carSpeed)
            .SetDelay(delay * GameManager.Instance.reactionDelayPerCar).SetSpeedBased()
            .OnStart(() => moving = true).OnComplete(()=> moving=false)
            .SetEase(GameManager.Instance.carMovementEase)
            .SetId(gameObject.GetInstanceID());
    }

    public void Movement(float newPlace)
    {
        wheelRotation += new Vector3((newPlace - place) * 150f, 0, 0);
        for (int a = 0; a < wheels.Length; a++)
            wheels[a].transform.localEulerAngles = wheelRotation;
        place = newPlace;

        Vector3 newPosition = path.tween.PathGetPoint(newPlace / path.pathLength);
        if(newPosition != transform.position)
        {
            float angle = Vector3.SignedAngle((newPosition - transform.position).normalized, transform.forward, Vector3.up);
            wheels[0].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30,wheelRotation.z);
            wheels[1].transform.localEulerAngles = new Vector3(wheelRotation.x, -angle * 30, wheelRotation.z);
            transform.rotation = Quaternion.LookRotation(newPosition - transform.position);
        }
        transform.position = newPosition;
        if (Mathf.Approximately(newPlace,path.pathLength))
            arrived = true;
    }


}