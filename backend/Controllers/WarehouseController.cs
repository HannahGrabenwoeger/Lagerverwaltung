using Microsoft.AspNetCore.Mvc;
using Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private static readonly List<Warehouse> Warehouses = new List<Warehouse>
    {
        new Warehouse { Id = Guid.NewGuid(), Name = "Lager A", Location = "Wien" },
        new Warehouse { Id = Guid.NewGuid(), Name = "Lager B", Location = "Salzburg" }
    };

    [HttpGet]
    public IActionResult GetWarehouses()
    {
        return Ok(Warehouses);
    }

    [HttpGet("{id}")]
    public IActionResult GetWarehouseById(Guid id)
    {
        var warehouse = Warehouses.FirstOrDefault(w => w.Id == id);
        if (warehouse == null)
        {
            return NotFound(new { message = "Lager nicht gefunden" });
        }
        return Ok(warehouse);
    }
}