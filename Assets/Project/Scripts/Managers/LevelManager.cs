using UnityExtensions;

public class LevelManager : MonoSingleton<LevelManager>
{

    public Level[] levels;
    public int[] levelIndexes = {0, 0};
    public Level level;
    
    public void CreateLevel()
    {
        if (FindObjectOfType<Level>())
            return;
        levelIndexes = GetLevelIndexes();
        level = Instantiate(levels[levelIndexes[0]]);
        for (int a = 0; a < level.sections[levelIndexes[1]].elements.Count; a++)
                level.sections[levelIndexes[1]].elements[a].Show();
        
    }


    int[] GetLevelIndexes()
    {
        int sectionIndex = 0;
        for (int a = 0; a < levels.Length; a++)
        {
            for (int b = 0; b < levels[a].sections.Length; b++)
            {
                if (GameManager.Instance.upgrades[1].upgradeLevel == sectionIndex)
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
    }
    
}