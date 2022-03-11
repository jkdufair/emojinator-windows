using System.Diagnostics;

namespace The_Emojinator
{
	public class Emoji
	{
        public string? Name { get; set; }

        public string Url {
            get
            {
                return $"https://emoji-server.azurewebsites.net/emoji/{Name}";
            }
            private set { }
        }
    }
}
