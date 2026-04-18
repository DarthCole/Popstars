[System.Serializable]
public class TriviaQuestion
{
    public string id;
    public string category;
    public string difficulty;
    public string question;
    public string[] options;
    public int correctIndex;
    public string explanation;
}