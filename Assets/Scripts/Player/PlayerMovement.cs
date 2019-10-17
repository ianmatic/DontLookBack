using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector3 movement;
    private bool verticalMove;
    private bool horizontalMove;

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
    }

    // Update is called once per frame
    void Update()
    {
        CheckCollision();
        ChangeMovement();
        MovePlayer();
    }

    void CheckCollision()
    {

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

    public void ToggleVertical()
    {
        verticalMove = !verticalMove;
    }
}
