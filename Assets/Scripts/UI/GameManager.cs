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
        currentLevel = 1;
        levelBeat = new bool[lastLevel];
        levelBeat[0] = true;
    }

    public void BeatLevel()
    {
        if (currentLevel >= 0 && currentLevel < levelBeat.Length)
        {
            levelBeat[currentLevel] = true;
        }

    }

    public bool IsLastLevel()
    {
        return currentLevel == lastLevel;
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
            return currentLevel;
        }
        set
        {
            currentLevel = value;
        }
    }

    public void ResetLevelBeat()
    {
        for(int i = 1; i < lastLevel; i++)
        {
            levelBeat[i] = false;
        }
    }
}
