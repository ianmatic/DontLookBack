using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameObject player;
    GameObject enemy;
    Animator animator;

    bool doorOpen;
    float doorOpenTimer;
    public bool needKey;
    public bool exitDoor;

    public Material lockedTexture;
    public Material unlockedTexture;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        animator = gameObject.GetComponent<Animator>();

        doorOpen = false;
        doorOpenTimer = 3f;
        if(needKey) { ApplyDoorTexture(lockedTexture); }
    }

    void Update()
    {
        if(NearDoor() && (!EnemyNearDoor() || !doorOpen) ) // Checks if the player is near the door
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

        if(EnemyNearDoor())
        {
            if(!doorOpen){
                enemyPathfinding enemyScript = enemy.GetComponent<enemyPathfinding>();
                enemyScript.Door = this;
                if(enemyScript.enemyStateProp != enemyPathfinding.State.Opening){
                enemyScript.enemyStateProp = enemyPathfinding.State.Opening;
                }
                if(!needKey)
                {
                    doorOpenTimer-= Time.deltaTime;
                    if(doorOpenTimer < 0){
                        enemyScript.revertState();
                        animator.SetBool("doorOpen", true);
                        doorOpen = true;
                        doorOpenTimer = 3f;
                    }
                }
            }
        }
    }

    bool NearDoor() // Checks if a player is near the door
    {
        return (gameObject.transform.position - player.transform.position).magnitude < 1.5f;
    }

    bool EnemyNearDoor()
    {
        return (gameObject.transform.position - enemy.transform.position).magnitude < 2.5f;
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

    public float DoorOpenTimer
    {
        get { return doorOpenTimer;}
    }
}
