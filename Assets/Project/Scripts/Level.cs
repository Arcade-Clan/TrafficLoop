using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour
{

   [Serializable]
   public class SectionClass
   {
      public Transform cam;
      public List<GameObject> elements = new List<GameObject>();
   }
   public SectionClass[] sections;
   
}