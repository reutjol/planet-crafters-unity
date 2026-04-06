using System.Collections.Generic;
using UnityEngine;

public class WheelSlicesBuilder : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private WheelConfig wheelConfig;

    [Header("Scene References")]
    [SerializeField] private RectTransform slicesContainer;
    [SerializeField] private WheelSliceView slicePrefab;

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
            sliceRect.pivot = new Vector2(0.5f, 0f);

            sliceRect.anchoredPosition = Vector2.zero;
            sliceRect.localScale = Vector3.one;

            float angle = 90 - (i * angleStep);
            sliceRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        Debug.Log("WheelSlicesBuilder: Build finished");
    }

    private void ClearContainer()
    {
        for (int i = slicesContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(slicesContainer.GetChild(i).gameObject);
        }
    }
}