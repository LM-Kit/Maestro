using LMKitMaestro.Models;
using SQLite;

namespace LMKitMaestro.Data
{
    public sealed class LMKitMaestroDatabase : ILMKitMaestroDatabase
    {
        public static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, "LMKitMaestroSQLite.db3");

        private SQLiteAsyncConnection? _sqlDatabase;

        private async Task Init()
        {
            if (_sqlDatabase is not null)
            {
                return;
            }

            try
            {
                _sqlDatabase = new SQLiteAsyncConnection(DatabasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

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
