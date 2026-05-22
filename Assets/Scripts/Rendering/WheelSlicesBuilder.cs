using System.Collections.Generic;
using UnityEngine;

public class WheelSlicesBuilder : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WheelConfig wheelConfig;

    [Header("Scene References")]
    [SerializeField] private RectTransform slicesContainer;
    [SerializeField] private WheelSliceView slicePrefab;

    [Header("Reference Wheel Layout")]
    [SerializeField] private float rewardRadius = 345f;
    [SerializeField] private Vector2 rewardViewSize = new Vector2(160f, 130f);
    [SerializeField] private float firstRewardAngle = 90f;

    private IWheelRewardIconProvider iconProvider;

    public void Initialize(IWheelRewardIconProvider iconProvider)
    {
        this.iconProvider = iconProvider;
        Debug.Log("WheelSlicesBuilder: Initialize called");
    }

    public void Build()
    {
        Debug.Log("WheelSlicesBuilder: Build called");

        if (wheelConfig == null)
        {
            Debug.LogWarning("WheelSlicesBuilder: wheelConfig is missing");
            return;
        }

        if (slicesContainer == null)
        {
            Debug.LogWarning("WheelSlicesBuilder: slicesContainer is missing");
            return;
        }

        if (slicePrefab == null)
        {
            Debug.LogWarning("WheelSlicesBuilder: slicePrefab is missing");
            return;
        }

        if (iconProvider == null)
        {
            Debug.LogWarning("WheelSlicesBuilder: iconProvider is missing");
            return;
        }

        ClearContainer();

        List<WheelRewardDto> rewards = wheelConfig.rewards;
        int count = rewards.Count;

        if (count <= 0)
        {
            Debug.LogWarning("WheelSlicesBuilder: rewards list is empty");
            return;
        }

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            WheelRewardDto reward = rewards[i];
            WheelSliceView slice = Instantiate(slicePrefab, slicesContainer);

            slice.SetTitle(WheelRewardTextFormatter.Format(reward));
            slice.SetIcon(iconProvider.GetIcon(reward));

            RectTransform sliceRect = slice.GetComponent<RectTransform>();

            if (sliceRect == null)
            {
                Debug.LogWarning($"WheelSlicesBuilder: slice {slice.name} has no RectTransform");
                continue;
            }

            sliceRect.anchorMin = new Vector2(0.5f, 0.5f);
            sliceRect.anchorMax = new Vector2(0.5f, 0.5f);
            sliceRect.pivot = new Vector2(0.5f, 0.5f);
            sliceRect.sizeDelta = rewardViewSize;

            float angle = firstRewardAngle - (i * angleStep);
            sliceRect.anchoredPosition = AngleToPosition(angle, rewardRadius);
            sliceRect.localScale = Vector3.one;
            sliceRect.localRotation = Quaternion.identity;
        }

        Debug.Log("WheelSlicesBuilder: Build finished");
    }

    private static Vector2 AngleToPosition(float angleDegrees, float radius)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;
    }

    private void ClearContainer()
    {
        for (int i = slicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slicesContainer.GetChild(i).gameObject);
        }
    }
}
