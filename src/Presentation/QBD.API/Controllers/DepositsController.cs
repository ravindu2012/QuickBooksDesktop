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
public class DepositsController : ControllerBase
{
    private readonly IRepository<Deposit> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ITransactionPostingService _posting;

    public DepositsController(
        IRepository<Deposit> repo,
        IUnitOfWork uow,
        ITransactionPostingService posting)
    {
        _repo = repo;
        _uow = uow;
        _posting = posting;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? bankAccountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(d => d.BankAccount).AsQueryable();
        if (bankAccountId.HasValue) query = query.Where(d => d.BankAccountId == bankAccountId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(d => d.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var deposit = await _repo.Query()
            .Include(d => d.BankAccount)
            .Include(d => d.Lines).ThenInclude(l => l.FromAccount)
            .Include(d => d.Lines).ThenInclude(l => l.PaymentMethod)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (deposit == null) return NotFound();
        return Ok(deposit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Deposit deposit)
    {
        deposit.Status = DocStatus.Draft;
        deposit.Total = deposit.Lines.Sum(l => l.Amount);

        var created = await _repo.AddAsync(deposit);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var deposit = await _repo.GetByIdAsync(id);
        if (deposit == null) return NotFound();
        if (deposit.Status != DocStatus.Draft) return BadRequest("Only draft deposits can be posted.");

        await _posting.PostTransactionAsync(TransactionType.Deposit, id);
        deposit.Status = DocStatus.Posted;
        await _repo.UpdateAsync(deposit);
        await _uow.SaveChangesAsync();
        return Ok(deposit);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deposit = await _repo.GetByIdAsync(id);
        if (deposit == null) return NotFound();
        if (deposit.Status == DocStatus.Posted) return BadRequest("Cannot delete posted deposits.");

        await _repo.DeleteAsync(deposit);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
