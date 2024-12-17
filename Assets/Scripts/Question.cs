using System.Collections.Generic;

namespace Hanzo.Quiz
{

    // Question class and related structure
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public string[] answers; // Array of answer options
        public int correctAnswerIndex; // Index of the correct answer in the answers array
    }

    [System.Serializable]
    public class QuestionList
    {
        public List<Question> questions; // List of questions
    }
}
