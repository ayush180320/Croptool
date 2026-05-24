using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoCropper
{
    public static class FFmpegHelper
    {
        private static string ffmpegPath = "ffmpeg.exe"; // Ensure ffmpeg.exe is in the same folder as your built app

        public static async Task<string> AutoDetectCropAsync(string inputPath)
        {
            string cropValue = "No crop detected";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputPath}\" -t 2 -vf cropdetect -f null -",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (Process process = Process.Start(startInfo)!)
                {
                    string output = await process.StandardError.ReadToEndAsync();
                    
                    Match match = Regex.Match(output, @"crop=\d+:\d+:\d+:\d+");
                    if (match.Success)
                    {
                        cropValue = match.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: Make sure ffmpeg.exe is installed. {ex.Message}";
            }

            return cropValue;
        }

        public static void EncodeProRes(string inputPath, string outputPath, string cropFilter)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputPath}\" -vf \"{cropFilter}\" -c:v prores_ks -profile:v 3 -c:a pcm_s16le \"{outputPath}\"",
                UseShellExecute = true, 
                CreateNoWindow = false
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to launch FFmpeg: {ex.Message}");
            }
        }
    }
}
