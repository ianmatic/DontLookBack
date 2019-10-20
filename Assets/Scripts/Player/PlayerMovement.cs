using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SpecialPlayerState
{
    OnLadder,
    None,
    Stairs,
}

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector3 movement;
    private SpecialPlayerState specialPlayerState = SpecialPlayerState.None;
    private RoomManager roomManager;
    private Vector3 futurePos;
    private bool willCollide = false;

    // Start is called before the first frame update
    void Start()
    {
        if (speed == 0.0f)
        {
            speed = 1.0f;
        }
        movement = new Vector3(0.0f, 0.0f);
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        futurePos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        willCollide = false;
        ChangeMovement();
        CheckCollisions();
        MovePlayer();
    }

    void CheckCollisions()
    {
        EnemyCollision();
        LadderCollision();
        StairCollision();
        DoorCollision();
        RoomCollision();
    }

    void LadderCollision()
    {
        switch (specialPlayerState) // Switches between whether the player is on the ladder or off the ladder. 
        {
            case SpecialPlayerState.None: // Off ladder
                if (PlayerOnLadder()) // touching ladder
                {
                    specialPlayerState = SpecialPlayerState.OnLadder;
                }
                break;
            case SpecialPlayerState.OnLadder: // On ladder
                if (!PlayerOnLadder()) // no longer touching ladder
                {
                    specialPlayerState = SpecialPlayerState.None;
                }
                break;
        }
    }

    /// <summary>
    /// Puts the player in stair mode, and takes them out of stair mode based on positioning and input
    /// </summary>
    void StairCollision()
    {
        if (roomManager.CurrentStair) // has used stairs
        {
            Bounds topStair = roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;
            bool aboveTopStair = futurePos.y - GetComponent<Renderer>().bounds.extents.y > topStair.center.y + topStair.extents.y; // bottom of player is above top stair
            float temp = GetComponent<Renderer>().bounds.extents.y;
            float result = futurePos.y - GetComponent<Renderer>().bounds.extents.y - 0.05f;
            bool belowBottomStair = futurePos.y - GetComponent<Renderer>().bounds.extents.y - 0.05f < bottomStair.center.y - bottomStair.extents.y;  //bottom of player is below bottom stair
            if (specialPlayerState == SpecialPlayerState.Stairs && // on stairs and above or below stairs
                (aboveTopStair || belowBottomStair))
            {
                // reset z value
                futurePos.z = 0;
                specialPlayerState = SpecialPlayerState.None;
                roomManager.CurrentStair = null;
            }
        }

        foreach (GameObject stair in roomManager.StairList)
        {
            Bounds topStair = stair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = stair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;
            if ((GetComponent<Collider>().bounds.Intersects(bottomStair) || GetComponent<Collider>().bounds.Intersects(topStair)) // touching the stairs while not already on them
                && specialPlayerState != SpecialPlayerState.Stairs)
            {
                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow)) // need to press appropriate key to start climbing stairs
                {
                    specialPlayerState = SpecialPlayerState.Stairs;
                    roomManager.CurrentStair = stair;
                }
            }            
        }
    }

    bool PlayerOnLadder()
    {
        foreach (GameObject ladder in roomManager.LadderList)
        {
            if (ladder.GetComponent<BoxCollider>().bounds.Intersects(gameObject.GetComponent<BoxCollider>().bounds))
            {
                gameObject.GetComponent<Rigidbody>().useGravity = false;
                return true; // touching some ladder
            }
        }
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        return false; // not touching any ladder
    }

    void DoorCollision()
    {

    }

    //Checks for each room and looks at wall collisions
    void RoomCollision()
    {
        foreach (GameObject room in roomManager.RoomList)
        {
            foreach (Transform child in room.transform)
            {
                if (child.name.Contains("Wall"))
                {
                    CheckWallCollision(child);
                }
            }

        }
    }

    //Finds all 4 walls to the room and checks for collision
    void CheckWallCollision(Transform wall)
    {
        Bounds futureBounds = new Bounds(futurePos, gameObject.GetComponent<Renderer>().bounds.size);
        if (wall.GetComponent<BoxCollider>().bounds.Intersects(futureBounds))
        {
            if (wall.gameObject.name == "LeftWall")
            {
                movement.x = 0;
                transform.position = new Vector3(wall.position.x + wall.GetComponent<Renderer>().bounds.size.x / 2 + (transform.localScale.x / 2) + 0.01f, transform.position.y);
                willCollide = true;
            }
            if (wall.gameObject.name == "RightWall")
            {
                movement.x = 0;
                transform.position = new Vector3(wall.position.x - wall.GetComponent<Renderer>().bounds.size.x / 2 - (transform.localScale.x / 2) + 0.01f, transform.position.y);
                willCollide = true;
            }
            if (wall.gameObject.name == "BottomWall")
            {
                if ((specialPlayerState != SpecialPlayerState.None && !wall.GetComponent<WallProperties>().isPasable) || // trying to go through impassable wall on ladder or stairs
                    (specialPlayerState == SpecialPlayerState.None)) // not on ladder nor stairs, so apply collision for all walls 
                {
                    movement.y = 0;
                    transform.position = new Vector3(transform.position.x, wall.position.y + wall.GetComponent<Renderer>().bounds.size.y / 2 + (transform.localScale.y / 2) + 0.01f);
                    willCollide = true;
                }
            }
        }
    }

    /// <summary>
    /// Check collision with enemy(s)
    /// </summary>
    void EnemyCollision()
    {
        if (GetComponent<Renderer>().bounds.Intersects(GameObject.FindGameObjectWithTag("Enemy").GetComponent<Renderer>().bounds))
        {
            KillPlayer();
        }
    }

    /// <summary>
    /// Kills the player and presents game over screen or restarts game
    /// </summary>
    void KillPlayer()
    {
        gameObject.SetActive(false);
    }

    void ChangeMovement()
    {
        switch (specialPlayerState)
        {
            case SpecialPlayerState.None:
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    movement = new Vector3(-1.0f, 0.0f);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    movement = new Vector3(1.0f, 0.0f);
                }
                else
                {
                    movement = new Vector3(0.0f, 0.0f);
                }
                break;
            case SpecialPlayerState.OnLadder:
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    movement = new Vector3(-1.0f, 0.0f);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    movement = new Vector3(1.0f, 0.0f);
                }
                else if (Input.GetKey(KeyCode.UpArrow))
                {
                    movement = new Vector3(0.0f, 1.0f);
                } 
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    movement = new Vector3(0.0f, -1.0f);
                }
                else 
                {
                    movement = new Vector3(0.0f, 0.0f);
                }
                break;
            case SpecialPlayerState.Stairs:
                StairProperties stair = roomManager.CurrentStair.GetComponent<StairProperties>();
                Vector3 stairDir = stair.topStair.transform.position - stair.bottomStair.transform.position;

                if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow)) // move down
                {
                    // set the z value
                    transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                    movement = -stairDir.normalized / 1.5f;
                }
                else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.RightArrow)) // move up
                {
                    // set the z value
                    transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                    movement = stairDir.normalized / 1.5f;
                }
                break;

        }

        movement *= speed * Time.deltaTime;

        futurePos = transform.position + movement; // where the player wants to go
    }

    void MovePlayer()
    {
        if (!willCollide) // safe to move to new pos
        {
            transform.position = futurePos; // Changed this so collisions could work. - TJ
        }
    }
}
