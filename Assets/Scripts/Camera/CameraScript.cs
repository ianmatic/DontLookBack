using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public RoomManager roomManager;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        SetupVirtualCameras();
    }




    void SetupVirtualCameras()
    {
        foreach (GameObject room in roomManager.RoomList)
        {
            foreach (Transform child in room.transform)
            {
                // setup camera
                if (child.tag == "MainCamera")
                {
                    child.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = player.transform;
                    child.gameObject.SetActive(false);
                } 
                else if (child.name == "Spot Light") // setup light
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}
