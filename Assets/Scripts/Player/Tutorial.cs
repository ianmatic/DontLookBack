using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public GameObject targetRoom; // room that must be entered to end the tutorial
    public GameObject targetSpot; // spot that is only revealed when tutorial is over
    [HideInInspector]
    public bool inTutorial = true;

    private RoomManager roomManager;
    // Start is called before the first frame update
    void Start()
    {
        roomManager = FindObjectOfType<RoomManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (roomManager.PlayerRoom == targetRoom) // player has finished the tutorial by entering the target room
        {
            inTutorial = false;
            targetSpot.SetActive(true); // show the hiding spot
            GameObject.FindGameObjectWithTag("Enemy").GetComponent<enemyPathfinding>().enabled = true; // enable the enemy
            gameObject.SetActive(false);
        }
        else
        {
            inTutorial = true;
            targetSpot.SetActive(false); // hide the hiding spot
            GameObject.FindGameObjectWithTag("Enemy").GetComponent<enemyPathfinding>().enabled = false; // disable the enemy
        }
    }
}
