using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace The_Emojinator
{
    public class EmojiDataContext : INotifyPropertyChanged
    {
        private IEnumerable<Emoji>? _emojis;

        public EmojiDataContext()
        {
            FetchEmojiListAsync();
        }
        public IEnumerable<Emoji>? Emojis {
            get { 
                return _emojis;
            }
            set
            {
                _emojis = value;
                OnPropertyChanged(nameof(Emojis));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private async Task FetchEmojiListAsync()
        {
            using HttpClient httpClient = new();
            var response = await httpClient.GetAsync("https://emoji-server.azurewebsites.net/emojis");
            if (response == null) return;
            var emojis = await response.Content.ReadAsStringAsync();
            if (emojis == null) return;
            var deserializedEmojis = JsonConvert.DeserializeObject<List<string>>(emojis);
            if (deserializedEmojis == null) return;
            Emojis = deserializedEmojis.Select(e => new Emoji
            {
                Url = $"https://emoji-server.azurewebsites.net/emoji/{e}"
            });
        }
    }
}
