using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

public class SpecialBuild : MonoBehaviour
{

    public GameObject childPanel;
    public GameObject[] UIElements;
    public Material material;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0)&&Input.touchCount == 3))
            childPanel.SetActive(!childPanel.activeSelf);
    }

    public void Add1000()
    {
        GM.Instance.gold += 1000;
        UIM.Instance.UpdateGold();
        UIM.Instance.UpdateEconomyUI();
    }

    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        Application.LoadLevel(1);
    }

    public void CloseUI()
    {
        if (UIElements[0].activeSelf)
        {
            for (int a = 0; a < UIElements.Length; a++)
                UIElements[a].Hide();
        }
        else
        {
            for (int a = 0; a < UIElements.Length; a++)
                UIElements[a].Show();
        }
    }
    
    public void ChangeEditorSimulationSpeed(float value)
    {
        GM.Instance.editorSimulationSpeed = value;
    }

    public void MagentaColor()
    {
        ColorUtility.TryParseHtmlString("#FF00FF", out Color color);
        material.SetColor("_Color", color);
    }

    public void BlueColor()
    {
        ColorUtility.TryParseHtmlString("#0000FF", out Color color);
        material.SetColor("_Color", color);
    }

    public void GreenColor()
    {
        ColorUtility.TryParseHtmlString("#00B140", out Color color);
        material.SetColor("_Color", color);
    }
    
    public void OriginalColor()
    {
        ColorUtility.TryParseHtmlString("#AECDFF", out Color color);
        material.SetColor("_Color", color);
    }

    public void AddCars(int value)
    {
        int carIndex = value;
        GM.Instance.cars[carIndex].carLevel += 1;
        PlayerPrefs.SetInt(GM.Instance.cars[carIndex].carName, GM.Instance.cars[carIndex].carLevel);
        GM.Instance.trafficController.AddCar(carIndex);
        GM.Instance.trafficController.ProcessProductionIndex();
        UIM.Instance.UpdateEconomyUI();
    }
}
