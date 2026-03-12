// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Customers;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditMemosController : ControllerBase
{
    private readonly IRepository<CreditMemo> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public CreditMemosController(
        IRepository<CreditMemo> repo,
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
    public async Task<IActionResult> GetAll([FromQuery] int? customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(c => c.Customer).AsQueryable();
        if (customerId.HasValue) query = query.Where(c => c.CustomerId == customerId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var memo = await _repo.Query()
            .Include(c => c.Customer)
            .Include(c => c.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (memo == null) return NotFound();
        return Ok(memo);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreditMemo memo)
    {
        memo.CreditNumber = await _numberSeq.GetNextNumberAsync("CreditMemo");
        memo.Status = DocStatus.Draft;
        memo.Subtotal = memo.Lines.Sum(l => l.Amount);
        memo.Total = memo.Subtotal + memo.Tax;
        memo.BalanceRemaining = memo.Total;

        var created = await _repo.AddAsync(memo);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var memo = await _repo.GetByIdAsync(id);
        if (memo == null) return NotFound();
        if (memo.Status != DocStatus.Draft) return BadRequest("Only draft credit memos can be posted.");

        await _posting.PostTransactionAsync(TransactionType.CreditMemo, id);
        memo.Status = DocStatus.Posted;
        await _repo.UpdateAsync(memo);
        await _uow.SaveChangesAsync();
        return Ok(memo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var memo = await _repo.GetByIdAsync(id);
        if (memo == null) return NotFound();
        if (memo.Status == DocStatus.Posted) return BadRequest("Cannot delete posted credit memos.");

        await _repo.DeleteAsync(memo);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
