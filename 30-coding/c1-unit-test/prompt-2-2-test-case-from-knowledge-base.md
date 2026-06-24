# Prompt 2.2 — Unit Test Case จาก Knowledge Base & Programmer Experience

> ใช้สำหรับ: สร้าง test case ที่ design spec ไม่ได้ระบุไว้โดยตรง  
> วิธีใช้: Copy prompt ด้านล่าง → วางใน Claude → แนบไฟล์ที่ระบุ

---

## ไฟล์ที่ต้องอ่านก่อนสร้าง test case

```
อ่านไฟล์เหล่านี้ทั้งหมดก่อน:

1. 80-knowledge-base/coding/03-unit-test/std-unit-test.md
2. 80-knowledge-base/coding/03-unit-test/rule-input-validation.md
3. 80-knowledge-base/coding/03-unit-test/lesson-learned-validation-bugs.md
4. LeaveRequest/src/LeaveRequest.Application/Services/LeaveService.cs
5. LeaveRequest/src/LeaveRequest.Application/Services/ApprovalService.cs
6. LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveRequest.cs
7. LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveBalance.cs
```

---

## Prompt

```text
อ่านเอกสารเหล่านี้ก่อนทำงาน:
1. unit test standard: 80-knowledge-base/coding/03-unit-test/std-unit-test.md
2. validation/input rule: 80-knowledge-base/coding/03-unit-test/rule-input-validation.md
3. lesson learned & bug history: 80-knowledge-base/coding/03-unit-test/lesson-learned-validation-bugs.md
4. source code:
   - LeaveRequest/src/LeaveRequest.Application/Services/LeaveService.cs
   - LeaveRequest/src/LeaveRequest.Application/Services/ApprovalService.cs
   - LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveRequest.cs
   - LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveBalance.cs

จากนั้นสร้าง unit test case ที่ไม่ได้มาจาก design spec โดยตรง แต่ควรมีตามมาตรฐานองค์กรและประสบการณ์จริง สำหรับ method ต่อไปนี้:
- LeaveService.SubmitLeaveRequestAsync()
- LeaveService.CancelLeaveRequestAsync()
- ApprovalService.ApproveLeaveRequestAsync()
- ApprovalService.RejectLeaveRequestAsync()

Output เป็นตารางรูปแบบนี้:

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|

กฎของ Output:
- Test ID รูปแบบ: TC-KB-001, TC-KB-002, ...
- Test Type ใช้ค่า: NullEmpty / MaxLength / Boundary / WhiteSpace / EnumValidation / DataType / RegressionBug
- Source ต้องระบุอย่างใดอย่างหนึ่ง:
  - "rule-input-validation.md → {section}" เช่น "rule-input-validation.md → 1.3 StartDate"
  - "lesson-learned-validation-bugs.md → BUG-{number}" เช่น "lesson-learned-validation-bugs.md → BUG-001"
  - "std-unit-test.md → Programmer Experience: {เหตุผลสั้นๆ}"
- ต้องครอบคลุมประเภทต่อไปนี้เมื่อเกี่ยวข้อง:
  - null / empty / whitespace input (string fields ทุกตัว)
  - max length boundary (field ที่มี MaxLength constraint)
  - decimal boundary (DurationDays = 0, 0.5, ค่าติดลบ)
  - enum/valid value (HalfDayPeriod ต้องเป็น AM หรือ PM เท่านั้น)
  - date boundary (StartDate = วันนี้, StartDate = เมื่อวาน)
  - regression ตาม bug history ใน lesson-learned
- ไม่ต้องสร้าง test case ที่ซ้ำกับ design spec — แนวทางนี้เพิ่มเติมในส่วนที่ spec ไม่ครอบคลุม
```

---

## ตัวอย่าง Output ที่ถูกต้อง

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-KB-001 | SubmitLeaveRequestAsync | EmployeeId เป็น whitespace | EmployeeId="   " | ErrorCode=EMPLOYEE_NOT_FOUND | WhiteSpace | rule-input-validation.md → 1.1 EmployeeId |
| TC-KB-002 | SubmitLeaveRequestAsync | StartDate = วันนี้ (boundary) | StartDate=Today | ผ่าน validation (boundary ที่ยอมรับ) | Boundary | rule-input-validation.md → 1.3 StartDate |
| TC-KB-003 | SubmitLeaveRequestAsync | StartDate = เมื่อวาน | StartDate=Yesterday | ErrorCode=INVALID_DATE_RANGE | Boundary | rule-input-validation.md → 1.3 StartDate |
| TC-KB-004 | SubmitLeaveRequestAsync | HalfDayPeriod = "MORNING" | IsHalfDay=true, HalfDayPeriod="MORNING" | ErrorCode=INVALID_HALF_DAY | EnumValidation | rule-input-validation.md → 1.5 IsHalfDay |
| TC-KB-005 | SubmitLeaveRequestAsync | RejectionReason ยาว 501 ตัว | RejectionReason=string(501) | Validation error MaxLength | MaxLength | rule-input-validation.md → 3.1 RejectionReason |
| TC-KB-006 | ApproveLeaveRequestAsync | Employee มี ManagerId=null | ApproverId=EMP002, EmployeeId=EMP999 (ไม่มี Manager) | ErrorCode=UNAUTHORIZED | RegressionBug | lesson-learned-validation-bugs.md → BUG-001 |
| TC-KB-007 | RejectLeaveRequestAsync | RejectionReason เป็น whitespace | RejectionReason="   " | ErrorCode=REJECTION_REASON_REQUIRED | WhiteSpace | rule-input-validation.md → 3.1 RejectionReason |

---

## หมายเหตุ

- ผลลัพธ์จาก prompt นี้ใช้รวมกับผลลัพธ์จาก **prompt-2-1** ก่อนนำไปเขียน test code
- ถ้าไม่แน่ใจว่า test case ควรอยู่ใน 2.1 หรือ 2.2 ให้ถามว่า "spec ระบุ behavior นี้ไว้ชัดเจนไหม?" — ถ้าใช่ = 2.1, ถ้าไม่ = 2.2
