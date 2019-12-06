using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic WASD-style camera movement 
/// </summary>
public class CameraMovement : MonoBehaviour
{
    [Tooltip("How fast the camera turns with the mouse")]
    public float mouseSensitivity = 1;

    [Tooltip("The default speed of the camera while moving, in m/s")]
    public float speed = 5;

    [Tooltip("The speed of the camera while holding Shift, in m/s")]
    public float sprintSpeed = 10;

    /// <summary>
    /// Current camera yaw angle
    /// </summary>
    float viewYaw = 0;

    /// <summary>
    /// Current camera pitch angle in degrees
    /// </summary>
    float viewPitch = 0;
    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        // Move the camera by keyboard inputs
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

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
