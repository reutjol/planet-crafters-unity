using UnityEngine;

public class WheelRootView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransformReference;

    public RectTransform RectTransformReference => rectTransformReference;
}