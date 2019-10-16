using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    GameObject player;
    SpriteRenderer playerSprite;
    SpriteRenderer sprite;
    BoxCollider2D spriteBox;

    bool doorOpen;
    public bool needKey;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = gameObject.GetComponent<SpriteRenderer>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        spriteBox = gameObject.GetComponent<BoxCollider2D>();

        doorOpen = false;
    }

    void Update()
    {
        if(NearDoor()) // Checks if the player is near the door
        {
            if(Input.GetKeyDown(KeyCode.E)) // Player can press 'e' to interact
            {
                if(needKey)
                {
                    
                }
                else // Changes the state of the box collider and whether the door is open
                {
                    doorOpen = !doorOpen;
                    spriteBox.enabled = !spriteBox.enabled;
                }
            }
        }
    }

    bool NearDoor() // Checks if a player is near the door
    {
        if (sprite.bounds.Intersects(playerSprite.bounds))
            return true;
        else
            return false;
    }
}
