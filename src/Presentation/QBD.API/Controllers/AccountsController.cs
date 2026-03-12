// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Accounting;
using QBD.Domain.Enums;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly IRepository<Account> _repo;
    private readonly IRepository<GLEntry> _glRepo;
    private readonly IUnitOfWork _uow;

    public AccountsController(IRepository<Account> repo, IRepository<GLEntry> glRepo, IUnitOfWork uow)
    {
        _repo = repo;
        _glRepo = glRepo;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AccountType? type, [FromQuery] bool? activeOnly)
    {
        var query = _repo.Query();
        if (type.HasValue) query = query.Where(a => a.AccountType == type.Value);
        if (activeOnly == true) query = query.Where(a => a.IsActive);
        var accounts = await query.OrderBy(a => a.SortOrder).ToListAsync();
        return Ok(accounts);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var account = await _repo.Query()
            .Include(a => a.SubAccounts)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Account account)
    {
        account.IsActive = true;
        var created = await _repo.AddAsync(account);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Account account)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = account.Name;
        existing.Number = account.Number;
        existing.AccountType = account.AccountType;
        existing.Description = account.Description;
        existing.IsActive = account.IsActive;
        existing.ParentId = account.ParentId;

        await _repo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _repo.GetByIdAsync(id);
        if (account == null) return NotFound();
        if (account.IsSystemAccount) return BadRequest("Cannot delete system accounts.");

        await _repo.DeleteAsync(account);
        await _uow.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id}/ledger")]
    public async Task<IActionResult> GetLedger(int id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var account = await _repo.GetByIdAsync(id);
        if (account == null) return NotFound();

        var query = _glRepo.Query().Where(g => g.AccountId == id && !g.IsVoid);
        if (from.HasValue) query = query.Where(g => g.PostingDate >= from.Value);
        if (to.HasValue) query = query.Where(g => g.PostingDate <= to.Value);

        var entries = await query.OrderBy(g => g.PostingDate).ThenBy(g => g.Id).ToListAsync();
        return Ok(entries);
    }
}
