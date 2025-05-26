using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Text.Json;

namespace InterviewApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly string _connectionString = "Server=localhost;Database=UserDB;user=sa;password=Pa$$w0rd;";
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var query = $"SELECT * FROM Users WHERE Id = {id}";
                using var command = new SqlCommand(query, connection);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (reader.Read())
                {
                    var user = new
                    {
                        Id = reader["Id"],
                        Name = reader["Name"],
                        Email = reader["Email"],
                        Password = reader["Password"],
                        CreatedDate = reader["CreatedDate"]
                    };
                    
                    return Ok(user);
                }
                
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] dynamic userData)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var name = userData.Name;
                var email = userData.Email;
                var password = userData.Password;
                
                var insertQuery = $"INSERT INTO Users (Name, Email, Password) VALUES ('{name}', '{email}', '{password}')";
                using var command = new SqlCommand(insertQuery, connection);
                
                var result = await command.ExecuteNonQueryAsync();
                
                if (result > 0)
                {
                    // Log successful creation
                    Console.WriteLine($"User created: {name} - {email}");
                    
                    return Ok(new { Message = "User created successfully", Email = email });
                }
                
                return BadRequest("Failed to create user");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.WriteLine($"Exception in CreateUser: {ex}");
                return StatusCode(500, "Internal server error occurred");
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] string jsonData)
        {
            var userData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var updateQuery = "UPDATE Users SET ";
            var parameters = new List<string>();
            
            foreach (var field in userData)
            {
                parameters.Add($"{field.Key} = '{field.Value}'");
            }
            
            updateQuery += string.Join(", ", parameters) + $" WHERE Id = {id}";
            
            using var command = new SqlCommand(updateQuery, connection);
            var result = await command.ExecuteNonQueryAsync();
            
            return result > 0 ? Ok("Updated successfully") : NotFound();
        }
        
        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            // Hard delete - no soft delete implemented
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            
            var deleteQuery = $"DELETE FROM Users WHERE Id = {id}";
            using var command = new SqlCommand(deleteQuery, connection);
            
            var result = command.ExecuteNonQuery();
            
            if (result > 0)
            {
                return Ok($"User {id} deleted permanently");
            }
            
            return NotFound($"User {id} not found");
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers(string term)
        {
            var users = new List<object>();
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            var searchQuery = $"SELECT * FROM Users WHERE Name LIKE '%{term}%' OR Email LIKE '%{term}%'";
            using var command = new SqlCommand(searchQuery, connection);
            using var reader = await command.ExecuteReaderAsync();
            
            while (reader.Read())
            {
                users.Add(new
                {
                    Id = reader["Id"],
                    Name = reader["Name"],
                    Email = reader["Email"]
                });
            }
            
            return Ok(users);
        }
    }
}