using EFCore.DataClassification.WebApi.DTOs;
using EFCore.DataClassification.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EFCore.DataClassification.WebApi.Controllers;

/// <summary>
/// Controller for managing User entities with classified data.
/// Demonstrates CRUD operations on entities with data classification attributes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(AppDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>List of all users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users
            .Select(u => MapToDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Gets a specific user by ID.
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { error = $"User with ID {id} not found" });

        return Ok(MapToDto(user));
    }

    /// <summary>
    /// Creates a new user with classified data.
    /// Properties like Address, PhoneNumber, and Salary have data classification applied.
    /// </summary>
    /// <param name="dto">User creation data</param>
    [HttpPost]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new User
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Adress = dto.Address ?? string.Empty,      // Classified: Medium - Home Address
            Email = dto.Email ?? string.Empty,
            PhoneNumber = dto.PhoneNumber ?? string.Empty, // Classified: High - Phone Number
            Salary = dto.Salary ?? 0,                  // Classified: High - Financial
            AdminId = dto.AdminId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created user {UserId} with classified data", user.Id);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user));
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="dto">Updated user data</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { error = $"User with ID {id} not found" });

        // Update only provided fields
        if (dto.Name != null) user.Name = dto.Name;
        if (dto.Surname != null) user.Surname = dto.Surname;
        if (dto.Address != null) user.Adress = dto.Address;
        if (dto.Email != null) user.Email = dto.Email;
        if (dto.PhoneNumber != null) user.PhoneNumber = dto.PhoneNumber;
        if (dto.Salary.HasValue) user.Salary = dto.Salary.Value;
        if (dto.AdminId.HasValue) user.AdminId = dto.AdminId.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", user.Id);

        return Ok(MapToDto(user));
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="id">User ID</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { error = $"User with ID {id} not found" });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted user {UserId}", id);

        return NoContent();
    }

    /// <summary>
    /// Gets users filtered by admin.
    /// </summary>
    /// <param name="adminId">Admin ID</param>
    [HttpGet("by-admin/{adminId:int}")]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAdmin(int adminId)
    {
        var users = await _context.Users
            .Where(u => u.AdminId == adminId)
            .Select(u => MapToDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Search users by name or surname.
    /// </summary>
    /// <param name="query">Search query</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Ok(Array.Empty<UserResponseDto>());

        var users = await _context.Users
            .Where(u => u.Name.Contains(query) || u.Surname.Contains(query))
            .Select(u => MapToDto(u))
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Maps User entity to response DTO.
    /// </summary>
    private static UserResponseDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Surname = user.Surname,
        Address = user.Adress,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Salary = user.Salary,
        AdminId = user.AdminId
    };
}


