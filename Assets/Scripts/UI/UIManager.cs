using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    /// <summary>
    /// Temporary until we get game over screen
    /// </summary>
    private void OnGUI()
    {
        // Dead Player
        if (!player.activeSelf)
        {
            // Game Over Label
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2 - 20, 100, 20), "Game Over");

            // Restart Button
            if(GUI.Button(new Rect(Screen.width / 2 - 140, Screen.height / 2 + 50, 150, 50), "Click Here To Restart"))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
