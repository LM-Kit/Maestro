using LMKit.Maestro.Models;

namespace LMKit.Maestro.Data;

public interface IMaestroDatabase
{
    Task<List<ConversationLog>> GetConversations();
    Task<int> SaveConversation(ConversationLog conversationLog);
    Task<int> DeleteConversation(ConversationLog conversationLog);
}
