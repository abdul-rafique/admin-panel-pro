using AdminPanel.Api.Data;
using AdminPanel.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Api.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController(DataContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            string? role, string? search
        )
        {
            var query = context.Users.Include(u => u.Role).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.Name.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role.Name == role);

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

            if (user is null) return NotFound();

            return user;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(int id, User updatedUser)
        {
            if (id != updatedUser.Id) return BadRequest();

            var user = await context.Users.FindAsync(id);
            if (user is null) return NotFound();

            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.RoleId = updatedUser.RoleId;

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserById(int id)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is null) return NotFound();

            context.Remove(user);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
