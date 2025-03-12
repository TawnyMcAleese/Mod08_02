using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class FileBackupSystem
{
    static string sourceDirectory = @"C:\Users\Tawny\source\repos\CS3280\Mod08_02\Mod08_02\WorkFiles\";  // Change as needed
    static string backupDirectory = @"C:\Users\Tawny\source\repos\CS3280\Mod08_02\Mod08_02\Backup\";     // Change as needed
    static string logFilePath = Path.Combine(backupDirectory, "backup_log.txt");

    static void Main()
    {
        Console.WriteLine($"📂 Monitoring directory: {sourceDirectory}");
        Console.WriteLine("Press Ctrl+C to stop the program.\n");

        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"❌ Error: Source directory does not exist: {sourceDirectory}");
            return;
        }

        if (!Directory.Exists(backupDirectory))
        {
            Directory.CreateDirectory(backupDirectory);
        }

        using (FileSystemWatcher watcher = new FileSystemWatcher(sourceDirectory))
        {
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Changed += OnFileChanged;
            watcher.Created += OnFileChanged;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("🔍 Watching for changes...");
            Console.ReadLine();  // Keep program running
        }
    }

    static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        string filePath = e.FullPath;
        string fileName = Path.GetFileName(filePath);

        // Wait to ensure file is fully written
        System.Threading.Thread.Sleep(500);

        if (!File.Exists(filePath))
            return;  // File might be deleted before processing

        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            string backupFileName = $"{fileName}.{timestamp}";
            string backupFilePath = Path.Combine(backupDirectory, backupFileName);

            // Copy the modified file to the backup directory
            File.Copy(filePath, backupFilePath, true);
            Console.WriteLine($"✅ Backup created: {backupFilePath}");

            LogBackupOperation(fileName, filePath, backupFilePath);
            CleanupOldBackups(fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error backing up file {fileName}: {ex.Message}");
        }
    }

    static void LogBackupOperation(string fileName, string originalFilePath, string backupFilePath)
    {
        long fileSize = new FileInfo(originalFilePath).Length;
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Backup created: {fileName}\n" +
                          $"Original: {originalFilePath}\n" +
                          $"Backup: {backupFilePath}\n" +
                          $"Size: {fileSize / 1024.0:F2} KB\n";

        File.AppendAllText(logFilePath, logEntry + "\n");
    }

    static void CleanupOldBackups(string fileName)
    {
        string[] backupFiles = Directory.GetFiles(backupDirectory, $"{fileName}.*")
                                        .OrderByDescending(f => f)
                                        .ToArray();

        if (backupFiles.Length > 3)
        {
            for (int i = 3; i < backupFiles.Length; i++)
            {
                Console.WriteLine($"🗑 Removing old backup: {backupFiles[i]}");
                File.Delete(backupFiles[i]);

                File.AppendAllText(logFilePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Old version removed: {Path.GetFileName(backupFiles[i])}\n");
            }
        }
    }
}
