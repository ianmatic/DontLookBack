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
        GameObject virCam;
        foreach (GameObject room in roomManager.RoomList)
        {
            virCam = room.transform.GetChild(0).gameObject;
            virCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = player.transform;
            virCam.SetActive(false);
            room.transform.GetChild(1).gameObject.SetActive(false);
        }
    }
}
