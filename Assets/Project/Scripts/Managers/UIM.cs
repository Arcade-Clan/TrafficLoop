using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

public class UIM : MonoSingleton<UIM>
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

   [Header("AdButtons")] 
   public GameObject add3CarButton;
   public GameObject speedUpButton;
   public Image speedUpFiller;
   public GameObject addIncomeButton;
   public TMP_Text addIncomeText;
   public GameObject addNewCarButton;
   public GameObject autoTapButton;
   public Image autoTapFiller;
   public GameObject upgradeAllCarButton;
   public Image upgradeAllCarFiller;
   void Awake()
   {
      tutorialProgression = PlayerPrefs.GetInt("TutorialProgression");
   }

   private void Start()
   {
      if (!GM.Instance.tutorialOn)
         PlayerPrefs.SetInt("TutorialProgression",100);
      oldGold = GM.Instance.gold;
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
   
   void Update()
   {
      List<Car> cars = new();
      for (int a = 0; a < GM.Instance.cars.Length; a++)
      {
         cars.AddRange(GM.Instance.cars[a].cars);
      }

      carAmount.text = "" + cars.Count;
      carAmountPerMinute.text =
         "" + Mathf.RoundToInt(GM.Instance.baseSecondCreation / PM.Instance.TotalCarCount());

      incomePerMinute.text = "$" + CalculateIncome() * GM.Instance.simulationSpeed + "/M";
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

      for (int a = 0; a < GM.Instance.cars.Length; a++)
      {
         income += GM.Instance.cars[a].carLevel * GM.Instance.cars[a].carValue *
            GM.Instance.upgrades[2].Value() *
            60 / GM.Instance.baseSecondCreation;
      }

      return Mathf.RoundToInt(income);
   }

   public void CreateText(int value, Vector3 position)
   {
      GameObject newText = Instantiate(counterText, GM.Instance.canvas.transform, true);
      newText.GetComponentInChildren<TextMeshProUGUI>().text = "+" + value;
      newText.transform.SetAsFirstSibling();
      newText.GetComponent<RectTransform>().anchoredPosition =
         RectTransformUtility.WorldToScreenPoint(GM.Instance.cam, position) /
         GM.Instance.canvas.transform.localScale.x;
   }


   public void UpdateEconomyUI()
   {
      float cost;
      for (int a = 0; a < upgrades.Length; a++)
      {
         if (GM.Instance.upgrades[a].Max(a))
         {
            upgrades[a].upgradePanel.Hide();
         }
         else
         {

            cost = GM.Instance.upgrades[a].Cost();
            //upgrades[a].levelText.text = "LEVEL " + (GameManager.Instance.upgrades[a].upgradeLevel + 1);
            upgrades[a].goldText.text = "$" + PriceFormatting(cost);
            upgrades[a].coverImage.gameObject.SetActive(cost > GM.Instance.gold);
            upgrades[a].coverImage.transform.parent.GetComponent<Button>().enabled = cost <= GM.Instance.gold;
            if (cost <= GM.Instance.gold)
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
      cost = GM.Instance.merge.Cost();
      merge.goldText.text = "$" + PriceFormatting(cost);
      if (cost > GM.Instance.gold || !PM.Instance.CanMerge())
      {
         merge.coverImage.gameObject.SetActive(true);
         merge.coverImage.transform.parent.GetComponent<Button>().enabled = false;
      }
      else
      {
         merge.coverImage.gameObject.SetActive(false);
         merge.coverImage.transform.parent.GetComponent<Button>().enabled = true;
      }

      if (cost <= GM.Instance.gold)
         TriggerTutorialProgression(3);
      
   }

   private int oldGold;
   public void UpdateGold()
   {
      PlayerPrefs.SetInt("Gold",GM.Instance.gold);
      DOVirtual.Int(oldGold, GM.Instance.gold,0.5f,UpdateGoldText);
      oldGold = GM.Instance.gold;
      goldText.DOScale(1.2f, 0.25f).OnComplete(()=>goldText.DOScale(1f, 0.25f).SetDelay(0.5f));
   }

   void UpdateGoldText(int value)
   {
      goldText.text = PriceFormatting(value);
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