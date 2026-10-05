// ===========================================================================
//
//  FILE:    Logger.cs
//  DESC:    Creates detailed logs of Game Errors and Warnings
//  
//  NOTE:    I don't remember how the fuck this works. I hope I don't have to 
//           mess with it again. ~ HeadMonitor 10/03/2026
//
// ===========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class Logger : MonoBehaviour
{
    [Serializable]
    private class LogEntry
    {
        public string Type;
        public string Message;
        public string StackTrace;
        public string Timestamp;
        public int Count;
    }

    private int _maxLogAmount;
    private string _sessionUUID;
    private string _logsFolder;
    private string _logName;
    private string _logFilePath;
    private readonly Dictionary<string, LogEntry> _entries = new();

    /// <summary>
    /// Creates the [Logger] object on start.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeAutoRunner()
    {
        if (PlayerPrefs.GetInt("Logger_Enabled", 1) != 1) return;

        GameObject loggerObject = new("[Logger]");
        loggerObject.AddComponent<Logger>();
        DontDestroyOnLoad(loggerObject);
    }

    private void Awake()
    {
        _maxLogAmount = PlayerPrefs.GetInt("Logger_MaxLogs", 50);

        // Startup Arguments for maxLogAmount
        string[] args = Environment.GetCommandLineArgs();
        const string key = "-maxLogAmount=";
        string match = Array.Find(args, arg => arg.StartsWith(key, StringComparison.OrdinalIgnoreCase));
        if (match != null && int.TryParse(match[key.Length..], out int amount)) _maxLogAmount = amount;   

        do
        {
            _sessionUUID = Guid.NewGuid().ToString();
            _logsFolder = Path.Combine(Application.persistentDataPath, "Logs");
            _logName = _sessionUUID + ".json";
            _logFilePath = Path.Combine(_logsFolder, _logName);
        }
        while (File.Exists(_logFilePath));

        Directory.CreateDirectory(_logsFolder);

        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        LogMessage(type.ToString(), logString, stackTrace);
    }

    private void LogMessage(string logType, string logMessage, string stackTrace)
    {
        try
        {
            // Build a unique key for this error
            string key = $"{logType}|{logMessage}|{stackTrace}";

            if (_entries.TryGetValue(key, out var entry))
            {
                // Repeated error, increment
                entry.Count++;
            }
            else
            {
                // First occurrence, store timestamp now
                entry = new LogEntry
                {
                    Type = logType,
                    Message = logMessage,
                    StackTrace = stackTrace,
                    Timestamp = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                    Count = 1
                };

                _entries[key] = entry;
            }

            // Remove oldest entries if we exceed max log amount
            while (_entries.Count > _maxLogAmount)
            {
                // Find the oldest entry by timestamp
                string oldestKey = null;
                DateTime oldestTime = DateTime.MaxValue;

                foreach (var kvp in _entries)
                {
                    if (DateTime.TryParse(kvp.Value.Timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTime))
                    {
                        if (parsedTime < oldestTime)
                        {
                            oldestTime = parsedTime;
                            oldestKey = kvp.Key;
                        }
                    }
                }

                if (oldestKey != null)
                {
                    _entries.Remove(oldestKey);
                }
                else
                {
                    // Fallback: remove first entry if timestamp parsing fails
                    var firstKey = _entries.Keys.GetEnumerator();
                    firstKey.MoveNext();
                    _entries.Remove(firstKey.Current);
                }
            }

            // Persist everything as a JSON array
            var list = new List<LogEntry>(_entries.Values);
            var json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(_logFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write log: {e.Message}\nPath: {_logFilePath}");
        }
    }
}
