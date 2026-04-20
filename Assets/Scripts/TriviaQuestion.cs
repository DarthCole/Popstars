[System.Serializable]
public class TriviaQuestion
{
    public string   question;
    public string[] answers;      // index 0 is always the CORRECT answer
    public string[] wrongAnswers; // 3 wrong answers for solo multiple-choice mode
    public int      correctIndex; // always 0 in the bank; shuffled at runtime
}
