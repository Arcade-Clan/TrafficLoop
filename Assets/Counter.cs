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
            ProcessCounter(Random.Range(10000000,99999999));
    }

    
    public void ProcessCounter(float value)
    {
        int count = Mathf.RoundToInt(value);
        print(count);
        for (int a = 0; a < counters.Length; a++)
        {
            DOTween.Kill(counters[a]);
            int newCount = Mathf.FloorToInt((count / Mathf.Pow(10f, a))% 10);
            print(newCount);
        }
    }
    
}