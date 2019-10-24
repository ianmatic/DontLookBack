using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameObject player;
    Animator animator;

    bool doorOpen;
    public bool needKey;
    public bool exitDoor;

    public Material lockedTexture;
    public Material unlockedTexture;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = gameObject.GetComponent<Animator>();

        doorOpen = false;
        if(needKey) { ApplyDoorTexture(lockedTexture); }
    }

    void Update()
    {
        if(NearDoor()) // Checks if the player is near the door
        {
            if(Input.GetKeyDown(KeyCode.E)) // Player can press 'e' to interact
            {
                if(!needKey)
                {
                    animator.SetBool("doorOpen", !animator.GetBool("doorOpen"));
                    doorOpen = !doorOpen;

                    if (exitDoor)
                    {
                        SceneLoader.LoadScene("victoryScene");
                    }
                }
            }
        }
    }

    bool NearDoor() // Checks if a player is near the door
    {
        return (gameObject.transform.position - player.transform.position).magnitude < 1.5f;
    }

    public void OpenLock() //Uses a key on the door
    {
        needKey = false;
        ApplyDoorTexture(unlockedTexture);
    }

    void ApplyDoorTexture(Material m)
    {
        transform.GetChild(0).GetComponent<Renderer>().material = m;
    }

    public bool DoorOpen
    {
        get { return doorOpen; }
    }
}
