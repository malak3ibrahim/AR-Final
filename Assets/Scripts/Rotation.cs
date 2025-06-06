// Attach this CarRotator script to both Car A and Car B prefabs.

using UnityEngine;

public class Rotation : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second.")]
    public float rotationSpeed = 10f;

    // Whether the car should rotate right now
    private bool isRotating = false;

    void Update()
    {
        if (!isRotating)
            return;

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);
    }

    public void ToggleRotation()
    {
        isRotating = !isRotating;
    }
}
