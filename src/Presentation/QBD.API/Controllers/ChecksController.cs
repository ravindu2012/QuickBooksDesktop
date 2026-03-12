// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Banking;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChecksController : ControllerBase
{
    private readonly IRepository<Check> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public ChecksController(
        IRepository<Check> repo,
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
    public async Task<IActionResult> GetAll([FromQuery] int? bankAccountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(c => c.BankAccount).AsQueryable();
        if (bankAccountId.HasValue) query = query.Where(c => c.BankAccountId == bankAccountId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var check = await _repo.Query()
            .Include(c => c.BankAccount)
            .Include(c => c.ExpenseLines).ThenInclude(l => l.Account)
            .Include(c => c.ItemLines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (check == null) return NotFound();
        return Ok(check);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Check check)
    {
        check.CheckNumber = await _numberSeq.GetNextNumberAsync("Check");
        check.Status = DocStatus.Draft;
        check.Amount = check.ExpenseLines.Sum(l => l.Amount) + check.ItemLines.Sum(l => l.Amount);

        var created = await _repo.AddAsync(check);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var check = await _repo.GetByIdAsync(id);
        if (check == null) return NotFound();
        if (check.Status != DocStatus.Draft) return BadRequest("Only draft checks can be posted.");

        await _posting.PostTransactionAsync(TransactionType.Check, id);
        check.Status = DocStatus.Posted;
        await _repo.UpdateAsync(check);
        await _uow.SaveChangesAsync();
        return Ok(check);
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(int id)
    {
        var check = await _repo.GetByIdAsync(id);
        if (check == null) return NotFound();
        if (check.Status != DocStatus.Posted) return BadRequest("Only posted checks can be voided.");

        await _posting.VoidTransactionAsync(TransactionType.Check, id);
        check.Status = DocStatus.Voided;
        await _repo.UpdateAsync(check);
        await _uow.SaveChangesAsync();
        return Ok(check);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var check = await _repo.GetByIdAsync(id);
        if (check == null) return NotFound();
        if (check.Status == DocStatus.Posted) return BadRequest("Cannot delete posted checks.");

        await _repo.DeleteAsync(check);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
