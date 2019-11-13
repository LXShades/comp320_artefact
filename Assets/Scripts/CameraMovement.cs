using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 1;
    public float speed = 5;
    public float sprintSpeed = 10;

    float horizontalRotation = 0;
    float verticalRotation = 0;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : speed;

        horizontalRotation += Input.GetAxis("Mouse X") * mouseSensitivity;
        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.position += transform.forward * Input.GetAxis("Vertical") * Time.deltaTime * currentSpeed;
        transform.position += transform.right * Input.GetAxis("Horizontal") * Time.deltaTime * currentSpeed;

        if (verticalRotation > 180)
        {
            verticalRotation = verticalRotation - 360;
        }

        verticalRotation = Mathf.Clamp(verticalRotation, -89.9f, 89.9f);
        horizontalRotation = horizontalRotation % 360;

        transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0);
    }
}
