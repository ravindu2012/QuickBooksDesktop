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
public class EstimatesController : ControllerBase
{
    private readonly IRepository<Estimate> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;

    public EstimatesController(IRepository<Estimate> repo, IUnitOfWork uow, INumberSequenceService numberSeq)
    {
        _repo = repo;
        _uow = uow;
        _numberSeq = numberSeq;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? customerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(e => e.Customer).AsQueryable();
        if (customerId.HasValue) query = query.Where(e => e.CustomerId == customerId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(e => e.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var estimate = await _repo.Query()
            .Include(e => e.Customer)
            .Include(e => e.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (estimate == null) return NotFound();
        return Ok(estimate);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Estimate estimate)
    {
        estimate.EstimateNumber = await _numberSeq.GetNextNumberAsync("Estimate");
        estimate.Status = DocStatus.Draft;
        estimate.Subtotal = estimate.Lines.Sum(l => l.Amount);
        estimate.Total = estimate.Subtotal + estimate.Tax;

        var created = await _repo.AddAsync(estimate);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var estimate = await _repo.GetByIdAsync(id);
        if (estimate == null) return NotFound();

        await _repo.DeleteAsync(estimate);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
