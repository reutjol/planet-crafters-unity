using System;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject selectedMark;

    private int avatarIndex;
    private Action<int> onClicked;

    public void Setup(int index, Sprite avatarSprite, bool isSelected, Action<int> clickCallback)
    {
        avatarIndex = index;
        onClicked = clickCallback;

        if (avatarImage != null)
            avatarImage.sprite = avatarSprite;

        if (selectedMark != null)
            selectedMark.SetActive(isSelected);

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
        else
        {
            Debug.LogWarning($"AvatarSlotView on {gameObject.name}: button is not assigned.");
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedMark != null)
            selectedMark.SetActive(isSelected);
    }

    private void HandleClick()
    {
        onClicked?.Invoke(avatarIndex);
    }
}