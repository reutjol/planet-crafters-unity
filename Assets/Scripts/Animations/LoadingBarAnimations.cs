using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animates a loading bar UI element by cycling through sprite frames.
/// Auto-starts on enable, auto-stops on disable.
/// </summary>
public class LoadingBarAnimations : MonoBehaviour
{
    public Image image;
    public Sprite[] apriteArray;
    public float frameRate = 0.07f;

    private int indexSprite = 0;

    private Coroutine animationCoroutine;

    private void OnEnable()
    {
        startAnimation();
    }

    private void OnDisable()
    {
        stopAnimation();
    }

    public void startAnimation()
    {
        if (animationCoroutine == null)
        {
            animationCoroutine = StartCoroutine(animateLoadingBar());
        }
    }

    public void stopAnimation()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator animateLoadingBar()
    {
        while (true)
        {
            image.sprite = apriteArray[indexSprite];
            indexSprite++;
            if (indexSprite >= apriteArray.Length)
            {
                indexSprite = 0;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }
}
