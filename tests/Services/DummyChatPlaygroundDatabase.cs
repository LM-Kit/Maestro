using LMKitMaestro.Data;
using LMKitMaestro.Models;

namespace LMKitMaestroTests;

internal class DummyLMKitMaestroDatabase : ILMKitMaestroDatabase
{
    public List<ConversationLog> Conversations = new List<ConversationLog>();

    public async Task<int> DeleteConversation(ConversationLog conversationLog)
    {
        return Conversations.Remove(conversationLog) ? 1 : 0;
    }

    public async Task<List<ConversationLog>> GetConversations()
    {
        return Conversations;
    }

    public async Task<int> SaveConversation(ConversationLog conversationLog)
    {
        if (Conversations.Contains(conversationLog))
        {
            return 0;
        }
        else
        {
            Conversations.Add(conversationLog);
            return 1;
        }
    }
}
