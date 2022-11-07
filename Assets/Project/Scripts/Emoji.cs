using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;
using Random = UnityEngine.Random;

public class Emoji : MonoBehaviour
{
    private void OnEnable()
    {
        Randomize();
    }
    public void Randomize()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>(true);
        int randomEmoji = Random.Range(0, sprites.Length);
        for (int a = 0; a < sprites.Length; a++)
            sprites[a].Hide();
        sprites[randomEmoji].Show();
        StartCoroutine("LookAtUserRoutine");
    }

    IEnumerator LookAtUserRoutine()
    {
        while (true)
        {        
            transform.LookAt(GM.Instance.cam.transform);
            yield return null;
        }
    }
}