using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageBehaviour : MonoBehaviour
{
    private bool pickedUp;
    private bool justDropped;
    private bool justPickedUp;

    private Transform packageHolder;

    private CustomGravityRigidbody gravityBody;
    private BoxCollider coll;

    private bool playerClose;

    private void Awake()
    {
        packageHolder = GameObject.FindGameObjectWithTag("PackageHolder").transform;
        gravityBody = GetComponent<CustomGravityRigidbody>();
        coll = GetComponent<BoxCollider>();
    }

    private void Update()
    {

        justDropped = false;
        if(pickedUp)
        {
            transform.position = packageHolder.position;
            transform.rotation = packageHolder.rotation;

            if(Input.GetButtonDown("Interact") && !justPickedUp)
            {
                pickedUp = false;

                gravityBody.enabled = true;
                coll.enabled = true;
                justDropped = true;
            }
        }

        if(playerClose && Input.GetButtonDown("Interact") && !pickedUp && !justDropped)
        {
            pickedUp = true;

            gravityBody.enabled = false;
            coll.enabled = false;

            justPickedUp = true;
        }

        justPickedUp = false;

        playerClose = false;
    }

    private void OnTriggerStay(Collider other)
    {
        playerClose = other.gameObject.tag == "Player";
    }
}
