using FluentValidation;
using HealthCheckApi.Dto;

namespace HealthCheckApi.Validators;

public class UpdateUrlRequestValidator : AbstractValidator<UpdateUrlRequest>
{
    public UpdateUrlRequestValidator()
    {
        RuleFor(x => x.NewUrl)
        .NotEmpty()
        .WithMessage("Url field cannot be empty");

        RuleFor(x => x.Interval)
        .NotNull()
        .GreaterThanOrEqualTo(1)
        .LessThanOrEqualTo(1440)
        .WithMessage("Interval must be between 1 minute and 24 hours(in minutes)");

        RuleFor(x => x.Id)
        .NotEmpty()
        .WithMessage("User id field cannot be empty");
    }
}
