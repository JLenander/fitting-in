using TMPro;
using UnityEngine;

public class Hoop : MonoBehaviour
{
    [SerializeField] private string ballTag = "Basketball"; // tag of the basketball
    [SerializeField] private TextMeshProUGUI scoreText;
    public int score = 0;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the hoop has the correct tag
        if (other.CompareTag(ballTag))
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.scoreBall = true;
            }

            score++;

            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
    }
}
