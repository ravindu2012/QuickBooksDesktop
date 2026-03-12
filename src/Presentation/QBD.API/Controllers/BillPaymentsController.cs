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
public class BillPaymentsController : ControllerBase
{
    private readonly IRepository<BillPayment> _repo;
    private readonly IUnitOfWork _uow;
    private readonly ITransactionPostingService _posting;

    public BillPaymentsController(
        IRepository<BillPayment> repo,
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
            .Include(bp => bp.PaymentAccount);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(bp => bp.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _repo.Query()
            .Include(bp => bp.PaymentAccount)
            .Include(bp => bp.PaymentMethod)
            .Include(bp => bp.Applications).ThenInclude(a => a.Bill).ThenInclude(b => b.Vendor)
            .FirstOrDefaultAsync(bp => bp.Id == id);
        if (payment == null) return NotFound();
        return Ok(payment);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BillPayment payment)
    {
        payment.Status = DocStatus.Draft;
        payment.Amount = payment.Applications.Sum(a => a.AmountApplied);

        var created = await _repo.AddAsync(payment);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var payment = await _repo.GetByIdAsync(id);
        if (payment == null) return NotFound();
        if (payment.Status != DocStatus.Draft) return BadRequest("Only draft bill payments can be posted.");

        await _posting.PostTransactionAsync(TransactionType.BillPayment, id);
        payment.Status = DocStatus.Posted;
        await _repo.UpdateAsync(payment);
        await _uow.SaveChangesAsync();
        return Ok(payment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var payment = await _repo.GetByIdAsync(id);
        if (payment == null) return NotFound();
        if (payment.Status == DocStatus.Posted) return BadRequest("Cannot delete posted bill payments.");

        await _repo.DeleteAsync(payment);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
