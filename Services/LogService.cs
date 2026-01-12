using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Interfazxn.Models;

namespace Interfazxn.Services
{
    /// <summary>
    /// Servicio de logging para eventos del sistema
    /// Mantiene historial de conexiones, errores y tramas procesadas
    /// </summary>
    public class LogService
    {
        private ObservableCollection<LogEntry> _logs;
        private const int MaxLogEntries = 1000;

        public ObservableCollection<LogEntry> Logs 
        { 
            get { return _logs ??= new ObservableCollection<LogEntry>(); }
        }

        public void LogInfo(string message, string category = "INFO")
        {
            AddLog(message, LogLevel.Info, category);
        }

        public void LogWarning(string message, string category = "WARNING")
        {
            AddLog(message, LogLevel.Warning, category);
        }

        public void LogError(string message, string category = "ERROR")
        {
            AddLog(message, LogLevel.Error, category);
        }

        public void LogSuccess(string message, string category = "SUCCESS")
        {
            AddLog(message, LogLevel.Success, category);
        }

        private void AddLog(string message, LogLevel level, string category)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message,
                Level = level,
                Category = category
            };

            Logs.Insert(0, entry);

            // Limitar número de logs
            while (Logs.Count > MaxLogEntries)
            {
                Logs.RemoveAt(Logs.Count - 1);
            }
        }

        public void ClearLogs()
        {
            Logs.Clear();
        }
    }

    /// <summary>
    /// Representa una entrada en el log
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public LogLevel Level { get; set; }
        public string Category { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Category}] {Message}";
        }
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Success
    }
}
