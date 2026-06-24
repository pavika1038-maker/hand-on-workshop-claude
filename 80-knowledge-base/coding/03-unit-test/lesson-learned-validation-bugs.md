# Lesson Learned — Validation Bugs & Edge Cases

> บันทึกข้อผิดพลาดและ edge case ที่พบจากการพัฒนาและ smoke test  
> ใช้เป็น context ให้ AI สร้าง test case ครอบคลุม defect ที่เคยเกิด  
> อัปเดตล่าสุด: 2026-06-16

---

## BUG-001: Manager Approval Queue ไม่แสดงคำขอของบางพนักงาน

**พบเมื่อ:** Session 3 Smoke Test (SCR-004)  
**อาการ:** HR (EMP003 นันทา) ส่งคำขอลา แต่ไม่ขึ้นใน approval queue ของ Manager ใด  
**สาเหตุ:** Seed data กำหนด `ManagerId = null` สำหรับ EMP003 — query ใน repository ดึงเฉพาะ leave requests ที่ `employee.ManagerId == approverId` ดังนั้นถ้า null จะไม่ match  
**แก้ไข:** ตั้ง `ManagerId = "EMP002"` ใน EmployeeConfiguration.cs + สร้าง migration `FixManagerId`

**Test Case ที่ต้องมี:**
```
GetPendingApprovalsForManager_Should_ReturnEmpty_When_EmployeeHasNoManager
GetPendingApprovalsForManager_Should_ReturnRequests_When_EmployeeManagerMatches
```

---

## BUG-002: Leave Balance ไม่ถูก Deduct เมื่อ Approve

**พบเมื่อ:** Code Review (deviation log DEV-007)  
**อาการ:** หลัง Manager approve คำขอ, วันลาคงเหลือยังไม่ลดลง  
**สาเหตุ:** `ApproveLeaveRequestAsync` update status แต่ไม่ได้เรียก balance repository เพื่อย้าย `PendingDays → UsedDays`  
**แก้ไข:** ต้องเรียก `_balanceRepo.UpdateAsync()` หลัง update status และ wrap ใน transaction

**Test Case ที่ต้องมี:**
```
ApproveLeaveRequest_Should_DeductBalanceFromPending_When_Approved
ApproveLeaveRequest_Should_IncrementUsedDays_When_Approved
ApproveLeaveRequest_Should_DecrementPendingDays_When_Approved
```

---

## BUG-003: วันที่ Overlap ไม่ได้รับการตรวจสอบ

**พบเมื่อ:** Design Review (deviation log DEV-012)  
**อาการ:** Employee สามารถยื่นคำขอลาช่วงวันที่ซ้อนทับกับคำขอที่ Pending/Approved ได้  
**สาเหตุ:** ไม่มี business rule validation ตรวจสอบ date overlap ก่อน submit  
**แก้ไข (Pending):** ยังไม่ได้ implement — รอใส่ใน next sprint

**Test Case ที่ต้องมี (สำหรับเมื่อ implement แล้ว):**
```
SubmitLeaveRequest_Should_ReturnError_When_DateOverlapsWithPendingRequest
SubmitLeaveRequest_Should_ReturnError_When_DateOverlapsWithApprovedRequest
SubmitLeaveRequest_Should_AllowSubmit_When_DateOverlapsWithRejectedRequest
```

---

## BUG-004: SLA Calculation นับวันหยุดเสาร์-อาทิตย์

**พบเมื่อ:** Code Review (deviation log DEV-014)  
**อาการ:** SLA deadline คำนวณรวมวันหยุดสุดสัปดาห์ ทำให้ escalate ช้ากว่าที่ควร  
**สาเหตุ:** `SlaService.CalculateDeadline()` ใช้ `AddDays()` ธรรมดา ไม่ได้ skip วันหยุด  
**แก้ไข (Pending):** ต้องใช้ business day calculation

**Test Case ที่ต้องมี:**
```
CalculateSlaDeadline_Should_SkipWeekends_When_SubmittedOnFriday
CalculateSlaDeadline_Should_SkipWeekends_When_SubmittedOnSaturday
CalculateSlaDeadline_Should_Add2BusinessDays_When_SubmittedOnMonday
```

---

## BUG-005: Enum Mismatch ระหว่าง Frontend และ Backend

**พบเมื่อ:** Session 3 Smoke Test — status filter ไม่ทำงาน  
**อาการ:** Frontend ส่ง status = `"PendingApproval"` แต่ backend expect `"Pending"` (ค่าจาก `LeaveStatus.ToString()`)  
**สาเหตุ:** TypeScript interface กำหนด status string ไม่ตรงกับ C# enum name  
**แก้ไข:** แก้ `leaveRequest.ts` ให้ใช้ `'Pending'` แทน `'PendingApproval'`

**Test Case ที่ต้องมี (backend):**
```
GetLeaveHistory_Should_FilterByStatus_When_PendingStatusProvided
GetLeaveHistory_Should_ReturnAll_When_NoStatusFilter
```

---

## BUG-006: ApiResponse Structure ไม่ตรงกับ Frontend

**พบเมื่อ:** Session 3 — Login ไม่ผ่าน, error message ไม่แสดง  
**อาการ:** Frontend parse response ไม่ได้ เพราะ backend return nested `error: { code, message }` แต่ frontend expect flat `errorCode`, `message`  
**สาเหตุ:** C# `ApiResponse<T>` มี nested class `ApiError` — ไม่ตรงกับ TypeScript interface  
**แก้ไข:** Flatten structure ใน `ApiResponse.cs`

**Test Case ที่ต้องมี:**
```
// Integration test level — ตรวจ JSON shape ของ response
```

---

## BUG-007: Cancel Request ไม่มี Transaction

**พบเมื่อ:** Code Review (deviation log DEV-009)  
**อาการ (potential):** ถ้า update LeaveRequest สำเร็จแต่ update LeaveBalance ล้มเหลว จะเกิด inconsistent state  
**สาเหตุ:** `RejectCancelAsync` ไม่มี `using var transaction = await _dbContext.Database.BeginTransactionAsync()`  
**แก้ไข (Pending):** ต้อง wrap ใน transaction

**Test Case ที่ต้องมี:**
```
CancelLeaveRequest_Should_RollbackStatus_When_BalanceUpdateFails
```

---

## BUG-008: Whitespace ไม่ถูก Trim ก่อน Validate

**พบเมื่อ:** Code Review  
**อาการ (potential):** `employeeId = "   "` (spaces only) ผ่าน null check แต่ควรถือว่าเป็น invalid  
**แก้ไข:** ใช้ `string.IsNullOrWhiteSpace()` ทุกครั้ง ไม่ใช้แค่ `string.IsNullOrEmpty()`

**Test Case ที่ต้องมี:**
```
SubmitLeaveRequest_Should_ReturnError_When_EmployeeIdIsWhitespace
SubmitLeaveRequest_Should_ReturnError_When_ReasonIsWhitespaceOnly  // (ถ้า required)
RejectLeaveRequest_Should_ReturnError_When_RejectionReasonIsWhitespace
```

---

## สรุป Edge Cases ที่ต้อง Cover

| # | Scenario | เกี่ยวกับ |
|---|----------|----------|
| EC-001 | Submit leave บนวันนี้ (boundary: StartDate = Today) | Date validation |
| EC-002 | Submit leave เมื่อวาน (StartDate < Today) | Date validation |
| EC-003 | StartDate = EndDate (1 วัน) | Duration calculation |
| EC-004 | IsHalfDay = true, HalfDayPeriod = "AM" | Duration = 0.5 |
| EC-005 | IsHalfDay = true, StartDate ≠ EndDate | Invalid |
| EC-006 | IsHalfDay = true, HalfDayPeriod = null | Invalid |
| EC-007 | RemainingDays = 0 | Insufficient balance |
| EC-008 | RemainingDays = DurationDays (boundary) | ต้องผ่าน |
| EC-009 | RemainingDays = DurationDays - 0.5 | Insufficient |
| EC-010 | EmployeeId = whitespace string | Not found |
| EC-011 | Approve request ของตัวเอง | Unauthorized |
| EC-012 | Approve request ที่ Approved แล้ว | Invalid transition |
| EC-013 | Reject โดยไม่ใส่ reason | Required |
| EC-014 | Cancel request ที่ Cancelled แล้ว | Invalid transition |
| EC-015 | RejectionReason ยาว 501 ตัวอักษร | MaxLength exceeded |
