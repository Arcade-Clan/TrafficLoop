using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hand : MonoBehaviour
{

    public Image[] handSpriteRenderers;
    public float scale;
    void Start()
    {
        scale = FindObjectOfType<Canvas>().transform.localScale.x;
    }
    
    void Update()
    {
        GetComponent<RectTransform>().anchoredPosition= Input.mousePosition/ scale;
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