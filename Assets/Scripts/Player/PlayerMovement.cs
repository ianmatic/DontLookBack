﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum SpecialPlayerState
{
    OnLadder,
    None,
    Stairs,
    Hiding
}

enum PlayerMoveControl
{
    Left,
    Right,
    Up,
    Down,
    None
}

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector3 movement;
    private SpecialPlayerState specialPlayerState = SpecialPlayerState.None;
    private RoomManager roomManager;
    private Vector3 futurePos;
    private bool touchingFloorVerticalMovement = false;
    AudioManager audioManager;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        if (speed == 0.0f)
        {
            speed = 1.0f;
        }
        movement = new Vector3(0.0f, 0.0f);
        roomManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<RoomManager>();
        audioManager = FindObjectOfType<AudioManager>();

        foreach (Transform child in transform)
        {
            if (child.name == "playerModel")
            {
                animator = child.GetComponent<Animator>();
                break;
            }
        }

        futurePos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        ChangeMovement();
        CheckCollisions();
        MovePlayer();
    }

    void CheckCollisions()
    {
        EnemyCollision();
        DoorCollision();
        KeyCollision();
        RoomCollision();
        StairCollision();
        LadderCollision();
        HidingCollision();
    }


    void HidingCollision()
    {
        // Press C to toggle hiding
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (GameObject hidingSpot in roomManager.HidingSpotList)
            {
                // at a hiding spot
                if (hidingSpot.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
                {
                    roomManager.CurrentHidingSpot = hidingSpot;

                    audioManager.Play("hide", gameObject);
                    if (specialPlayerState == SpecialPlayerState.Hiding)
                    {
                        specialPlayerState = SpecialPlayerState.None;
                        futurePos.z = 0; // reset z value
                    }
                    else
                    {
                        specialPlayerState = SpecialPlayerState.Hiding;
                    }
                    break;
                }
            }
        }

        // set z value very frame, not just when when button pressed
        if (specialPlayerState == SpecialPlayerState.Hiding)
        {
            futurePos.z = roomManager.CurrentHidingSpot.transform.position.z;
        }
    }

    void LadderCollision()
    {
        // end ladder collision
        if (roomManager.CurrentLadder) // has used ladder
        {
            Bounds topLadder = roomManager.CurrentLadder.GetComponent<LadderProperties>().topLadder.GetComponent<Collider>().bounds;
            Bounds bottomLadder = roomManager.CurrentLadder.GetComponent<LadderProperties>().bottomLadder.GetComponent<Collider>().bounds;

            bool aboveTopLadder = GetComponent<Collider>().bounds.min.y > topLadder.max.y; // bottom of player is above top ladder
            bool belowBottomLadder = futurePos.y - GetComponent<Collider>().bounds.extents.y < bottomLadder.min.y || touchingFloorVerticalMovement;  //bottom of player is below bottom ladder, with some leeway space
            if (specialPlayerState == SpecialPlayerState.OnLadder && // on stairs and above or below ladder
                (aboveTopLadder || belowBottomLadder))
            {
                // reset z value
                futurePos.z = 0;
                specialPlayerState = SpecialPlayerState.None;
                roomManager.CurrentLadder = null;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }
        }

        // start ladder collision
        foreach (GameObject ladder in roomManager.LadderList)
        {
            Bounds topLadder = ladder.GetComponent<LadderProperties>().topLadder.GetComponent<Collider>().bounds;
            Bounds bottomLadder = ladder.GetComponent<LadderProperties>().bottomLadder.GetComponent<Collider>().bounds;

            if ((GetComponent<Collider>().bounds.Intersects(bottomLadder) || GetComponent<Collider>().bounds.Intersects(topLadder)) // touching the ladder while not already on it
                && specialPlayerState != SpecialPlayerState.OnLadder)
            {
                // at the top, so can only go down
                if (Vector3.Distance(transform.position, topLadder.center) < Vector3.Distance(transform.position, bottomLadder.center))
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    {
                        specialPlayerState = SpecialPlayerState.OnLadder;
                        roomManager.CurrentLadder = ladder;
                        gameObject.GetComponent<Rigidbody>().useGravity = false;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
                else // at the bottom, so can only go up
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    {
                        specialPlayerState = SpecialPlayerState.OnLadder;
                        roomManager.CurrentLadder = ladder;
                        gameObject.GetComponent<Rigidbody>().useGravity = false;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
            }
        }
    }

    void DoorCollision()
    {
        Bounds futureBounds = new Bounds(futurePos, GetComponent<Collider>().bounds.size);
        foreach (GameObject door in roomManager.DoorList)
        {
            foreach (Transform child in door.transform) // get the physical door (door itself has no renderer)
            {
                if (child.name == "DoorWall")
                {
                    if (child.GetComponent<BoxCollider>().bounds.Intersects(futureBounds) && !door.GetComponent<Door>().DoorOpen) // touching a closed door
                    {
                        if (child.GetComponent<Renderer>().bounds.center.x < futureBounds.center.x) // door is left of player
                        {
                            // place player on right side
                            movement.x = 0;
                            futurePos = new Vector3(door.transform.position.x + child.GetComponent<Renderer>().bounds.size.x / 2 + (transform.localScale.x / 2) + 0.01f, futurePos.y);
                        }
                        else if (child.GetComponent<Renderer>().bounds.center.x > futureBounds.center.x) // door is right of player
                        {
                            // place player on left side
                            movement.x = 0;
                            futurePos = new Vector3(door.transform.position.x - child.GetComponent<Renderer>().bounds.size.x / 2 - GetComponent<BoxCollider>().bounds.extents.x - 0.01f, futurePos.y);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Puts the player in stair mode, and takes them out of stair mode based on positioning and input
    /// </summary>
    void StairCollision()
    {
        // end stair collision
        if (roomManager.CurrentStair) // has used stairs
        {
            Bounds topStair = roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;

            bool aboveTopStair = GetComponent<Collider>().bounds.min.y > topStair.max.y; // bottom of player is above top stair
            bool belowBottomStair = futurePos.y - GetComponent<Collider>().bounds.extents.y < bottomStair.min.y || touchingFloorVerticalMovement;  //bottom of player is below bottom ladder, with some leeway space
            if (specialPlayerState == SpecialPlayerState.Stairs && // on stairs and above or below stairs
                (aboveTopStair || belowBottomStair))
            {
                // reset z value
                futurePos.z = 0;
                specialPlayerState = SpecialPlayerState.None;
                roomManager.CurrentStair = null;
                gameObject.GetComponent<Rigidbody>().useGravity = true;
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                //animator.gameObject.transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y + 90, transform.rotation.z);
            }
        }

        // start stair collision
        foreach (GameObject stair in roomManager.StairList)
        {
            Bounds topStair = stair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = stair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;

            if ((GetComponent<Collider>().bounds.Intersects(bottomStair) || GetComponent<Collider>().bounds.Intersects(topStair)) // touching the stairs while not already on them
                && specialPlayerState != SpecialPlayerState.Stairs)
            {
                 // at the top, so can only go down
                if (Vector3.Distance(transform.position, topStair.center) < Vector3.Distance(transform.position, bottomStair.center))
                {
                    if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    {
                        specialPlayerState = SpecialPlayerState.Stairs;
                        roomManager.CurrentStair = stair;
                        gameObject.GetComponent<Rigidbody>().useGravity = false;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
                else // at the bottom, so can only go up
                {
                    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    {
                        specialPlayerState = SpecialPlayerState.Stairs;
                        roomManager.CurrentStair = stair;
                        gameObject.GetComponent<Rigidbody>().useGravity = false;
                        gameObject.GetComponent<Rigidbody>().isKinematic = true;
                    }
                }
            }
        }
    }

    void KeyCollision()
    {
        foreach (GameObject key in roomManager.KeyList)
        {
            if (key.GetComponent<BoxCollider>().bounds.Intersects(gameObject.GetComponent<BoxCollider>().bounds))
            {
                key.GetComponent<Key>().GrabKey();
            }
        }
    }

    //Checks for each room and looks at wall collisions
    void RoomCollision()
    {
        touchingFloorVerticalMovement = false;
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
        Bounds futureBounds = new Bounds(futurePos, GetComponent<Collider>().bounds.size);
        if (wall.GetComponent<BoxCollider>().bounds.Intersects(futureBounds))
        {
            if (wall.gameObject.name == "LeftWall")
            {
                movement.x = 0;
                futurePos = new Vector3(wall.position.x + wall.GetComponent<Renderer>().bounds.extents.x + GetComponent<Collider>().bounds.extents.x + 0.01f, futurePos.y);
            }
            if (wall.gameObject.name == "RightWall")
            {
                movement.x = 0;
                futurePos = new Vector3(wall.position.x - wall.GetComponent<Renderer>().bounds.extents.x - GetComponent<Collider>().bounds.extents.x - 0.01f, futurePos.y);
            }
            if (wall.gameObject.name == "BottomWall")
            {
                if ((specialPlayerState != SpecialPlayerState.None && !wall.GetComponent<WallProperties>().isPasable) || // trying to go through impassable wall on ladder or stairs
                    (specialPlayerState == SpecialPlayerState.None)) // not on ladder nor stairs, so apply collision for all walls 
                {
                    movement.y = 0;
                    // super small number added to y to prevent stuck in collisions, but so small that gravity induced jitter can't be seen
                    futurePos = new Vector3(futurePos.x, wall.position.y + wall.GetComponent<Collider>().bounds.size.y / 2 + GetComponent<Collider>().bounds.extents.y + 0.000001f);

                    // player has touched the floor while on ladder or stairs
                    if (specialPlayerState == SpecialPlayerState.OnLadder || specialPlayerState == SpecialPlayerState.Stairs && (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)))
                    {
                        touchingFloorVerticalMovement = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check collision with enemy(s)
    /// </summary>
    void EnemyCollision()
    {
        if (GetComponent<Collider>().bounds.Intersects(GameObject.FindGameObjectWithTag("Enemy").GetComponent<Collider>().bounds))
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
        SceneLoader.LoadScene("endingScene");
    }
    void PlayerWins()
    {
        SceneLoader.LoadScene("victoryScene");
    }
    void ChangeMovement()
    {
        Quaternion currentOrientation;
        Quaternion targetOrientation;
        switch (specialPlayerState)
        {
            case SpecialPlayerState.None:
                //disable non normal move looping sounds
                audioManager.EndLoopSound("playerLadder");
                audioManager.EndLoopSound("playerStairs");
                switch (ControlMovement())
                {
                    case PlayerMoveControl.Left:
                        movement = new Vector3(-1.0f, 0.0f);
                        animator.SetBool("isRunning", true);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        currentOrientation = transform.rotation;
                        targetOrientation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                        transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .1f);
                        audioManager.PlayLoopSound("playerRun", gameObject);
                        break;
                    case PlayerMoveControl.Right:
                        movement = new Vector3(1.0f, 0.0f);
                        animator.SetBool("isRunning", true);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        currentOrientation = transform.rotation;
                        targetOrientation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                        transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .1f);
                        audioManager.PlayLoopSound("playerRun", gameObject);
                        break;
                    default:
                        movement = new Vector3(0.0f, 0.0f);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        audioManager.EndLoopSound("playerRun");
                        break;
                }
                break;
            case SpecialPlayerState.OnLadder:
                switch (ControlMovement())
                {
                    case PlayerMoveControl.Down:
                        // set the z value
                        transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentLadder.transform.position.z - .35f);
                        movement = new Vector3(0.0f, -1.0f);
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", true);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        animator.enabled = true;
                        audioManager.PlayLoopSound("playerLadder", gameObject);
                        break;
                    case PlayerMoveControl.Up:
                        // set the z value
                        transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentLadder.transform.position.z - .35f);
                        movement = new Vector3(0.0f, 1.0f);
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", true);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        animator.enabled = true;
                        audioManager.PlayLoopSound("playerLadder", gameObject);
                        break;
                    default:
                        animator.enabled = false; // pause animation
                        audioManager.EndLoopSound("playerLadder");
                        break;
                }
                break;
            case SpecialPlayerState.Stairs:
                StairProperties stair = roomManager.CurrentStair.GetComponent<StairProperties>();
                Vector3 stairDir = stair.topStair.transform.position - stair.bottomStair.transform.position;

                if (stair.tag == "RightStair")
                {
                    switch (ControlMovement())
                    {
                        case PlayerMoveControl.Left:
                        case PlayerMoveControl.Down:
                            // set the z value
                            transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                            movement = -stairDir.normalized / 1.5f;

                            animator.SetBool("isRunning", false);
                            animator.SetBool("isClimbing", false);
                            animator.SetBool("isScaling", true);
                            animator.SetBool("isHiding", false);
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                            animator.enabled = true;
                            audioManager.PlayLoopSound("playerStairs", gameObject);
                            break;
                        case PlayerMoveControl.Right:
                        case PlayerMoveControl.Up:
                            // set the z value
                            transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                            movement = stairDir.normalized / 1.5f;

                            animator.SetBool("isRunning", false);
                            animator.SetBool("isClimbing", false);
                            animator.SetBool("isScaling", true);
                            animator.SetBool("isHiding", false);
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                            animator.enabled = true;
                            audioManager.PlayLoopSound("playerStairs", gameObject);
                            break;
                        default:
                            animator.enabled = false; // pause animation
                            audioManager.EndLoopSound("playerStairs");
                            break;
                    }
                    break;
                }
                else
                {
                    switch (ControlMovement())
                    {
                        case PlayerMoveControl.Right:
                        case PlayerMoveControl.Down:
                            // set the z value
                            transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                            movement = -stairDir.normalized / 1.5f;

                            animator.SetBool("isRunning", false);
                            animator.SetBool("isClimbing", false);
                            animator.SetBool("isScaling", true);
                            animator.SetBool("isHiding", false);
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                            animator.enabled = true;
                            audioManager.PlayLoopSound("playerStairs", gameObject);
                            break;
                        case PlayerMoveControl.Left:
                        case PlayerMoveControl.Up:
                            // set the z value
                            transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentStair.transform.position.z);
                            movement = stairDir.normalized / 1.5f;

                            animator.SetBool("isRunning", false);
                            animator.SetBool("isClimbing", false);
                            animator.SetBool("isScaling", true);
                            animator.SetBool("isHiding", false);
                            transform.rotation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                            animator.enabled = true;
                            audioManager.PlayLoopSound("playerStairs", gameObject);
                            break;
                        default:
                            animator.enabled = false;
                            audioManager.EndLoopSound("playerStairs");
                            break;
                    }
                    break;
                }
            case SpecialPlayerState.Hiding:
                // set the z value
                transform.position = new Vector3(transform.position.x, transform.position.y, roomManager.CurrentHidingSpot.transform.position.z);
                transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);

                animator.SetBool("isRunning", false);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isScaling", false);
                animator.SetBool("isHiding", true);
                break;
        }
        movement *= speed * Time.deltaTime;
        futurePos = transform.position + movement; // where the player wants to go
    }

    void MovePlayer()
    {
        transform.position = futurePos;
    }

    //Write all movement control options in here
    PlayerMoveControl ControlMovement()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) //move up
        {
            return PlayerMoveControl.Up;
        }
        else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) //move down
        {
            return PlayerMoveControl.Down;
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) //move left
        {
            return PlayerMoveControl.Left;
        }
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) //move right
        {
            return PlayerMoveControl.Right;
        }
        //no movement controls being input
        return PlayerMoveControl.None;
    }



    public bool IsHiding
    {
        get { return specialPlayerState == SpecialPlayerState.Hiding; }
    }
}
