using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private GameManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameManager.Instance;
        GameObject levelSelectButtons = GameObject.Find("LevelSelection_Panel");
        levelSelectButtons.SetActive(false);
        for(int i = 1; i < levelSelectButtons.transform.childCount; i++)
        {
            if(!GameManager.Instance.IsLevelBeat(i))
            {
                Destroy(levelSelectButtons.transform.GetChild(i - 1).gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
