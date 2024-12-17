using UnityEngine;

namespace Hanzo.Quiz
{
    public class ScoreSystem : MonoBehaviour
    {
        public static ScoreSystem instance; // Singleton instance to allow global access to the score
        private int playerScore = 0; // Track the player's score

        private void Awake()
        {
            // Ensure only one instance of the ScoreSystem exists
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Keep the score across scenes
            }
            else
            {
                Destroy(gameObject); // Destroy duplicate instances
            }
        }

        // Method to add to the player's score
        public void AddScore()
        {
            playerScore++;
            Debug.Log("Player's Score: " + playerScore);
        }

        // Method to retrieve the player's current score
        public int GetScore()
        {
            return playerScore;
        }

        // Reset the score when the game starts or ends
        public void ResetScore()
        {
            playerScore = 0;
        }
    }
}
