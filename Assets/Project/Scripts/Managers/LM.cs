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
        for (int a = 0; a < level.sections[levelIndexes[1]].elements.Count; a++)
                level.sections[levelIndexes[1]].elements[a].Show();
        if (level.sections[levelIndexes[1]].cam)
        {
            level.sections[levelIndexes[1]].cam.Hide();
            GM.Instance.cam.transform.position = level.sections[levelIndexes[1]].cam.position;
            GM.Instance.cam.transform.rotation = level.sections[levelIndexes[1]].cam.rotation;
        }
        UpdateGates();
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
        if(level.sections[levelIndexes[1]].cam)
        {
            level.sections[levelIndexes[1]].cam.Hide();
            GM.Instance.cam.transform.DOMove(level.sections[levelIndexes[1]].cam.position, 1f).SetEase(Ease.InOutSine);
            GM.Instance.cam.transform.DORotateQuaternion(level.sections[levelIndexes[1]].cam.rotation, 1f).SetEase(Ease.InOutSine);
        }
    }
    
}