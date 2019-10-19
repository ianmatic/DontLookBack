using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SpecialPlayerState
{
    OnLadder,
    None
}

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector3 movement;
    private bool verticalMove;
    private bool horizontalMove;
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
        verticalMove = false;
        horizontalMove = true;
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
                    verticalMove = true;
                    specialPlayerState = SpecialPlayerState.OnLadder;
                }
                break;
            case SpecialPlayerState.OnLadder: // On ladder
                if (!PlayerOnLadder()) // no longer touching ladder
                {
                    verticalMove = false;
                    specialPlayerState = SpecialPlayerState.None;
                }
                break;
        }
    }

    bool PlayerOnLadder()
    {
        foreach (GameObject ladder in roomManager.LadderList)
        {
            if (ladder.GetComponent<Renderer>().bounds.Intersects(gameObject.GetComponent<BoxCollider>().bounds))
            {
                return true; // touching some ladder
            }
        }
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
                if ((specialPlayerState == SpecialPlayerState.OnLadder && !wall.GetComponent<WallProperties>().isPasable) || // trying to go through impassable wall on ladder
                    (specialPlayerState != SpecialPlayerState.OnLadder)) // not on ladder, so apply collision for all walls 
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
        if (horizontalMove && Input.GetKey(KeyCode.LeftArrow))
        {
            movement = new Vector3(-1.0f, 0.0f);
        }
        else if (horizontalMove && Input.GetKey(KeyCode.RightArrow))
        {
            movement = new Vector3(1.0f, 0.0f);
        }
        else if (verticalMove && Input.GetKey(KeyCode.UpArrow))
        {
            movement = new Vector3(0.0f, 1.0f);
        }
        else if (verticalMove && Input.GetKey(KeyCode.DownArrow))
        {
            movement = new Vector3(0.0f, -1.0f);
        }
        else
        {
            movement = new Vector3(0.0f, 0.0f);
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
