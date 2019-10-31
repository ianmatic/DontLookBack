using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private const int lastLevel = 3;

    private int currentLevel;
    private bool[] levelBeat;
    protected GameManager()
    {
        currentLevel = 0;
        levelBeat = new bool[lastLevel];
    }

    public void BeatLevel()
    {
        levelBeat[currentLevel] = true;
    }

    public bool IsLastLevel()
    {
        return currentLevel + 1 == lastLevel;
    }

    public bool IsLevelBeat(int level)
    {
        if(level >= lastLevel) { return true; }
        return levelBeat[level];
    }

    public int CurrentLevel
    {
        get
        {
            return currentLevel + 1;
        }
        set
        {
            currentLevel = value - 1;
        }
    }

    public void ResetLevelBeat()
    {
        for(int i = 0; i < lastLevel; i++)
        {
            levelBeat[i] = false;
        }
    }
}
