using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Counter : MonoBehaviour
{

    public Transform[] counters;


    void Update()
    {
        if(Input.GetMouseButtonDown(1))
            ProcessCounter(Random.Range(0,999999.99f));
    }

    
    public void ProcessCounter(float value)
    {
        int count = Mathf.RoundToInt(value*100);
        print(count);
        for (int a = 0; a < counters.Length; a++)
        {
            int newCount = Mathf.FloorToInt((count / Mathf.Pow(10f, a))% 10);
            DOTween.Kill(counters[a]);
            counters[a].DOLocalRotateQuaternion(Quaternion.Euler(0,newCount*36,0), 1f ).SetEase(Ease.OutSine);
            print(newCount);
        }
    }
    
}