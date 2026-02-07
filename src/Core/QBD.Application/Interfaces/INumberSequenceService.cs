namespace QBD.Application.Interfaces;

public interface INumberSequenceService
{
    Task<string> GetNextNumberAsync(string entityType);
}
