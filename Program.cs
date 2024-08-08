/*
MP4 file splitter using MediaToolkit
Splits an MP4 file in half and keeps the first half, using the MediaToolkit library.
*/

using System;
using System.Diagnostics;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        // Prompt the user for the file name
        Console.WriteLine("Please enter the name of the MP4 file (including extension):");
        string fileName = Console.ReadLine();

        // Get the current directory
        string currentDirectory = Directory.GetCurrentDirectory();

        // Get the full path to the input file
        string inputFilePath = Path.Combine(currentDirectory, fileName);

        // Define output file path with "_half" suffix
        string outputFilePath = Path.Combine(currentDirectory, Path.GetFileNameWithoutExtension(fileName) + "_half.mp4");

        // Check if the input file exists
        if (File.Exists(inputFilePath))
        {
            // Check if the output file already exists
            if (File.Exists(outputFilePath))
            {
                Console.WriteLine($"File {Path.GetFileName(outputFilePath)} already exists. Do you want to delete it and create a new one? (y/n)");
                string overwrite = Console.ReadLine();
                if (overwrite.ToLower() == "y")
                {
                    File.Delete(outputFilePath);
                }
                else
                {
                    Console.WriteLine("Operation cancelled.");
                    return;
                }
            }

            // Split the file
            KeepFirstHalf(inputFilePath, outputFilePath);
        }
        else
        {
            Console.WriteLine("File not found: " + Path.GetFileName(inputFilePath));
        }
    }

    static void KeepFirstHalf(string inputFilePath, string outputFilePath)
    {
        try
        {
            // Get the duration of the input file
            var duration = GetMediaDuration(inputFilePath);
            if (duration == TimeSpan.Zero)
            {
                Console.WriteLine("Could not determine the duration of the input file.");
                return;
            }

            // Calculate the half duration
            var halfDuration = TimeSpan.FromSeconds(duration.TotalSeconds / 2);

            // Run FFmpeg to split the file
            var ffmpegPath = "ffmpeg"; // Assuming ffmpeg is in the PATH
            var arguments = $"-i \"{inputFilePath}\" -t {halfDuration} -c copy \"{outputFilePath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("MP4 file split successfully, first half saved as " + Path.GetFileName(outputFilePath));
            }
            else
            {
                var error = process.StandardError.ReadToEnd();
                Console.WriteLine("An error occurred: " + error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    static TimeSpan GetMediaDuration(string filePath)
    {
        var ffmpegPath = "ffmpeg"; // Assuming ffmpeg is in the PATH
        var arguments = $"-i \"{filePath}\" -hide_banner";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        var durationMatch = System.Text.RegularExpressions.Regex.Match(error, @"Duration: (\d{2}):(\d{2}):(\d{2})\.(\d{2})");
        if (durationMatch.Success)
        {
            int hours = int.Parse(durationMatch.Groups[1].Value);
            int minutes = int.Parse(durationMatch.Groups[2].Value);
            int seconds = int.Parse(durationMatch.Groups[3].Value);
            int milliseconds = int.Parse(durationMatch.Groups[4].Value) * 10;
            return new TimeSpan(0, hours, minutes, seconds, milliseconds);
        }

        return TimeSpan.Zero;
    }
}

