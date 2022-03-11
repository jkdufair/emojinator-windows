using FuseSharp;
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
			#pragma warning disable CS4014 // We don't need to await here. Binding will update when it comes back
			FetchEmojiListAsync();
			#pragma warning restore CS4014
		}

		public IEnumerable<Emoji>? AllEmojis { get; set; }

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
				var fuse = new Fuse(location: 0, distance: 20, threshold: 0.99, maxPatternLength: 32, isCaseSensitive: false, tokenize: false);
				if (string.IsNullOrEmpty(value))
				{
					FilteredEmojis = AllEmojis;
				} else
				{
					var results = fuse.Search(value ?? "", AllEmojis);
					FilteredEmojis = results.Select(x =>
					{
						return AllEmojis.ElementAt(x.Index);
					}).Take(30);
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private async Task FetchEmojiListAsync()
		{
			using (var httpClient = new HttpClient())
			{
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
}
