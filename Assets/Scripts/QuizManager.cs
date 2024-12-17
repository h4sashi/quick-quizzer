using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using PlayFab.ClientModels;
using PlayFab;

namespace Hanzo.Quiz
{
    public class QuizManager : MonoBehaviour
    {
        public AudioSource correctSound;
        public AudioSource errorSound;

        public TextMeshProUGUI questionText; 
        public Button[] answerButtons; 
        public TextMeshProUGUI questionCounterText; 
        public TextMeshProUGUI feedbackText;

        private List<Question> questions;
        private int currentQuestionIndex;
        private int questionCounter = 0;
        private int totalQuestions = 15;  
        private Question currentQuestion;

        private string[] positiveMessages = { "Correct!!", "Sharp!!!", "Ooshey!!!" };
        private string[] negativeMessages = { "Oops!", "Oti Wrong!", "Eeyahh!" };

        public UnityEvent OnQuestionFinished;

        void Start()
        {
            LoadQuestions();
            DisplayQuestion();
        }

        void LoadQuestions()
        {
            TextAsset jsonFile = Resources.Load<TextAsset>("questions");
            if (jsonFile != null)
            {
                QuestionList questionList = JsonUtility.FromJson<QuestionList>(jsonFile.text);
                questions = questionList.questions;

                ShuffleQuestions();
            }
            else
            {
                Debug.LogError("Failed to load questions JSON file");
            }
        }

        void ShuffleQuestions()
        {
            for (int i = 0; i < questions.Count; i++)
            {
                Question temp = questions[i];
                int randomIndex = Random.Range(i, questions.Count);
                questions[i] = questions[randomIndex];
                questions[randomIndex] = temp;
            }
        }

        void DisplayQuestion()
        {
            if (questions == null || questions.Count == 0)
            {
                Debug.LogError("No questions loaded");
                return;
            }

            if (questionCounter >= totalQuestions)
            {
                Debug.Log("Quiz completed!");
                return;
            }

            int randomIndex = Random.Range(0, questions.Count);
            currentQuestion = questions[randomIndex];
            questions.RemoveAt(randomIndex);

            questionText.text = currentQuestion.questionText;

            int answerCount = currentQuestion.answers.Length;

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < answerCount)
                {
                    answerButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentQuestion.answers[i];
                    answerButtons[i].gameObject.SetActive(true);

                    int answerIndex = i;
                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                }
                else
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }

            questionCounter++;
            questionCounterText.text = "Question " + questionCounter + " out of " + totalQuestions;
        }

        void OnAnswerSelected(int answerIndex)
        {
            bool isCorrect = answerIndex == currentQuestion.correctAnswerIndex;
            Color correctColor = Color.green;
            Color wrongColor = Color.red;

            foreach (Button button in answerButtons)
            {
                button.onClick.RemoveAllListeners();
            }

            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i == answerIndex)
                {
                    Color targetColor = isCorrect ? correctColor : wrongColor;
                    StartCoroutine(LerpButtonColor(answerButtons[i], targetColor, 0.1f));

                    if (isCorrect)
                    {
                        correctSound.Play();
                        ScoreSystem.instance.AddScore();
                        DisplayFeedback(answerButtons[i].GetComponentInChildren<TextMeshProUGUI>(), positiveMessages);
                    }
                    else
                    {
                        errorSound.Play();
                        DisplayFeedback(answerButtons[i].GetComponentInChildren<TextMeshProUGUI>(), negativeMessages);
                    }
                }
            }

            StartCoroutine(GoToNextQuestionAfterDelay(1.5f)); // Auto-go to next question after 1.5 seconds
        }

        void DisplayFeedback(TextMeshProUGUI feedbackMessage, string[] messages)
        {
            feedbackMessage.text = messages[Random.Range(0, messages.Length)];
            StartCoroutine(BounceAnimation(feedbackMessage));
        }

        public void NextQuestion()
        {
            if (questionCounter < totalQuestions)
            {
                ResetButtonColors();
                DisplayQuestion();
            }
            else
            {
                Debug.Log("Quiz finished! No more questions to load.");
                OnQuestionFinished?.Invoke();
                PlayFabLogin.Instance.SendLeaderboard(ScoreSystem.instance.GetScore());
            }
        }

        void ResetButtonColors()
        {
            foreach (Button button in answerButtons)
            {
                Image buttonImage = button.GetComponent<Image>();
                buttonImage.color = Color.white;
                button.gameObject.SetActive(true);
            }
        }

        IEnumerator LerpButtonColor(Button button, Color targetColor, float duration)
        {
            Image buttonImage = button.GetComponent<Image>();
            Color initialColor = buttonImage.color;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                buttonImage.color = Color.Lerp(initialColor, targetColor, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            buttonImage.color = targetColor;
        }

        IEnumerator GoToNextQuestionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            NextQuestion();
        }

        IEnumerator BounceAnimation(TextMeshProUGUI textMeshPro)
        {
            Vector3 originalScale = textMeshPro.transform.localScale;
            Vector3 targetScale = Vector3.one;
            float duration = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                float t = Mathf.Clamp01(elapsedTime / duration);
                float bounce = Mathf.Sin(t * Mathf.PI * 2) * 0.5f + 0.5f;
                textMeshPro.transform.localScale = Vector3.Lerp(originalScale, targetScale * bounce, t);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            textMeshPro.transform.localScale = targetScale;
        }
    }
}
