using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Isembard.Models;

namespace Isembard.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    #pragma warning disable CA1822 // Mark members as static
    private ObservableCollection<CrashLog> CrashLogs { get; } = new ObservableCollection<CrashLog>();
    #pragma warning restore CA1822 // Mark members as static

    public MainWindowViewModel()
    {
        FinalReport = new Report();
        CrashLogCollection = new ObservableCollection<string>();
        LoadCrashLogsAsync(_selectedCrashLog);
    }
    
    [ObservableProperty]
    private string _crashLogsPath;
    [ObservableProperty] 
    public Report _finalReport;
    [ObservableProperty]
    public string _selectedCrashLog;
    [ObservableProperty]
    private string _modsPath;

    private ObservableCollection<string> _crashLogCollection;

    public ObservableCollection<string> CrashLogCollection
    {
        get => _crashLogCollection;
        set
        {
            _crashLogCollection = value;
            OnPropertyChanged(nameof(CrashLogCollection));
        }
    }
    
    public string FinalReportContents => FinalReport.ReportContents;
    public string FinalReportSummary => FinalReport.ReportSummary;
    
    private async Task LoadCrashLogsAsync(string selectedCrashLog = null)
    {
        var _logInterpreter = new LogInterpreter();
        _logInterpreter.CrashLogsPath = CrashLogsPath;

        var loadedCrashLogs = await _logInterpreter.LoadCrashLogsAsync(selectedCrashLog);

        CrashLogs.Clear();
        CrashLogCollection.Clear();

        foreach (var crashLog in loadedCrashLogs)
        {
            CrashLogs.Add(crashLog);
            CrashLogCollection.Add(crashLog.LogName);
        }
    }
    
    [RelayCommand]
    public void LoadCrashLogsCommand()
    {
        LoadCrashLogsAsync(SelectedCrashLog);
    }
    
    [RelayCommand]
    public void LoadModsCommand()
    {
        if (Directory.Exists(ModsPath))
        {
            Console.WriteLine($"Mods directory loaded: {ModsPath}");
        }
        else
        {
            Console.WriteLine("Invalid mods directory path.");
        }
    }
    
    [RelayCommand]
    public async void InterpretCrashCommand()
    {
        FinalReport.ReportContents = string.Empty;
        FinalReport.ReportSummary = string.Empty;
        string dxDiagLogPath = Path.Combine(Path.GetTempPath(), "dxdiag_output.txt");
        LogInterpreter.GenerateDxDiagLog();
        var ErrorReports = LogInterpreter.ParseDxDiagLog(dxDiagLogPath);
        CrashInterpreter.AnalyzeErrorReports(ErrorReports, this);
        CrashInterpreter.ReadLogs(CrashLogs, this);
        
        // Use the last error log for matching
        await CrashInterpreter.AnalyzeModsAgainstErrorAsync(ModsPath, CrashLogs, this);
        
        Console.WriteLine($"Updated Final Report Contents: {FinalReport.ReportContents}");
        Console.WriteLine($"Updated Final Summary Contents: {FinalReport.ReportSummary}");
        
        OnPropertyChanged(nameof(FinalReportContents));
        OnPropertyChanged(nameof(FinalReportSummary));
    }

    public partial class Report : ObservableObject
    {
        private string reportContents = string.Empty;
        private string reportSummary  = string.Empty;

        public string ReportContents
        {
            get => reportContents;
            set
            {
                SetProperty(ref reportContents, value);
                OnPropertyChanged(nameof(ReportContents));
            }
        }
        
        public string ReportSummary
        {
            get => reportSummary;
            set
            {
                SetProperty(ref reportSummary, value);
                OnPropertyChanged(nameof(ReportSummary));
            }
        }

        public void AddToFinalReportContent(string addition)
        {
            ReportContents += addition + Environment.NewLine;
        }
        
        public void AddToFinalReportSummary(string addition)
        {
            ReportSummary += Environment.NewLine + addition;
        }
    }
    
    // Debug Commands
    

}