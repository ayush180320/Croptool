using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace VideoCropper
{
    public partial class MainWindow : Window
    {
        private Rectangle? activeLine = null;
        private string videoFilePath = @"C:\video.mp4"; // Will be dynamic in a full app

        private double actualVideoWidth = 1920; 
        private double actualVideoHeight = 1080;

        public MainWindow()
        {
            InitializeComponent();
            try {
                VideoPlayer.Source = new Uri(videoFilePath);
                VideoPlayer.Play();
            } catch { } // Failsafe if default video doesn't exist yet
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

            if (activeLine == LineTop) Canvas.SetTop(LineTop, mousePos.Y);
            else if (activeLine == LineBottom) Canvas.SetTop(LineBottom, mousePos.Y);
            else if (activeLine == LineLeft) Canvas.SetLeft(LineLeft, mousePos.X);
            else if (activeLine == LineRight) Canvas.SetLeft(LineRight, mousePos.X);

            CalculateCropValues();
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            activeLine = null;
            CropCanvas.ReleaseMouseCapture();
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LineTop.Width = LineBottom.Width = CropCanvas.ActualWidth;
            LineLeft.Height = LineRight.Height = CropCanvas.ActualHeight;
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

            // Prevent negative values from dragging out of bounds
            TxtTop.Text = Math.Max(0, topCrop).ToString();
            TxtBottom.Text = Math.Max(0, bottomCrop).ToString();
            TxtLeft.Text = Math.Max(0, leftCrop).ToString();
            TxtRight.Text = Math.Max(0, rightCrop).ToString();
        }

        private async void BtnAutoCrop_Click(object sender, RoutedEventArgs e)
        {
            string cropString = await FFmpegHelper.AutoDetectCropAsync(videoFilePath);
            MessageBox.Show($"FFmpeg detected crop: {cropString}");
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            int width = (int)actualVideoWidth - int.Parse(TxtLeft.Text) - int.Parse(TxtRight.Text);
            int height = (int)actualVideoHeight - int.Parse(TxtTop.Text) - int.Parse(TxtBottom.Text);
            string ffmpegCropFilter = $"crop={width}:{height}:{TxtLeft.Text}:{TxtTop.Text}";

            FFmpegHelper.EncodeProRes(videoFilePath, @"C:\output.mov", ffmpegCropFilter);
            MessageBox.Show("Encoding started! Check your output folder.");
        }
    }
}
