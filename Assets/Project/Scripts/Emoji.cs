using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

public class Emoji : MonoBehaviour
{
    public void Randomize()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        int randomEmoji = Random.Range(0, sprites.Length);
        for (int a = 0; a < sprites.Length; a++)
            sprites[a].Hide();
        sprites[randomEmoji].Show();
    }
}