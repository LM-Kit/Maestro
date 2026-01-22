using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using SQLite;

namespace LMKit.Maestro.Data
{
    public sealed class MaestroDatabase : IMaestroDatabase
    {
        // Static fallback for legacy access
        public static string DefaultDatabasePath => Path.Combine(FileSystem.AppDataDirectory, "MaestroSQLite.db3");
        
        private readonly string _databasePath;
        
        public string DatabasePath => _databasePath;

        private SQLiteAsyncConnection? _sqlDatabase;

        public MaestroDatabase(IAppSettingsService appSettingsService)
        {
            // Read path once at construction - changes require restart
            var historyDir = appSettingsService.ChatHistoryDirectory;
            _databasePath = Path.Combine(historyDir, "MaestroSQLite.db3");
        }

        private async Task Init()
        {
            if (_sqlDatabase is not null)
            {
                return;
            }

            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_databasePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _sqlDatabase = new SQLiteAsyncConnection(_databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                await _sqlDatabase.CreateTableAsync<ConversationLog>();
            }
            catch (Exception)
            {
            }
        }

        public async Task<List<ConversationLog>> GetConversations()
        {
            await Init();
            return await _sqlDatabase!.Table<ConversationLog>().ToListAsync();
        }

        public async Task<int> SaveConversation(ConversationLog conversation)
        {
            await Init();

            if (conversation.ID != 0)
            {
                return await _sqlDatabase!.UpdateAsync(conversation);
            }
            else
            {
                return await _sqlDatabase!.InsertAsync(conversation);
            }
        }

        public async Task<int> DeleteConversation(ConversationLog conversation)
        {
            await Init();
            return await _sqlDatabase!.DeleteAsync(conversation);
        }
    }
}
