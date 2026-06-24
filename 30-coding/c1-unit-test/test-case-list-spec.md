# Unit Test Cases — Derived from Design Spec (TC-SPEC)

> ผลลัพธ์จาก Prompt 2.1  
> Implementation class จริง: `LeaveRequestService.cs`  
> Source spec: Method Signature §4.4–4.5, Sequence Diagram §2–5, SRS SF-003/SF-004/SF-007  
> Open Question resolved: **Reason field = required** (อ้างอิง Method Signature §4.5)

---

## SubmitLeaveRequestAsync — 15 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-001 | SubmitLeaveRequestAsync | Happy Path — Regular employee, Annual Leave, balance sufficient | employeeId="EMP001", leaveTypeId=ANNUAL, start=2026-07-01, end=2026-07-03, balance.remaining=8 | Status=Pending, LeaveRequestRef ไม่ null; INSERT LeaveRequest; UPDATE LeaveBalance.PendingDays += 3; COMMIT | HappyPath | Method Sig §4.4, Seq Diagram §2, SRS SF-003 §2.3.6 |
| TC-SPEC-002 | SubmitLeaveRequestAsync | Exception — Employee not found | employeeId="EMP999" | throw EmployeeNotFoundException; ไม่เรียก SaveChangesAsync | ExceptionFlow | Method Sig §4.4 step 1, Seq Diagram §4 |
| TC-SPEC-003 | SubmitLeaveRequestAsync | Exception — Employee inactive | employeeId="EMP_INACTIVE" (IsActive=false) | throw EmployeeNotFoundException; ไม่เรียก SaveChangesAsync | ExceptionFlow | Method Sig §4.4 step 1 "IsActive=true" |
| TC-SPEC-004 | SubmitLeaveRequestAsync | Exception — LeaveType not found/deleted | leaveTypeId=99 (ไม่มี หรือ IsDeleted=true) | throw LeaveTypeNotFoundException; ไม่เรียก SaveChangesAsync | ExceptionFlow | Method Sig §4.4 step 2 |
| TC-SPEC-005 | SubmitLeaveRequestAsync | BusinessRule VR-001 — Outsource ขอลาประเภทที่ไม่มีสิทธิ์ | employeeId="OUT001" (Outsource), leaveTypeId=ANNUAL (IsAvailableForOutsource=false) | throw InvalidLeaveTypeForEmployeeException | BusinessRule | Method Sig §4.4 step 3 VR-001, SRS SF-003 §2.3.9 BR-011 |
| TC-SPEC-006 | SubmitLeaveRequestAsync | BusinessRule VR-003 — Probation ขอลาพักผ่อน | employeeId="EMP_NEW" (hireDate 2 เดือน), leaveTypeId=ANNUAL | throw ProbationPeriodException | BusinessRule | Method Sig §4.4 step 4 VR-003, SRS SF-003 §2.3.9 BR-007 |
| TC-SPEC-007 | SubmitLeaveRequestAsync | BusinessRule VR-004 — อายุงาน < 1 ปี ขอลาพักผ่อน | employeeId="EMP_JUNIOR" (hireDate 8 เดือน), leaveTypeId=ANNUAL | throw AnnualLeaveInsufficientServiceException | BusinessRule | Method Sig §4.4 step 5 VR-004, SRS SF-003 §2.3.8 ERR-LR-004 |
| TC-SPEC-008 | SubmitLeaveRequestAsync | BusinessRule VR-005 — ลาพักผ่อน ไม่แจ้งล่วงหน้า ≥ 1 วัน | leaveTypeId=ANNUAL, startDate=วันนี้ | throw LeaveAdvanceNoticeException (requiredDays=1) | BusinessRule | Method Sig §4.4 step 6 VR-005, SRS §2.3.9 BR-003 |
| TC-SPEC-009 | SubmitLeaveRequestAsync | BusinessRule VR-006 — ลากิจ ไม่แจ้งล่วงหน้า ≥ 3 วันทำการ | leaveTypeId=BUSINESS, startDate=พรุ่งนี้ | throw LeaveAdvanceNoticeException (requiredDays=3) | BusinessRule | Method Sig §4.4 step 7 VR-006, SRS §2.3.9 BR-004 |
| TC-SPEC-010 | SubmitLeaveRequestAsync | BusinessRule VR-007 — ลาป่วย ≥ 3 วัน ไม่แนบใบรับรองแพทย์ | leaveTypeId=SICK, duration=3, attachmentIds=[] | throw MedicalCertificateRequiredException | BusinessRule | Method Sig §4.4 step 8 VR-007, SRS SF-003 §2.3.9 BR-006 |
| TC-SPEC-011 | SubmitLeaveRequestAsync | AlternativeFlow — ลาป่วย ≥ 3 วัน พร้อมแนบใบรับรอง | leaveTypeId=SICK, duration=3, attachmentIds=[Guid] | Status=Pending; ผ่าน VR-007 | AlternativeFlow | Method Sig §4.4 step 8 VR-007 |
| TC-SPEC-012 | SubmitLeaveRequestAsync | ExceptionFlow VR-002 — วันลาไม่พอ | balance.remaining=1, ขอลา 3 วัน | throw InsufficientLeaveBalanceException (remaining=1, requested=3) | ExceptionFlow | Method Sig §4.4 step 9 VR-002, SRS SF-003 §2.3.8 ERR-LR-002 |
| TC-SPEC-013 | SubmitLeaveRequestAsync | ExceptionFlow — วันที่ซ้อนทับกับ Pending/Approved อื่น | ช่วงวันที่ overlap กับ Pending อื่น | throw DateConflictException | ExceptionFlow | Method Sig §4.4 step 10, Seq Diagram §5 |
| TC-SPEC-014 | SubmitLeaveRequestAsync | Transaction Atomicity — Validation fail ไม่มีการเปลี่ยน DB | VR-002 fail input | ไม่มี INSERT/UPDATE; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §7 NFR-010, Seq Diagram §5 |
| TC-SPEC-015 | SubmitLeaveRequestAsync | Post-commit — PublishLeaveSubmittedAsync ถูกเรียกหลัง COMMIT | happy path input | PublishLeaveSubmittedAsync ถูกเรียก 1 ครั้งหลัง COMMIT | HappyPath | Method Sig §4.4, Seq Diagram §7 |

---

## CancelPendingLeaveRequestAsync — 5 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-016 | CancelPendingLeaveRequestAsync | Happy Path — เจ้าของยกเลิก Pending | employeeId="EMP001" (เจ้าของ), leaveRequestId (Status=Pending, PendingDays=3) | Status→Cancelled; PendingDays -= 3; COMMIT | HappyPath | Method Sig §4.4, SRS SF-007 §2.7.2, BR-014 |
| TC-SPEC-017 | CancelPendingLeaveRequestAsync | ExceptionFlow — ไม่พบคำขอ | leaveRequestId ไม่มีในระบบ | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.4 step 1 |
| TC-SPEC-018 | CancelPendingLeaveRequestAsync | ExceptionFlow — ไม่ใช่เจ้าของ | employeeId="EMP002" ยกเลิกคำขอของ EMP001 | throw UnauthorizedLeaveActionException | ExceptionFlow | Method Sig §4.4 step 2 RBAC, SRS SF-007 §2.7.2 |
| TC-SPEC-019 | CancelPendingLeaveRequestAsync | BusinessRule VR-009 — ห้ามยกเลิก Status=Rejected | leaveRequestId (Status=Rejected) | throw InvalidLeaveStatusTransitionException | BusinessRule | Method Sig §4.4 step 3 VR-009, SRS SF-007 §2.7.6 BR-017 |
| TC-SPEC-020 | CancelPendingLeaveRequestAsync | BusinessRule — ห้ามยกเลิก Status=Approved ด้วย method นี้ | leaveRequestId (Status=Approved) | throw InvalidLeaveStatusTransitionException | BusinessRule | Method Sig §4.4, SRS SF-008 |

---

## ApproveLeaveRequestAsync — 6 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-021 | ApproveLeaveRequestAsync | Happy Path — Manager อนุมัติ | managerId="MGR001", leaveRequestId (Status=Pending, PendingDays=3) | Status=Approved; PendingDays -= 3; UsedDays += 3; INSERT ApprovalHistory; COMMIT | HappyPath | Method Sig §4.5, §7 Transaction, Seq Diagram §5 |
| TC-SPEC-022 | ApproveLeaveRequestAsync | ExceptionFlow — ไม่พบคำขอ | leaveRequestId ไม่มีในระบบ | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-023 | ApproveLeaveRequestAsync | ExceptionFlow — Status ไม่ใช่ Pending | leaveRequestId (Status=Approved แล้ว) | throw InvalidLeaveStatusTransitionException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-024 | ApproveLeaveRequestAsync | ExceptionFlow — ไม่ใช่ Manager ของเจ้าของคำขอ | managerId="MGR002" (ไม่รับผิดชอบ EMP001) | throw UnauthorizedLeaveActionException | ExceptionFlow | Method Sig §4.5 step 2 RBAC, SRS SF-004 §2.4.9 BR-012 |
| TC-SPEC-025 | ApproveLeaveRequestAsync | Transaction — ApprovalHistory อยู่ใน Transaction เดียวกัน | happy path input | ApprovalHistory INSERT อยู่ใน transaction เดียวกับ UPDATE LeaveRequest และ LeaveBalance | BusinessRule | Method Sig §7, §4.5 "BEGIN TRANSACTION … INSERT ApprovalHistory … COMMIT" |
| TC-SPEC-026 | ApproveLeaveRequestAsync | Post-commit — PublishLeaveApprovedAsync ถูกเรียกหลัง COMMIT | happy path input | PublishLeaveApprovedAsync ถูกเรียก 1 ครั้งหลัง COMMIT | HappyPath | Method Sig §4.5 |

---

## RejectLeaveRequestAsync — 6 test cases

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-027 | RejectLeaveRequestAsync | Happy Path — Manager ปฏิเสธพร้อมเหตุผล | managerId="MGR001", leaveRequestId (Status=Pending, PendingDays=3), reason="ช่วงนั้นคนน้อย" | Status=Rejected; PendingDays -= 3; INSERT ApprovalHistory; COMMIT | HappyPath | Method Sig §4.5, §7 Transaction |
| TC-SPEC-028 | RejectLeaveRequestAsync | ExceptionFlow — ไม่พบคำขอ | leaveRequestId ไม่มีในระบบ | throw LeaveRequestNotFoundException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-029 | RejectLeaveRequestAsync | BusinessRule BR-013 — reason ว่างเปล่า | reason="" | throw ArgumentException; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §4.5 step 3 BR-013 |
| TC-SPEC-030 | RejectLeaveRequestAsync | BusinessRule BR-013 — reason เป็น null | reason=null | throw ArgumentException; ไม่เรียก SaveChangesAsync | BusinessRule | Method Sig §4.5, SRS SF-004 §2.4.9 BR-013 |
| TC-SPEC-031 | RejectLeaveRequestAsync | ExceptionFlow — Status ไม่ใช่ Pending | leaveRequestId (Status=Rejected แล้ว) | throw InvalidLeaveStatusTransitionException | ExceptionFlow | Method Sig §4.5 step 1 |
| TC-SPEC-032 | RejectLeaveRequestAsync | Post-commit — PublishLeaveRejectedAsync พร้อม reason | happy path input | PublishLeaveRejectedAsync ถูกเรียก 1 ครั้งพร้อม reason ตรงกัน หลัง COMMIT | HappyPath | Method Sig §4.5 |

---

## Summary

| Method | HappyPath | AlternativeFlow | ExceptionFlow | BusinessRule | รวม |
|--------|-----------|-----------------|---------------|--------------|-----|
| SubmitLeaveRequestAsync | 2 | 1 | 5 | 7 | 15 |
| CancelPendingLeaveRequestAsync | 1 | 0 | 2 | 2 | 5 |
| ApproveLeaveRequestAsync | 2 | 0 | 2 | 2 | 6 |
| RejectLeaveRequestAsync | 2 | 0 | 2 | 2 | 6 |
| **รวม** | **7** | **1** | **11** | **13** | **32** |
