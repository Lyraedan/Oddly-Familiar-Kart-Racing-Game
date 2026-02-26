using UnityEngine;

public class RotateAtSpeed : MonoBehaviour
{
    [Tooltip("Rotation speed in degrees per second")]
    public Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}