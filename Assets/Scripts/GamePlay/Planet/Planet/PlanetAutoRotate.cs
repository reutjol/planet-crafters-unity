using UnityEngine;

/// <summary>
/// Automatically rotates a planet GameObject continuously at a configurable speed.
/// Used for visual effect on planet selection screen.
/// </summary>
public class PlanetAutoRotate : MonoBehaviour
{
    [Header("Rotation Speed (degrees per second)")]
    public Vector3 rotationSpeed = new Vector3(10f, 20f, 5f);

    void Update()
    {
        transform.Rotate(
            rotationSpeed.x * Time.deltaTime,
            rotationSpeed.y * Time.deltaTime,
            rotationSpeed.z * Time.deltaTime,
            Space.Self
        );
    }
}
