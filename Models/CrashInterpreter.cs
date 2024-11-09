using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Isembard.ViewModels;
using Newtonsoft.Json.Linq;

namespace Isembard.Models;

public static class CrashInterpreter
{
    private static string? LastDebug;
    private static string? LastError;
    private static string? LastGame;
    private static string? LastSystem;

    public static void ReadLogs(ObservableCollection<CrashLog> crashLogs, MainWindowViewModel viewModel)
    {
        foreach (var crashLog in crashLogs)
        {
            var lastDebugLog = crashLog.DebugContent != null && crashLog.DebugContent.Any()
                ? crashLog.DebugContent.Last()
                : "No Debug Logs Available";
            var lastErrorLog = crashLog.ErrorContent != null && crashLog.ErrorContent.Any()
                ? crashLog.ErrorContent.Last()
                : "No Error Logs Available";
            var lastGameLog = crashLog.GameContent != null && crashLog.GameContent.Any()
                ? crashLog.GameContent.Last()
                : "No Game Logs Available";
            var lastSystemLog = crashLog.SystemContent != null && crashLog.SystemContent.Any()
                ? crashLog.SystemContent.Last()
                : "No System Logs Available";

            Console.WriteLine($"Crash Log: {crashLog.LogName}");
            Console.WriteLine($"Last Debug Log: {lastDebugLog}");
            Console.WriteLine($"Last Error Log: {lastErrorLog}");
            Console.WriteLine($"Last Game Log: {lastGameLog}");
            Console.WriteLine($"Last System Log: {lastSystemLog}");
            Console.WriteLine("----------");

            var logs = new List<string> { lastDebugLog, lastErrorLog, lastGameLog, lastSystemLog };
            bool matchFound = false;

            // Find matching logs (exact matches between any two logs)
            for (int i = 0; i < logs.Count; i++)
            {
                for (int j = i + 1; j < logs.Count; j++)
                {
                    if (logs[i] == logs[j] && logs[i] != "No Debug Logs Available" &&
                        logs[i] != "No Error Logs Available" && logs[i] != "No Game Logs Available" &&
                        logs[i] != "No System Logs Available")
                    {
                        matchFound = true;
                        string matchingLog = logs[i];

                        // Now check if the matching log contains any entry from the KnowledgeDictionary
                        foreach (var key in Knowledge.KnowledgeDictionary.Keys)
                        {
                            if (matchingLog.Contains(key, StringComparison.OrdinalIgnoreCase))
                            {
                                Console.WriteLine($"Knowledge Match Found in Matching Log: {matchingLog}");
                                Console.WriteLine($"Simplified Meaning: {Knowledge.KnowledgeDictionary[key]}");

                                string newKnowledge = $"Knowledge Match Found in Matching Log: {matchingLog}";
                                viewModel.FinalReport.ReportContents += newKnowledge + Environment.NewLine;
                                viewModel.FinalReport.ReportSummary += Knowledge.KnowledgeDictionary[key];

                                // Exit the loop after finding the first match to avoid duplicates
                                break;
                            }
                        }

                        if (!Knowledge.KnowledgeDictionary.Keys.Any(key =>
                                matchingLog.Contains(key, StringComparison.OrdinalIgnoreCase)))
                        {
                            Console.WriteLine(
                                $"Matching Log Found: {matchingLog}, but no simplified meaning available.");
                            string newKnowledge = $"Matching Log: {matchingLog}, but no simplified meaning available.";
                            viewModel.FinalReport.ReportSummary += newKnowledge + Environment.NewLine;
                            viewModel.FinalReport.ReportSummary +=
                                "Hm no solution could be offered with certainty. Are you using mods? If not, please send your crash folder over on the GitHub page." +
                                Environment.NewLine;
                        }

                        // Since we only need to find if two logs match, break out of the loop once found
                        break;
                    }
                }

                // Exit the outer loop if a match was found
                if (matchFound)
                {
                    break;
                }
            }

            // If no matching logs were found, check the lastErrorLog against the KnowledgeDictionary
            if (!matchFound)
            {
                Console.WriteLine("No matching logs were found.");
                viewModel.FinalReport.ReportContents += "No matching logs were found." + Environment.NewLine;

                // Check if lastErrorLog contains any entry from the KnowledgeDictionary
                foreach (var key in Knowledge.KnowledgeDictionary.Keys)
                {
                    if (lastErrorLog.Contains(key, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Possible Issue Found in Last Error Log: {lastErrorLog}");
                        Console.WriteLine($"Suggested Meaning: {Knowledge.KnowledgeDictionary[key]}");

                        string possibleIssue =
                            $"Possible Issue Found in Last Error Log: {lastErrorLog} - Suggested Meaning: {Knowledge.KnowledgeDictionary[key]}";
                        string possibleSolution = $"Suggested Meaning: {Knowledge.KnowledgeDictionary[key]}";
                        viewModel.FinalReport.ReportContents += possibleIssue + Environment.NewLine;
                        viewModel.FinalReport.ReportContents += possibleSolution + Environment.NewLine;

                        // Break after finding the first relevant match
                        break;
                    }
                }
            }
        }
    }

    public static void AnalyzeErrorReports(List<LogInterpreter.ErrorReport> errorReports, MainWindowViewModel viewModel)
    {
        int isVictoria = 0;
        int? victoriaErrorCode = null;
        foreach (var report in errorReports)
        {
            if (report.Properties.TryGetValue("P1", out var processName))
            {
                if (processName == "victoria3.exe")
                {
                    isVictoria = 1;
                    Console.WriteLine("Found victoria3.exe in dxdiag.");
                    viewModel.FinalReport.ReportSummary +=
                        "Victoria 3 crashes detected. Victoria 3 may be the culprit, investigating further." +
                        Environment.NewLine;
                    if (report.Properties.TryGetValue("Event Name", out var victoriaEventName))
                    {
                        switch (victoriaEventName)
                        {
                            case "APPCRASH":
                                Console.WriteLine("Victoria 3 crash detected.");
                                viewModel.FinalReport.ReportSummary +=
                                    "Victoria 3 crash confirmed." + Environment.NewLine;
                                break;
                            case "RADAR_PRE_LEAK_64":
                                Console.WriteLine("Victoria 3 encountered a memory issue.");
                                viewModel.FinalReport.ReportSummary +=
                                    "Victoria 3 memory issue confirmed. Most likely not the game; try increasing pagefile to 32GB and ensure the disk has 10% free space at least." +
                                    Environment.NewLine;
                                break;
                            default:
                                Console.WriteLine($"Unknown event: {victoriaEventName}");
                                viewModel.FinalReport.ReportSummary +=
                                    "Hm, it's Victoria 3 but we're not sure why. Send us your crash logs!" +
                                    Environment.NewLine;
                                break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Issue is not with Victoria 3.");
                        viewModel.FinalReport.ReportSummary +=
                            "Issue is not with Victoria 3. Investigating potential causes." + Environment.NewLine;
                        if (report.Properties.TryGetValue("Event Name", out var eventName))
                        {
                            switch (eventName)
                            {
                                case "APPCRASH":
                                    Console.WriteLine("Application crash detected. Will find out why.");
                                    break;
                                case "BlueScreen":
                                    Console.WriteLine(
                                        "System blue screen error detected. Run Windows & driver updates, and BIOS. Run DISM then SFC scan. Otherwise, hardware issue.");
                                    break;
                                case "RADAR_PRE_LEAK_64":
                                    Console.WriteLine("Memory issue detected.");
                                    break;
                                case "LiveKernelEvent":
                                    Console.WriteLine(
                                        "System issue detected. Run DISM then SFC scan. Otherwise, hardware error.");
                                    break;
                                default:
                                    Console.WriteLine($"Unknown event: {eventName}");
                                    break;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No recognizable event name found in the error report.");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No dxdiag errors.");
            }

            Console.WriteLine("----------");
        }
    }


    public static async Task AnalyzeModsAgainstErrorAsync(string modDirectoryPath, string lastError,
        MainWindowViewModel viewModel)
    {
        Console.WriteLine("Starting AnalyzeModsAgainstErrorAsync method.");
        if (Directory.Exists(modDirectoryPath) && !string.IsNullOrEmpty(lastError))
        {
            Console.WriteLine($"Mod directory path: {modDirectoryPath}");
            Console.WriteLine($"Last error: {lastError}");

            // Extract the file name from the second pair of square brackets, ignoring anything after ':'
            var regex = new Regex(@"\[.*?\]\s*\[(.*?)(?::.*?)?\]");
            var match = regex.Match(lastError);

            if (match.Success && match.Groups.Count > 1)
            {
                string fileNameToMatch = match.Groups[1].Value.Trim();
                Console.WriteLine($"Extracted file name to match: {fileNameToMatch}");

                if (!string.IsNullOrEmpty(fileNameToMatch))
                {
                    // Get all files in the mod directory asynchronously
                    var modFiles = await Task.Run(() =>
                        Directory.GetFiles(modDirectoryPath, "*.*", SearchOption.AllDirectories));
                    Console.WriteLine($"Number of files found in mod directory: {modFiles.Length}");

                    bool matchFound = false;
                    foreach (var file in modFiles)
                    {
                        string fileName = Path.GetFileName(file);

                        // Check if the file name matches the extracted file name from the error log
                        if (fileNameToMatch.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            // Attempt to read the metadata.json file in the mod folder
                            string modFolderPath = Path.GetDirectoryName(file);
                            string metadataFilePath = Path.Combine(modFolderPath, "metadata.json");

                            if (File.Exists(metadataFilePath))
                            {
                                try
                                {
                                    var metadataContent = await File.ReadAllTextAsync(metadataFilePath);
                                    var metadataJson = JObject.Parse(metadataContent);
                                    string modName = metadataJson["name"]?.ToString() ?? "Unknown Mod Name";

                                    string message =
                                        $"Potential Mod Issue Detected: '{modName}' may be causing the problem due to '{fileNameToMatch}' error.";
                                    Console.WriteLine(message);
                                    viewModel.FinalReport.AddToFinalReportSummary(message);
                                    matchFound = true;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error reading metadata.json: {ex.Message}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("metadata.json not found in the mod folder.");
                            }
                        }
                    }

                    if (!matchFound)
                    {
                        Console.WriteLine("No matching mods found for the extracted file name.");
                        viewModel.FinalReport.AddToFinalReportSummary(
                            "No matching mods found for the extracted file name from the last error log.");
                    }

                    if (modFiles.Length == 0)
                    {
                        Console.WriteLine("No mods found in the directory.");
                        viewModel.FinalReport.AddToFinalReportSummary(
                            "No mods found in the directory, but the crash log indicates mods could be causing the issue.");
                    }
                }
                else
                {
                    Console.WriteLine("No valid file name found in the last error log for analysis.");
                    viewModel.FinalReport.AddToFinalReportSummary(
                        "No valid file name found in the last error log for analysis.");
                }
            }
            else
            {
                Console.WriteLine("No file reference found in the last error log.");
                viewModel.FinalReport.AddToFinalReportSummary("No file reference found in the last error log.");
            }
        }
        else
        {
            Console.WriteLine("Mod directory not found or the last error is empty.");
            viewModel.FinalReport.AddToFinalReportSummary("Mod directory not found or the last error is empty.");
        }
    }
}