namespace LeaveRequest.Application.Validators;

using FluentValidation;
using LeaveRequest.Application.DTOs;

public class CreateLeaveTypeRequestValidator : AbstractValidator<CreateLeaveTypeRequest>
{
    public CreateLeaveTypeRequestValidator()
    {
        RuleFor(x => x.TypeCode)
            .NotEmpty().WithMessage("TypeCode is required.")
            .MaximumLength(20).WithMessage("TypeCode must not exceed 20 characters.")
            .Matches(@"^[A-Z_]+$").WithMessage("TypeCode must be uppercase letters and underscores only.");

        RuleFor(x => x.TypeNameTh)
            .NotEmpty().WithMessage("TypeNameTh is required.")
            .MaximumLength(100).WithMessage("TypeNameTh must not exceed 100 characters.");

        RuleFor(x => x.TypeNameEn)
            .NotEmpty().WithMessage("TypeNameEn is required.")
            .MaximumLength(100).WithMessage("TypeNameEn must not exceed 100 characters.");

        RuleFor(x => x.MaxDaysPerYear)
            .GreaterThan(0).WithMessage("MaxDaysPerYear must be greater than 0.")
            .When(x => x.MaxDaysPerYear.HasValue);
    }
}
