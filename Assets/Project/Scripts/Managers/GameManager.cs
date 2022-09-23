using DG.Tweening;
using UnityEngine;
using UnityExtensions;

public class GameManager : MonoSingleton<GameManager>
{

    public float reactionDelayPerCar;
    public float carSpeed = 0.1f;
    public Ease carMovementEase = Ease.OutSine;
    
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }
}