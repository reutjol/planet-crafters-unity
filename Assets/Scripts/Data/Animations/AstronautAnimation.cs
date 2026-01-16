using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AstronautAnimation : MonoBehaviour
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
            animationCoroutine = StartCoroutine(animateAstronaut());
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

    private IEnumerator animateAstronaut()
    {
        while (true)
        {
            image.sprite = apriteArray[indexSprite];
            indexSprite++;
            if(indexSprite >= apriteArray.Length)
            {
                indexSprite = 0;
            }
            yield return new WaitForSeconds(frameRate);
        }
    }
}

