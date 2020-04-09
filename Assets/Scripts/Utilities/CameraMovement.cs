using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic WASD-style camera movement for impostor scene testing
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [Tooltip("How fast the camera turns with the mouse")]
    public float mouseSensitivity = 1;

    [Tooltip("The default speed of the camera while moving, in m/s")]
    public float speed = 5;

    [Tooltip("The speed of the camera while holding Shift, in m/s")]
    public float sprintSpeed = 10;

    [Tooltip("The acceleration of the speed while sprinting, in m/s/s")]
    public float sprintAcceleration = 2.0f;

    /// <summary>
    /// Current camera yaw angle
    /// </summary>
    float viewYaw = 0;

    /// <summary>
    /// Current camera pitch angle in degrees
    /// </summary>
    float viewPitch = 0;

    /// <summary>
    /// Used for acceleration while holding shift
    /// </summary>
    float activeSprintSpeed;
    
    /// <summary>
    /// Called by Unity upon creation. Locks and hides the cursor.
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    /// <summary>
    /// Called by Unity upon a frame. Moves the camera by user input.
    /// </summary>
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0))
        {
            activeSprintSpeed += Time.deltaTime * sprintAcceleration;
        }
        else
        {
            activeSprintSpeed = sprintSpeed;
        }

        // Move the camera by keyboard inputs
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? activeSprintSpeed : speed;

        transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * currentSpeed;
        transform.position += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * currentSpeed;
        
        // Turn the camera by mouse inputs
        viewYaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        viewPitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        if (viewPitch > 180)
        {
            viewPitch = viewPitch - 360;
        }

        viewPitch = Mathf.Clamp(viewPitch, -89.9f, 89.9f);
        viewYaw = viewYaw % 360;

        transform.rotation = Quaternion.Euler(viewPitch, viewYaw, 0);
    }
}
