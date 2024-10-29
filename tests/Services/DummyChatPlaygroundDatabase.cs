using LMKitMaestro.Data;
using LMKitMaestro.Models;

namespace LMKitMaestro.Tests.Services;

internal class DummyLMKitMaestroDatabase : ILMKitMaestroDatabase
{
    public List<ConversationLog> Conversations = new();

    public Task<int> DeleteConversation(ConversationLog conversationLog)
    {
        return Task.FromResult(Conversations.Remove(conversationLog) ? 1 : 0);
    }

    public Task<List<ConversationLog>> GetConversations()
    {
        return Task.FromResult(Conversations);
    }

    public Task<int> SaveConversation(ConversationLog conversationLog)
    {
        if (Conversations.Contains(conversationLog))
        {
            return Task.FromResult(0);
        }
        else
        {
            Conversations.Add(conversationLog);
            return Task.FromResult(1);
        }
    }
}
