namespace QBD.Application.Interfaces;

public interface IAuditService
{
    Task LogChangeAsync(string entityType, int entityId, string action, string? oldValues, string? newValues);
}
