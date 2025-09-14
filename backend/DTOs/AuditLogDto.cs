namespace AdminPanel.Api.DTOs;

public record AuditLogDto(
    int Id,
    string UserName,
    string UserEmail,
    string Action,
    string Details,
    DateTime Timestamp
);