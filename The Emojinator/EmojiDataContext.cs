using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace The_Emojinator
{
	public class EmojiDataContext : INotifyPropertyChanged
	{
		private IEnumerable<Emoji>? _filteredEmojis;
		private string? _emojiFilter;

		public EmojiDataContext()
		{
			FetchEmojiListAsync();
		}

        public IEnumerable<Emoji> AllEmojis { get; set; }

        public IEnumerable<Emoji>? FilteredEmojis
		{
			get
			{ 
				return _filteredEmojis;
			}
			set
			{
				_filteredEmojis = value;
				OnPropertyChanged(nameof(FilteredEmojis));
			}
		}

		public string? EmojiFilter
		{
			get
            {
				return _emojiFilter;
            }
			set
            {
				_emojiFilter = value;
				OnPropertyChanged(nameof(EmojiFilter));
				if (AllEmojis == null) return;
				if (string.IsNullOrEmpty(value) || value.Length < 3)
					FilteredEmojis = AllEmojis;
				else
					FilteredEmojis = AllEmojis.Where(x => x.Name != null && x.Name.Contains(value));


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
			AllEmojis = deserializedEmojis.Select(name => new Emoji
			{
				Name = name
			});
			FilteredEmojis = AllEmojis;
		}
	}
}
