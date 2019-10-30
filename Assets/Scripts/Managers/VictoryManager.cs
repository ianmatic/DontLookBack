using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        if(GameManager.Instance.IsLastLevel())
        {
            Destroy(GameObject.Find("Next_Level_Button"));
        }
        GameManager.Instance.BeatLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
