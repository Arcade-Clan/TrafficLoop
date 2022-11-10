using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyCarsPanel : MonoBehaviour
{
   [Serializable]
   public class ImageClass
   {

      public Sprite[] carSprite;
      public Image carImage;
      public TextMeshProUGUI carAmount;
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
         for (int a = 0; a < carImages.Length; a++)
         {
            if (GM.Instance.cars[a].carLevel > 0)
               carImages[a].carImage.sprite = carImages[a].carSprite[1];
            else
               carImages[a].carImage.sprite = carImages[a].carSprite[0];
            carImages[a].carAmount.text = "" + GM.Instance.cars[a].carLevel;
         }

         yield return null;
      }
   }
}