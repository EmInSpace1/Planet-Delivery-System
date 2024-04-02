using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class PlayerMovement : MonoBehaviour
{
    [Header("Walking")]
    [SerializeField] private Transform playerInputSpace;
    [SerializeField, Range(0f, 100f)] private float maxSpeed;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration, maxAirAcceleration;
    [SerializeField, Range(0, 90)] private float maxGroundAngle, maxStairsAngle;

    [Header("Jump")]
    [SerializeField, Range(0f, 10f)] private float jumpHeight;
    [SerializeField, Range(0, 5)] private int maxAirJumps;

    [Header("Snapping")]
    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed;
    [SerializeField, Min(0f)] private float probeDistance;
    [SerializeField] private LayerMask probeMask, stairsMask;

    [Header("References")]
    [SerializeField] private OrbitCamera orbitCamera;
    [SerializeField] private Transform holdLocation;
    [SerializeField] private PackageBehaviour package;

    Rigidbody body;

    Vector3 velocity, desiredVelocity;

    bool desiredJump;

    Vector3 contactNormal, steepNormal;

    int groundContactCount, steepContactCount;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;

    int jumpPhase;

    float minGroundDotProduct, minStairsDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    private Vector3 upAxis, rightAxis, forwardAxis;

    private Vector3 capsuleDirection;

    private Vector3 walkingDir;

    private bool holdingPackage;

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;

        OnValidate();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");

        walkingDir = new Vector3(playerInput.x, 0, playerInput.y);
    }

    void FixedUpdate()
    {
        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        UpdateState();
        AdjustVelocity();

        if (desiredJump)
        {
            desiredJump = false;
            if(!package.GetIsPickedUp()) Jump(gravity);
        }

        velocity += gravity * Time.deltaTime;

        body.velocity = velocity;
        ClearState();



        //rotate capule to gravity direction
        Vector3 currentUp = upAxis;
        capsuleDirection = Vector3.Lerp(capsuleDirection, currentUp, 0.1f);
        transform.up = capsuleDirection;

        //place the holding object at the correct location
        Vector3 cameraDirection = transform.position - playerInputSpace.position;
        holdLocation.position = cameraDirection/9 + transform.position;
    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = upAxis;
        }
    }

    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(
            body.position, -upAxis, out RaycastHit hit,
            probeDistance, probeMask
        ))
        {
            return false;
        }
        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if(upDot >= minGroundDotProduct)
            {
                steepContactCount = 0;
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
        walkingDir = xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }

    void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }

    public Vector3 GetPackageDirection()
    {
        Vector3 cameraDirection = transform.position - playerInputSpace.position;
        return cameraDirection;
    }

    public void SetHoldingPackage(bool newValue)
    {
        holdingPackage = newValue;
    }
}
