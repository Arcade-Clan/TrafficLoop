using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
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
   public ButtonClass[] upgrades;
   public ButtonClass merge;
   public int tutorialProgression;
   public GameObject[] tutorialHands;
   public bool tutorialInProgress;
   
   void Awake()
   {
      if (!GM.Instance.tutorialOn)
         PlayerPrefs.SetInt("TutorialProgression",100);
      tutorialProgression = PlayerPrefs.GetInt("TutorialProgression");
   }

   private void Start()
   {
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
         income += GM.Instance.cars[a].carLevel * GM.Instance.cars[a].carValue * AdsM.Instance.adDetails[3].multiplierValue * 60f / GM.Instance.baseSecondCreation;
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
      
      for (int a = 0; a < upgrades.Length; a++)
      {
         if (a == 2&& !LM.Instance.gates[GM.Instance.upgrades[2].upgradeLevel].gameObject.activeInHierarchy)
         {
            upgrades[a].button.interactable = false;
            upgrades[a].goldText.text = "MAX";
         }
         else if (GM.Instance.upgrades[a].Max(a))
            upgrades[a].button.Hide();
         else
            ProcessButtons(a);
      }
      ProcessMerge();
   }
   
   
   

   void ProcessButtons(int index)
   {
      float cost = GM.Instance.upgrades[index].Cost();
      upgrades[index].goldText.text = "$" + PriceFormatting(cost);
      bool canBeBought = cost <= GM.Instance.gold;
      upgrades[index].button.Show();
      if (canBeBought)
      {
         if(upgrades[index].state != "CanBuy")
         {
            upgrades[index].StopCoroutine("WaitForAdRoutine");
            upgrades[index].adImage.Hide();
            upgrades[index].button.interactable = true;
            upgrades[index].state = "CanBuy";
         }
      }
      else
      {
         if (upgrades[index].state == "CanBuy")
         {
            upgrades[index].adImage.Hide();
            upgrades[index].button.interactable = false;
            upgrades[index].state = "NoMoney";
            upgrades[index].StartCoroutine("WaitForAdRoutine");
         }
      }
      
      if (canBeBought)
      {
         if (index == 0)
            TriggerTutorialProgression(0);
         else if (index == 1)
            TriggerTutorialProgression(4);
         else if (index == 2)
            TriggerTutorialProgression(2);
      }  
   }
   
   void ProcessMerge()
   {
      float cost = GM.Instance.merge.Cost();
      merge.goldText.text = "$" + PriceFormatting(cost);
      bool canBeBought = cost <= GM.Instance.gold;
      if (canBeBought && PM.Instance.CanMerge())
      {
         if(merge.state != "CanBuy")
         {
            merge.StopCoroutine("WaitForAdRoutine");
            merge.adImage.Hide();
            merge.button.interactable = true;
            merge.state = "CanBuy";
         }
      }
      else
      {
         bool canMerge = PM.Instance.CanMerge();
         if ((merge.state == "CanBuy"||merge.state == "NoMerge") && canMerge)
         {
            merge.adImage.Hide();
            merge.button.interactable = false;
            merge.state = "NoMoney";
            merge.StartCoroutine("WaitForAdRoutine");
         }
         else if ((merge.state == "CanBuy"||merge.state == "AdButton") && !canMerge)
         {
            merge.adImage.Hide();
            merge.button.interactable = false;
            merge.state = "NoMerge";
         }
      }
      
      if (canBeBought)
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