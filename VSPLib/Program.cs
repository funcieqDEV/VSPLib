using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class VSPLib
{
    static async Task Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: vpm [init|install <name|url>]");
            return;
        }

        string command = args[0];
        if (command == "install" && args.Length == 2)
        {
            await InstallAsync(args[1]);
        }
        else if (command == "init" && args.Length == 1)
        {
            InitConfig();
        }
        else
        {
            Console.WriteLine("Unknown command or insufficient arguments.");
        }
    }

    static async Task InstallAsync(string name)
    {
        string libsPath = GetConfigPath();
        if (string.IsNullOrEmpty(libsPath))
        {
            Console.WriteLine("Cannot find \"libs\" path in config.txt!");
            return;
        }

        string url = name.StartsWith("http") ? name : $"https://raw.githubusercontent.com/funcieqDEV/VSharpLibs/main/libs/{name}.dll";
        string outputPath = Path.Combine(libsPath, $"{name}.dll");

        try
        {
            using HttpClient client = new HttpClient();
            using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error downloading file from: {url}");
                return;
            }

            long totalBytes = response.Content.Headers.ContentLength ?? -1;
            await using Stream contentStream = await response.Content.ReadAsStreamAsync();
            await using FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            const int bufferSize = 8192;
            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            int bytesRead;
            char[] loadingSymbols = { '|', '/', '-', '\\' };
            int symbolIndex = 0;

            Console.WriteLine("Downloading...");

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;

                if (totalBytes != -1)
                {
                    double percent = (double)totalRead / totalBytes * 100;
                    const int barWidth = 40;
                    int completedWidth = (int)(barWidth * totalRead / totalBytes);
                    string progressBar = new string('=', completedWidth) + new string(' ', barWidth - completedWidth);

                    string readableSize = FormatBytes(totalRead);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"\r[{progressBar}] ");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{percent:0.00}% ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{readableSize} downloaded");
                }
                else
                {
                    string readableSize = FormatBytes(totalRead);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"\r{loadingSymbols[symbolIndex++ % loadingSymbols.Length]} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"{readableSize} downloaded");
                }
            }

            Console.WriteLine("\nDownload complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during download: {ex.Message}");
        }
        finally
        {
            Console.ResetColor();
        }
    }

    static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:0.##} KB";
        else if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):0.##} MB";
        else
            return $"{bytes / (1024.0 * 1024 * 1024):0.##} GB";
    }

    static void InitConfig()
    {
        if (File.Exists("config.txt"))
        {
            Console.WriteLine("config.txt already exists.");
            return;
        }

        Console.WriteLine("Enter the path to the directory where libraries will be stored (e.g., C:\\libs):");
        string libsPath = Console.ReadLine();

        if (string.IsNullOrEmpty(libsPath))
        {
            Console.WriteLine("Path cannot be empty.");
            return;
        }

        try
        {
            Directory.CreateDirectory(libsPath);
            File.WriteAllText("config.txt", $"libs={libsPath}");
            Console.WriteLine($"config.txt has been created with the path: {libsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating config.txt: {ex.Message}");
        }
    }

    static string GetConfigPath()
    {
        if (!File.Exists("config.txt"))
            return "";

        foreach (var line in File.ReadLines("config.txt"))
        {
            if (line.StartsWith("libs="))
                return line.Substring(5);
        }
        return "";
    }
}
