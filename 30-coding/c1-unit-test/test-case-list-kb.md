# Unit Test Cases — KB Standard & Programmer Experience (TC-KB)

> ผลลัพธ์จาก Prompt 2.2  
> Implementation class จริง: `LeaveRequestService.cs`  
> TC-KB ≠ TC-SPEC — ทุก test case ในชุดนี้คือสิ่งที่ spec ไม่ได้พูดถึง  
> Bug fixes applied: TC-KB-030/031/032 (RejectAsync null guard), TC-KB-026 (managerId null), TC-KB-010/011 (HalfDayPeriod validation)

---

## SubmitLeaveRequestAsync — 17 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-KB-001 | SubmitLeaveRequestAsync | EmployeeId = null | employeeId = null | throw ArgumentNullException / EMPLOYEE_NOT_FOUND (ไม่ NullRef) | NullEmpty | rule-input-validation.md → 1.1 EmployeeId |
| TC-KB-002 | SubmitLeaveRequestAsync | EmployeeId = empty string | employeeId = "" | throw EMPLOYEE_NOT_FOUND | NullEmpty | rule-input-validation.md → 1.1 EmployeeId |
| TC-KB-003 | SubmitLeaveRequestAsync | EmployeeId = whitespace | employeeId = " " | throw EMPLOYEE_NOT_FOUND (ต้องใช้ IsNullOrWhiteSpace) | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-004 | SubmitLeaveRequestAsync | EmployeeId ยาวเกิน MaxLength | employeeId = string 51 ตัว | throw validation error; ไม่ถึง repo | MaxLength | rule-input-validation.md → 1.1 MaxLength=50 |
| TC-KB-005 | SubmitLeaveRequestAsync | LeaveTypeId = 0 | request.LeaveTypeId = 0 | throw INVALID_LEAVE_TYPE (0 ไม่ valid) | Boundary | rule-input-validation.md → 1.2 Required > 0 |
| TC-KB-006 | SubmitLeaveRequestAsync | StartDate = เมื่อวาน | StartDate = Today.AddDays(-1) | throw INVALID_DATE_RANGE | Boundary | rule-input-validation.md → 1.3, lesson-learned → EC-002 |
| TC-KB-007 | SubmitLeaveRequestAsync | StartDate = วันนี้ (boundary) | StartDate = Today | ผ่าน date validation; ดำเนิน business rule ต่อ | Boundary | rule-input-validation.md → 1.3, lesson-learned → EC-001 |
| TC-KB-008 | SubmitLeaveRequestAsync | EndDate < StartDate | StartDate=2026-07-05, EndDate=2026-07-03 | throw INVALID_DATE_RANGE | DataType | rule-input-validation.md → 1.3 EndDate ≥ StartDate |
| TC-KB-009 | SubmitLeaveRequestAsync | StartDate = default(DateOnly) | StartDate = 0001-01-01 | throw INVALID_DATE_RANGE (ห้ามเป็น default) | DataType | rule-input-validation.md → 1.3, 5.3 |
| TC-KB-010 | SubmitLeaveRequestAsync | IsHalfDay=true, HalfDayPeriod = "MORNING" | IsHalfDay=true, HalfDayPeriod="MORNING" | throw INVALID_HALF_DAY | EnumValidation | rule-input-validation.md → 1.5 valid="AM"\|"PM" only |
| TC-KB-011 | SubmitLeaveRequestAsync | IsHalfDay=true, HalfDayPeriod = null | IsHalfDay=true, HalfDayPeriod=null | throw INVALID_HALF_DAY | EnumValidation | rule-input-validation.md → 1.5 |
| TC-KB-012 | SubmitLeaveRequestAsync | IsHalfDay=true, StartDate ≠ EndDate | IsHalfDay=true, Start=2026-07-01, End=2026-07-02 | throw INVALID_HALF_DAY | Boundary | rule-input-validation.md → 1.5, lesson-learned → EC-005 |
| TC-KB-013 | SubmitLeaveRequestAsync | IsHalfDay=true → DurationDays ต้องเป็น 0.5 | IsHalfDay=true, Start=End=2026-07-01, remaining=0.5 | DurationDays=0.5; ผ่าน balance check | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-004 |
| TC-KB-014 | SubmitLeaveRequestAsync | RemainingDays = DurationDays พอดี (boundary) | Entitled=5, Used=2, Pending=0, CarriedForward=0, ขอลา 3 วัน | ผ่าน (3 ≥ 3); submit สำเร็จ | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-008 |
| TC-KB-015 | SubmitLeaveRequestAsync | RemainingDays < DurationDays (just under) | Remaining=2.5, ขอลา 3 วัน | throw INSUFFICIENT_BALANCE | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-009 |
| TC-KB-016 | SubmitLeaveRequestAsync | Reason ยาวเกิน MaxLength | Reason = string 501 ตัว | throw validation error (MaxLength=500) | MaxLength | rule-input-validation.md → 1.4 MaxLength=500 |
| TC-KB-017 | SubmitLeaveRequestAsync | Reason = null (optional) | Reason = null | ผ่าน; entity.Reason stored as null | NullEmpty | rule-input-validation.md → 1.4 Optional |

---

## CancelLeaveRequestAsync — 5 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-KB-018 | CancelLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-019 | CancelLeaveRequestAsync | employeeId = whitespace | employeeId = " " | throw UNAUTHORIZED (whitespace ≠ lr.EmployeeId; ไม่ NullRef) | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-020 | CancelLeaveRequestAsync | Status=Approved → CancelRequest ไม่ยกเลิกทันที | lr.Status=Approved, employeeId=เจ้าของ | Status→CancelRequested; INSERT CancelRequest; ไม่เปลี่ยนเป็น Cancelled ทันที | DataType | rule-input-validation.md → 4.2 |
| TC-KB-021 | CancelLeaveRequestAsync | Status=CancelRequested → ห้ามซ้ำ | lr.Status=CancelRequested | throw INVALID_STATUS | EnumValidation | rule-input-validation.md → 4.2 |
| TC-KB-022 | CancelLeaveRequestAsync | PendingDays ติดลบไม่ได้ | lr.Status=Pending, DurationDays=3, balance.PendingDays=1 | balance.PendingDays=0 (Math.Max(0,...) ป้องกัน negative) | Boundary | std-unit-test.md → Programmer Experience |

---

## ApproveLeaveRequestAsync — 7 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-KB-023 | ApproveLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-024 | ApproveLeaveRequestAsync | managerId = null | managerId = null | throw ArgumentNullException / UNAUTHORIZED (ไม่ NullRef) | NullEmpty | rule-input-validation.md → 2.2 |
| TC-KB-025 | ApproveLeaveRequestAsync | managerId = whitespace | managerId = " " | throw UNAUTHORIZED | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-026 | ApproveLeaveRequestAsync | Employee.ManagerId = null (ไม่มี Manager) | employee.ManagerId=null, managerId="MGR001" | throw UNAUTHORIZED (null != "MGR001"); ไม่ NullRef | RegressionBug | lesson-learned → BUG-001 |
| TC-KB-027 | ApproveLeaveRequestAsync | Self-approval | managerId=lr.EmployeeId="EMP001", employee.ManagerId="EMP001" | throw UNAUTHORIZED (ห้าม approve ของตัวเอง) | BusinessRule | rule-input-validation.md → 2.2 |
| TC-KB-028 | ApproveLeaveRequestAsync | Balance deduction: PendingDays → UsedDays | balance.PendingDays=3, UsedDays=2, DurationDays=3 | หลัง approve: PendingDays=0, UsedDays=5; Verify balanceRepo.Update() | RegressionBug | lesson-learned → BUG-002 |
| TC-KB-029 | ApproveLeaveRequestAsync | balance = null (ไม่มี LeaveBalance record) | balanceRepo.GetAsync() returns null | approve สำเร็จ; balanceRepo.Update() ไม่ถูกเรียก | Boundary | std-unit-test.md → Programmer Experience |

---

## RejectLeaveRequestAsync — 7 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-KB-030 | RejectLeaveRequestAsync | RejectionReason = null | comment = null | throw REJECTION_REASON_REQUIRED | NullEmpty | rule-input-validation.md → 3.1 |
| TC-KB-031 | RejectLeaveRequestAsync | RejectionReason = empty string | comment = "" | throw REJECTION_REASON_REQUIRED | NullEmpty | rule-input-validation.md → 3.1 |
| TC-KB-032 | RejectLeaveRequestAsync | RejectionReason = whitespace | comment = " " | throw REJECTION_REASON_REQUIRED (IsNullOrWhiteSpace) | WhiteSpace | lesson-learned → BUG-008, rule-input-validation.md → 3.1 |
| TC-KB-033 | RejectLeaveRequestAsync | RejectionReason ยาวเกิน MaxLength | comment = string 501 ตัว | throw validation error (MaxLength=500) | MaxLength | rule-input-validation.md → 3.1, 5.1 |
| TC-KB-034 | RejectLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-035 | RejectLeaveRequestAsync | Balance restore: PendingDays คืนกลับเมื่อ Reject | balance.PendingDays=3, DurationDays=3 | หลัง reject: PendingDays=0; Verify balanceRepo.Update() | RegressionBug | lesson-learned → BUG-002 (symmetric กับ approve) |
| TC-KB-036 | RejectLeaveRequestAsync | Self-reject | managerId=lr.EmployeeId="EMP001" | throw UNAUTHORIZED | BusinessRule | rule-input-validation.md → 2.2 |

---

## Summary

| Method | NullEmpty | WhiteSpace | MaxLength | Boundary | EnumValidation | DataType | RegressionBug | รวม |
|--------|-----------|------------|-----------|----------|----------------|----------|---------------|-----|
| Submit | 3 | 1 | 2 | 5 | 2 | 2 | 2 | 17 |
| Cancel | 0 | 1 | 0 | 1 | 1 | 1 | 1 | 5 |
| Approve | 1 | 1 | 0 | 1 | 0 | 1 | 3 | 7 |
| Reject | 2 | 1 | 1 | 0 | 0 | 1 | 2 | 7 |
| **รวม** | **6** | **4** | **3** | **7** | **3** | **5** | **8** | **36** |
