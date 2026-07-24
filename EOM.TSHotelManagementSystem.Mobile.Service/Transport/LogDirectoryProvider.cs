using Microsoft.Maui.Storage;

namespace EOM.TSHotelManagementSystem.Mobile.Service;

/// <summary>
/// 统一的日志目录管理：所有日志文件（crash.log、request.log、http-logs/）共用同一目录。
/// </summary>
public static class LogDirectoryProvider
{
    private static string? _logDirectory;

    /// <summary>日志目录路径（外部存储 files 目录，文件管理器可访问）</summary>
    public static string LogDirectory
    {
        get
        {
            if (_logDirectory is not null) return _logDirectory;

#if ANDROID
            var externalDir = Android.App.Application.Context.GetExternalFilesDir(null);
            _logDirectory = externalDir?.AbsolutePath
                ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
#else
            _logDirectory = FileSystem.AppDataDirectory;
#endif
            return _logDirectory;
        }
    }
}
