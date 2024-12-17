using System.Collections;
using Hanzo.Quiz;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText; // Reference to the UI text component
    public RectTransform uiElement; // Reference to the UI element to animate
    public float finalPositionY = 27.8976f; // Final y-position to stop
    public float duration = 1f; // Duration of the bounce animation
    public float overshootHeight = 50f; // Maximum overshoot height
    public int bounceCount = 3; // Number of bounces before settling

    public UnityEvent onCancel;
    public UnityEvent onPlayAgain;


    private Vector3 originalPosition; // Store the original position

    void Start()
    {
        if (uiElement == null)
        {
            // Automatically grab the RectTransform of the UI element this script is attached to
            uiElement = GetComponent<RectTransform>();
        }

        // Store the original position
        originalPosition = uiElement.localPosition;

        // Start the bounce animation
        StartCoroutine(BounceToPosition());

        // Update the score text
        if (ScoreSystem.instance != null)
        {
            scoreText.text = ScoreSystem.instance.GetScore().ToString() + "/15";
        }
        else
        {
            Debug.LogError("ScoreSystem instance is not available");
        }
    }

    IEnumerator BounceToPosition()
    {
        Vector3 startPos = originalPosition;
        Vector3 endPos = new Vector3(startPos.x, finalPositionY, startPos.z);
        float elapsedTime = 0f;
        float bounceFactor = overshootHeight; // Variable to decrease bounce height

        while (elapsedTime < duration)
        {
            // Time-based interpolation
            float t = Mathf.Clamp01(elapsedTime / duration);

            // Calculate a smooth interpolation between start and final position
            uiElement.localPosition = Vector3.Lerp(startPos, endPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Add the bounce effect after reaching the final position
        for (int i = 0; i < bounceCount; i++)
        {
            yield return StartCoroutine(BounceEffect(endPos, bounceFactor));
            bounceFactor *= 0.5f; // Reduce bounce height on each iteration
        }

        // Ensure the final position is exactly the target position
        uiElement.localPosition = endPos;
    }

    // Coroutine for the bouncing effect
    IEnumerator BounceEffect(Vector3 targetPos, float bounceHeight)
    {
        Vector3 bounceTarget = new Vector3(targetPos.x, targetPos.y + bounceHeight, targetPos.z);
        float bounceTime = 0.2f; // Duration for each bounce
        float elapsedTime = 0f;

        // Move upwards to the overshoot point
        while (elapsedTime < bounceTime)
        {
            float t = elapsedTime / bounceTime;
            uiElement.localPosition = Vector3.Lerp(targetPos, bounceTarget, Mathf.Sin(t * Mathf.PI));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset elapsedTime and move back down to the original target position
        elapsedTime = 0f;
        while (elapsedTime < bounceTime)
        {
            float t = elapsedTime / bounceTime;
            uiElement.localPosition = Vector3.Lerp(bounceTarget, targetPos, Mathf.Sin(t * Mathf.PI));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Make sure it finishes at the exact target position
        uiElement.localPosition = targetPos;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LogOut()
    {
        StartCoroutine(LoggingOut(1.5f));
    }

    IEnumerator LoggingOut(float t)
    {
        PlayFabLogin.Instance.LogoutUser();
        yield return new WaitForSeconds(t);

        SceneManager.LoadScene("Menu");



    }

}


