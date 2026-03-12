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
public class SalesReceiptsController : ControllerBase
{
    private readonly IRepository<SalesReceipt> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public SalesReceiptsController(
        IRepository<SalesReceipt> repo,
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
        var query = _repo.Query().Include(s => s.Customer);
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var receipt = await _repo.Query()
            .Include(s => s.Customer)
            .Include(s => s.Lines).ThenInclude(l => l.Item)
            .Include(s => s.PaymentMethod)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (receipt == null) return NotFound();
        return Ok(receipt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SalesReceipt receipt)
    {
        receipt.SalesReceiptNumber = await _numberSeq.GetNextNumberAsync("SalesReceipt");
        receipt.Status = DocStatus.Draft;
        receipt.Subtotal = receipt.Lines.Sum(l => l.Amount);
        receipt.Total = receipt.Subtotal + receipt.Tax;

        var created = await _repo.AddAsync(receipt);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var receipt = await _repo.GetByIdAsync(id);
        if (receipt == null) return NotFound();
        if (receipt.Status != DocStatus.Draft) return BadRequest("Only draft sales receipts can be posted.");

        await _posting.PostTransactionAsync(TransactionType.SalesReceipt, id);
        receipt.Status = DocStatus.Posted;
        await _repo.UpdateAsync(receipt);
        await _uow.SaveChangesAsync();
        return Ok(receipt);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var receipt = await _repo.GetByIdAsync(id);
        if (receipt == null) return NotFound();
        if (receipt.Status == DocStatus.Posted) return BadRequest("Cannot delete posted sales receipts.");

        await _repo.DeleteAsync(receipt);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
