using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoCropper
{
    public static class FFmpegHelper
    {
        private static string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg.exe");

        public static async Task<string> AutoDetectCropAsync(string inputPath)
        {
            if (!File.Exists(ffmpegPath)) return $"Engine missing at: {ffmpegPath}";

            string cropValue = "No crop detected";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputPath}\" -t 2 -vf cropdetect -f null -",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo)!)
            {
                string output = await process.StandardError.ReadToEndAsync();
                Match match = Regex.Match(output, @"crop=\d+:\d+:\d+:\d+");
                if (match.Success) cropValue = match.Value;
            }
            return cropValue;
        }

        public static void EncodeProRes(string inputPath, string outputPath, string cropFilter)
        {
            if (!File.Exists(ffmpegPath))
            {
                System.Windows.MessageBox.Show("FFmpeg engine not found.");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputPath}\" -vf \"{cropFilter}\" -c:v prores_ks -profile:v 3 -c:a pcm_s16le \"{outputPath}\"",
                UseShellExecute = true, 
                CreateNoWindow = false
            };

            Process.Start(startInfo);
        }
    }
}
