namespace LeaveRequest.Application.Validators;

using FluentValidation;
using LeaveRequest.Application.DTOs;

public class LeaveHistoryFilterRequestValidator : AbstractValidator<LeaveHistoryFilterRequest>
{
    public LeaveHistoryFilterRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page ต้องมากกว่า 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize ต้องอยู่ระหว่าง 1-100");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("EndDate ต้องไม่ก่อน StartDate");
    }
}
