namespace LMKit.Maestro;

internal static class AppConstants
{
    public const string AppVersion = "0.1.3";
    public const string AppName = "LM-Kit Maestro";
    public static string AppNameWithVersion => $"{AppName} {AppVersion}";

    public const string DatabaseFilename = "MaestroSQLite.db3";

    public const SQLite.SQLiteOpenFlags Flags =
        // open the database in read/write mode
        SQLite.SQLiteOpenFlags.ReadWrite |
        // create the database if it doesn't exist
        SQLite.SQLiteOpenFlags.Create |
        // enable multi-threaded database access
        SQLite.SQLiteOpenFlags.SharedCache;

    public const string ChatRoute = "Chat";

    public const string ModelsRoute = "Models";
}