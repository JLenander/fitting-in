using System;
using FMODUnity;
using TMPro;
using UnityEngine;

public class Hoop : MonoBehaviour
{
    [SerializeField] private string ballTag = "Basketball"; // tag of the basketball
    [SerializeField] private string phoneTag = "Phone";
    [SerializeField] private TextMeshProUGUI scoreText;
    public int score = 0;
    public StudioEventEmitter scoreSfx;

    private float phoneCooldown = 0.5f;
    private float lastPhoneScoreTime = -1f;

    private void OnDestroy()
    {
        scoreSfx.Stop();
    }

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
            scoreSfx.Play();

            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
        else if (other.CompareTag(phoneTag))
        {
            if (Time.time - lastPhoneScoreTime < phoneCooldown)
                return;

            lastPhoneScoreTime = Time.time;
            score += 10;
            scoreSfx.Play();

            if (scoreText != null)
            {
                scoreText.text = score.ToString();
            }
        }
    }
}
