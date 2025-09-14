using AdminPanel.Api.Data;
using AdminPanel.Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminPanel.Api.Controllers
{
    [Route("api/audit-logs")]
    [ApiController]
    public class AuditLogsController(DataContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<AuditLogDto>>> GetPagedLogs(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10,
            [FromQuery] string? user = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null
        )
        {
            if (page < 1) page = 1;
            if (limit > 100) limit = 20;

            var query = context.AuditLogs
                .Include(log => log.User)
                .AsNoTracking()
                .OrderByDescending(log => log.Timestamp)
                .AsQueryable();

            if (!string.IsNullOrEmpty(user))
                query = query.Where(log => log.User.Name.Contains(user) || log.User.Email.Contains(user));

            if (!string.IsNullOrEmpty(action))
                query = query.Where(log => log.Action == action);

            if (startDate.HasValue)
                query = query.Where(log => log.Timestamp >= startDate);

            if (endDate.HasValue)
                query = query.Where(log => log.Timestamp <= endDate);

            var total = await query.CountAsync();

            var logs = await query
                .Skip(page - 1)
                .Take(limit)
                .Select(log => new AuditLogDto(
                    log.Id,
                    log.User.Name,
                    log.User.Email,
                    log.Action,
                    log.Details ?? "",
                    log.Timestamp
                ))
                .ToListAsync();

            var result = new PaginatedResult<AuditLogDto>(logs, total, page, limit);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuditLogDto>> GetLogById(int id)
        {
            var log = await context.AuditLogs
                .Include(log => log.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(log => log.Id == id);

            if (log is null) return NotFound();

            var dto = new AuditLogDto(
                log.Id,
                log.User.Name,
                log.User.Email,
                log.Action,
                log.Details ?? "",
                log.Timestamp
            );

            return dto;
        }
    }
}
