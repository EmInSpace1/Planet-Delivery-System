using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PackageBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;

    private bool pickedUp;
    private bool justDropped;
    private bool justPickedUp;

    private Transform packageHolder;

    private CustomGravityRigidbody gravityBody;
    private BoxCollider coll;
    private Rigidbody body;

    private bool playerClose;

    private bool canFinish;

    private void Awake()
    {
        packageHolder = GameObject.FindGameObjectWithTag("PackageHolder").transform;
        gravityBody = GetComponent<CustomGravityRigidbody>();
        coll = GetComponent<BoxCollider>();
        body = GetComponent<Rigidbody>();
    }

    private void Update()
    {
            if(playerClose && Input.GetButtonDown("Interact") && !pickedUp && !justDropped)
            {
                pickedUp = true;

                gravityBody.enabled = false;
                coll.enabled = false;

                justPickedUp = true;
            }

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


        if (Input.GetButtonDown("Fire2") && pickedUp)
        {
            pickedUp = false;

            gravityBody.enabled = true;
            coll.enabled = true;
            justDropped = true;
            body.AddForce(player.GetPackageDirection()*200);
        }

        justPickedUp = false;
        playerClose = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerClose = true;

            if (!pickedUp)
            {
                canFinish = true;
            } else canFinish = false;
        } else canFinish = false;
    }

    public bool GetIsPickedUp()
    {
        return pickedUp;
    }

    public bool FinishConditionsMet()
    {
        if (playerClose && !pickedUp)
        {
            return true;
        } else return false;
    }
}
