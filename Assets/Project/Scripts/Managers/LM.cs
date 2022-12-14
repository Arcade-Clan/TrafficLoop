using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityExtensions;

public class LM : MonoSingleton<LM>
{

    public Level[] levels;
    public int[] levelIndexes = {0, 0};
    public Level level;
    public Gate[] gates;
    public void CreateLevel()
    {
        levelIndexes = GetLevelIndexes();
        gates = level.GetComponentsInChildren<Gate>(true);
        for (int a = 0; a < gates.Length; a++)
            gates[a].value = a + 1;
        for (int a = 0; a < level.sections[levelIndexes[1]].elements.Count; a++)
            level.sections[levelIndexes[1]].elements[a].Show();
        if (level.sections[levelIndexes[1]].cam)
        {
            level.sections[levelIndexes[1]].cam.Hide();
            GM.Instance.cam.transform.position = level.sections[levelIndexes[1]].cam.position;
            GM.Instance.cam.transform.rotation = level.sections[levelIndexes[1]].cam.rotation;
        }

        ProcessTaggedObjects();
        UpdateGates();
    }

    public Ease roadScaleEase = Ease.OutBack;
    public float roadScaleDuration = 0.5f;
    public Ease roadPositionEase = Ease.OutSine;
    public float roadStartHeight = 10;
    public float roadPositionDuration = 0.5f;
    public float roadDelayDuration = 0.05f;
    
    public void ProcessTaggedObjects()
    {
        List<GameObject> roads = GameObject.FindGameObjectsWithTag("Road").ToList();
        roads = roads.OrderBy(p => Mathf.Abs(p.transform.position.x+p.transform.position.z)).ToList();
        for (int a = 0; a < roads.Count; a++)
            Place(roads[a].transform,a);
    }
    
    private void Place(Transform road,int index)
    {
        road.tag = "Untagged";
        Vector3 position = road.localPosition;
        Vector3 scale = road.localScale;
        road.localScale = Vector3.zero;
        road.localPosition += Vector3.up * roadStartHeight;
        road.DOScale(scale, roadScaleDuration).SetDelay(index* roadDelayDuration).SetEase(roadScaleEase).SetUpdate(UpdateType.Normal,true);
        road.DOLocalMoveY(position.y, roadPositionDuration).SetDelay(index * roadDelayDuration).SetEase(roadPositionEase).SetUpdate(UpdateType.Normal, true);
    }
    
    public void UpdateGates()
    {
        for (int a = 0; a < gates.Length; a++)
        {
            if(GM.Instance.upgrades[2].upgradeLevel>=a)
                gates[a].Show();
            else
                gates[a].Hide();
        }  
    }


    int[] GetLevelIndexes()
    {
        int sectionIndex = 0;
        for (int a = 0; a < levels.Length; a++)
        {
            for (int b = 0; b < levels[a].sections.Length; b++)
            {
                if (GM.Instance.upgrades[1].upgradeLevel == sectionIndex)
                    return new []{a, b};
                sectionIndex += 1;
            }
        }
        return new []{-1, -1};
    }

    public void SwitchLevel()
    {
        int[] oldLevelIndexes = levelIndexes;
        levelIndexes = GetLevelIndexes();

        if (oldLevelIndexes[0] != levelIndexes[0])
        {
            Destroy(level.gameObject);
            level = Instantiate(levels[levelIndexes[0]]);
            for (int a = 0; a < level.sections[levelIndexes[1]].elements.Count; a++)
                level.sections[levelIndexes[1]].elements[a].Show();
        }
        else
        {
            for (int a = 0; a < level.sections[oldLevelIndexes[1]].elements.Count; a++)
                level.sections[oldLevelIndexes[1]].elements[a].Hide();
            for (int a = 0; a < level.sections[levelIndexes[1]].elements.Count; a++)
                level.sections[levelIndexes[1]].elements[a].Show();
        }
        
        GM.Instance.trafficController.RecalculateTrafficElements();
        ProcessTaggedObjects();
        if(level.sections[levelIndexes[1]].cam)
        {
            level.sections[levelIndexes[1]].cam.Hide();
            GM.Instance.cam.transform.DOMove(level.sections[levelIndexes[1]].cam.position, 1f).SetEase(Ease.InOutSine);
            GM.Instance.cam.transform.DORotateQuaternion(level.sections[levelIndexes[1]].cam.rotation, 1f).SetEase(Ease.InOutSine);
        }
    }
    
}