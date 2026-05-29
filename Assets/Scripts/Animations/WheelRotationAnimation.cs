using System;
using System.Collections;
using UnityEngine;

public class WheelRotationAnimation : MonoBehaviour
{
    [SerializeField] private RectTransform wheelRoot;

    private Coroutine activeRoutine;
    private float currentZRotation;

    public event Action<float> RotationChanged;

    public bool IsSpinning { get; private set; }
    public float CurrentZRotation => currentZRotation;

    private void Awake()
    {
        if (wheelRoot != null)
            currentZRotation = wheelRoot.localEulerAngles.z;
    }

    public void ResetRotation()
    {
        currentZRotation = 0f;

        if (wheelRoot != null)
            ApplyWheelRotation();
    }

    public void Play(float deltaAngle, float duration, Action onCompleted = null)
    {
        if (wheelRoot == null || IsSpinning)
            return;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(PlayRoutine(deltaAngle, duration, onCompleted));
    }

    private IEnumerator PlayRoutine(float deltaAngle, float duration, Action onCompleted)
    {
        IsSpinning = true;

        float rotated = 0f;
        float angularSpeed = deltaAngle / duration;

        while (rotedLessThanTarget(rotated, deltaAngle))
        {
            float step = angularSpeed * Time.deltaTime;

            if (rotated + step > deltaAngle)
                step = deltaAngle - rotated;

            rotated += step;
            currentZRotation -= step; // clockwise

            ApplyWheelRotation();

            yield return null;
        }

        IsSpinning = false;
        activeRoutine = null;

        onCompleted?.Invoke();
    }

    private bool rotedLessThanTarget(float rotated, float target)
    {
        return rotated < target;
    }

    private void ApplyWheelRotation()
    {
        wheelRoot.localEulerAngles = new Vector3(0f, 0f, currentZRotation);
        RotationChanged?.Invoke(currentZRotation);
    }
}
