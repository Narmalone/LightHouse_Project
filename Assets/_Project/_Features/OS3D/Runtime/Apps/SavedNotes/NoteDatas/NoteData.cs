namespace LightHouse.Features.Computer.NoteSystem
{
    [System.Serializable]
    public class NoteData
    {
        public string ID;
        public string Title;
        public string Content;

        public NoteData(string title, string content)
        {
            Title = title;
            Content = content;
            ID = System.Guid.NewGuid().ToString();
        }
    }

}
