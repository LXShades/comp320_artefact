using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player character movement and shooting
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Control")]
    // Acceleration in m/s/s
    public float acceleration = 12.0f;
    // Friction in m/s/s
    public float friction = 8.0f;
    // Maximum player speed in m/s
    public float maxSpeed = 8.0f;
    // Vertical speed to apply while jumping
    public float jumpSpeed = 10.0f;
    // Look sensitivity in degrees per 100 pixels
    public float lookSensitivity = 10.0f;

    [Header("Physics")]
    // The maximum slope angle that the character can stand on, in degrees, before they are not considered to be on the ground
    public float maxSlopeAngle = 45.0f;
    // The gravity in m/s/s
    public float gravity = 9.8f;

    [Header("Hierarchy")]
    // Camera used for looking and turning
    public Camera eyes;
    // Slingshot for hitting things and pain
    public Slingshot slingshot;

    // Velocity of the player character in m/s
    public Vector3 velocity = Vector3.zero;

    public Vector3 horizontalVelocity
    {
        get
        {
            return new Vector3(velocity.x, 0, velocity.z);
        }
        set
        {
            velocity.x = value.x;
            velocity.z = value.z;
        }
    }

    // Horizontal look direction angle, in degrees
    private float eyeHorizontalAngle = 0;
    // Vertical look direction angle, in degrees
    private float eyeVerticalAngle = 0;

    // Whether the character is standing on the ground
    private bool isOnGround;

    // Collider to use when moving the character
    private CapsuleCollider capsule;

    void Start()
    {
        eyeHorizontalAngle = transform.rotation.y;

        capsule = GetComponent<CapsuleCollider>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Receive inputs
        ApplyInputLook();
        ApplyInputAcceleration();
        ApplyInputJumps();

        // Handle combat
        HandleSlingshot();

        // Handle movement/physics
        ApplyFriction();
        ApplyGravity();
        ApplyVelocity();
    }

    /// <summary>
    /// Applies inputs to the player's look direction
    /// </summary>
    void ApplyInputLook()
    {
        // Look around according to mouse input
        eyeHorizontalAngle = (eyeHorizontalAngle + Input.GetAxis("Mouse X") * lookSensitivity / 100f) % 360f;
        eyeVerticalAngle = Mathf.Clamp(eyeVerticalAngle - Input.GetAxis("Mouse Y") * lookSensitivity / 100f, -89.9f, 89.9f);

        transform.rotation = Quaternion.Euler(0, eyeHorizontalAngle, 0);
        eyes.transform.localRotation = Quaternion.Euler(eyeVerticalAngle, 0, 0);
    }

    /// <summary>
    /// Takes input and applies acceleration the user's speed
    /// </summary>
    void ApplyInputAcceleration()
    {
        Vector3 inputDirection = transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal");

        if (inputDirection.sqrMagnitude > 0)
        {
            if (inputDirection.sqrMagnitude > 1)
            {
                inputDirection.Normalize();
            }

            // Accelerate, but cancel out friction effects while at it
            velocity += inputDirection * (acceleration * Time.deltaTime);
            velocity += inputDirection.normalized * (friction * Time.deltaTime);

            // Clamp the velocity at the max speed
            if (horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity = horizontalVelocity * (maxSpeed / horizontalVelocity.magnitude);
            }
        }
    }

    void ApplyInputJumps()
    {
        if (Input.GetButtonDown("Jump") && isOnGround)
        {
            velocity.y = jumpSpeed;
        }
    }

    /// <summary>
    /// Applies slowdown to the character
    /// </summary>
    void ApplyFriction()
    {
        if (horizontalVelocity.magnitude > friction * Time.deltaTime)
        {
            horizontalVelocity -= horizontalVelocity.normalized * (Time.deltaTime * friction);
        }
        else
        {
            horizontalVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Applies gravity to the character
    /// </summary>
    void ApplyGravity()
    {
        velocity.y -= gravity * Time.deltaTime;
    }

    /// <summary>
    /// Applies final movement to the character based on its velocity
    /// </summary>
    void ApplyVelocity()
    {
        if (Time.deltaTime > 0 && velocity.sqrMagnitude > 0)
        {
            velocity = Move(velocity * Time.deltaTime) / Time.deltaTime;
        }
    }

    // Charges up and/or fires the slingshot based on player inputs
    void HandleSlingshot()
    {
        if (Input.GetButton("Fire1"))
        {
            slingshot.ChargeUp();
        }

        if (Input.GetButtonUp("Fire1"))
        {
            slingshot.Fire();
        }
    }


    Vector3 Move(Vector3 movementVector)
    {
        Vector3 capsuleUp = transform.TransformVector(new Vector3(0, Mathf.Max(capsule.height * 0.5f - capsule.radius, capsule.radius), 0));
        Vector3 capsuleUpTip = transform.TransformVector(new Vector3(0, capsule.height * 0.5f, 0));
        Vector3 capsuleCenter = transform.TransformPoint(capsule.center);
        Vector3 capsuleBottom = capsuleCenter - capsuleUpTip;
        float castRadius = capsuleUpTip.magnitude - capsuleUp.magnitude;
        Vector3 man = capsuleCenter - capsuleUp.normalized * (capsuleUp.magnitude + capsule.radius * Mathf.Max(transform.localScale.x, transform.localScale.z));

        RaycastHit hit;

        // Reset onGround state
        isOnGround = false;

        // Query our movement collision multiple times, correcting the movement vector to slide along surfaces as we go
        const int numIterations = 3;
        for (int iteration = 0; iteration < numIterations; iteration++)
        {
            if (Physics.CapsuleCast(capsuleCenter + capsuleUp, capsuleCenter - capsuleUp,
                castRadius, movementVector.normalized, out hit,
                movementVector.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                bool isGroundSurface = Vector3.Dot(hit.normal, Vector3.up) >= Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

                if (iteration != numIterations - 1)
                {
                    Vector3 embeddedVector = movementVector * (1 - hit.distance / movementVector.magnitude);

                    if (!isGroundSurface)
                    {
                        // the first several iterations will try to slide along surfaces
                        movementVector += hit.normal * (-Vector3.Dot(embeddedVector, hit.normal) + 0.0001f);
                    }
                    else
                    {
                        Debug.DrawLine(capsuleBottom, capsuleBottom + movementVector, Color.blue);
                        Debug.DrawLine(hit.point, hit.point + embeddedVector, Color.red);
                        // This is a ground surface, so it's probably best that we don't slide along it. We have a grip on it.
                        // So instead, we'll cancel vertical motion and maintain our other motion...
                        movementVector += Vector3.up * (-Vector3.Dot(Vector3.up, embeddedVector) + 0.0001f);

                        isOnGround = true;
                    }
                }
                else
                {
                    Debug.DrawLine(capsuleBottom, capsuleBottom + movementVector, Color.blue);

                    // the last iteration will just outright remove excess movement, to avoid clipping through stuff
                    movementVector *= ((hit.distance - 0.001f) / movementVector.magnitude);

                    Debug.DrawLine(capsuleBottom, capsuleBottom + movementVector, Color.green);
                }
            }
            else
            {
                break;
            }
        }

        // Move!
        transform.position += movementVector;
        return movementVector;
    }

    /// <summary>
    /// Finds the position we are currently aiming at
    /// </summary>
    /// <returns></returns>
    public Vector3 GetTargetPosition()
    {
        // Find the surface under the cursor, or just fire forward if none is found
        RaycastHit hit;
        const float range = 100.0f;
        Vector3 target = eyes.transform.position + eyes.transform.forward * range;

        if (Physics.Raycast(eyes.transform.position, eyes.transform.forward, out hit, range, ~LayerMask.GetMask("BlockOnlyPlayer"), QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        return target;
    }
}