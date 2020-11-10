using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace MogAmpUI.Pages
{
	public partial class Player : UserControl
	{
		/*
         * Statics, constants, enums, magic, etc.
         */

		private enum PlayerState
		{
			JustOpened   = 0,
			FileOpened   = 1,
			FileUploaded = 2,
			BardsPlaying = 3,

			BardsWaiting = 4,
		}

		/*
         * Constructors, destructors, overrides, etc.
         */

		public Player()
		{
			InitializeComponent();

			this.Label_SongName.IgnoreParentWidth = true;
			this.Label_SongTime.IgnoreParentWidth = false;
			this.State = PlayerState.JustOpened;
		}

		/*
		 * UI element interaction handling
		 */

		private void Button_LoadSong_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openDialog = new OpenFileDialog();
			openDialog.Multiselect = false;
			openDialog.RestoreDirectory = true;
			openDialog.DefaultExt = "mmsong";
			openDialog.Filter = "mmsong files (*.mmsong)|*.mmsong|All files (*.*)|*.*";
			if (openDialog.ShowDialog() ?? false && !string.IsNullOrEmpty(openDialog.FileName))
			{
				// temp execution for showcase
				this.State = PlayerState.FileOpened;

				this.Label_SongName.Content = Path.GetFileNameWithoutExtension(openDialog.FileName);
				this.Label_SongTime.Content = "00:00 / 01:23";
			}
		}

		private void Button_SendSong_Click(object sender, RoutedEventArgs e)
		{
			// temp execution for showcase
			this.State = PlayerState.FileUploaded;
		}

		private void Button_Play_Click(object sender, RoutedEventArgs e)
		{
			// temp execution for showcase
			this.State = PlayerState.BardsPlaying;

			this.Label_SongTime.Content = "00:45 / 01:23";
		}

		private void Button_Stop_Click(object sender, RoutedEventArgs e)
		{
			// temp execution for showcase
			this.State = PlayerState.BardsWaiting;
		}

		private void Button_LoadSong_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Path_LoadSong.Fill = (sender as Button).IsEnabled ? Brushes.White : Brushes.DimGray;
		}

		private void Button_SendSong_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Path_SendSong.Fill = (sender as Button).IsEnabled ? Brushes.White : Brushes.DimGray;
		}

		private void Button_Play_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Path_Play.Fill = (sender as Button).IsEnabled ? Brushes.White : Brushes.DimGray;
		}

		private void Button_Stop_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.Path_Stop.Fill = (sender as Button).IsEnabled ? Brushes.White : Brushes.DimGray;
		}

		/*
		 * UI state modification and management
		 */

		private PlayerState State
		{
			set { this.UpdateUIForPlayerState(value); }
		}

		private void UpdateUIForPlayerState(PlayerState state)
		{
			if (state == PlayerState.JustOpened)
			{
				this.Button_LoadSong.IsEnabled = true;
				this.Button_SendSong.IsEnabled = false;
				this.Button_Play.IsEnabled = false;
				this.Button_Stop.IsEnabled = false;
				this.Label_PlayerStatus.Content = "Kupo!";
				this.Label_SongName.Content = "";
				this.Label_SongTime.Content = "00:00 / 00:00";
			}
			else if (state == PlayerState.FileOpened)
			{
				this.Button_LoadSong.IsEnabled = true;
				this.Button_SendSong.IsEnabled = true;
				this.Button_Play.IsEnabled = false;
				this.Button_Stop.IsEnabled = false;
				this.Label_PlayerStatus.Content = "Loaded";
			}
			else if (state == PlayerState.FileUploaded)
			{
				this.Button_LoadSong.IsEnabled = true;
				this.Button_SendSong.IsEnabled = false;
				this.Button_Play.IsEnabled = true;
				this.Button_Stop.IsEnabled = false;
				this.Label_PlayerStatus.Content = "Ready";
			}
			else if (state == PlayerState.BardsPlaying)
			{
				this.Button_LoadSong.IsEnabled = false;
				this.Button_SendSong.IsEnabled = false;
				this.Button_Play.IsEnabled = false;
				this.Button_Stop.IsEnabled = true;
				this.Label_PlayerStatus.Content = "Playing";
			}
			else if (state == PlayerState.BardsWaiting)
			{
				this.Button_LoadSong.IsEnabled = true;
				this.Button_SendSong.IsEnabled = false;
				this.Button_Play.IsEnabled = false;
				this.Button_Stop.IsEnabled = false;
				this.Label_PlayerStatus.Content = "Waiting";
			}

			ClearAllFocusElements();
		}

		/*
		 * Misc. helpers
		 */

		private void ClearAllFocusElements()
		{
			// this should prevent buttons from owning focus if we click on them
			FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), null);

			// and also ensure that accidental keypresses won't trigger UI actions
			Keyboard.ClearFocus();
		}
	}
}
