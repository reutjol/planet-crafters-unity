using System.Collections;
using TMPro;
using UnityEngine;

public class BonusTextView : MonoBehaviour
{
    [SerializeField] private TMP_Text _bonusText;
    [SerializeField] private MapController _mapController;
    [SerializeField] private float _displayDuration = 5f;

    private Color _baseColor;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (_bonusText != null)
            _baseColor = _bonusText.color;

        if (_bonusText != null)
            _bonusText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (_mapController != null)
            _mapController.OnBonusScored += Show;
    }

    private void OnDisable()
    {
        if (_mapController != null)
            _mapController.OnBonusScored -= Show;
    }

    private void Show(int bonus)
    {
        if (_bonusText == null) return;

        _bonusText.text = $"+{bonus}";

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeAnimation());
    }

    private IEnumerator FadeAnimation()
    {
        _bonusText.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 1f);
        _bonusText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < _displayDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _displayDuration);
            _bonusText.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 1f - t);
            yield return null;
        }

        _bonusText.gameObject.SetActive(false);
        _fadeCoroutine = null;
    }
}
