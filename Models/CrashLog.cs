using System;
using System.Collections.Generic;

public class CrashLog
{
    public DateTime ModifiedDate { get; set; }
    public string LogName { get; set; } = string.Empty;

    public List<string> DebugContent { get; set; } = new List<string>();
    public List<string> ErrorContent { get; set; } = new List<string>();
    public List<string> GameContent { get; set; } = new List<string>();
    public List<string> SystemContent { get; set; } = new List<string>();
}
