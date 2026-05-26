using UnityEngine;
using UnityEngine.Serialization;

public class PlanetAutoRotate : MonoBehaviour
{
    [Header("Rotation Speed (degrees per second)")]
    [FormerlySerializedAs("rotationSpeed")] [SerializeField] private Vector3 _rotationSpeed = new Vector3(10f, 20f, 5f);

    private void Update()
    {
        transform.Rotate(
            _rotationSpeed.x * Time.deltaTime,
            _rotationSpeed.y * Time.deltaTime,
            _rotationSpeed.z * Time.deltaTime,
            Space.Self
        );
    }
}
