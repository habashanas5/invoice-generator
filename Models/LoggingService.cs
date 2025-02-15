namespace Invoice_Generator.Models
{
    public class LoggingService
    {
        private readonly ILogger<LoggingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingService(ILogger<LoggingService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void LogUserActivity(string action, string details)
        {
            var user = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
            _logger.LogInformation("User: {User} | Action: {Action} | Details: {Details} | Timestamp: {Timestamp}",
                user, action, details, DateTime.UtcNow);
        }
    }
}
