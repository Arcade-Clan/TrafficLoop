using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

public class TrafficLight : MonoBehaviour
{

   public GameObject[] lights;
   public BoxCollider trafficCollider;
   public float timer = 5;
   public int trafficIndex;
   
   public void Pass(bool value)
   {
      
      if(value)
      {
         lights[0].Show();
         lights[1].Hide();
         trafficCollider.center = new Vector3(trafficCollider.center.x,10f, trafficCollider.center.z);
      }
      else
      {
         lights[0].Hide();
         lights[1].Show();
         trafficCollider.center = new Vector3(trafficCollider.center.x, 0.5f, trafficCollider.center.z);
      }
   }
}