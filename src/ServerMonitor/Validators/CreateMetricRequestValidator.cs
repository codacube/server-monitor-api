using FluentValidation;
using ServerMonitor.DTOs;

namespace ServerMonitor.Validators;

public class CreateMetricRequestValidator : AbstractValidator<CreateMetricRequest>
{
    public CreateMetricRequestValidator()
    {
        RuleFor(x => x.ServerName)
            .NotEmpty()
            .Must(serverName => !string.IsNullOrWhiteSpace(serverName))
            .WithMessage("Server name is required.")
            .MaximumLength(50)
            .WithMessage("Server name must be 50 characters or fewer.");

        RuleFor(x => x.CpuUsagePercent)
            .InclusiveBetween(0.0, 100.0)
            .WithMessage("CPU usage must be between 0 and 100.");

        RuleFor(x => x.MemoryUsageMb)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Memory usage must be zero or greater.");

        RuleFor(x => x.Timestamp)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.Timestamp.HasValue)
            .WithMessage("Timestamp cannot be in the future.");
    }
}
