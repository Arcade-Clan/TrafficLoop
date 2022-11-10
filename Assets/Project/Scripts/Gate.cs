using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class Gate : MonoBehaviour
{

    public Transform model;
    public Ease gateOpenEase = Ease.OutElastic;
    public Ease gateCloseEase = Ease.OutElastic;
    public float openTime = 1;
    public float closeTime = 1;

    private void OnEnable()
    {
        Vector3 position = model.localPosition;
        Vector3 scale = model.localScale;
        model.localScale = Vector3.zero;
        model.localPosition+=Vector3.up*5;
        model.DOScale(scale,1.5f*Random.Range(0.8f,1.2f)).SetEase(Ease.OutElastic);
        model.DOLocalMoveY(position.y, 1*Random.Range(0.8f,1.2f)).SetEase(Ease.OutBounce);
    }

    void OnTriggerEnter(Collider other)
    {
        GM.Instance.IncreaseMoney(other.transform.parent.GetComponent<Car>(),transform.position+Vector3.up);
        DOTween.Kill(model);
        model.DOLocalRotate(new Vector3(0,180,0), openTime).SetEase(gateOpenEase);
    }

    void OnTriggerExit(Collider other)
    {
        DOTween.Kill(model);
        model.DOLocalRotate(new Vector3(0, 90, 0), closeTime).SetEase(gateCloseEase);
    }
    
    
    
}