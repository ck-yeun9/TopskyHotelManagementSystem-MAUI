using Microsoft.Maui.Storage;
using System.Text;

namespace EOM.TSHotelManagementSystem.Mobile.Service;

/// <summary>
/// HTTP 请求文件日志：把每次请求的完整 URL、方法、请求体、响应状态码、响应体、异常
/// 写入 app 私有文件目录（FileSystem.AppDataDirectory，免权限），同时输出到 Console.Error
/// （Android 上进 logcat，tag: mono-rt/mono-stdout），便于 adb 抓取。
/// 文件位置（Android）：/data/data/com.eom.topsy.mobile/files/http-logs.txt
/// </summary>
public static class HttpLogger
{
    private static readonly object _lock = new();
    private const string LogFileName = "http-logs.txt";
    private const int MaxBodyLogLength = 200_000; // 单条 body 最多记录 20 万字符，防内存爆

    /// <summary>日志文件完整路径</summary>
    public static string LogFilePath => Path.Combine(FileSystem.AppDataDirectory, LogFileName);

    /// <summary>写一条原始日志（带时间戳，落盘 + logcat）</summary>
    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        // 同时输出到 logcat（Release 也能看到）
        Console.Error.WriteLine(line);
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
        }
        catch
        {
            // 日志写入失败不得影响业务
        }
    }

    /// <summary>记录请求：方法、完整 URL、请求体、是否带 Token</summary>
    public static void LogRequest(string method, string url, string? body, bool hasToken = false)
    {
        var sb = new StringBuilder();
        sb.Append($"===> {method} {url}");
        if (hasToken)
        {
            sb.Append("  [Auth: Bearer ***]");
        }
        if (!string.IsNullOrEmpty(body))
        {
            sb.AppendLine();
            sb.Append($"    Body: {Truncate(body)}");
        }
        Log(sb.ToString());
    }

    /// <summary>记录响应：状态码、完整响应体</summary>
    public static void LogResponse(int statusCode, string? content)
    {
        Log($"<=== HTTP {statusCode}{Environment.NewLine}    Response: {Truncate(content) ?? "(null)"}");
    }

    /// <summary>记录异常</summary>
    public static void LogException(string context, Exception ex)
    {
        Log($"!!! EXCEPTION {context}: {ex}");
    }

    private static string? Truncate(string? s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }
        return s.Length > MaxBodyLogLength
            ? s[..MaxBodyLogLength] + $"...(truncated, total {s.Length} chars)"
            : s;
    }
}
