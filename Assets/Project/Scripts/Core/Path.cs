using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;

public class Path : MonoBehaviour
{

    [HideInInspector]
    public DOTweenPath path;
    [HideInInspector]
    public float pathLength;
    [HideInInspector]
    public Tween tween;
    public bool endPath;
    public SpriteRenderer trafficLight;
    public float lightTimer = 5f;
    [ReadOnly]
    public bool canPass;
    public int trafficIndex;
    
    void Start()
    {
        tween = path.GetTween();
        pathLength = tween.PathLength();
    }
    
    public void Pass()
    {
        canPass = true;
        trafficLight.color = Color.green;
    }

    public void Stop()
    {
        canPass = false;
        trafficLight.color = Color.red;
    }
}