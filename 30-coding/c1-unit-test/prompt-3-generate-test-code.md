# Prompt 3 — Generate Unit Test Code

> ใช้สำหรับ: สร้าง test code จาก test case list ที่ merge จาก prompt 2.1 และ 2.2  
> วิธีใช้: Copy prompt ด้านล่าง → วางใน Claude → แนบไฟล์ที่ระบุ

---

## ไฟล์ที่ต้องอ่านก่อนสร้าง test code

```
อ่านไฟล์เหล่านี้ทั้งหมดก่อน:

1. 80-knowledge-base/coding/03-unit-test/std-unit-test.md
2. 80-knowledge-base/coding/03-unit-test/output-template.md
3. 80-knowledge-base/coding/03-unit-test/output-sample.md
4. LeaveRequest/src/LeaveRequest.Application/Services/LeaveRequestService.cs
5. LeaveRequest/src/LeaveRequest.Application/DTOs/LeaveRequestDtos.cs
6. LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveRequest.cs
7. LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveBalance.cs
```

---

## Prompt

```text
อ่านไฟล์เหล่านี้ก่อนทำงาน:
1. unit test standard + naming convention: 80-knowledge-base/coding/03-unit-test/std-unit-test.md
2. test class template: 80-knowledge-base/coding/03-unit-test/output-template.md
3. ตัวอย่าง test จริง: 80-knowledge-base/coding/03-unit-test/output-sample.md
4. source code ที่ต้อง test:
   - LeaveRequest/src/LeaveRequest.Application/Services/LeaveRequestService.cs
   - LeaveRequest/src/LeaveRequest.Application/DTOs/LeaveRequestDtos.cs
   - LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveRequest.cs
   - LeaveRequest/src/LeaveRequest.Domain/Entities/LeaveBalance.cs

จากนั้นสร้าง unit test code สำหรับ LeaveRequestService โดยใช้ test case list ใน:
30-coding/c1-unit-test/test-case-list-merged.md

Requirements:
- Framework: xUnit + Moq + FluentAssertions + SQLite in-memory (ห้ามใช้ EF InMemory)
- Test class ชื่อ: LeaveRequestServiceTests
- System Under Test: private readonly LeaveRequestService _sut
- ใช้ Arrange-Act-Assert pattern เสมอ แยกด้วย blank line และ comment
- Naming convention: {MethodName}_Should_{ExpectedBehavior}_When_{Condition}
- Method ที่ต้องมี test:
  * SubmitLeaveRequestAsync() — TC-SPEC-001~xxx และ TC-KB-001~xxx ที่เกี่ยวข้อง
  * CancelAsync() — TC-SPEC-xxx~xxx และ TC-KB-xxx~xxx ที่เกี่ยวข้อง
  * ApproveAsync() — TC-SPEC-xxx~xxx และ TC-KB-xxx~xxx ที่เกี่ยวข้อง
  * RejectAsync() — TC-SPEC-xxx~xxx และ TC-KB-xxx~xxx ที่เกี่ยวข้อง

- Mock dependencies:
  * ILeaveRequestRepository
  * ILeaveBalanceRepository
  * IEmployeeRepository
  * ILogger<LeaveRequestService>
  * AppDbContext (ใช้ SQLite in-memory จริง)

- สำหรับ test ที่เกี่ยวกับ balance/transaction ให้ใช้ SQLite in-memory db แทน mock
- ใส่ comment บน test method บอก Test ID และ Source เช่น:
  // TC-SPEC-001 | Source: SRS SCR-003 Main Flow
  // TC-KB-001  | Source: rule-input-validation.md → 1.1 EmployeeId

- ถ้า test case ใดยังมี open question (เช่น reason optional vs required) ให้ใส่ [TODO: resolve open question] ไว้ใน comment แทนการเดา

Output:
- ไฟล์เดียว: tests/LeaveRequest.Application.Tests/Services/LeaveRequestServiceTests.cs
- ครบถ้วนสมบูรณ์ รัน dotnet test ได้ทันที
- แต่ละ test ต้องรันได้อิสระจากกัน (no shared state)
```

---

## หมายเหตุก่อนรัน prompt นี้

ต้อง resolve สิ่งเหล่านี้ก่อน:

| รายการ | สถานะ |
|--------|-------|
| Fix `RejectAsync()` — เพิ่ม null/whitespace guard สำหรับ RejectionReason | ✅ Done |
| Fix `ApproveAsync()` — เพิ่ม null guard สำหรับ managerId | ✅ Done |
| Fix `SubmitLeaveRequestAsync()` — เพิ่ม HalfDayPeriod enum validation | ✅ Done |
| Resolve open question: Reason field optional (SRS §2.4.9) vs required (Method Sig §4.5) | ✅ required (Method Sig §4.5) |
| Merge test case list จาก TC-SPEC และ TC-KB เป็นตารางเดียว | ✅ Done → test-case-list-merged.md (68 cases) |

ถ้า fix code และ merge test case ก่อนจะได้ test code ที่ถูกต้องตั้งแต่รอบแรก — ไม่ต้องแก้ซ้ำหลายรอบ
