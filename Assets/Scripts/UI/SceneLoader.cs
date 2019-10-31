using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public void LoadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public void LoadLevel()
    {
        string levelNum;
        if(sceneName == null) { levelNum = GameManager.Instance.CurrentLevel.ToString(); }
        else
        {
            levelNum = sceneName;
            GameManager.Instance.CurrentLevel = int.Parse(levelNum);
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level" + levelNum);
    }
    public void LoadNewGame()
    {
        GameManager.Instance.CurrentLevel = 1;
        GameManager.Instance.ResetLevelBeat();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
    public void LoadNextLevel()
    {
        GameManager.Instance.CurrentLevel++;
        LoadLevel();
    }
    static public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
    public void LoadSceneFromManager()
    {
        GameObject manager = GameObject.FindGameObjectWithTag("LevelManager");
        string currentLevel = manager.GetComponent<RoomManager>().nameOfCurrentLevel;
        Destroy(manager);
        LoadScene(currentLevel);
    }
    public void EndGame()
    {
        Application.Quit();
    }
}
