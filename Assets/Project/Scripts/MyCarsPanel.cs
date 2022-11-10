using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

public class MyCarsPanel : MonoBehaviour
{
   [Serializable]
   public class ImageClass
   {
      public Image[] carImages;
      public TextMeshProUGUI carAmount;
      public TextMeshProUGUI carValue;
   }

   public ImageClass[] carImages;


   void OnEnable()
   {
      StartCoroutine("ProcessImages");
   }

   IEnumerator ProcessImages()
   {
      while (true)
      {
         bool opened = false;
         for (int a = carImages.Length - 1; a >= 0; a--)
         {
            if (GM.Instance.cars[a].carLevel > 0||opened)
            {
               opened = true;
               carImages[a].carImages[1].Hide();
               carImages[a].carImages[0].Show();
            }
            else
            {
               carImages[a].carImages[0].Hide();
               carImages[a].carImages[1].Show();
            }
            carImages[a].carAmount.text = "" + GM.Instance.cars[a].carLevel;
            carImages[a].carValue.text = "$" + GM.Instance.cars[a].carValue;
         }

         yield return null;
      }
   }
}