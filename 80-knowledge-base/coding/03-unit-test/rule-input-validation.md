# Input Validation Rules — Leave Request and Approval System

> กฎ validation ที่ใช้ใน Application Layer  
> ไฟล์นี้ใช้เป็น context ให้ AI สร้าง unit test ครอบคลุม validation ทั้งหมด  
> อัปเดตล่าสุด: 2026-06-16

---

## 1. SubmitLeaveRequest (SCR-003)

### 1.1 EmployeeId

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| Required (ไม่ null/empty/whitespace) | `null`, `""`, `"   "` | `EMPLOYEE_NOT_FOUND` |
| ต้องมีอยู่ใน database | `"EMP999"` (ไม่มีใน DB) | `EMPLOYEE_NOT_FOUND` |
| MaxLength = 50 | string ยาว 51+ ตัวอักษร | validation error |

### 1.2 LeaveTypeId

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| Required (> 0) | `0`, `-1` | `INVALID_LEAVE_TYPE` |
| ต้องมีอยู่ใน LeaveTypes table | `999` | `INVALID_LEAVE_TYPE` |

### 1.3 StartDate / EndDate

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| StartDate ≥ วันนี้ (DateOnly) | ก่อนหน้านี้ | `INVALID_DATE_RANGE` |
| EndDate ≥ StartDate | EndDate < StartDate | `INVALID_DATE_RANGE` |
| ทั้งคู่ต้อง Required | `default(DateOnly)` | `INVALID_DATE_RANGE` |

> **หมายเหตุ:** ระบบเปรียบเทียบด้วย `DateOnly.FromDateTime(DateTime.Today)` (local time)

### 1.4 Reason

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| Optional (ไม่ required) | — | — |
| MaxLength = 500 | string ยาว 501+ ตัวอักษร | validation error |

### 1.5 IsHalfDay

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| ถ้า `IsHalfDay = true` → StartDate ต้องเท่ากับ EndDate | `StartDate != EndDate` | `INVALID_HALF_DAY` |
| ถ้า `IsHalfDay = true` → `HalfDayPeriod` ต้องเป็น `"AM"` หรือ `"PM"` | `null`, `""`, `"MORNING"` | `INVALID_HALF_DAY` |
| ถ้า `IsHalfDay = false` → ไม่สนใจ HalfDayPeriod | — | — |

### 1.6 Leave Balance (Business Rule)

| กฎ | เงื่อนไข | Error Code |
|----|---------|------------|
| RemainingDays ≥ DurationDays | `remaining < duration` | `INSUFFICIENT_BALANCE` |
| RemainingDays = EntitledDays + CarriedForwardDays - UsedDays - PendingDays | คำนวณผิด | — |
| DurationDays ต้องคำนวณถูกต้อง (IsHalfDay = 0.5) | — | — |

---

## 2. ApproveLeaveRequest (SCR-004)

### 2.1 LeaveRequestId

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| Required | `Guid.Empty`, null | `REQUEST_NOT_FOUND` |
| ต้องมีอยู่ใน database และ IsDeleted = false | ไม่มีใน DB | `REQUEST_NOT_FOUND` |

### 2.2 Manager Authorization

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| ManagerId ของ requester ต้องเป็น approverId | Manager คนอื่น approve | `UNAUTHORIZED` |
| ห้าม approve คำขอของตัวเอง | `approverId == request.EmployeeId` | `UNAUTHORIZED` |

### 2.3 Status Transition

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| สามารถ approve ได้เฉพาะ Status = Pending | Approved, Rejected, Cancelled | `INVALID_STATUS_TRANSITION` |

---

## 3. RejectLeaveRequest (SCR-004)

### 3.1 RejectionReason

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| Required (ไม่ null/empty/whitespace) | `null`, `""`, `"   "` | `REJECTION_REASON_REQUIRED` |
| MaxLength = 500 | string ยาว 501+ ตัวอักษร | validation error |

### 3.2 Status Transition

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| สามารถ reject ได้เฉพาะ Status = Pending | Approved, Rejected, Cancelled | `INVALID_STATUS_TRANSITION` |

---

## 4. CancelLeaveRequest (SCR-006)

### 4.1 ความเป็นเจ้าของ

| กฎ | ค่าที่ invalid | Error Code |
|----|--------------|------------|
| เฉพาะ requester (EmployeeId) เท่านั้นที่ cancel ได้ | คนอื่น cancel | `UNAUTHORIZED` |

### 4.2 Status Transition

| สถานะเดิม | ผลลัพธ์ | Error |
|----------|---------|-------|
| Pending | Cancelled (ทันที) | — |
| Approved | CancelRequested (รอ Manager) | — |
| Rejected / Cancelled / CancelRequested | ไม่อนุญาต | `INVALID_STATUS_TRANSITION` |

---

## 5. กฎที่ใช้ทั่วทั้งระบบ

### 5.1 String Fields

| ประเภท | กฎ |
|--------|----|
| EmployeeId, ApprovedBy, RejectedBy | MaxLength = 50 |
| FullNameTh, FullNameEn | MaxLength = 150 |
| Email | MaxLength = 150, format valid |
| Department, Position | MaxLength = 100 |
| Reason, RejectionReason | MaxLength = 500 |
| LeaveRequestRef | MaxLength = 30 |
| HalfDayPeriod | MaxLength = 2, ค่าที่ valid = "AM" หรือ "PM" เท่านั้น |

### 5.2 Decimal Fields

| Field | กฎ |
|-------|----|
| EntitledDays, UsedDays, PendingDays, CarriedForwardDays, DurationDays | ≥ 0 |
| DurationDays | ค่าต่ำสุด = 0.5 (ครึ่งวัน), ทวีคูณของ 0.5 |

### 5.3 Date Fields

| Field | ประเภท | กฎ |
|-------|--------|----|
| StartDate, EndDate | DateOnly | ห้ามเป็น default(DateOnly) |
| HireDate, AbcStartDate | DateOnly | Optional |
| CreatedAt, UpdatedAt, ApprovedAt, RejectedAt | DateTime UTC | ต้องเป็น UTC |

---

## 6. Error Code Reference

| Error Code | ความหมาย |
|------------|---------|
| `EMPLOYEE_NOT_FOUND` | ไม่พบพนักงานในระบบ |
| `INVALID_LEAVE_TYPE` | ประเภทการลาไม่ถูกต้อง |
| `INVALID_DATE_RANGE` | ช่วงวันที่ไม่ถูกต้อง |
| `INVALID_HALF_DAY` | ข้อมูลการลาครึ่งวันไม่ถูกต้อง |
| `INSUFFICIENT_BALANCE` | วันลาคงเหลือไม่เพียงพอ |
| `REQUEST_NOT_FOUND` | ไม่พบคำขอลาในระบบ |
| `UNAUTHORIZED` | ไม่มีสิทธิ์ดำเนินการ |
| `INVALID_STATUS_TRANSITION` | ไม่สามารถเปลี่ยนสถานะได้ |
| `REJECTION_REASON_REQUIRED` | ต้องระบุเหตุผลการปฏิเสธ |
