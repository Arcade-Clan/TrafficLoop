using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Gate : MonoBehaviour
{

    public Transform model;
    public Ease gateOpenEase = Ease.OutElastic;
    public Ease gateCloseEase = Ease.OutElastic;
    public float openTime = 1;
    public float closeTime = 1;
    
    void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.IncreaseMoney(other.transform.parent.GetComponent<Car>(),transform.position+Vector3.up);
        DOTween.Kill(model);
        model.DOLocalRotate(new Vector3(0,180,0), openTime).SetEase(gateOpenEase);
    }

    void OnTriggerExit(Collider other)
    {
        GameManager.Instance.IncreaseMoney(other.transform.parent.GetComponent<Car>(), transform.position + Vector3.up);
        DOTween.Kill(model);
        model.DOLocalRotate(new Vector3(0, 90, 0), closeTime).SetEase(gateCloseEase);
    }
    
}