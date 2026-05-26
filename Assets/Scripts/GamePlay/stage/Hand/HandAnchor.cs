using UnityEngine;
using UnityEngine.Serialization;

public class HandAnchor : MonoBehaviour
{
    [FormerlySerializedAs("handCamera")]       [SerializeField] private Camera _handCamera;
    [FormerlySerializedAs("horizontalAnchor")] [Range(0, 1)] [SerializeField] private float _horizontalAnchor = 0.65f;
    [FormerlySerializedAs("verticalAnchor")]   [Range(0, 1)] [SerializeField] private float _verticalAnchor = 0.7f;
    [FormerlySerializedAs("distanceToCamera")] [SerializeField] private float _distanceToCamera = 0.6f;

    private void Update()
    {
        if (_handCamera == null) return;

        Vector3 viewportPoint = new Vector3(_horizontalAnchor, _verticalAnchor, _distanceToCamera);
        transform.position = _handCamera.ViewportToWorldPoint(viewportPoint);
        transform.localEulerAngles = Vector3.zero;
    }
}
