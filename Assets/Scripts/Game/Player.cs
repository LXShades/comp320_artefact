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

    // How far the player has travelled overall
    public float distanceTravelled = 0;

    // Horizontal look direction angle, in degrees
    private float eyeHorizontalAngle = 0;
    // Vertical look direction angle, in degrees
    private float eyeVerticalAngle = 0;

    // Whether the character is standing on the ground
    private bool isOnGround;

    // Character controller
    private CharacterController controller;


    void Start()
    {
        eyeHorizontalAngle = transform.rotation.y;

        controller = GetComponent<CharacterController>();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (GameManager.singleton.timeRemaining <= 0f)
        {
            // game is over, stop everything plz
            return;
        }

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
            Vector3 lastPosition = transform.position;
            CollisionFlags collisions;
            collisions = controller.Move(velocity * Time.deltaTime);

            isOnGround = collisions.HasFlag(CollisionFlags.Below);

            if (isOnGround && velocity.y < 0)
            {
                velocity.y = 0;
            }

            if (velocity.magnitude > 0.5f)
            {
                velocity = (transform.position - lastPosition) / Time.deltaTime;
            }

            distanceTravelled += velocity.magnitude * Time.deltaTime;
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