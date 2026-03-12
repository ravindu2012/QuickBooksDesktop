// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

namespace QBD.Application.Interfaces;

public interface INumberSequenceService
{
    Task<string> GetNextNumberAsync(string entityType);
}
