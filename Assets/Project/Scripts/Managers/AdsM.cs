using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;
public class AdsM : MonoSingleton<AdsM>
{
    public float speedUp;
    public int speedUpMultiplier;
    public int speedUpTimer;

    void Start()
    {
        UIM.Instance.addIncomeText.text = "" + Mathf.RoundToInt(GM.Instance.upgrades[1].Value() / 2f);
    }
    
    public void Add3Car()
    {
        AM.Instance.StartCoroutine("Add3CarRoutine");
    }

    public void SpeedUp()
    {
        StartCoroutine("SpeedUpRoutine");
    }

    IEnumerator SpeedUpRoutine()
    {
        UIM.Instance.speedUpButton.Hide();
        speedUp = speedUpMultiplier;
        yield return new WaitForSecondsRealtime(speedUpTimer);
        speedUp = 1;
        UIM.Instance.speedUpButton.Show();
    }

    public void AddIncome()
    {
        GM.Instance.gold += Mathf.RoundToInt(GM.Instance.upgrades[1].Value() / 2f);
        UIM.Instance.UpdateGold();
        UIM.Instance.UpdateEconomyUI();
    }
}