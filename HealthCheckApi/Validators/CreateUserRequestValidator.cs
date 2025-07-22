using System;
using FluentValidation;
using HealthCheckApi.Dto;

namespace HealthCheckApi.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
        .NotEmpty()
        .WithMessage("Username cannot be empty");

        RuleFor(x => x.Email)
        .NotEmpty()
        .WithMessage("Email cannot be empty");

        RuleFor(x => x.Password)
        .NotEmpty()
        .MinimumLength(4)
        .WithMessage("Password must be a size greater than 3");
    }
}
