using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpecialPlayerState
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
    Hide,
    None
}

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector3 movement;
    private SpecialPlayerState specialPlayerState = SpecialPlayerState.None;
    private PlayerMoveControl currentHoldMoveControl = PlayerMoveControl.None;
    private PlayerMoveControl currentPressMoveControl = PlayerMoveControl.None;
    private RoomManager roomManager;
    private Vector3 futurePos;
    private bool touchingFloorVerticalMovement = false;
    private bool atTopStair = false;
    private bool atTopLadder = false;
    private bool touchingLeftWall = false;
    private bool touchingRightWall = false;
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
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
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
    void Update()
    {
        Time.timeScale = 1.0f;

        ChangeMovement();
        currentHoldMoveControl = ControlHoldControl();
        currentPressMoveControl = ControlPressControl();
        MovePlayer();
    }

    void FixedUpdate()
    {
        CheckFixedCollisions();
    }

    void CheckFixedCollisions()
    {
        EnemyCollision();
        KeyCollision();
        RoomCollision();
        DoorCollision();
        StairCollision();
        LadderCollision();
        HidingCollision();
    }

    void HidingCollision()
    {
        roomManager.CurrentHidingSpot = null;
        foreach (GameObject hidingSpot in roomManager.HidingSpotList)
        {
            // at a hiding spot
            if (hidingSpot.GetComponent<Collider>().bounds.Intersects(GetComponent<Collider>().bounds))
            {
                roomManager.CurrentHidingSpot = hidingSpot;
                break;
            }
        }
    }

    void LadderCollision()
    {
        // reset currentLadder when not on ladder
        if (specialPlayerState != SpecialPlayerState.OnLadder)
        {
            roomManager.CurrentLadder = null;
        }

        // start ladder collision
        foreach (GameObject ladder in roomManager.LadderList)
        {
            Bounds topLadder = ladder.GetComponent<LadderProperties>().topLadder.GetComponent<Collider>().bounds;
            Bounds bottomLadder = ladder.GetComponent<LadderProperties>().bottomLadder.GetComponent<Collider>().bounds;

            if ((GetComponent<Collider>().bounds.Intersects(bottomLadder) || GetComponent<Collider>().bounds.Intersects(topLadder)) // touching the ladder while not already on it
                && specialPlayerState != SpecialPlayerState.OnLadder)
            {
                atTopLadder = Vector3.Distance(transform.position, topLadder.center) < Vector3.Distance(transform.position, bottomLadder.center);
                roomManager.CurrentLadder = ladder;
            }
        }

        // end ladder collision
        if (roomManager.CurrentLadder) // has used ladder
        {
            Bounds topLadder = roomManager.CurrentLadder.GetComponent<LadderProperties>().topLadder.GetComponent<Collider>().bounds;
            Bounds bottomLadder = roomManager.CurrentLadder.GetComponent<LadderProperties>().bottomLadder.GetComponent<Collider>().bounds;

            bool aboveTopLadder = GetComponent<Collider>().bounds.min.y > topLadder.max.y; // bottom of player is above top ladder
            bool belowBottomLadder = GetComponent<Collider>().bounds.min.y < bottomLadder.min.y || touchingFloorVerticalMovement;  //bottom of player is below bottom ladder, with some leeway space
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
                            touchingLeftWall = true;
                            movement.x = 0;
                            futurePos = new Vector3(door.transform.position.x + child.GetComponent<Renderer>().bounds.size.x / 2 + (transform.localScale.x / 2) + 0.01f, futurePos.y);
                        }
                        else if (child.GetComponent<Renderer>().bounds.center.x > futureBounds.center.x) // door is right of player
                        {
                            // place player on left side
                            touchingRightWall = true;
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
        // reset currentStair when not on stair
        if (specialPlayerState != SpecialPlayerState.Stairs)
        {
            roomManager.CurrentStair = null;
        }

        // start stair collision
        foreach (GameObject stair in roomManager.StairList)
        {
            Bounds topStair = stair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = stair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;

            if ((GetComponent<Collider>().bounds.Intersects(bottomStair) || GetComponent<Collider>().bounds.Intersects(topStair)) // touching the stairs while not already on them
                && specialPlayerState != SpecialPlayerState.Stairs)
            {
                atTopStair = Vector3.Distance(transform.position, topStair.center) < Vector3.Distance(transform.position, bottomStair.center);

                roomManager.CurrentStair = stair;
            }
        }


        // end stair collision
        if (roomManager.CurrentStair) // has used stairs
        {
            Bounds topStair = roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds;
            Bounds bottomStair = roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds;

            bool aboveTopStair = GetComponent<Collider>().bounds.min.y > topStair.max.y; // bottom of player is above top stair
            bool belowBottomStair = GetComponent<Collider>().bounds.min.y < bottomStair.min.y || touchingFloorVerticalMovement;  //bottom of player is below bottom ladder, with some leeway space
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
        touchingLeftWall = false;
        touchingRightWall = false;
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
                touchingLeftWall = true;
                movement.x = 0;
                transform.position = new Vector3(wall.position.x + wall.GetComponent<Renderer>().bounds.extents.x + GetComponent<Collider>().bounds.extents.x - .2f, transform.position.y); ;
            }
            if (wall.gameObject.name == "RightWall")
            {
                touchingRightWall = true;
                movement.x = 0;
                transform.position = new Vector3(wall.position.x - wall.GetComponent<Renderer>().bounds.extents.x - GetComponent<Collider>().bounds.extents.x + .2f, transform.position.y); ;
            }
            if (wall.gameObject.name == "BottomWall")
            {
                if ((specialPlayerState != SpecialPlayerState.None && !wall.GetComponent<WallProperties>().isPasable) || // trying to go through impassable wall on ladder or stairs
                    (specialPlayerState == SpecialPlayerState.None)) // not on ladder nor stairs, so apply collision for all walls 
                {
                    movement.y = 0;
                    // super small number added to y to prevent stuck in collisions, but so small that gravity induced jitter can't be seen
                    transform.position = new Vector3(transform.position.x, wall.position.y + wall.GetComponent<Collider>().bounds.size.y / 2 + GetComponent<Collider>().bounds.extents.y + 0.000001f);

                    // player has touched the floor while on ladder or stairs
                    if ((specialPlayerState == SpecialPlayerState.OnLadder || specialPlayerState == SpecialPlayerState.Stairs) && currentHoldMoveControl == PlayerMoveControl.Down)
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
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
        if (GetComponent<Collider>().bounds.Intersects(enemy.GetComponent<Collider>().bounds) && enemy.GetComponent<enemyPathfinding>().EnemyState == enemyPathfinding.State.Hunting) // enemy can't kill you when not hunting you
        {
            //KillPlayer();
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
                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                switch (currentHoldMoveControl)
                {
                    case PlayerMoveControl.Left:
                        if (!touchingLeftWall)
                        {
                            movement = new Vector3(-1.0f, 0.0f);
                        }

                        animator.SetBool("isRunning", true);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        currentOrientation = transform.rotation;
                        targetOrientation = Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z);
                        transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .2f);
                        audioManager.PlayLoopSound("playerRun", gameObject);
                        break;
                    case PlayerMoveControl.Right:
                        if (!touchingRightWall)
                        {
                            movement = new Vector3(1.0f, 0.0f);
                        }
                        animator.SetBool("isRunning", true);
                        animator.SetBool("isClimbing", false);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        currentOrientation = transform.rotation;
                        targetOrientation = Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z);
                        transform.rotation = Quaternion.Lerp(currentOrientation, targetOrientation, .2f);
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

                // stairs
                if (roomManager.CurrentStair)
                {
                    // at the top, so can only go down
                    if (atTopStair)
                    {
                        if (currentPressMoveControl == PlayerMoveControl.Down)
                        {
                            transform.position = new Vector3(roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds.center.x, 
                                roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds.center.y + 0.8f, 
                                roomManager.CurrentStair.GetComponent<StairProperties>().topStair.GetComponent<Collider>().bounds.center.z);
                            specialPlayerState = SpecialPlayerState.Stairs;
                            gameObject.GetComponent<Rigidbody>().useGravity = false;
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                    else // at the bottom, so can only go up
                    {
                        if (currentPressMoveControl == PlayerMoveControl.Up)
                        {
                            transform.position = new Vector3(roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds.center.x,
                                roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds.center.y + 0.8f,
                                roomManager.CurrentStair.GetComponent<StairProperties>().bottomStair.GetComponent<Collider>().bounds.center.z);
                            specialPlayerState = SpecialPlayerState.Stairs;
                            gameObject.GetComponent<Rigidbody>().useGravity = false;
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                }

                // ladder
                // at the top, so can only go down
                if (roomManager.CurrentLadder)
                {
                    if (atTopLadder)
                    {
                        if (currentPressMoveControl == PlayerMoveControl.Down)
                        {
                            specialPlayerState = SpecialPlayerState.OnLadder;
                            gameObject.GetComponent<Rigidbody>().useGravity = false;
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                    else // at the bottom, so can only go up
                    {
                        if (currentPressMoveControl == PlayerMoveControl.Up)
                        {
                            specialPlayerState = SpecialPlayerState.OnLadder;
                            gameObject.GetComponent<Rigidbody>().useGravity = false;
                            gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        }
                    }
                }


                // hiding
                if (roomManager.CurrentHidingSpot && currentPressMoveControl == PlayerMoveControl.Hide)
                {
                    audioManager.Play("hide", gameObject);
                    specialPlayerState = SpecialPlayerState.Hiding;
                }
                break;
            case SpecialPlayerState.OnLadder:
                audioManager.EndLoopSound("playerRun");
                switch (currentHoldMoveControl)
                {
                    case PlayerMoveControl.Down:
                        // set the x and z values
                        transform.position = new Vector3(roomManager.CurrentLadder.transform.position.x, transform.position.y, roomManager.CurrentLadder.transform.position.z - .35f);
                        movement = new Vector3(0.0f, -.75f);
                        transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
                        animator.SetBool("isRunning", false);
                        animator.SetBool("isClimbing", true);
                        animator.SetBool("isScaling", false);
                        animator.SetBool("isHiding", false);
                        animator.enabled = true;
                        audioManager.PlayLoopSound("playerLadder", gameObject);
                        break;
                    case PlayerMoveControl.Up:
                        // set the x and z values
                        transform.position = new Vector3(roomManager.CurrentLadder.transform.position.x, transform.position.y, roomManager.CurrentLadder.transform.position.z - .35f);
                        movement = new Vector3(0.0f, .75f);
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
                audioManager.EndLoopSound("playerRun");
                StairProperties stair = roomManager.CurrentStair.GetComponent<StairProperties>();
                Vector3 stairDir = stair.topStair.transform.position - stair.bottomStair.transform.position;

                if (stair.tag == "RightStair")
                {
                    switch (currentHoldMoveControl)
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
                }
                else
                {
                    switch (currentHoldMoveControl)
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
                }
                break;
            case SpecialPlayerState.Hiding:
                audioManager.EndLoopSound("playerRun");
                // set the z value
                transform.position = new Vector3(roomManager.CurrentHidingSpot.transform.position.x, transform.position.y, roomManager.CurrentHidingSpot.transform.position.z);
                transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);


                animator.SetBool("isRunning", false);
                animator.SetBool("isClimbing", false);
                animator.SetBool("isScaling", false);
                animator.SetBool("isHiding", true);

                // take out of hiding
                if (currentPressMoveControl == PlayerMoveControl.Hide)
                {
                    specialPlayerState = SpecialPlayerState.None;
                    transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                }

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
    PlayerMoveControl ControlHoldControl()
    {

        if (specialPlayerState == SpecialPlayerState.None)
        {
            // horizontal movement
            if (Input.GetKey(KeyCode.A)) //move left
            {
                if (Input.GetKey(KeyCode.D)) // move right
                {
                    return PlayerMoveControl.None;
                }
                return PlayerMoveControl.Left;
            }
            else if (Input.GetKey(KeyCode.D)) //move right
            {
                if (Input.GetKey(KeyCode.A)) // move left
                {
                    return PlayerMoveControl.None;
                }
                return PlayerMoveControl.Right;
            }
        }
        else if (specialPlayerState != SpecialPlayerState.Hiding) // ladders or stairs
        {
            // Vertical movement
            if (Input.GetKey(KeyCode.W)) //move up
            {
                if (Input.GetKey(KeyCode.S)) // move down
                {
                    return PlayerMoveControl.None;
                }
                return PlayerMoveControl.Up;
            }
            else if (Input.GetKey(KeyCode.S)) //move down
            {
                if (Input.GetKey(KeyCode.W)) // move up
                {
                    return PlayerMoveControl.None;
                }
                return PlayerMoveControl.Down;
            }
        }
        //no movement controls being input
        return PlayerMoveControl.None;
    }
    PlayerMoveControl ControlPressControl()
    {
        if (Input.GetKeyDown(KeyCode.W)) //move up
        {
            return PlayerMoveControl.Up;
        }
        else if (Input.GetKeyDown(KeyCode.S)) //move down
        {
            return PlayerMoveControl.Down;
        }
        if (Input.GetKeyDown(KeyCode.A)) //move left
        {
            return PlayerMoveControl.Left;
        }
        else if (Input.GetKeyDown(KeyCode.D)) //move right
        {
            return PlayerMoveControl.Right;
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            return PlayerMoveControl.Hide;
        }
        //no movement controls being input
        return PlayerMoveControl.None;
    }



    public bool IsHiding
    {
        get { return specialPlayerState == SpecialPlayerState.Hiding; }
    }

    public SpecialPlayerState SpecialPlayerState
    {
        get { return specialPlayerState; }
    }
}
