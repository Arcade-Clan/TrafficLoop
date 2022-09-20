using UnityEngine;
using UnityExtensions;

public class GameManager : MonoSingleton<GameManager>
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}

