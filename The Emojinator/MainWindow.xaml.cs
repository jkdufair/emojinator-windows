using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

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
				case System.Windows.Input.Key.Up:
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(Math.Min(selectedIndex + 8, EmojiListBox.Items.Count));
					isHandled = true;
					break;
				case System.Windows.Input.Key.Down:
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(Math.Max(selectedIndex - 8, 0));
					isHandled = true;
					break;
				case System.Windows.Input.Key.Left:
					isHandled = true;
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(Math.Max(selectedIndex - 1, 0));
					break;
				case System.Windows.Input.Key.Right:
					isHandled = true;
					EmojiListBox.SelectedItem = EmojiListBox.Items.GetItemAt(Math.Min(selectedIndex + 1, EmojiListBox.Items.Count));
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
