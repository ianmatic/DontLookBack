using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    GameObject player;
    SpriteRenderer playerSprite;
    SpriteRenderer sprite;

    int state;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sprite = gameObject.GetComponent<SpriteRenderer>();
        playerSprite = player.GetComponent<SpriteRenderer>();

        state = 0;
    }

    void Update()
    {
        switch(state) // Switches between whether the player is on the ladder or off the ladder. 
        {
            case 0: // Off ladder
                if(PlayerOnLadder()) 
                {
                    player.GetComponent<PlayerMovement>().ToggleVertical();
                    state = 1;
                }
                break;

            case 1: // On ladder
                if (!PlayerOnLadder())
                {
                    player.GetComponent<PlayerMovement>().ToggleVertical();
                    state = 0;
                }
                break;
        }
    }

    bool PlayerOnLadder() // Checks if the sprites are within bounds of each other.
    {
        if (sprite.bounds.Intersects(playerSprite.bounds))       
            return true;
        else
            return false;
    }
}
