namespace LeaveRequest.Application.Validators;

using FluentValidation;
using LeaveRequest.Application.DTOs;

public class CreateLeaveRequestDtoValidator : AbstractValidator<CreateLeaveRequestDto>
{
    public CreateLeaveRequestDtoValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .GreaterThan((byte)0).WithMessage("กรุณาเลือกประเภทการลา");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("วันที่เริ่มลาต้องไม่เป็นอดีต");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("วันสิ้นสุดต้องไม่ก่อนวันเริ่มลา");

        RuleFor(x => x.HalfDayPeriod)
            .Must(v => v == "AM" || v == "PM")
            .WithMessage("HalfDayPeriod ต้องเป็น AM หรือ PM เท่านั้น")
            .When(x => x.IsHalfDay);

        RuleFor(x => x.Reason)
            .MaximumLength(500).WithMessage("เหตุผลต้องไม่เกิน 500 ตัวอักษร")
            .When(x => x.Reason != null);

        RuleFor(x => x.AttachmentIds)
            .NotNull().WithMessage("AttachmentIds ต้องไม่เป็น null");
    }
}
