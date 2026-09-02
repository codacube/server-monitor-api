using FluentValidation.TestHelper;
using ServerMonitor.DTOs;
using ServerMonitor.Validators;
using Xunit;

namespace ServerMonitor.Tests;

public class CreateMetricRequestValidatorTests
{
    private readonly CreateMetricRequestValidator _validator = new();

    [Fact]
    public void ShouldFail_WhenMemoryUsageIsNegative()
    {
        var model = new CreateMetricRequest(
            "server-1",
            50,
            -1,
            DateTime.UtcNow);

        var result = _validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.MemoryUsageMb);
    }

    [Fact]
    public void ShouldPass_WhenValuesAreValid()
    {
        var model = new CreateMetricRequest(
            "server-1",
            42.5,
            2048,
            DateTime.UtcNow.AddMinutes(-1));

        var result = _validator.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
