using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

public class ButtonClass : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public Button button;
    public GameObject adImage;
    public string state = "NoMoney";
    public bool adsEnabled = false;
    public IEnumerator WaitForAdRoutine()
    {
        yield return new WaitForSeconds(2);
        adImage.Show();
        button.interactable = true;
        state = "AdButton";
    }
}