// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Vendors;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : ControllerBase
{
    private readonly IRepository<Bill> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public BillsController(
        IRepository<Bill> repo,
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
    public async Task<IActionResult> GetAll([FromQuery] int? vendorId, [FromQuery] DocStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(b => b.Vendor).AsQueryable();
        if (vendorId.HasValue) query = query.Where(b => b.VendorId == vendorId.Value);
        if (status.HasValue) query = query.Where(b => b.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(b => b.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var bill = await _repo.Query()
            .Include(b => b.Vendor)
            .Include(b => b.ExpenseLines).ThenInclude(l => l.Account)
            .Include(b => b.ItemLines).ThenInclude(l => l.Item)
            .Include(b => b.Terms)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (bill == null) return NotFound();
        return Ok(bill);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Bill bill)
    {
        bill.BillNumber = await _numberSeq.GetNextNumberAsync("Bill");
        bill.Status = DocStatus.Draft;
        bill.AmountDue = bill.ExpenseLines.Sum(l => l.Amount) + bill.ItemLines.Sum(l => l.Amount);
        bill.BalanceDue = bill.AmountDue;

        var created = await _repo.AddAsync(bill);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var bill = await _repo.GetByIdAsync(id);
        if (bill == null) return NotFound();
        if (bill.Status != DocStatus.Draft) return BadRequest("Only draft bills can be posted.");

        await _posting.PostTransactionAsync(TransactionType.Bill, id);
        bill.Status = DocStatus.Posted;
        await _repo.UpdateAsync(bill);
        await _uow.SaveChangesAsync();
        return Ok(bill);
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(int id)
    {
        var bill = await _repo.GetByIdAsync(id);
        if (bill == null) return NotFound();
        if (bill.Status != DocStatus.Posted) return BadRequest("Only posted bills can be voided.");

        await _posting.VoidTransactionAsync(TransactionType.Bill, id);
        bill.Status = DocStatus.Voided;
        await _repo.UpdateAsync(bill);
        await _uow.SaveChangesAsync();
        return Ok(bill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var bill = await _repo.GetByIdAsync(id);
        if (bill == null) return NotFound();
        if (bill.Status == DocStatus.Posted) return BadRequest("Cannot delete posted bills. Void first.");

        await _repo.DeleteAsync(bill);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
