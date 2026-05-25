using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Win32; 

namespace VideoCropper
{
    public partial class MainWindow : Window
    {
        private Rectangle? activeLine = null;
        private string videoFilePath = ""; 

        private double actualVideoWidth = 1920; 
        private double actualVideoHeight = 1080;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files|*.mp4;*.mkv;*.mov;*.avi|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                videoFilePath = openFileDialog.FileName;
                TxtFilePath.Text = videoFilePath;
                
                VideoPlayer.Source = new Uri(videoFilePath);
                VideoPlayer.Play();
                VideoPlayer.Pause(); 
            }
        }

        private void Line_MouseDown(object sender, MouseButtonEventArgs e)
        {
            activeLine = sender as Rectangle;
            CropCanvas.CaptureMouse();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (activeLine == null) return;

            Point mousePos = e.GetPosition(CropCanvas);

            if (activeLine == LineTop) Canvas.SetTop(LineTop, Math.Max(0, mousePos.Y));
            else if (activeLine == LineBottom) Canvas.SetTop(LineBottom, Math.Min(CropCanvas.ActualHeight, mousePos.Y));
            else if (activeLine == LineLeft) Canvas.SetLeft(LineLeft, Math.Max(0, mousePos.X));
            else if (activeLine == LineRight) Canvas.SetLeft(LineRight, Math.Min(CropCanvas.ActualWidth, mousePos.X));

            CalculateCropValues();
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            activeLine = null;
            CropCanvas.ReleaseMouseCapture();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas.SetTop(LineTop, 0);
            Canvas.SetTop(LineBottom, CropCanvas.ActualHeight);
            Canvas.SetLeft(LineLeft, 0);
            Canvas.SetLeft(LineRight, CropCanvas.ActualWidth);
        }

        private void CalculateCropValues()
        {
            if (CropCanvas.ActualWidth == 0 || CropCanvas.ActualHeight == 0) return;

            double scaleX = actualVideoWidth / CropCanvas.ActualWidth;
            double scaleY = actualVideoHeight / CropCanvas.ActualHeight;

            int topCrop = (int)(Canvas.GetTop(LineTop) * scaleY);
            int bottomCrop = (int)(actualVideoHeight - (Canvas.GetTop(LineBottom) * scaleY));
            int leftCrop = (int)(Canvas.GetLeft(LineLeft) * scaleX);
            int rightCrop = (int)(actualVideoWidth - (Canvas.GetLeft(LineRight) * scaleX));

            TxtTop.Text = Math.Max(0, topCrop).ToString();
            TxtBottom.Text = Math.Max(0, bottomCrop).ToString();
            TxtLeft.Text = Math.Max(0, leftCrop).ToString();
            TxtRight.Text = Math.Max(0, rightCrop).ToString();
        }

        private async void BtnAutoCrop_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(videoFilePath))
            {
                MessageBox.Show("Please browse for a video file first.");
                return;
            }

            string cropString = await FFmpegHelper.AutoDetectCropAsync(videoFilePath);
            MessageBox.Show($"FFmpeg detected the following crop format: {cropString}");
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(videoFilePath))
            {
                MessageBox.Show("Please browse for a video file first.");
                return;
            }

            int width = (int)actualVideoWidth - int.Parse(TxtLeft.Text) - int.Parse(TxtRight.Text);
            int height = (int)actualVideoHeight - int.Parse(TxtTop.Text) - int.Parse(TxtBottom.Text);
            string ffmpegCropFilter = $"crop={width}:{height}:{TxtLeft.Text}:{TxtTop.Text}";

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "MOV File|*.mov",
                DefaultExt = ".mov"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FFmpegHelper.EncodeProRes(videoFilePath, saveFileDialog.FileName, ffmpegCropFilter);
                MessageBox.Show("Encoding started! A command window will open to show progress.");
            }
        }
    }
}
