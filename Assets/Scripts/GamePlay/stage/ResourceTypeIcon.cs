using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the stage's resource type icon in the corner of the screen.
/// Reads the resourceType from the active planet's stage data.
/// </summary>
public class ResourceTypeIcon : MonoBehaviour
{
    [Header("Image")]
    [SerializeField] private Image iconImage;

    [Header("Sprites per resource")]
    [SerializeField] private Sprite rockSprite;
    [SerializeField] private Sprite terraSprite;
    [SerializeField] private Sprite bioSprite;
    [SerializeField] private Sprite crystalSprite;

    private void Start()
    {
        var stageId = AppSession.Instance?.SelectedStageId;
        var planet  = AppSession.Instance?.ActivePlanet;

        if (string.IsNullOrEmpty(stageId) || planet?.stages == null)
        {
            gameObject.SetActive(false);
            return;
        }

        var stage = planet.stages.Find(s => s.stageId == stageId);
        if (stage == null)
        {
            gameObject.SetActive(false);
            return;
        }

        ApplyResource(stage.meta?.resourceType);
    }

    private void ApplyResource(string resourceType)
    {
        Sprite sprite = resourceType switch
        {
            "terra"   => terraSprite,
            "bio"     => bioSprite,
            "crystal" => crystalSprite,
            _         => rockSprite,
        };

        if (sprite == null)
        {
            gameObject.SetActive(false);
            return;
        }

        iconImage.sprite = sprite;
        gameObject.SetActive(true);
    }
}
