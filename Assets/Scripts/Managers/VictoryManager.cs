using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(GameManager.Instance.CurrentLevel);
        if(GameManager.Instance.IsLastLevel())
        {
            Destroy(GameObject.Find("Next_Level_Button"));
        }
        GameManager.Instance.BeatLevel();
        Debug.Log(GameManager.Instance.CurrentLevel);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
