using TMPro;
using UnityEngine;

public class SpinCreditsView : MonoBehaviour
{
    [SerializeField] private TMP_Text textLabel;

    public void SetCredits(int credits)
    {
        if (textLabel != null)
            textLabel.text = credits.ToString();
    }
}
