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
public class InvoicesController : ControllerBase
{
    private readonly IRepository<Invoice> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public InvoicesController(
        IRepository<Invoice> repo,
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
    public async Task<IActionResult> GetAll([FromQuery] int? customerId, [FromQuery] DocStatus? status,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(i => i.Customer).AsQueryable();
        if (customerId.HasValue) query = query.Where(i => i.CustomerId == customerId.Value);
        if (status.HasValue) query = query.Where(i => i.Status == status.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await _repo.Query()
            .Include(i => i.Customer)
            .Include(i => i.Lines).ThenInclude(l => l.Item)
            .Include(i => i.Terms)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Invoice invoice)
    {
        invoice.InvoiceNumber = await _numberSeq.GetNextNumberAsync("Invoice");
        invoice.Status = DocStatus.Draft;

        // Calculate totals
        invoice.Subtotal = invoice.Lines.Sum(l => l.Amount);
        invoice.Total = invoice.Subtotal + invoice.TaxTotal;
        invoice.BalanceDue = invoice.Total;

        var created = await _repo.AddAsync(invoice);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Invoice invoice)
    {
        var existing = await _repo.Query()
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (existing == null) return NotFound();
        if (existing.Status != DocStatus.Draft) return BadRequest("Only draft invoices can be edited.");

        existing.CustomerId = invoice.CustomerId;
        existing.Date = invoice.Date;
        existing.DueDate = invoice.DueDate;
        existing.TermsId = invoice.TermsId;
        existing.Memo = invoice.Memo;
        existing.BillToAddress = invoice.BillToAddress;
        existing.ShipToAddress = invoice.ShipToAddress;

        // Replace lines
        existing.Lines.Clear();
        foreach (var line in invoice.Lines)
        {
            existing.Lines.Add(new InvoiceLine
            {
                ItemId = line.ItemId,
                Description = line.Description,
                Qty = line.Qty,
                Rate = line.Rate,
                Amount = line.Amount,
                TaxCodeId = line.TaxCodeId,
                ClassId = line.ClassId
            });
        }

        existing.Subtotal = existing.Lines.Sum(l => l.Amount);
        existing.TaxTotal = invoice.TaxTotal;
        existing.Total = existing.Subtotal + existing.TaxTotal;
        existing.BalanceDue = existing.Total - existing.AmountPaid;

        await _repo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != DocStatus.Draft) return BadRequest("Only draft invoices can be posted.");

        await _posting.PostTransactionAsync(TransactionType.Invoice, id);
        invoice.Status = DocStatus.Posted;
        await _repo.UpdateAsync(invoice);
        await _uow.SaveChangesAsync();
        return Ok(invoice);
    }

    [HttpPost("{id}/void")]
    public async Task<IActionResult> Void(int id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status != DocStatus.Posted) return BadRequest("Only posted invoices can be voided.");

        await _posting.VoidTransactionAsync(TransactionType.Invoice, id);
        invoice.Status = DocStatus.Voided;
        await _repo.UpdateAsync(invoice);
        await _uow.SaveChangesAsync();
        return Ok(invoice);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var invoice = await _repo.GetByIdAsync(id);
        if (invoice == null) return NotFound();
        if (invoice.Status == DocStatus.Posted) return BadRequest("Cannot delete posted invoices. Void first.");

        await _repo.DeleteAsync(invoice);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
