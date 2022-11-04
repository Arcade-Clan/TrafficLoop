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

   public TextMeshProUGUI goldText;
   public TextMeshProUGUI carAmount;
   public TextMeshProUGUI carAmountPerMinute;
   public TextMeshProUGUI incomePerMinute;
   public Image trafficDensity;
   public GameObject counterText;


   [Serializable]
   public class UpgradeClass
   {
      public TextMeshProUGUI goldText;
      public GameObject upgradePanel;
      public Image coverImage;
   }

   public UpgradeClass[] upgrades;

   [Serializable]
   public class MergeClass
   {
      public TextMeshProUGUI goldText;
      public GameObject upgradePanel;
      public Image coverImage;
   }

   public MergeClass merge;

   public int tutorialProgression;
   public GameObject[] tutorialHands;
   public bool tutorialInProgress;

   void Awake()
   {
      tutorialProgression = PlayerPrefs.GetInt("TutorialProgression");
   }

   public void TriggerTutorialProgression(int index)
   {
      if (tutorialProgression != index)
         return;
      tutorialInProgress = true;
      tutorialHands[index].Show();
   }


   public bool IncreaseTutorialProgression(int index)
   {
      if (tutorialProgression > 4)
         return true;
      if (tutorialProgression != index)
         return false;
      tutorialProgression++;
      PlayerPrefs.SetInt("TutorialProgression", tutorialProgression);
      tutorialHands[index].Hide();
      tutorialInProgress = false;
      if (index == 0)
         TriggerTutorialProgression(1);
      return true;
   }

   public void NextButton()
   {
      PlayerPrefs.SetInt("Gold", 0);
      PlayerPrefs.SetInt(GameManager.Instance.upgrades[0].upgradeName, 0);
      PlayerPrefs.SetInt(GameManager.Instance.upgrades[1].upgradeName, 0);
      PlayerPrefs.SetInt(GameManager.Instance.upgrades[2].upgradeName, 0);
      PlayerPrefs.SetInt(GameManager.Instance.merge.mergeName, 0);
      SceneManager.LoadScene(1);
   }

   void Update()
   {
      List<Car> cars = new();
      for (int a = 0; a < GameManager.Instance.cars.Length; a++)
      {
         cars.AddRange(GameManager.Instance.cars[a].cars);
      }

      carAmount.text = "" + cars.Count;
      carAmountPerMinute.text =
         "" + Mathf.RoundToInt(GameManager.Instance.baseSecondCreation / GameManager.Instance.TotalCarCount());

      incomePerMinute.text = "$" + CalculateIncome() * GameManager.Instance.simulationSpeed + "/M";
      float density = 0;
      for (int a = 0; a < cars.Count; a++)
      {
         if (cars[a].collidedCar)
            density += 1;
      }

      if (cars.Count > 0)
         trafficDensity.fillAmount = Mathf.Lerp(trafficDensity.fillAmount, density / cars.Count, 0.1f);
   }


   int CalculateIncome()
   {
      float income = 0;

      for (int a = 0; a < GameManager.Instance.cars.Length; a++)
      {
         income += GameManager.Instance.cars[a].carLevel * GameManager.Instance.cars[a].carValue *
            GameManager.Instance.upgrades[2].Value() *
            60 / GameManager.Instance.baseSecondCreation;
      }

      return Mathf.RoundToInt(income);
   }

   public void CreateText(int value, Vector3 position)
   {
      GameObject newText = Instantiate(counterText, GameManager.Instance.canvas.transform, true);
      newText.GetComponentInChildren<TextMeshProUGUI>().text = "+" + value;
      newText.transform.SetAsFirstSibling();
      newText.GetComponent<RectTransform>().anchoredPosition =
         RectTransformUtility.WorldToScreenPoint(GameManager.Instance.cam, position) /
         GameManager.Instance.canvas.transform.localScale.x;
   }


   public void UpdateEconomyUI()
   {
      float cost;
      for (int a = 0; a < upgrades.Length; a++)
      {
         if (GameManager.Instance.upgrades[a].Max(a))
         {
            upgrades[a].upgradePanel.Hide();
         }
         else
         {

            cost = GameManager.Instance.upgrades[a].Cost(a);
            //upgrades[a].levelText.text = "LEVEL " + (GameManager.Instance.upgrades[a].upgradeLevel + 1);
            upgrades[a].goldText.text = "$" + PriceFormatting(cost);
            upgrades[a].coverImage.gameObject.SetActive(cost > GameManager.Instance.gold);
            upgrades[a].coverImage.transform.parent.GetComponent<Button>().enabled = cost <= GameManager.Instance.gold;
            if (cost <= GameManager.Instance.gold)
            {
               if (a == 0)
                  TriggerTutorialProgression(0);
               else if (a == 2)
                  TriggerTutorialProgression(2);
               else if (a == 1)
                  TriggerTutorialProgression(4);
            }
         }
      }


         
      merge.upgradePanel.Show();
      cost = GameManager.Instance.merge.Cost();
      merge.goldText.text = "$" + PriceFormatting(cost);
      if (cost > GameManager.Instance.gold || !GameManager.Instance.CanMerge())
      {
         merge.coverImage.gameObject.SetActive(true);
         merge.coverImage.transform.parent.GetComponent<Button>().enabled = false;
      }
      else
      {
         merge.coverImage.gameObject.SetActive(false);
         merge.coverImage.transform.parent.GetComponent<Button>().enabled = true;
      }

      if (cost <= GameManager.Instance.gold)
         TriggerTutorialProgression(3);
      
   }

   public void UpdateGold()
   {
      PlayerPrefs.SetInt("Gold",GameManager.Instance.gold);
      goldText.text = PriceFormatting(GameManager.Instance.gold);
   }


   public string PriceFormatting(float value)
   {
      if (value >= 1000000)
         return string.Format("{0:N2}", value / 1000000f)+"M";
      else if (value >= 1000)
         return string.Format("{0:N2}", value / 1000f)+"K";
      return "" + value;
   }
}
