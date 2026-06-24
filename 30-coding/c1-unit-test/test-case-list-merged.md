# Merged Test Case List — LeaveRequestService

> รวม TC-SPEC (32 cases) + TC-KB (36 cases) = **68 test cases**  
> พร้อมใช้เป็น input สำหรับ Prompt 3 — Generate Test Code  
> Open Question resolved: Reason field = **required** (Method Sig §4.5)  
> Bug fixes applied: RejectAsync null guard, ApproveAsync managerId null guard, HalfDayPeriod validation

---

## SubmitLeaveRequestAsync — 32 test cases (15 SPEC + 17 KB)

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-001 | SubmitLeaveRequestAsync | Happy Path — Regular, Annual, balance sufficient | employeeId="EMP001", leaveTypeId=ANNUAL, start=2026-07-01, end=2026-07-03, remaining=8 | Status=Pending, LeaveRequestRef ไม่ null; PendingDays+=3; COMMIT | HappyPath | Method Sig §4.4, Seq Diagram §2, SRS SF-003 §2.3.6 |
| TC-SPEC-002 | SubmitLeaveRequestAsync | Exception — Employee not found | employeeId="EMP999" | throw EmployeeNotFoundException | ExceptionFlow | Method Sig §4.4 step 1 |
| TC-SPEC-003 | SubmitLeaveRequestAsync | Exception — Employee inactive | employeeId="EMP_INACTIVE" (IsActive=false) | throw EmployeeNotFoundException | ExceptionFlow | Method Sig §4.4 step 1 |
| TC-SPEC-004 | SubmitLeaveRequestAsync | Exception — LeaveType not found | leaveTypeId=99 | throw LeaveTypeNotFoundException | ExceptionFlow | Method Sig §4.4 step 2 |
| TC-SPEC-005 | SubmitLeaveRequestAsync | VR-001 — Outsource ขอลาประเภทไม่มีสิทธิ์ | employeeId=Outsource, leaveTypeId=ANNUAL (IsAvailableForOutsource=false) | throw InvalidLeaveTypeForEmployeeException | BusinessRule | Method Sig §4.4 step 3 VR-001 |
| TC-SPEC-006 | SubmitLeaveRequestAsync | VR-003 — Probation ขอลาพักผ่อน | hireDate=2 เดือนที่แล้ว, leaveTypeId=ANNUAL | throw ProbationPeriodException | BusinessRule | Method Sig §4.4 step 4 VR-003 |
| TC-SPEC-007 | SubmitLeaveRequestAsync | VR-004 — อายุงาน < 1 ปี ขอลาพักผ่อน | hireDate=8 เดือนที่แล้ว, leaveTypeId=ANNUAL | throw AnnualLeaveInsufficientServiceException | BusinessRule | Method Sig §4.4 step 5 VR-004 |
| TC-SPEC-008 | SubmitLeaveRequestAsync | VR-005 — ลาพักผ่อน ไม่แจ้งล่วงหน้า ≥ 1 วัน | leaveTypeId=ANNUAL, startDate=Today | throw LeaveAdvanceNoticeException (required=1) | BusinessRule | Method Sig §4.4 step 6 VR-005 |
| TC-SPEC-009 | SubmitLeaveRequestAsync | VR-006 — ลากิจ ไม่แจ้งล่วงหน้า ≥ 3 วันทำการ | leaveTypeId=BUSINESS, startDate=Tomorrow | throw LeaveAdvanceNoticeException (required=3) | BusinessRule | Method Sig §4.4 step 7 VR-006 |
| TC-SPEC-010 | SubmitLeaveRequestAsync | VR-007 — ลาป่วย ≥ 3 วัน ไม่แนบใบรับรอง | leaveTypeId=SICK, duration=3, attachments=[] | throw MedicalCertificateRequiredException | BusinessRule | Method Sig §4.4 step 8 VR-007 |
| TC-SPEC-011 | SubmitLeaveRequestAsync | AlternativeFlow — ลาป่วย ≥ 3 วัน พร้อมใบรับรอง | leaveTypeId=SICK, duration=3, attachments=[Guid] | Status=Pending; ผ่าน VR-007 | AlternativeFlow | Method Sig §4.4 step 8 |
| TC-SPEC-012 | SubmitLeaveRequestAsync | VR-002 — วันลาไม่พอ | remaining=1, ขอลา 3 วัน | throw InsufficientLeaveBalanceException | ExceptionFlow | Method Sig §4.4 step 9 VR-002 |
| TC-SPEC-013 | SubmitLeaveRequestAsync | วันที่ overlap กับ Pending/Approved อื่น | ช่วงวันที่ซ้อนทับ | throw DateConflictException | ExceptionFlow | Method Sig §4.4 step 10 |
| TC-SPEC-014 | SubmitLeaveRequestAsync | Transaction Atomicity — Validation fail ไม่เปลี่ยน DB | VR-002 fail | ไม่มี INSERT/UPDATE; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §7 NFR-010 |
| TC-SPEC-015 | SubmitLeaveRequestAsync | Post-commit — PublishLeaveSubmittedAsync | happy path | PublishLeaveSubmittedAsync ถูกเรียก 1 ครั้งหลัง COMMIT | HappyPath | Method Sig §4.4 |
| TC-KB-001 | SubmitLeaveRequestAsync | EmployeeId = null | null | throw ArgumentNullException / EMPLOYEE_NOT_FOUND (ไม่ NullRef) | NullEmpty | rule-input-validation.md → 1.1 |
| TC-KB-002 | SubmitLeaveRequestAsync | EmployeeId = empty string | "" | throw EMPLOYEE_NOT_FOUND | NullEmpty | rule-input-validation.md → 1.1 |
| TC-KB-003 | SubmitLeaveRequestAsync | EmployeeId = whitespace | " " | throw EMPLOYEE_NOT_FOUND (IsNullOrWhiteSpace) | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-004 | SubmitLeaveRequestAsync | EmployeeId ยาวเกิน MaxLength | string 51 ตัว | throw validation error | MaxLength | rule-input-validation.md → 1.1 MaxLength=50 |
| TC-KB-005 | SubmitLeaveRequestAsync | LeaveTypeId = 0 | LeaveTypeId=0 | throw INVALID_LEAVE_TYPE | Boundary | rule-input-validation.md → 1.2 |
| TC-KB-006 | SubmitLeaveRequestAsync | StartDate = เมื่อวาน | Today.AddDays(-1) | throw INVALID_DATE_RANGE | Boundary | rule-input-validation.md → 1.3, lesson-learned → EC-002 |
| TC-KB-007 | SubmitLeaveRequestAsync | StartDate = วันนี้ (boundary: valid) | Today | ผ่าน date validation | Boundary | rule-input-validation.md → 1.3, lesson-learned → EC-001 |
| TC-KB-008 | SubmitLeaveRequestAsync | EndDate < StartDate | Start=07-05, End=07-03 | throw INVALID_DATE_RANGE | DataType | rule-input-validation.md → 1.3 |
| TC-KB-009 | SubmitLeaveRequestAsync | StartDate = default(DateOnly) | 0001-01-01 | throw INVALID_DATE_RANGE | DataType | rule-input-validation.md → 5.3 |
| TC-KB-010 | SubmitLeaveRequestAsync | IsHalfDay=true, HalfDayPeriod="MORNING" | HalfDayPeriod="MORNING" | throw INVALID_HALF_DAY | EnumValidation | rule-input-validation.md → 1.5 |
| TC-KB-011 | SubmitLeaveRequestAsync | IsHalfDay=true, HalfDayPeriod=null | null | throw INVALID_HALF_DAY | EnumValidation | rule-input-validation.md → 1.5 |
| TC-KB-012 | SubmitLeaveRequestAsync | IsHalfDay=true, Start ≠ End | Start=07-01, End=07-02 | throw INVALID_HALF_DAY | Boundary | rule-input-validation.md → 1.5, lesson-learned → EC-005 |
| TC-KB-013 | SubmitLeaveRequestAsync | IsHalfDay=true → DurationDays=0.5 | IsHalfDay=true, Start=End, remaining=0.5 | DurationDays=0.5; ผ่าน balance check | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-004 |
| TC-KB-014 | SubmitLeaveRequestAsync | RemainingDays = DurationDays พอดี (boundary) | Remaining=3, ขอลา 3 วัน | ผ่าน (3 ≥ 3) | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-008 |
| TC-KB-015 | SubmitLeaveRequestAsync | RemainingDays < DurationDays (just under) | Remaining=2.5, ขอลา 3 วัน | throw INSUFFICIENT_BALANCE | Boundary | rule-input-validation.md → 1.6, lesson-learned → EC-009 |
| TC-KB-016 | SubmitLeaveRequestAsync | Reason ยาวเกิน MaxLength | string 501 ตัว | throw validation error (MaxLength=500) | MaxLength | rule-input-validation.md → 1.4 |
| TC-KB-017 | SubmitLeaveRequestAsync | Reason = null (optional) | null | ผ่าน; stored as null | NullEmpty | rule-input-validation.md → 1.4 Optional |

---

## CancelLeaveRequestAsync — 10 test cases (5 SPEC + 5 KB)

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-016 | CancelLeaveRequestAsync | Happy Path — เจ้าของยกเลิก Pending | employeeId=เจ้าของ, Status=Pending, PendingDays=3 | Status→Cancelled; PendingDays-=3; COMMIT | HappyPath | Method Sig §4.4, SRS SF-007 BR-014 |
| TC-SPEC-017 | CancelLeaveRequestAsync | Exception — ไม่พบคำขอ | leaveRequestId ไม่มี | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.4 step 1 |
| TC-SPEC-018 | CancelLeaveRequestAsync | Exception — ไม่ใช่เจ้าของ | employeeId=EMP002 ยกเลิกของ EMP001 | throw UnauthorizedLeaveActionException | ExceptionFlow | Method Sig §4.4 step 2 |
| TC-SPEC-019 | CancelLeaveRequestAsync | VR-009 — ห้ามยกเลิก Status=Rejected | Status=Rejected | throw InvalidLeaveStatusTransitionException | BusinessRule | Method Sig §4.4 VR-009 |
| TC-SPEC-020 | CancelLeaveRequestAsync | ห้ามยกเลิก Status=Approved ด้วย method นี้ | Status=Approved | throw InvalidLeaveStatusTransitionException | BusinessRule | Method Sig §4.4, SRS SF-008 |
| TC-KB-018 | CancelLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-019 | CancelLeaveRequestAsync | employeeId = whitespace | " " | throw UNAUTHORIZED (ไม่ NullRef) | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-020 | CancelLeaveRequestAsync | Status=Approved → CancelRequested (ไม่ยกเลิกทันที) | Status=Approved, employeeId=เจ้าของ | Status→CancelRequested; INSERT CancelRequest | DataType | rule-input-validation.md → 4.2 |
| TC-KB-021 | CancelLeaveRequestAsync | Status=CancelRequested → ห้ามซ้ำ | Status=CancelRequested | throw INVALID_STATUS | EnumValidation | rule-input-validation.md → 4.2 |
| TC-KB-022 | CancelLeaveRequestAsync | PendingDays ติดลบไม่ได้ | PendingDays=1, DurationDays=3 | PendingDays=0 (Math.Max guard) | Boundary | std-unit-test.md → Programmer Experience |

---

## ApproveLeaveRequestAsync — 13 test cases (6 SPEC + 7 KB)

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-021 | ApproveLeaveRequestAsync | Happy Path — Manager อนุมัติ | managerId=MGR001, Status=Pending, PendingDays=3 | Status=Approved; PendingDays-=3; UsedDays+=3; INSERT ApprovalHistory; COMMIT | HappyPath | Method Sig §4.5, §7 |
| TC-SPEC-022 | ApproveLeaveRequestAsync | Exception — ไม่พบคำขอ | leaveRequestId ไม่มี | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-023 | ApproveLeaveRequestAsync | Exception — Status ไม่ใช่ Pending | Status=Approved แล้ว | throw InvalidLeaveStatusTransitionException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-024 | ApproveLeaveRequestAsync | Exception — ไม่ใช่ Manager ของเจ้าของ | managerId=MGR002 (ไม่รับผิดชอบ EMP001) | throw UnauthorizedLeaveActionException | ExceptionFlow | Method Sig §4.5 step 2, SRS SF-004 BR-012 |
| TC-SPEC-025 | ApproveLeaveRequestAsync | Transaction — ApprovalHistory ใน transaction เดียว | happy path | ApprovalHistory INSERT อยู่ใน transaction เดียวกับ UPDATE | BusinessRule | Method Sig §7 |
| TC-SPEC-026 | ApproveLeaveRequestAsync | Post-commit — PublishLeaveApprovedAsync | happy path | PublishLeaveApprovedAsync ถูกเรียก 1 ครั้งหลัง COMMIT | HappyPath | Method Sig §4.5 |
| TC-KB-023 | ApproveLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-024 | ApproveLeaveRequestAsync | managerId = null | null | throw ArgumentNullException / UNAUTHORIZED (ไม่ NullRef) | NullEmpty | rule-input-validation.md → 2.2 |
| TC-KB-025 | ApproveLeaveRequestAsync | managerId = whitespace | " " | throw UNAUTHORIZED | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-026 | ApproveLeaveRequestAsync | Employee.ManagerId = null | employee.ManagerId=null, managerId="MGR001" | throw UNAUTHORIZED (null != "MGR001"); ไม่ NullRef | RegressionBug | lesson-learned → BUG-001 |
| TC-KB-027 | ApproveLeaveRequestAsync | Self-approval | managerId=lr.EmployeeId="EMP001" | throw UNAUTHORIZED | BusinessRule | rule-input-validation.md → 2.2 |
| TC-KB-028 | ApproveLeaveRequestAsync | Balance deduction: PendingDays → UsedDays | PendingDays=3, UsedDays=2, Duration=3 | PendingDays=0, UsedDays=5; Verify Update() | RegressionBug | lesson-learned → BUG-002 |
| TC-KB-029 | ApproveLeaveRequestAsync | balance = null (ไม่มี LeaveBalance record) | balanceRepo returns null | approve สำเร็จ; Update() ไม่ถูกเรียก | Boundary | std-unit-test.md → Programmer Experience |

---

## RejectLeaveRequestAsync — 13 test cases (6 SPEC + 7 KB)

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-027 | RejectLeaveRequestAsync | Happy Path — Manager ปฏิเสธพร้อมเหตุผล | managerId=MGR001, Status=Pending, reason="ช่วงนั้นคนน้อย" | Status=Rejected; PendingDays-=3; INSERT ApprovalHistory; COMMIT | HappyPath | Method Sig §4.5, §7 |
| TC-SPEC-028 | RejectLeaveRequestAsync | Exception — ไม่พบคำขอ | leaveRequestId ไม่มี | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-029 | RejectLeaveRequestAsync | BR-013 — reason = empty string | reason="" | throw ArgumentException; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §4.5 step 3 BR-013 |
| TC-SPEC-030 | RejectLeaveRequestAsync | BR-013 — reason = null | reason=null | throw ArgumentException; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §4.5 BR-013 |
| TC-SPEC-031 | RejectLeaveRequestAsync | Exception — Status ไม่ใช่ Pending | Status=Rejected แล้ว | throw InvalidLeaveStatusTransitionException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-032 | RejectLeaveRequestAsync | Post-commit — PublishLeaveRejectedAsync | happy path | PublishLeaveRejectedAsync ถูกเรียก 1 ครั้งพร้อม reason หลัง COMMIT | HappyPath | Method Sig §4.5 |
| TC-KB-030 | RejectLeaveRequestAsync | RejectionReason = null | null | throw REJECTION_REASON_REQUIRED | NullEmpty | rule-input-validation.md → 3.1 |
| TC-KB-031 | RejectLeaveRequestAsync | RejectionReason = empty string | "" | throw REJECTION_REASON_REQUIRED | NullEmpty | rule-input-validation.md → 3.1 |
| TC-KB-032 | RejectLeaveRequestAsync | RejectionReason = whitespace | " " | throw REJECTION_REASON_REQUIRED (IsNullOrWhiteSpace) | WhiteSpace | lesson-learned → BUG-008 |
| TC-KB-033 | RejectLeaveRequestAsync | RejectionReason ยาวเกิน MaxLength | string 501 ตัว | throw validation error (MaxLength=500) | MaxLength | rule-input-validation.md → 3.1 |
| TC-KB-034 | RejectLeaveRequestAsync | leaveRequestId = Guid.Empty | Guid.Empty | throw REQUEST_NOT_FOUND | DataType | rule-input-validation.md → 2.1 |
| TC-KB-035 | RejectLeaveRequestAsync | Balance restore — PendingDays คืนกลับ | PendingDays=3, Duration=3 | PendingDays=0; Verify Update() | RegressionBug | lesson-learned → BUG-002 |
| TC-KB-036 | RejectLeaveRequestAsync | Self-reject | managerId=lr.EmployeeId | throw UNAUTHORIZED | BusinessRule | rule-input-validation.md → 2.2 |

---

## Grand Summary

| Method | TC-SPEC | TC-KB | รวม |
|--------|---------|-------|-----|
| SubmitLeaveRequestAsync | 15 | 17 | **32** |
| CancelLeaveRequestAsync | 5 | 5 | **10** |
| ApproveLeaveRequestAsync | 6 | 7 | **13** |
| RejectLeaveRequestAsync | 6 | 7 | **13** |
| **รวมทั้งหมด** | **32** | **36** | **68** |
