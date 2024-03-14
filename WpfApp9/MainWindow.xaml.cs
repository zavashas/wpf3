using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace WpfApp9
{
    public partial class MainWindow : Window
    {
        private List<string> audioFiles;
        private int currentIndex;
        private bool repeatTrue;
        private bool shuffleTrue;
        private Random random;
        private List<string> playHistory;
        private DispatcherTimer positionTimer;

        public MainWindow()
        {
            InitializeComponent();
            audioFiles = new List<string>();
            currentIndex = 0;
            repeatTrue = false;
            shuffleTrue = false;
            random = new Random();
            playHistory = new List<string>();
            positionTimer = new DispatcherTimer();
            positionTimer.Interval = TimeSpan.FromSeconds(1);
            positionTimer.Tick += PositionTimer_Tick;
            mediaElement.MediaEnded += MediaElement_MediaEnded;
        }
        private bool play = false;
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Multiselect = false,
                Title = "Выберите папку с музыкой"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                string selectedFolder = dialog.FileName;
                string[] newAudioFiles = Directory.GetFiles(selectedFolder, "*.mp3", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(selectedFolder, "*.m4a", SearchOption.AllDirectories))
                    .Concat(Directory.GetFiles(selectedFolder, "*.wav", SearchOption.AllDirectories))
                    .ToArray();

                audioFiles = newAudioFiles.ToList();

                if (audioFiles.Count > 0)
                {
                    PlayAudio(0);
                    UpdateAudioFilesListBox();
                }
            }
        }

        private void UpdateAudioFilesListBox()
        {
            AudioFilesListBox.Items.Clear();
            foreach (string file in audioFiles)
            {
                AudioFilesListBox.Items.Add(Path.GetFileName(file));
            }
        }


        public void PlayAudio(int index)
        {
            if (index >= 0 && index < audioFiles.Count)
            {
                currentIndex = index;
                mediaElement.Source = new Uri(audioFiles[currentIndex], UriKind.Absolute);
                mediaElement.Play();
                playHistory.Add(audioFiles[currentIndex]);
                UpdatePositionTimer();
            }
        }

        private void ShuffleAudioFiles()
        {
            int n = audioFiles.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                string value = audioFiles[k];
                audioFiles[k] = audioFiles[n];
                audioFiles[n] = value;
            }
            PlayAudio(0);
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeatTrue)
            {
                PlayAudio(currentIndex);
            }
            else if (shuffleTrue)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayNextAudio();
            }
        }

        private void PlayNextAudio()
        {
            int nextIndex;
            if (shuffleTrue)
            {
                nextIndex = random.Next(audioFiles.Count);
            }
            else
            {
                nextIndex = (currentIndex + 1) % audioFiles.Count;
            }
            PlayAudio(nextIndex);
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement.Source != null)
            {
                if (mediaElement.CanPause && play)
                {
                    mediaElement.Pause();
                    play = false;
                    PlayPause.Content = "Пауза";
                }
                else
                {
                    mediaElement.Play();
                    play = true;
                    PlayPause.Content = "Играть";
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleTrue)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayAudio((currentIndex - 1 + audioFiles.Count) % audioFiles.Count);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleTrue)
            {
                ShuffleAudioFiles();
            }
            else
            {
                PlayAudio((currentIndex + 1) % audioFiles.Count);
            }
        }

        private void Repeat_Click(object sender, RoutedEventArgs e)
        {
            repeatTrue = !repeatTrue;
            Repeat.Content = repeatTrue ? "Вкл" : "Выкл";
        }

        private void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleTrue = !shuffleTrue;
            Shuffle.Content = shuffleTrue ? "Вкл" : "Выкл";
            if (shuffleTrue && audioFiles.Count > 0)
            {
                ShuffleAudioFiles();
            }
        }

        private void PositionTimer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                PositionSlider.Value = mediaElement.Position.TotalSeconds;
                PositionSlider.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;

                TimeSpan elapsedTime = mediaElement.Position;
                CurrentTimeTextBlock.Text = string.Format("{0:mm\\:ss}", elapsedTime);

                TimeSpan remainingTime = mediaElement.NaturalDuration.TimeSpan - elapsedTime;
                RemainingTimeTextBlock.Text = string.Format("-{0:mm\\:ss}", remainingTime);
            }
        }

        private void UpdatePositionTimer()
        {
            positionTimer.Stop();
            positionTimer.Start();
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                TimeSpan newPosition = TimeSpan.FromSeconds(e.NewValue);
                mediaElement.Position = newPosition;
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mediaElement.Volume = e.NewValue / 100.0;
        }

        private void History_Click(object sender, RoutedEventArgs e)
        {
            Window1 window1 = new Window1(playHistory);
            window1.ShowDialog();
        }
        private void AudioFilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AudioFilesListBox.SelectedItem != null)
            {
                int selectedIndex = AudioFilesListBox.SelectedIndex;
                PlayAudio(selectedIndex);
                AudioFilesListBox.SelectedItem = null;
            }
        }
    }

}