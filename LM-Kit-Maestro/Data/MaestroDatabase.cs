using LMKit.Maestro.Models;
using LMKit.Maestro.Services;
using SQLite;

namespace LMKit.Maestro.Data
{
    public sealed class MaestroDatabase : IMaestroDatabase
    {
        private readonly IAppSettingsService _appSettingsService;
        
        public string DatabasePath => Path.Combine(_appSettingsService.ChatHistoryDirectory, "MaestroSQLite.db3");

        private SQLiteAsyncConnection? _sqlDatabase;
        private string? _currentDatabasePath;

        public MaestroDatabase(IAppSettingsService appSettingsService)
        {
            _appSettingsService = appSettingsService;
        }

        private async Task Init()
        {
            var targetPath = DatabasePath;
            
            // Reinitialize if path changed
            if (_sqlDatabase is not null && _currentDatabasePath == targetPath)
            {
                return;
            }

            // Close existing connection if path changed
            if (_sqlDatabase is not null && _currentDatabasePath != targetPath)
            {
                await _sqlDatabase.CloseAsync();
                _sqlDatabase = null;
            }

            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _sqlDatabase = new SQLiteAsyncConnection(targetPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
                _currentDatabasePath = targetPath;

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
