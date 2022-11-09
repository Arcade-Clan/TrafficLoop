using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;

public class Road : MonoBehaviour
{

    [HideInInspector]
    public DOTweenPath path;
    [HideInInspector]
    public float pathLength;
    [HideInInspector]
    public Tween tween;

    void Start()
    {
        tween = path.GetTween();
        pathLength = tween.PathLength();
    }
}