using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameObject player;
    GameObject enemy;
    Animator animator;

    bool doorOpen;
    public bool needKey;
    public bool exitDoor;

    private Color lockColor;
    private Color unlockColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        animator = gameObject.GetComponent<Animator>();

        lockColor = Color.red;
        unlockColor = Color.green;

        doorOpen = false;
        if(needKey) { AlterDoorLight(lockColor); }
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

                    if (doorOpen)
                    {
                        FindObjectOfType<AudioManager>().Play("playerOpen");
                    } else
                    {
                        FindObjectOfType<AudioManager>().Play("playerClose");
                    }

                    if(transform.childCount > 1) { Destroy(transform.GetChild(1).gameObject); }

                    if (exitDoor)
                    {
                        SceneLoader.LoadScene("victoryScene");
                    }
                } else
                {
                    FindObjectOfType<AudioManager>().Play("doorLocked");
                }
            }
        }

        if(EnemyNearDoor())
        {
            animator.SetBool("doorOpen", true);
        }
    }

    bool NearDoor() // Checks if a player is near the door
    {
        return (gameObject.transform.position - player.transform.position).magnitude < 2.0f;
    }

    bool EnemyNearDoor()
    {
        return (gameObject.transform.position - enemy.transform.position).magnitude < 2.5f;
    }

    public void OpenLock() //Uses a key on the door
    {
        needKey = false;
        FindObjectOfType<AudioManager>().Play("doorUnlocked");
        AlterDoorLight(unlockColor);
    }

    public bool DoorOpen
    {
        get { return doorOpen; }
    }

    void AlterDoorLight(Color c)
    {
        Light doorlight = transform.GetChild(1).GetComponent<Light>();
        doorlight.color = c;
    }
}
