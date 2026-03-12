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
public class TransfersController : ControllerBase
{
    private readonly IRepository<Transfer> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ITransactionPostingService _posting;

    public TransfersController(
        IRepository<Transfer> repo,
        IUnitOfWork uow,
        ITransactionPostingService posting)
    {
        _repo = repo;
        _uow = uow;
        _posting = posting;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var transfer = await _repo.Query()
            .Include(t => t.FromAccount)
            .Include(t => t.ToAccount)
            .FirstOrDefaultAsync(t => t.Id == id);
        if (transfer == null) return NotFound();
        return Ok(transfer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Transfer transfer)
    {
        transfer.Status = DocStatus.Draft;
        var created = await _repo.AddAsync(transfer);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var transfer = await _repo.GetByIdAsync(id);
        if (transfer == null) return NotFound();
        if (transfer.Status != DocStatus.Draft) return BadRequest("Only draft transfers can be posted.");

        await _posting.PostTransactionAsync(TransactionType.Transfer, id);
        transfer.Status = DocStatus.Posted;
        await _repo.UpdateAsync(transfer);
        await _uow.SaveChangesAsync();
        return Ok(transfer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transfer = await _repo.GetByIdAsync(id);
        if (transfer == null) return NotFound();
        if (transfer.Status == DocStatus.Posted) return BadRequest("Cannot delete posted transfers.");

        await _repo.DeleteAsync(transfer);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
