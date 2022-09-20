using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{

    public SpriteRenderer[] handSpriteRenderers;
    public float distance = 5;
    void Update()
    {
        var v3 = Input.mousePosition;
        v3.z = distance;
        v3 = Camera.main.ScreenToWorldPoint(v3);
        transform.position = v3;
        if (Input.GetMouseButton(0))
        {
            handSpriteRenderers[0].enabled = true;
            handSpriteRenderers[1].enabled = false;
        }
        else
        {
            handSpriteRenderers[1].enabled = true;
            handSpriteRenderers[0].enabled = false;
        }
    }
    
}