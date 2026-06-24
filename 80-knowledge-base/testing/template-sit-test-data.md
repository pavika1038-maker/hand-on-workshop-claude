# Template — SIT Test Data

> ใช้ template นี้สร้างไฟล์ `sit-test-data-{module}.md`  
> วัตถุประสงค์: กำหนด test account, leave data, และ invalid data สำหรับแต่ละ scenario  
> อ้างอิง: std-sit.md §8

---

## Header Section

```md
# SIT Test Data — {Module Name}

**Module:** {ชื่อ module}  
**วันที่สร้าง:** {YYYY-MM-DD}  
**อ้างอิง Scenario:** sit-scenario-{module}.md  
**Version:** 1.0
```

---

## Section 1: Test Accounts

```md
## Test Accounts

| Role | EmployeeId | ชื่อ | Password / Header | สิทธิ์พิเศษ |
| --- | --- | --- | --- | --- |
| Employee | EMP001 | สมชาย ใจดี | X-Employee-Id: EMP001 | ยื่นลา / ยกเลิกลาของตัวเอง |
| Manager | EMP002 | วิชัย รักงาน | X-Employee-Id: EMP002 | อนุมัติ/ปฏิเสธคำขอ subordinate |
| HR | EMP003 | นันทา พร้อมใจ | X-Employee-Id: EMP003 | ดูรายการทั้งหมด |
```

---

## Section 2: Leave Balance Setup

```md
## Leave Balance (ก่อนเริ่มทดสอบ)

| EmployeeId | LeaveTypeId | LeaveTypeName | EntitledDays | UsedDays | PendingDays | Remaining |
| --- | --- | --- | --- | --- | --- | --- |
| EMP001 | 1 | Annual Leave | 10 | 0 | 0 | 10 |
| EMP001 | 2 | Sick Leave | 30 | 0 | 0 | 30 |
| EMP001 | 3 | Business Leave | 3 | 0 | 0 | 3 |
| EMP003 | 1 | Annual Leave | 10 | 0 | 0 | 10 |
```

---

## Section 3: Test Data แยกตาม Scenario

```md
## Test Data แยกตาม Scenario

### SIT-001: Employee submits leave request successfully

| Field | Value | หมายเหตุ |
| --- | --- | --- |
| EmployeeId | EMP001 | |
| LeaveTypeId | 1 (Annual) | |
| StartDate | วันพรุ่งนี้ + 7 วัน | หลีกเลี่ยงวันหยุด |
| EndDate | StartDate + 2 วัน | รวม 3 วัน |
| IsHalfDay | false | |
| Reason | "ทดสอบ SIT-001 ยื่นคำขอลาสำเร็จ" | |
| **Expected Status** | Pending | |
| **Expected Balance** | PendingDays = 3, Remaining = 7 | |

### SIT-002: Manager approves leave request

| Field | Value |
| --- | --- |
| ApproverId (Header) | EMP002 |
| LeaveRequestId | {Id จาก SIT-001} |
| **Expected Status** | Approved |
| **Expected Balance** | UsedDays = 3, PendingDays = 0 |

### SIT-003: Manager rejects leave request with reason

| Field | Value |
| --- | --- |
| ApproverId (Header) | EMP002 |
| LeaveRequestId | {Id ที่สถานะ Pending} |
| RejectionReason | "ทดสอบ SIT-003 ปฏิเสธพร้อมเหตุผล" |
| **Expected Status** | Rejected |
| **Expected Balance** | PendingDays กลับ 0, Remaining คืน |

### SIT-004: System prevents submit when balance is insufficient

| Field | Value | หมายเหตุ |
| --- | --- | --- |
| EmployeeId | EMP001 (หลัง SIT-002 Remaining = 7) | |
| LeaveTypeId | 1 (Annual) | |
| StartDate | วันที่ future | |
| EndDate | StartDate + 7 วัน | รวม 8 วัน (เกิน 7 ที่เหลือ) |
| **Expected Error** | INSUFFICIENT_BALANCE | |
| **Expected Message** | "วันลาคงเหลือไม่เพียงพอ" | |
```

---

## Section 4: Invalid Data สำหรับ Validation Tests

```md
## Invalid Data — Validation Scenarios

| Test Case | Field | Value | Expected Error | Expected Message |
| --- | --- | --- | --- | --- |
| ไม่ระบุประเภทการลา | LeaveTypeId | 0 | INVALID_LEAVE_TYPE | ประเภทการลาไม่ถูกต้อง |
| วันที่ในอดีต | StartDate | เมื่อวาน | INVALID_DATE_RANGE | วันเริ่มลาต้องไม่น้อยกว่าวันนี้ |
| EndDate < StartDate | EndDate | StartDate - 1 | INVALID_DATE_RANGE | วันสิ้นสุดต้องไม่น้อยกว่าวันเริ่ม |
| IsHalfDay แต่ StartDate ≠ EndDate | IsHalfDay=true, StartDate≠EndDate | — | INVALID_HALF_DAY | การลาครึ่งวันต้องเป็นวันเดียวกัน |
| IsHalfDay แต่ไม่ระบุ Period | HalfDayPeriod | null | INVALID_HALF_DAY | ต้องระบุ AM หรือ PM |
| Reject โดยไม่มีเหตุผล | RejectionReason | "" | REJECTION_REASON_REQUIRED | ต้องระบุเหตุผลการปฏิเสธ |
| Manager approve ของตัวเอง | ApproverId = EmployeeId | EMP001 = EMP001 | UNAUTHORIZED | ไม่สามารถอนุมัติคำขอของตัวเองได้ |
```

---

## Section 5: State-dependent Data (ต้องทำตามลำดับ)

```md
## Dependency Chain — ลำดับการเตรียมข้อมูล

SIT-001 (Submit) → SIT-002 (Approve) → SIT-006/007 (Cancel) → SIT-008/009 (Approve/Reject Cancel)

> ⚠️ ถ้า SIT-001 fail ทุก scenario ที่ depend จะ block ด้วย — ให้ fix SIT-001 ก่อน
```
