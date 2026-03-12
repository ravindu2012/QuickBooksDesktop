// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JournalEntriesController : ControllerBase
{
    private readonly IRepository<JournalEntry> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public JournalEntriesController(
        IRepository<JournalEntry> repo,
        IUnitOfWork uow,
        INumberSequenceService numberSeq,
        ITransactionPostingService posting)
    {
        _repo = repo;
        _uow = uow;
        _numberSeq = numberSeq;
        _posting = posting;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var total = await _repo.Query().CountAsync();
        var items = await _repo.Query()
            .OrderByDescending(j => j.PostingDate)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entry = await _repo.Query()
            .Include(j => j.Lines).ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.Id == id);
        if (entry == null) return NotFound();
        return Ok(entry);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JournalEntry entry)
    {
        // Validate balanced
        var totalDebits = entry.Lines.Sum(l => l.DebitAmount);
        var totalCredits = entry.Lines.Sum(l => l.CreditAmount);
        if (totalDebits != totalCredits)
            return BadRequest($"Debits ({totalDebits:C}) must equal Credits ({totalCredits:C}).");

        entry.EntryNumber = await _numberSeq.GetNextNumberAsync("JournalEntry");
        entry.Status = DocStatus.Draft;

        var created = await _repo.AddAsync(entry);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var entry = await _repo.GetByIdAsync(id);
        if (entry == null) return NotFound();
        if (entry.Status != DocStatus.Draft) return BadRequest("Only draft entries can be posted.");

        await _posting.PostTransactionAsync(TransactionType.JournalEntry, id);
        entry.Status = DocStatus.Posted;
        await _repo.UpdateAsync(entry);
        await _uow.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(int id)
    {
        var entry = await _repo.GetByIdAsync(id);
        if (entry == null) return NotFound();
        if (entry.Status != DocStatus.Posted) return BadRequest("Only posted entries can be voided.");

        await _posting.VoidTransactionAsync(TransactionType.JournalEntry, id);
        entry.Status = DocStatus.Voided;
        await _repo.UpdateAsync(entry);
        await _uow.SaveChangesAsync();
        return Ok(entry);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _repo.GetByIdAsync(id);
        if (entry == null) return NotFound();
        if (entry.Status == DocStatus.Posted) return BadRequest("Cannot delete posted journal entries.");

        await _repo.DeleteAsync(entry);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
