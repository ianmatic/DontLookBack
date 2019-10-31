using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.BeatLevel();
        GameObject nextLevelButton = GameObject.Find("Next_Level_Button");
        GameObject mainMenuButton = GameObject.Find("Main_Menu_Button");
        GameObject youWon = GameObject.Find("You_Won");
        if(GameManager.Instance.IsLastLevel())
        {
            Destroy(nextLevelButton);
            mainMenuButton.transform.position = new Vector3(youWon.transform.position.x, mainMenuButton.transform.position.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
