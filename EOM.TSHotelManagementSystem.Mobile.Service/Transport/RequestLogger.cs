using System.Text;
using Microsoft.Maui.Storage;

namespace EOM.TSHotelManagementSystem.Mobile.Service;

/// <summary>
/// 请求日志：记录每次 HTTP 请求的完整信息，写入外部存储根目录（与 crash.log 同级），
/// 文件名 request.log，方便用文件管理器查看。
/// </summary>
public static class RequestLogger
{
    private static readonly object _lock = new();
    private const int MaxBodyLogLength = 50_000;

    /// <summary>日志文件路径：与 crash.log 同目录</summary>
    public static string LogFilePath => Path.Combine(LogDirectoryProvider.LogDirectory, "request.log");

    /// <summary>记录一次完整的请求-响应周期</summary>
    public static void LogRequestResponse(
        string method,
        string url,
        int statusCode,
        long elapsedMs,
        string? requestBody = null,
        string? responseBody = null,
        IDictionary<string, string>? requestHeaders = null,
        string? errorMessage = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"========== [{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ==========");
        sb.AppendLine($"Method:     {method}");
        sb.AppendLine($"URL:        {url}");
        sb.AppendLine($"Status:     {statusCode}");
        sb.AppendLine($"Elapsed:    {elapsedMs}ms");

        if (requestHeaders is not null && requestHeaders.Count > 0)
        {
            sb.AppendLine("Req Headers:");
            foreach (var kv in requestHeaders)
                sb.AppendLine($"  {kv.Key}: {kv.Value}");
        }

        if (!string.IsNullOrEmpty(requestBody))
            sb.AppendLine($"Req Body:   {Truncate(requestBody)}");

        sb.AppendLine($"Response:   {Truncate(responseBody) ?? "(null)"}");

        if (!string.IsNullOrEmpty(errorMessage))
            sb.AppendLine($"Error:      {errorMessage}");

        sb.AppendLine();

        var text = sb.ToString();
        try
        {
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, text);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RequestLogger] 写入日志失败: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[RequestLogger] 日志路径: {LogFilePath}");
        }
    }

    /// <summary>记录异常（请求失败时调用）</summary>
    public static void LogException(string method, string url, long elapsedMs, Exception ex)
    {
        LogRequestResponse(
            method: method,
            url: url,
            statusCode: 0,
            elapsedMs: elapsedMs,
            errorMessage: $"{ex.GetType().Name}: {ex.Message}");
    }

    private static string? Truncate(string? s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return s.Length > MaxBodyLogLength
            ? s[..MaxBodyLogLength] + $"...(truncated, total {s.Length} chars)"
            : s;
    }
}
