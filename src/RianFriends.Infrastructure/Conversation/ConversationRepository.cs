using Microsoft.EntityFrameworkCore;
using RianFriends.Application.Abstractions;
using RianFriends.Domain.Conversation;
using RianFriends.Infrastructure.Persistence;

namespace RianFriends.Infrastructure.Conversation;

internal sealed class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationRepository(AppDbContext context) => _context = context;

    public Task<ConversationSession?> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default)
        => _context.ConversationSessions.FirstOrDefaultAsync(s => s.Id == sessionId, ct);

    public Task<int> CountSessionsByFriendIdAsync(Guid friendId, CancellationToken ct = default)
        => _context.ConversationSessions.CountAsync(s => s.FriendId == friendId, ct);

    public Task<List<Message>> GetRecentMessagesAsync(Guid sessionId, int count = 20, CancellationToken ct = default)
        => _context.Messages
            .Where(m => m.SessionId == sessionId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public Task<Message?> GetMessageByIdAsync(Guid messageId, CancellationToken ct = default)
        => _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId, ct);

    public void AddSession(ConversationSession session) => _context.ConversationSessions.Add(session);

    public void AddMessage(Message message) => _context.Messages.Add(message);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);
}
