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
   public TextMeshProUGUI incomePerMinute;
   public Image trafficDensity;
   
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
      incomePerMinute.text = "" + GameManager.Instance.upgrades[0].Value() * GameManager.Instance.upgrades[2].Value();
      List<Car> cars = GameManager.Instance.trafficController.cars;
      float density = 0;
      for (int a = 0; a < cars.Count; a++)
      {
         if (cars[a].forwardCar)
            density += 1;
      }
      if(cars.Count>0)
         trafficDensity.fillAmount = Mathf.Lerp(trafficDensity.fillAmount, density / cars.Count,0.1f);
   }
   
   
}
