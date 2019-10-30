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
