using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Dtos;
using Microsoft.EntityFrameworkCore;  

public class InventoryReportService
{
    private readonly AppDbContext _context;

    public InventoryReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<InventoryReportDto>> GetInventoryReportAsync()
    {
        return await _context.Products
            .Select(p => new InventoryReportDto
            {
                ProductName = p.Name,
                TotalQuantity = p.Quantity,
                TotalMovements = _context.Movements.Count(m => m.ProductId == p.WarehouseId),
                LastUpdated = _context.Movements
                    .Where(m => m.ProductId == p.WarehouseId)
                    .OrderByDescending(m => m.MovementsDate)
                    .Select(m => m.MovementsDate)
                    .FirstOrDefault()
            })
            .ToListAsync();  
    }
}