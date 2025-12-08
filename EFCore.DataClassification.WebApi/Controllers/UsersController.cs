using AutoMapper;
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
    private readonly IMapper _mapper;

    public UsersController(AppDbContext context, ILogger<UsersController> logger, IMapper mapper)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
    }

    /// <summary>
    /// Gets all users.
    /// </summary>
    /// <returns>List of all users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _context.Users.ToListAsync();
        var dtos = _mapper.Map<List<UserResponseDto>>(users);
        return Ok(dtos);
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

        var dto = _mapper.Map<UserResponseDto>(user);
        return Ok(dto);
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

        var user = _mapper.Map<User>(dto);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created user {UserId} with classified data", user.Id);

        var responseDto = _mapper.Map<UserResponseDto>(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, responseDto);
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

        // AutoMapper handles null checks automatically (configured in profile)
        _mapper.Map(dto, user);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId}", user.Id);

        var responseDto = _mapper.Map<UserResponseDto>(user);
        return Ok(responseDto);
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
            .ToListAsync();

        var dtos = _mapper.Map<List<UserResponseDto>>(users);
        return Ok(dtos);
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
            .ToListAsync();

        var dtos = _mapper.Map<List<UserResponseDto>>(users);
        return Ok(dtos);
    }
}



