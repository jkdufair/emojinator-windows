using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace The_Emojinator
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		int _previousItemCount = 0;
		KeyboardHook _hook = new();

		public MainWindow()
		{
			InitializeComponent();

			DataContext = new EmojiDataContext();
			ShowInTaskbar = false;
			WindowStartupLocation = WindowStartupLocation.CenterScreen;

			_hook.KeyPressed += new EventHandler<KeyPressedEventArgs>(hook_KeyPressed);
			_hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, System.Windows.Forms.Keys.F12);
		}

		private void EmojiFilterTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var selectedIndex = EmojiListBox.SelectedItem == null ? 0 : EmojiListBox.Items.IndexOf(EmojiListBox.SelectedItem);
			var isHandled = false;
			switch (e.Key)
			{
				case Key.Up:
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(
						Math.Max(selectedIndex - 8, 0));
					isHandled = true;
					break;
				case Key.Down:
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(
						Math.Min(selectedIndex + 8, EmojiListBox.Items.Count - 1));
					isHandled = true;
					break;
				case Key.Left:
					isHandled = true;
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(
						Math.Max(selectedIndex - 1, 0));
					break;
				case Key.Right:
					isHandled = true;
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(
						Math.Min(selectedIndex + 1, EmojiListBox.Items.Count));
					break;
				case Key.Enter:
					isHandled = true;
                    if (EmojiListBox.SelectedItem is Emoji selectedEmoji && selectedEmoji.Name != null)
						CopyEmojiToClipboard(selectedEmoji);
                    break;
				default:
					break;
			}
			e.Handled = isHandled;
		}

        private void EmojiListBox_LayoutUpdated(object sender, EventArgs e)
        {
			if (EmojiListBox.Items.Count != _previousItemCount)
            {
				_previousItemCount = EmojiListBox.Items.Count;
				if (EmojiListBox.Items.Count > 0)
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(0);
			}
				
        }

        private void EmojiListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not System.Windows.Controls.Image imageControl) return;
            if (imageControl.DataContext is not Emoji selectedEmoji) return;
            CopyEmojiToClipboard(selectedEmoji);
        }

        private void CopyEmojiToClipboard(Emoji selectedEmoji)
        {
			if (selectedEmoji.Name == null) return;
            Clipboard.SetText(
                $"Version:1.0{Environment.NewLine}StartHTML:000000096{Environment.NewLine}EndHTML:000000{280 + selectedEmoji.Name.Length}{Environment.NewLine}StartFragment:000000186{Environment.NewLine}EndFragment:000000{266 + selectedEmoji.Name.Length}" +
                $"{Environment.NewLine}<html><head><meta http-equiv=Content-Type content=\"text/html;charset=utf-8\"></head><body><meta charset='utf-8'><img src=\"{selectedEmoji.Url}\"/></body></html>",
            TextDataFormat.Html);
			Visibility = Visibility.Collapsed;
			BringTeamsToFront();
        }

		[System.Runtime.InteropServices.DllImport("User32.dll")]
		private static extern bool SetForegroundWindow(IntPtr handle);

		private IntPtr handle;

		private void BringTeamsToFront()
		{
			Process[] processess = Process.GetProcessesByName("Teams");
			if (processess.Length != 0)
			{
				//Set foreground window
				foreach (var process in processess)
                {
					if (process.MainWindowHandle != IntPtr.Zero)
                    {
						handle = process.MainWindowHandle;
						SetForegroundWindow(handle);
						return;
					}
				}
			}
		}

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
			Visibility = Visibility.Collapsed;
			e.Cancel = true;
        }

		void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
			Visibility = Visibility.Visible;
			Activate();
		}
	}
}
