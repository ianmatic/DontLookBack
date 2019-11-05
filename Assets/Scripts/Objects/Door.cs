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
        if (needKey) { AlterDoorLight(lockColor); }
        doorOpenTimer = 1.5f;
    }

    void Update()
    {
        if (NearDoor() && (!EnemyNearDoor() || !doorOpen)) // Checks if the player is near the door
        {
            if (Input.GetKeyDown(KeyCode.E)) // Player can press 'e' to interact
            {
                if (!needKey)
                {
                    if (LeftOrRight(player.transform.position))
                    {
                        if (animator.GetBool("doorOpenLeft"))
                        {
                            animator.SetBool("doorOpenLeft", !animator.GetBool("doorOpenLeft"));
                            doorOpen = !doorOpen;
                        }
                        else
                        {
                            animator.SetBool("doorOpen", !animator.GetBool("doorOpen"));
                            doorOpen = !doorOpen;
                        }
                    }
                    else
                    {
                        if (animator.GetBool("doorOpen"))
                        {
                            animator.SetBool("doorOpen", !animator.GetBool("doorOpen"));
                            doorOpen = !doorOpen;
                        }
                        else
                        {
                            animator.SetBool("doorOpenLeft", !animator.GetBool("doorOpenLeft"));
                            doorOpen = !doorOpen;
                        }
                    }


                    if (doorOpen)
                    {
                        FindObjectOfType<AudioManager>().Play("playerOpen", gameObject);
                    }
                    else
                    {
                        FindObjectOfType<AudioManager>().Play("playerClose", gameObject);
                    }

                    if (transform.childCount > 1) { Destroy(transform.GetChild(1).gameObject); }

                    if (exitDoor)
                    {
                        SceneLoader.LoadScene("victoryScene");
                    }
                }
                else
                {
                    FindObjectOfType<AudioManager>().Play("doorLocked", gameObject);
                }
            }
        }
        enemyPathfinding enemyScript = enemy.GetComponent<enemyPathfinding>();
        if (EnemyNearDoor() && enemyScript.EnemyState != enemyPathfinding.State.Climbing) // enemy sometimes tries to open door while climbing, so don't check when climbing
        {
            if (!doorOpen)
            {
                enemyScript.Door = this;
                if (enemyScript.EnemyState != enemyPathfinding.State.Opening) // enemyStoredState not set yet
                {
                    enemyScript.EnemyState = enemyPathfinding.State.Opening;
                }

                if (!needKey)
                {
                    if (enemyScript.EnemyStoredState == enemyPathfinding.State.Hunting)
                    {
                        enemyScript.HuntTimer -= Time.deltaTime;
                    }
                    doorOpenTimer -= Time.deltaTime;
                    if (doorOpenTimer < 0)
                    {
                        enemyScript.revertState();
                        animator.SetBool("doorOpen", true);
                        doorOpen = true;
                        doorOpenTimer = 1.5f;

                        // audio
                        FindObjectOfType<AudioManager>().Play("enemyDoor", gameObject);
                    }
                }
                else
                {
                    enemyScript.revertState();
                    enemyScript.SendBack();
                }
            } else {
                if (enemyScript.EnemyState == enemyPathfinding.State.Opening)
                {
                    enemyScript.revertState();
                }
            }

        }
    }

    bool NearDoor() // Checks if a player is near the door
    {
        return (gameObject.transform.position - player.transform.position).magnitude < 3.5f;
    }

    bool EnemyNearDoor()
    {
        return Vector2.Distance(gameObject.transform.position, enemy.transform.position) < 1.0f;
    }

    public void OpenLock() //Uses a key on the door
    {
        needKey = false;
        FindObjectOfType<AudioManager>().Play("doorUnlocked", gameObject);
        AlterDoorLight(unlockColor);
    }

    bool LeftOrRight(Vector3 position)
    {
        return (gameObject.transform.position.x - position.x) > 0;
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
    public float DoorOpenTimer
    {
        get { return doorOpenTimer; }
    }
}
