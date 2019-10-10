using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    public List<GameObject> rooms;
    public GameObject currentRoom;
    int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        rooms = new List<GameObject>();
        rooms.AddRange(GameObject.FindGameObjectsWithTag("testTag"));
        currentRoom = rooms[0];
        currentRoom.transform.GetChild(0).gameObject.SetActive(true);
        TransitionToRoom(currentRoom);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentRoom.transform.GetChild(0).gameObject.SetActive(false);
            currentRoom = rooms[i];
            if (i == 3)
            {
                i = 0;
            }
            else
            {
                i++;
            }
            TransitionToRoom(currentRoom);
        }
      
    }


    void TransitionToRoom(GameObject room)
    {
        room.transform.GetChild(0).gameObject.SetActive(true);
    }
}
