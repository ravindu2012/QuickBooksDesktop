using QBD.Application.Interfaces;
using QBD.Domain.Common;
using QBD.Infrastructure.Data;

namespace QBD.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly QBDesktopDbContext _context;

    public AuditService(QBDesktopDbContext context)
    {
        _context = context;
    }

    public async Task LogChangeAsync(string entityType, int entityId, string action, string? oldValues, string? newValues)
    {
        var entry = new AuditLogEntry
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogEntries.Add(entry);
        await _context.SaveChangesAsync();
    }
}
