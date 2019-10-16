using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public List<GameObject> rooms;
    public GameObject currentRoom;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        rooms = new List<GameObject>();
        rooms.AddRange(GameObject.FindGameObjectsWithTag("Room"));
        player = GameObject.FindGameObjectWithTag("Player");
        
        SetupVirtualCameras();
        currentRoom = FindCurrentRoom();
        TransitionToRoom(currentRoom);
    }

    // Update is called once per frame
    void Update()
    {
        // player moved to new room
        if (currentRoom != FindCurrentRoom())
        {
            // disable current room
            currentRoom.transform.GetChild(0).gameObject.SetActive(false);
            currentRoom.transform.GetChild(1).gameObject.SetActive(false);

            TransitionToRoom(FindCurrentRoom());
        }
    }


    void TransitionToRoom(GameObject room)
    {
        room.transform.GetChild(0).gameObject.SetActive(true);
        room.transform.GetChild(1).gameObject.SetActive(true);
        currentRoom = room;
    }

    GameObject FindCurrentRoom()
    {
        foreach (GameObject room in rooms)
        {
            if (room.GetComponent<BoxCollider>().bounds.Contains(player.transform.position))
            {
                return room;
            }
        }

        return null;
    }

    void SetupVirtualCameras()
    {
        GameObject virCam;
        foreach (GameObject room in rooms)
        {
            virCam = room.transform.GetChild(0).gameObject;
            virCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = player.transform;
            virCam.SetActive(false);
            room.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
