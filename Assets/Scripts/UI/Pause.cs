using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseCanvas;
    private bool shouldUnpause;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            bool isOpen = pauseCanvas.activeSelf;
            pauseCanvas.SetActive(!isOpen);
            if (isOpen)
            {
                Time.timeScale = 1;
                player.GetComponent<PlayerMovement>().enabled = true;
                // audio
                FindObjectOfType<AudioManager>().UnMute();
            }
            else
            {
                Time.timeScale = 0;
                player.GetComponent<PlayerMovement>().enabled = false;

                // audio
                FindObjectOfType<AudioManager>().Mute();
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab) && pauseCanvas.activeSelf)
        {
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
    public void ResetTime()
    {
        Time.timeScale = 1;
        player.GetComponent<PlayerMovement>().enabled = true;
        pauseCanvas.SetActive(!pauseCanvas.activeSelf);
    }
}
