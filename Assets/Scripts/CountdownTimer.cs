using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Hanzo.Quiz;  // Assuming you are using TextMeshProUGUI for the UI

public class CountdownTimer : MonoBehaviour
{
    public float startTimeInSeconds = 60f;  // Set the initial countdown time (e.g., 60 seconds for 1 minute)
    public TextMeshProUGUI timerText;       // Reference to your UI Text component

    private float timeRemaining;
    private bool timerRunning = false;

    public UnityEvent onTimeEnded;

    void Start()
    {
        timeRemaining = startTimeInSeconds;
        timerRunning = true;
    }

    void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                UpdateTimerUI(timeRemaining);  // Update to show "00:00"
                TimerEnded();
            }
        }
    }

    // Updates the UI to display the time in MM:SS format
    void UpdateTimerUI(float currentTime)
    {
        currentTime = Mathf.Max(0, currentTime); // Ensure no negative time

        // Convert float time to minutes and seconds
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        // Format the time as "MM:SS"
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Called when the timer reaches zero
    void TimerEnded()
    {
        Debug.Log("Timer has ended!");
        // Add any additional logic you want when the timer ends, e.g., end the quiz or stop the game
        onTimeEnded?.Invoke();
        PlayFabLogin.Instance.SendLeaderboard(ScoreSystem.instance.GetScore());
    }
}