using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Light>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 0.245f, transform.position.z);
        if (Input.GetKeyDown(KeyCode.F))
        {
            GetComponent<Light>().enabled = !GetComponent<Light>().enabled;
        }

        if (GetComponent<Light>().enabled) // activate flashlight
        {
            Plane zPlane = new Plane(Vector3.back, transform.position); // make a plane that is along the z axis at the player's position (parallel to our back walls)
            Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            float hitDistance;
            zPlane.Raycast(mouseRay, out hitDistance); // shoot a ray from mouse position to the plane, and retrieve the length of the ray

            Vector3 hitPoint = mouseRay.GetPoint(hitDistance); // get the exact point at which the ray hit the plane
            transform.rotation = Quaternion.LookRotation((hitPoint - player.transform.position).normalized); // look at the exact point from the player's position
            /*Debug.DrawRay(mouseRay.origin, mouseRay.direction * Vector3.Distance(mouseRay.origin, hitPoint), Color.red, 50);*/
        }
    }
}
