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
    public float delay = 0.5f;
    public GameObject particleObject;
    
    
    public void Start()
    {
        Vector3 position = model.localPosition;
        Vector3 scale = model.localScale;
        model.localScale = Vector3.one*4;
        model.localPosition+=Vector3.up*5;
        float random = Random.Range(0.8f, 1.2f);
        model.DOScale(scale, random).SetEase(Ease.OutBounce).SetDelay(delay).SetUpdate(UpdateType.Normal, true);
        model.DOLocalMoveY(position.y, random).SetEase(Ease.OutBounce).SetDelay(delay).SetUpdate(UpdateType.Normal, true);
        
        particleObject.transform.DOScale(1.5f,0.1f).SetDelay(delay + random / 2).OnStart(()=>particleObject.SetActive(true))
            .SetUpdate(UpdateType.Normal, true);
    }

    void OnTriggerEnter(Collider other)
    {
        GM.Instance.IncreaseMoney(other.transform.parent.GetComponent<Car>(),transform.position+Vector3.up);
        model.DOLocalRotate(new Vector3(0,180,0), openTime).SetEase(gateOpenEase);
    }

    void OnTriggerExit(Collider other)
    {
        model.DOLocalRotate(new Vector3(0, 90, 0), closeTime).SetEase(gateCloseEase);
    }
    
    
    
}