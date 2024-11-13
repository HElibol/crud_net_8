using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ProductProject.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly string _connectionString;

        public ProductsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = new List<Product>();

            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM Products", connection);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2)
                });
            }

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("SELECT * FROM Products WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var product = new Product
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2)
                };

                return Ok(product);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> CreateProduct(Product product)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("INSERT INTO Products (Name, Price) VALUES (@name, @price) RETURNING Id", connection);
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@price", product.Price);

            product.Id = (int)await command.ExecuteScalarAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("UPDATE Products SET Name = @name, Price = @price WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@name", product.Name);
            command.Parameters.AddWithValue("@price", product.Price);

            var affectedRows = await command.ExecuteNonQueryAsync();

            if (affectedRows == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var command = new NpgsqlCommand("DELETE FROM Products WHERE Id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            var affectedRows = await command.ExecuteNonQueryAsync();

            if (affectedRows == 0)
                return NotFound();

            return NoContent();
        }
    }
}
