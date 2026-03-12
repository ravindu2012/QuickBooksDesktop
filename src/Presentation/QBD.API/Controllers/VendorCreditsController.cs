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
public class VendorCreditsController : ControllerBase
{
    private readonly IRepository<VendorCredit> _repo;
    private readonly IUnitOfWork _uow;
    private readonly INumberSequenceService _numberSeq;
    private readonly ITransactionPostingService _posting;

    public VendorCreditsController(
        IRepository<VendorCredit> repo,
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
    public async Task<IActionResult> GetAll([FromQuery] int? vendorId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var query = _repo.Query().Include(v => v.Vendor).AsQueryable();
        if (vendorId.HasValue) query = query.Where(v => v.VendorId == vendorId.Value);

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(v => v.Date)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var credit = await _repo.Query()
            .Include(v => v.Vendor)
            .Include(v => v.Lines).ThenInclude(l => l.Account)
            .Include(v => v.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (credit == null) return NotFound();
        return Ok(credit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VendorCredit credit)
    {
        credit.RefNo = await _numberSeq.GetNextNumberAsync("VendorCredit");
        credit.Status = DocStatus.Draft;
        credit.Total = credit.Lines.Sum(l => l.Amount);
        credit.BalanceRemaining = credit.Total;

        var created = await _repo.AddAsync(credit);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        var credit = await _repo.GetByIdAsync(id);
        if (credit == null) return NotFound();
        if (credit.Status != DocStatus.Draft) return BadRequest("Only draft vendor credits can be posted.");

        await _posting.PostTransactionAsync(TransactionType.VendorCredit, id);
        credit.Status = DocStatus.Posted;
        await _repo.UpdateAsync(credit);
        await _uow.SaveChangesAsync();
        return Ok(credit);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var credit = await _repo.GetByIdAsync(id);
        if (credit == null) return NotFound();
        if (credit.Status == DocStatus.Posted) return BadRequest("Cannot delete posted vendor credits.");

        await _repo.DeleteAsync(credit);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
