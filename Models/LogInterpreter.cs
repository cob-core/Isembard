using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Isembard.ViewModels;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class LogInterpreter : ViewModelBase {
    [ObservableProperty]
    private System.DateTime _modifiedDate;

    [ObservableProperty]
    private string? _logName;
    [ObservableProperty]
    private List<string>? _debugContent;
    [ObservableProperty]
    private List<string>? _errorContent;
    [ObservableProperty]
    private List<string>? _gameContent;
    [ObservableProperty]
    private List<string>? _systemContent;

    public LogInterpreter(){

}

public LogInterpreter(CrashLog item)
{
    // Init the properties with the given values
    ModifiedDate = item.ModifiedDate;
    LogName = item.LogName;
    DebugContent = item.DebugContent;
    ErrorContent = item.ErrorContent;
    GameContent = item.GameContent;
    SystemContent = item.SystemContent;
}

private ObservableCollection<CrashLog> CrashLogs { get; } = new ObservableCollection<CrashLog>();
[ObservableProperty] private string _crashLogsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Paradox Interactive", "Victoria 3", "crashes");

public async Task<ObservableCollection<CrashLog>> LoadCrashLogsAsync(string selectedCrashLog = null)
{
    CrashLogs.Clear();

    if (Directory.Exists(_crashLogsPath))
    {
        var crashLogDirectories = Directory.GetDirectories(_crashLogsPath);

        // If a specific crash log is selected, only use that folder
        if (!string.IsNullOrEmpty(selectedCrashLog))
        {
            crashLogDirectories = crashLogDirectories.Where(d => d.EndsWith(selectedCrashLog)).ToArray();
        }

        var loadTasks = crashLogDirectories.Select(async directory =>
        {
            var directoryInfo = new DirectoryInfo(directory);
            var crashLog = new CrashLog
            {
                LogName = directoryInfo.Name,
                ModifiedDate = directoryInfo.LastWriteTime
            };

            string logsPath = Path.Combine(directory, "logs");
            if (Directory.Exists(logsPath))
            {
                var loadLogTasks = new List<Task>
                {
                    Task.Run(() => LoadLogIfExists(Path.Combine(logsPath, "debug.log"), crashLog.DebugContent)),
                    Task.Run(() => LoadLogIfExists(Path.Combine(logsPath, "error.log"), crashLog.ErrorContent)),
                    Task.Run(() => LoadLogIfExists(Path.Combine(logsPath, "game.log"), crashLog.GameContent)),
                    Task.Run(() => LoadLogIfExists(Path.Combine(logsPath, "system.log"), crashLog.SystemContent))
                };
                await Task.WhenAll(loadLogTasks);
            }

            lock (CrashLogs)
            {
                CrashLogs.Add(crashLog);
            }
        });

        await Task.WhenAll(loadTasks);
    }

    return CrashLogs;
}


private void LoadLogIfExists(string filePath, List<string> logContent)
{
    if (File.Exists(filePath))
    {
        LoadLogContent(filePath, logContent);
    }
}

private void LoadLogContent(string filePath, List<string> logContent, int linesToRead = 20)
{
    try
    {
        if (File.Exists(filePath))
        {
            var allLines = File.ReadLines(filePath).Reverse().Take(linesToRead).Reverse().ToList();
            var timePattern = new Regex(@"^\[\d{2}:\d{2}:\d{2}\]");

            foreach (var line in allLines)
            {
                if (timePattern.IsMatch(line))
                {
                    logContent.Add(line);
                }
            }
        }
        else
        {
            Console.WriteLine($"File not found: {filePath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error reading log file {filePath}: {ex.Message}");
    }
}



public static string GenerateDxDiagLog()
{
    string dxDiagLogPath = Path.Combine(Path.GetTempPath(), "dxdiag_output.txt");
    var processInfo = new ProcessStartInfo
    {
        FileName = "dxdiag",
        Arguments = "/t " + dxDiagLogPath,
        RedirectStandardOutput = false,
        UseShellExecute = false,
        CreateNoWindow = true
    };

    try
    {
        using (var process = Process.Start(processInfo))
        {
            process.WaitForExit();
            if (process.ExitCode == 0 && File.Exists(dxDiagLogPath))
            {
                Console.WriteLine("dxdiag generated successfully.");
                return File.ReadAllText(dxDiagLogPath);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error generating dxdiag log: {ex.Message}");
    }

    return string.Empty;
    return dxDiagLogPath;
}

public static List<ErrorReport> ParseDxDiagLog(string dxDiagLogPath)
{
    var errorReports = new List<ErrorReport>();

    if (!File.Exists(dxDiagLogPath))
    {
        Console.WriteLine("dxdiag log file not found.");
        return errorReports;
    }

    var lines = File.ReadAllLines(dxDiagLogPath);
    ErrorReport currentReport = null;
    var regex = new Regex(@"^\+{3} WER\[.*\] \+{3}:");

    foreach (var line in lines)
    {
        if (regex.IsMatch(line))
        {
            if (currentReport != null)
            {
                errorReports.Add(currentReport);
            }
            currentReport = new ErrorReport();
        }
        else if (currentReport != null && line.Contains(":"))
        {
            var parts = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                currentReport.Properties[key] = value;
            }
        }
    }

    if (currentReport != null)
    {
        errorReports.Add(currentReport);
    }
    Console.WriteLine(errorReports);
    return errorReports;
}

public class ErrorReport
{
    public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}


}