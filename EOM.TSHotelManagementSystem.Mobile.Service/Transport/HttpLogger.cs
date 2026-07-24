using System.Text;
using Microsoft.Maui.Storage;

namespace EOM.TSHotelManagementSystem.Mobile.Service;

/// <summary>
/// HTTP 请求文件日志：记录每次请求的完整信息，写入外部存储 files 目录，
/// 用文件管理器即可查看：内部存储 → Android → data → &lt;包名&gt; → files → http-logs/
/// </summary>
public static class HttpLogger
{
    private static readonly object _lock = new();
    private const int MaxBodyLogLength = 200_000;

    /// <summary>日志目录（外部存储 files/http-logs，文件管理器可直接访问）</summary>
    public static string LogDirectory
    {
        get
        {
            var httpDir = Path.Combine(LogDirectoryProvider.LogDirectory, "http-logs");
            try
            {
                Directory.CreateDirectory(httpDir);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HttpLogger] 创建日志目录失败: {ex.Message}");
            }
            return httpDir;
        }
    }

    /// <summary>当天日志文件路径</summary>
    private static string LogFilePath => Path.Combine(LogDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");

    /// <summary>写一条日志</summary>
    public static void Log(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        Console.Error.WriteLine(line);
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, line + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HttpLogger] 写入日志失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[HttpLogger] 日志路径: {LogFilePath}");
        }
    }

    /// <summary>记录完整请求信息</summary>
    public static void LogRequest(string method, string url, string? body,
        IDictionary<string, string>? headers = null, bool hasToken = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine($">>> REQUEST >>>");
        sb.AppendLine($"  Method:  {method}");
        sb.AppendLine($"  URL:     {url}");

        if (hasToken)
            sb.AppendLine($"  Auth:    Bearer ***");

        if (headers is not null && headers.Count > 0)
        {
            sb.AppendLine($"  Headers:");
            foreach (var kv in headers)
                sb.AppendLine($"    {kv.Key}: {kv.Value}");
        }

        if (!string.IsNullOrEmpty(body))
            sb.AppendLine($"  Body:    {Truncate(body)}");

        Log(sb.ToString());
    }

    /// <summary>记录完整响应信息</summary>
    public static void LogResponse(int statusCode, string? content, string? url = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"<<< RESPONSE <<<");
        if (url is not null)
            sb.AppendLine($"  URL:      {url}");
        sb.AppendLine($"  Status:   {statusCode}");
        sb.AppendLine($"  Response: {Truncate(content) ?? "(null)"}");

        Log(sb.ToString());
    }

    /// <summary>记录异常</summary>
    public static void LogException(string context, Exception ex)
    {
        Log($"!!! EXCEPTION {context}: {ex}");
    }

    private static string? Truncate(string? s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length > MaxBodyLogLength
            ? s[..MaxBodyLogLength] + $"...(truncated, total {s.Length} chars)"
            : s;
    }
}
