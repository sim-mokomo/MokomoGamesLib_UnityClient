namespace MokomoGamesLib.Runtime.Licences
{
    public class Licence
    {
        public Licence(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title { get; }
        public string Content { get; }
    }
}