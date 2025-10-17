using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float moveSpeed = 5f;

    void Update()
    {
        // --- ROTATION with Arrow Keys ---
        float xRotation = 0f;
        float yRotation = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            xRotation = 1f * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            xRotation = -1f * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            yRotation = -1f * rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            yRotation = 1f * rotationSpeed * Time.deltaTime;
        }

        // Apply rotation
        transform.Rotate(xRotation, yRotation, 0f, Space.Self);

        // --- MOVEMENT with WASD ---
        float moveX = 0f;
        float moveZ = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            moveZ = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveZ = -1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveX = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveX = 1f;
        }

        // Calculate movement vector
        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized * moveSpeed * Time.deltaTime;

        // Move relative to current rotation (camera's direction)
        transform.Translate(move, Space.Self);
    }
}
