using FuseSharp;
using Newtonsoft.Json;
using NHotkey;
using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace The_Emojinator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			KillOtherEmojinators();
			InitializeComponent();

#pragma warning disable CS4014 // We don't need to await here. Binding will update when it comes back
			FetchEmojiListAsync();
#pragma warning restore CS4014

			DataContext = this;
			ShowInTaskbar = false;
			WindowStartupLocation = WindowStartupLocation.CenterScreen;

			HotkeyManager.Current.AddOrReplace("ShowWindow", Key.Enter, ModifierKeys.Control | ModifierKeys.Alt, Hook_KeyPressed);
		}

		private void Hook_KeyPressed(object sender, HotkeyEventArgs e)
		{
			Visibility = Visibility.Visible;
			Activate();
		}

		private void SetSelectedItem(int index)
		{
			object item = EmojiListBox.Items.GetItemAt(index);
			EmojiListBox.SelectedItem = item;
			var emoji = (Emoji)item;
			emoji.Size = 48;
			SelectedEmojiName = $":{emoji.Name ?? ""}:";
			SelectedEmojiUrl = emoji.Url ?? "";
		}

		private void EmojiFilterTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var selectedIndex = EmojiListBox.SelectedItem == null ? 0 : EmojiListBox.Items.IndexOf(EmojiListBox.SelectedItem);
			var isHandled = false;

			switch (e.Key)
			{
				case Key.Up:
					SetSelectedItem(Math.Max(selectedIndex - 10, 0));
					isHandled = true;
					break;

				case Key.Down:
					SetSelectedItem(Math.Min(selectedIndex + 10, EmojiListBox.Items.Count - 1));
					isHandled = true;
					break;

				case Key.Left:
					SetSelectedItem(Math.Max(selectedIndex - 1, 0));
					isHandled = true;
					break;

				case Key.Right:
					SetSelectedItem(Math.Min(selectedIndex + 1, EmojiListBox.Items.Count));
					isHandled = true;
					break;

				case Key.Enter:
					if (EmojiListBox.SelectedItem is Emoji selectedEmoji && selectedEmoji.Name != null)
						CopyEmojiToClipboardWithModifiers(selectedEmoji);
					isHandled = true;
					break;

				case Key.Escape:
					ResetView();
					isHandled = true;
					break;

				default:
					break;
			}
			e.Handled = isHandled;
		}

		private void EmojiListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.OriginalSource is not System.Windows.Controls.Image imageControl) return;
			if (imageControl.DataContext is not Emoji selectedEmoji) return;
			CopyEmojiToClipboardWithModifiers(selectedEmoji);
		}

		private void CopyEmojiToClipboardWithModifiers(Emoji selectedEmoji)
		{
			var size = 24;
			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) size = 36;
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) size = 48;
			CopyEmojiToClipboard(new Emoji { Name = selectedEmoji.Name, Size = size });
		}

		private void CopyEmojiToClipboard(Emoji selectedEmoji)
		{
			if (selectedEmoji.Name == null) return;
			Clipboard.SetText(
				$"Version:1.0{Environment.NewLine}StartHTML:000000096{Environment.NewLine}EndHTML:000000{280 + selectedEmoji.Name.Length}{Environment.NewLine}StartFragment:000000186{Environment.NewLine}EndFragment:000000{286 + selectedEmoji.Name.Length * 2 + selectedEmoji.Url.Length}" +
				$"{Environment.NewLine}<html><head><meta http-equiv=Content-Type content=\"text/html;charset=utf-8\"></head><body><meta charset='utf-8'><img src=\"{selectedEmoji.Url}\" alt=\":{selectedEmoji.Name}:\" title=\":{selectedEmoji.Name}:\"/></body></html>",
			TextDataFormat.Html);
			ResetView();
			BringTeamsToFront();
		}

		[System.Runtime.InteropServices.DllImport("User32.dll")]
		private static extern bool SetForegroundWindow(IntPtr handle);

		private IntPtr _handle;

		private void BringTeamsToFront()
		{
			Process[] processes = Process.GetProcessesByName("Teams");
			if (processes.Length != 0)
			{
				//Set foreground window
				foreach (var process in processes)
				{
					if (process.MainWindowHandle != IntPtr.Zero)
					{
						_handle = process.MainWindowHandle;
						SetForegroundWindow(_handle);
						return;
					}
				}
			}
		}

		private void KillOtherEmojinators()
		{
			Process[] processes = Process.GetProcessesByName("The Emojinator");
			if (processes.Length != 0)
			{
				foreach (var process in processes)
				{
					if (process.Id != Process.GetCurrentProcess().Id)
					{
						process.Kill();
						return;
					}
				}
			}
		}

		private void ResetView()
		{
			EmojiFilterTextBox.Text = "";
			SetSelectedItem(0);
			Visibility = Visibility.Collapsed;
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			ResetView();
			e.Cancel = true;
		}

		private void myNotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			Visibility = Visibility.Visible;
			Activate();
		}

		private IEnumerable<Emoji>? _filteredEmojis;
		private string? _emojiFilter;
		private string? _selectedEmojiName;
		private string? _selectedEmojiUrl;

		public IEnumerable<Emoji>? AllEmojis { get; set; }

		public IEnumerable<Emoji>? FilteredEmojis
		{
			get => _filteredEmojis;
			set
			{
				_filteredEmojis = value;
				OnPropertyChanged(nameof(FilteredEmojis));
				SetSelectedItem(0);
			}
		}

		public string? SelectedEmojiName
		{
			get => _selectedEmojiName;
			set
			{
				_selectedEmojiName = value;
				OnPropertyChanged(nameof(SelectedEmojiName));
			}
		}

		public string? SelectedEmojiUrl
		{
			get => _selectedEmojiUrl;
			set
			{
				_selectedEmojiUrl = value;
				OnPropertyChanged(nameof(SelectedEmojiUrl));
			}
		}

		public string? EmojiFilter
		{
			get => _emojiFilter;
			set
			{
				_emojiFilter = value;
				OnPropertyChanged(nameof(EmojiFilter));
				if (AllEmojis == null) return;
				var fuse = new Fuse(location: 0, distance: 100, threshold: 0.35, maxPatternLength: 32, isCaseSensitive: false, tokenize: false);
				if (string.IsNullOrEmpty(value))
				{
					FilteredEmojis = AllEmojis;
				}
				else
				{
					var results = fuse.Search(value ?? "", AllEmojis);
					FilteredEmojis = results.Select(x => AllEmojis.ElementAt(x.Index)).Take(30);
				}
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void OnPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
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