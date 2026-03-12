// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Items;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _repo;
    private readonly IUnitOfWork _uow;

    public ItemsController(IRepository<Item> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ItemType? type, [FromQuery] bool? activeOnly)
    {
        var query = _repo.Query();
        if (type.HasValue) query = query.Where(i => i.ItemType == type.Value);
        if (activeOnly == true) query = query.Where(i => i.IsActive);
        var items = await query.OrderBy(i => i.ItemName).ToListAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.Query()
            .Include(i => i.IncomeAccount)
            .Include(i => i.ExpenseAccount)
            .Include(i => i.AssetAccount)
            .Include(i => i.SubItems)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Item item)
    {
        item.IsActive = true;
        var created = await _repo.AddAsync(item);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Item item)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.ItemName = item.ItemName;
        existing.ItemType = item.ItemType;
        existing.Description = item.Description;
        existing.SalesPrice = item.SalesPrice;
        existing.PurchaseCost = item.PurchaseCost;
        existing.IncomeAccountId = item.IncomeAccountId;
        existing.ExpenseAccountId = item.ExpenseAccountId;
        existing.AssetAccountId = item.AssetAccountId;
        existing.ReorderPoint = item.ReorderPoint;
        existing.IsActive = item.IsActive;
        existing.TaxCodeId = item.TaxCodeId;

        await _repo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return NotFound();

        await _repo.DeleteAsync(item);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
