using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Transform levelSelectButtons = GameObject.Find("LevelSelction_Panel").transform;
        for(int i = 0; i < levelSelectButtons.childCount; i++)
        {
            if(!GameManager.Instance.IsLevelBeat(i))
            {
                Destroy(levelSelectButtons.GetChild(i).gameObject);
            }
        }
        levelSelectButtons.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
