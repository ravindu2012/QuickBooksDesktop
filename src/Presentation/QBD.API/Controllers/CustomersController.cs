// Copyright (c) 2026, Ravindu Gajanayaka
// Licensed under GPLv3. See LICENSE

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QBD.Application.Interfaces;
using QBD.Domain.Entities.Customers;

namespace QBD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly IRepository<Customer> _repo;
    private readonly IUnitOfWork _uow;

    public CustomersController(IRepository<Customer> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var (items, total) = await _repo.GetPagedAsync(page, pageSize,
            filter: activeOnly == true ? c => c.IsActive : null,
            orderBy: c => c.CustomerName);
        return Ok(new { items, total, page, pageSize });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _repo.Query()
            .Include(c => c.Jobs)
            .Include(c => c.Invoices)
            .Include(c => c.Terms)
            .FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Customer customer)
    {
        customer.IsActive = true;
        var created = await _repo.AddAsync(customer);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Customer customer)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.CustomerName = customer.CustomerName;
        existing.Company = customer.Company;
        existing.BillToAddress = customer.BillToAddress;
        existing.ShipToAddress = customer.ShipToAddress;
        existing.Phone = customer.Phone;
        existing.Email = customer.Email;
        existing.TermsId = customer.TermsId;
        existing.CreditLimit = customer.CreditLimit;
        existing.TaxCodeId = customer.TaxCodeId;
        existing.IsActive = customer.IsActive;

        await _repo.UpdateAsync(existing);
        await _uow.SaveChangesAsync();
        return Ok(existing);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer == null) return NotFound();

        await _repo.DeleteAsync(customer);
        await _uow.SaveChangesAsync();
        return NoContent();
    }
}
