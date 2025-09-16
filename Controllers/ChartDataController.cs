using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;


namespace FruitSysWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChartDataController : ControllerBase
    {
        [HttpGet("TopCustomers")]
        public IActionResult GetTopCustomers()
        {
            // Zamijenite ovo sa stvarnim podacima iz baze
            var data = new
            {
                Categories = new[] { "Fruit Market DOO", "Fresh Fruits AD", "ABC Distributers", "Green Basket", "Nature Selection" },
                Values = new[] { 125000, 98000, 84500, 72000, 61000 }
            };

            return Ok(data);
        }

        [HttpGet("TopSuppliers")]
        public IActionResult GetTopSuppliers()
        {
            // Zamijenite ovo sa stvarnim podacima iz baze
            var data = new
            {
                Categories = new[] { "Orchadia Farms", "BerryBest", "Tropical Fruits Inc", "Apple Valley", "Citrus Garden" },
                Values = new[] { 85000, 72000, 68000, 55000, 48000 }
            };

            return Ok(data);
        }

        // Dodajte slične metode za ostale grafikone...
    }
}