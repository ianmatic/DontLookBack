using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        if (sceneName.Substring(0, 5) == "Level")
        {
            if (GameManager.Instance.IsLevelBeat(int.Parse(sceneName.Substring(5))))
            {
                GameManager.Instance.CurrentLevel = int.Parse(sceneName.Substring(5));
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            }
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
    public void StartNewGame()
    {
        GameManager.Instance.CurrentLevel = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Level1");
    }
    public void RestartLevel()
    {
        string levelName = "Level" + GameManager.Instance.CurrentLevel;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
    public void LoadNextLevel()
    {
        GameManager.Instance.CurrentLevel = GameManager.Instance.CurrentLevel + 1;
        string levelName = "Level" + GameManager.Instance.CurrentLevel;
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
    static public void LoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(name);
    }
    public void EndGame()
    {
        Application.Quit();
    }
}
