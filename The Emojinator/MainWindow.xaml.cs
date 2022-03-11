using System;
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

		public MainWindow()
		{
			InitializeComponent();
		}

		private void EmojiFilterTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
						Math.Min(selectedIndex + 8, EmojiListBox.Items.Count));
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
                    {
                        Clipboard.SetText(
                            $"Version:1.0{Environment.NewLine}StartHTML:000000096{Environment.NewLine}EndHTML:000000{280 + selectedEmoji.Name.Length}{Environment.NewLine}StartFragment:000000186{Environment.NewLine}EndFragment:000000{266 + selectedEmoji.Name.Length}" +
                            $"{Environment.NewLine}<html><head><meta http-equiv=Content-Type content=\"text/html;charset=utf-8\"></head><body><meta charset='utf-8'><img src=\"{selectedEmoji.Url}\"/></body></html>",
                        TextDataFormat.Html);
                    }
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
				EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(0);
			}
				
        }
    }
}
