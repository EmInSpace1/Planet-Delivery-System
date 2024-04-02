using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PackageBehaviour : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private GameObject pickUpText;
    [SerializeField] private GameObject throwText;

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

            pickUpText.SetActive(false);
            throwText.SetActive(true);
        }

        justDropped = false;
        if(pickedUp)
        {
            transform.position = packageHolder.position;
            transform.rotation = packageHolder.rotation;

            if((Input.GetButtonDown("Interact") || Input.GetKeyDown(KeyCode.Q)) && !justPickedUp)
            {
                pickedUp = false;

                gravityBody.enabled = true;
                coll.enabled = true;
                justDropped = true;
                throwText.SetActive(false);
            }
        }


        if (Input.GetButtonDown("Fire2") && pickedUp)
        {
            pickedUp = false;

            gravityBody.enabled = true;
            coll.enabled = true;
            justDropped = true;
            body.AddForce(player.GetPackageDirection()*200);
            throwText.SetActive(false);
        }

        justPickedUp = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerClose = true;

            if (!pickedUp)
            {
                canFinish = true;
                pickUpText.SetActive(true);
            }
            else canFinish = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            playerClose = false;
            canFinish = false;
            pickUpText.SetActive(false);
        }
    }

    public bool GetIsPickedUp()
    {
        return pickedUp;
    }

    public bool FinishConditionsMet()
    {
        if (playerClose && !pickedUp)
        {
            pickUpText.SetActive(false);
            return true;
        } else return false;
    }
}
