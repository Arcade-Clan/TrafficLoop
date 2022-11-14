using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ButtonJump : MonoBehaviour
{

    public float delay = 5;
    
    
    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(delay);
            yield return transform.DORotate(new Vector3(0, 0, 10), 0.15f).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Normal,true).WaitForCompletion();
            yield return transform.DORotate(new Vector3(0, 0, -10), 0.15f).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Normal, true)
                .WaitForCompletion();
            yield return transform.DORotate(new Vector3(0, 0, 0), 0.075f).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Normal, true)
                .WaitForCompletion();
        }
    }

}
