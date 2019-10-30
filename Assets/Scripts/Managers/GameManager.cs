using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    private int currentLevel;
    private const int lastLevel = 3;
    private bool[] levelBeat;

    protected GameManager()
    {
        currentLevel = 0;
        levelBeat = new bool[lastLevel];
    }

    public int CurrentLevel
    {
        get { return currentLevel + 1; }
        set { currentLevel = value - 1; }
    }

    public void BeatLevel()
    {
        levelBeat[currentLevel] = true;
        if(!IsLastLevel())
        {
            currentLevel++;
        }
    }

    public bool IsLastLevel()
    {
        return currentLevel + 1 == lastLevel;
    }

    public bool IsLevelBeat(int level)
    {
        return levelBeat[level - 1];
    }
}
