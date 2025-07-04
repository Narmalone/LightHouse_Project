[System.Serializable]
public class NoteData
{
    public string Title;
    public string Content;

    public NoteData(string title, string content)
    {
        Title = title;
        Content = content;
    }
}
