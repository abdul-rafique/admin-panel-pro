using AdminPanel.Api.Data;
using AdminPanel.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Api.Controllers
{
    [Authorize]
    [Route("api/roles")]
    [ApiController]
    public class RolesController(DataContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetUsers()
        {
            var query = await context.Roles.ToListAsync();
            return query;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRoleById(int id)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role is null) return NotFound();

            return role;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateRole(int id, Role updatedRole)
        {
            if (id != updatedRole.Id) return BadRequest();

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);

            if (role is null) return NotFound();

            role.Name = updatedRole.Name;
            role.Description = updatedRole.Description;

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserById(int id)
        {

            var role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role is null) return NotFound();

            context.Remove(role);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
