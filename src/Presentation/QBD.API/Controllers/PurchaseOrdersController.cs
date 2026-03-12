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
public class PurchaseOrdersController : ControllerBase
{
    private readonly IRepository<PurchaseOrder> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;

    public PurchaseOrdersController(IRepository<PurchaseOrder> repo, IUnitOfWork uow, INumberSequenceService numberSeq)
    {
        _repo = repo;
        _uow = uow;
        _numberSeq = numberSeq;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(p => p.Vendor).AsQueryable();
        if (vendorId.HasValue) query = query.Where(p => p.VendorId == vendorId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var po = await _repo.Query()
            .Include(p => p.Vendor)
            .Include(p => p.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (po == null) return NotFound();
        return Ok(po);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PurchaseOrder po)
    {
        po.PONumber = await _numberSeq.GetNextNumberAsync("PurchaseOrder");
        po.Status = DocStatus.Draft;
        po.Subtotal = po.Lines.Sum(l => l.Amount);
        po.Total = po.Subtotal;

        var created = await _repo.AddAsync(po);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var po = await _repo.GetByIdAsync(id);
        if (po == null) return NotFound();

        await _repo.DeleteAsync(po);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
