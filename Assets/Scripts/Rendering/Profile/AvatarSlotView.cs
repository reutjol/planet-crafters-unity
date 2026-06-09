using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarSlotView : MonoBehaviour
{
    public enum SlotState { Owned, Locked, StageUnlock }

    [SerializeField] private Button button;
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject selectedMark;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TMP_Text lockLabel;

    private string avatarId;
    private SlotState state;
    private Action<string> onClicked;

    public void Setup(string id, Sprite sprite, SlotState slotState, bool isSelected, Action<string> clickCallback, int stageRequired = 0)
    {
        avatarId = id;
        state = slotState;
        onClicked = clickCallback;

        if (avatarImage != null)
            avatarImage.sprite = sprite;

        bool locked = slotState != SlotState.Owned;

        if (selectedMark != null)
            selectedMark.SetActive(!locked && isSelected);

        if (lockOverlay != null)
            lockOverlay.SetActive(locked);

        if (lockLabel != null)
        {
            lockLabel.gameObject.SetActive(locked);
            lockLabel.text = slotState == SlotState.StageUnlock
                ? $"Available from stage {stageRequired}"
                : "Buy in Shop";
        }

        if (avatarImage != null)
            avatarImage.color = locked ? new Color(1f, 1f, 1f, 0.4f) : Color.white;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectedMark != null)
            selectedMark.SetActive(state == SlotState.Owned && isSelected);
    }

    private void HandleClick()
    {
        onClicked?.Invoke(avatarId);
    }
}
