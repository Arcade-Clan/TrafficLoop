using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityExtensions;

public class UIManager : MonoSingleton<UIManager>
{
  
   public List<GameObject> panels;
   public List<TextMeshProUGUI> texts;
   public TextMeshProUGUI goldText;
   public TextMeshProUGUI carAmount;
   public TextMeshProUGUI carAmountPerMinute;


   private void Start()
   {
      texts[0].text = "LEVEL " + (PlayerPrefs.GetInt("Level") + 1);
      texts[1].text = "LEVEL " + (PlayerPrefs.GetInt("Level") + 1) + " COMPLETED";
      texts[2].text = "LEVEL " + (PlayerPrefs.GetInt("Level") + 1) + " FAILED";
   }
   
   public void NextButton()
   {
      PlayerPrefs.SetInt("Gold", 0);
      PlayerPrefs.SetInt("Speed", 0);
      PlayerPrefs.SetInt("Size", 0);
      PlayerPrefs.SetInt("Income", 0);
      SceneManager.LoadScene(1);
   }

   void Update()
   {
      carAmount.text = "" + GameManager.Instance.trafficController.cars.Count;
      carAmountPerMinute.text = "" + GameManager.Instance.upgrades[0].Value();
   }
   
   
}
