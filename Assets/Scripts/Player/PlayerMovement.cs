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
    }

    // Update is called once per frame
    void Update()
    {
        ChangeMovement();
        CheckCollisions();
        MovePlayer();
    }

    void CheckCollisions()
    {
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
            CheckWallCollision(room.transform.GetChild(2));
        }
    }

    //Finds all 4 walls to the room and checks for collision
    void CheckWallCollision(Transform roomWalls)
    {
        for(int i = 1; i < roomWalls.childCount; i++)
        {
            Transform wall = roomWalls.GetChild(i);
            if (wall.GetComponent<BoxCollider>().bounds.Intersects(gameObject.GetComponent<BoxCollider>().bounds))
            {
                if(wall.gameObject.name == "LeftWall")
                {
                    movement.x = 0;
                    transform.position = new Vector3(wall.position.x + wall.lossyScale.z + (transform.localScale.x / 2), transform.position.y);
                }
                if (wall.gameObject.name == "RightWall")
                {
                    movement.x = 0;
                    transform.position = new Vector3(wall.position.x - wall.lossyScale.z - (transform.localScale.x / 2), transform.position.y);
                }
                if (wall.gameObject.name == "BottomWall" && specialPlayerState != SpecialPlayerState.OnLadder)
                {
                    movement.y = 0;
                    transform.position = new Vector3(transform.position.x, wall.position.y + wall.lossyScale.y + (transform.localScale.y / 2));
                }
            }
        }
    }

    void ChangeMovement()
    {
        if(horizontalMove && Input.GetKey(KeyCode.LeftArrow))
        {
            movement = new Vector3(-1.0f, 0.0f);
        }
        else if(horizontalMove && Input.GetKey(KeyCode.RightArrow))
        {
            movement = new Vector3(1.0f, 0.0f);
        }
        else if(verticalMove && Input.GetKey(KeyCode.UpArrow))
        {
            movement = new Vector3(0.0f, 1.0f);
        }
        else if(verticalMove && Input.GetKey(KeyCode.DownArrow))
        {
            movement = new Vector3(0.0f, -1.0f);
        }
        else
        {
            movement = new Vector3(0.0f, 0.0f);
        }

        movement *= speed * Time.deltaTime;
    }

    void MovePlayer()
    {
        //position += movement;

        transform.position += movement; // Changed this so collisions could work. - TJ
    }
}
