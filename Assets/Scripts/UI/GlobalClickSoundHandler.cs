using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GlobalClickSoundHandler : MonoBehaviour
{
    public static GlobalClickSoundHandler Instance { get; private set; }

    [SerializeField] private AudioClip _clickClip;

    private PointerEventData _pointerData;
    private readonly List<RaycastResult> _raycastResults = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _pointerData = new PointerEventData(EventSystem.current);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current == null) return;

        _pointerData.position = Input.mousePosition;
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(_pointerData, _raycastResults);

        foreach (var result in _raycastResults)
        {
            if (result.gameObject.GetComponentInParent<Button>() != null)
            {
                SoundEffectService.Instance?.Play(_clickClip, 1f);
                break;
            }
        }
    }
}
