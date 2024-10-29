using LMKitMaestro.Models;

namespace LMKitMaestro.Data;

public interface ILMKitMaestroDatabase
{
    Task<List<ConversationLog>> GetConversations();
    Task<int> SaveConversation(ConversationLog conversationLog);
    Task<int> DeleteConversation(ConversationLog conversationLog);
}
