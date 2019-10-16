using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed;
    private Vector2 position;
    private Vector2 movement;
    private bool verticalMove;

    // Start is called before the first frame update
    void Start()
    {
        if (speed == 0.0f)
        {
            speed = 1.0f;
        }
        position = transform.position;
        movement = new Vector2(0.0f, 0.0f);
        verticalMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        ChangeMovement();
        MovePlayer();
    }

    void ChangeMovement()
    {
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            movement = new Vector2(-1.0f, 0.0f);
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            movement = new Vector2(1.0f, 0.0f);
        }
        else if(verticalMove && Input.GetKey(KeyCode.UpArrow))
        {
            movement = new Vector2(0.0f, 1.0f);
        }
        else if(verticalMove && Input.GetKey(KeyCode.DownArrow))
        {
            movement = new Vector2(0.0f, -1.0f);
        }
        else
        {
            movement = new Vector2(0.0f, 0.0f);
        }

        movement *= speed * Time.deltaTime;
    }

    void MovePlayer()
    {
        //position += movement;

        transform.position += new Vector3(movement.x, movement.y); // Changed this so collisions could work. - TJ
    }

    public void ToggleVertical()
    {
        verticalMove = !verticalMove;
    }
}
