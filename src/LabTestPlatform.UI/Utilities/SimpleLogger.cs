using System;
using System.IO;
using System.Text;

namespace LabTestPlatform.UI.Utilities
{
    /// <summary>
    /// 简单的文件日志类 - 无需第三方依赖
    /// </summary>
    public static class SimpleLogger
    {
        private static readonly object _lock = new object();
        private static string? _logFilePath = null;
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化日志系统
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // 创建日志目录
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // 设置日志文件路径（按日期命名）
                var fileName = $"weibull-analysis-{DateTime.Now:yyyyMMdd-HHmmss}.txt";
                _logFilePath = Path.Combine(logDirectory, fileName);

                _isInitialized = true;

                // 写入启动信息
                WriteLog("INFO", "==========================================");
                WriteLog("INFO", "日志系统初始化");
                WriteLog("INFO", $"日志文件: {_logFilePath}");
                WriteLog("INFO", $"启动时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                WriteLog("INFO", "==========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 写入日志（内部方法）
        /// </summary>
        private static void WriteLog(string level, string message, Exception? ex = null)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            try
            {
                lock (_lock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = new StringBuilder();
                    logEntry.AppendLine($"[{timestamp}] [{level}] {message}");

                    if (ex != null)
                    {
                        logEntry.AppendLine($"异常类型: {ex.GetType().Name}");
                        logEntry.AppendLine($"异常消息: {ex.Message}");
                        logEntry.AppendLine($"堆栈跟踪: {ex.StackTrace}");
                        
                        if (ex.InnerException != null)
                        {
                            logEntry.AppendLine($"内部异常: {ex.InnerException.Message}");
                        }
                    }

                    // 写入文件
                    File.AppendAllText(_logFilePath, logEntry.ToString(), Encoding.UTF8);

                    // 同时输出到控制台（便于调试）
                    Console.Write(logEntry.ToString());
                }
            }
            catch (Exception writeEx)
            {
                Console.WriteLine($"写入日志失败: {writeEx.Message}");
            }
        }

        /// <summary>
        /// 记录调试信息
        /// </summary>
        public static void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        /// <summary>
        /// 记录一般信息
        /// </summary>
        public static void Info(string message)
        {
            WriteLog("INFO", message);
        }

        /// <summary>
        /// 记录警告信息
        /// </summary>
        public static void Warning(string message)
        {
            WriteLog("WARN", message);
        }

        /// <summary>
        /// 记录错误信息
        /// </summary>
        public static void Error(string message, Exception? ex = null)
        {
            WriteLog("ERROR", message, ex);
        }

        /// <summary>
        /// 记录分隔线
        /// </summary>
        public static void Separator(string title = "")
        {
            if (string.IsNullOrEmpty(title))
            {
                WriteLog("INFO", "==========================================");
            }
            else
            {
                WriteLog("INFO", $"========== {title} ==========");
            }
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public static string GetLogFilePath()
        {
            return _logFilePath ?? "未初始化";
        }
    }
}
