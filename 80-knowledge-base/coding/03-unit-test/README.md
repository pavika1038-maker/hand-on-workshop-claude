# Knowledge Base — Unit Testing

> ไฟล์ index สำหรับ knowledge base ทั้งหมดใน Session 4: Unit Testing  
> อัปเดตล่าสุด: 2026-06-16

---

## ไฟล์ทั้งหมดในโฟลเดอร์นี้

| ไฟล์ | วัตถุประสงค์ | ใช้เมื่อ |
|------|------------|---------|
| `std-unit-test.md` | มาตรฐาน unit test: pattern, naming, mock guideline | เริ่มเขียน test ใหม่ทุกครั้ง |
| `rule-input-validation.md` | กฎ validation ทั้งหมดของระบบ (fields, length, business rules) | สร้าง test สำหรับ validation |
| `lesson-learned-validation-bugs.md` | Bug history + Edge cases ที่เคยพบ | เพิ่ม test ป้องกัน regression |
| `output-template.md` | Template โครงสร้าง test class ว่างๆ | เริ่ม test file ใหม่ |
| `output-sample.md` | ตัวอย่าง test จริงสำหรับ LeaveService.SubmitLeaveRequestAsync | ดูตัวอย่างก่อน implement |

---

## วิธีใช้ร่วมกับ AI

### Prompt พื้นฐาน

```
อ่านไฟล์เหล่านี้ก่อน แล้วสร้าง unit test:

1. 80-knowledge-base/coding/03-unit-test/std-unit-test.md  (มาตรฐานและ pattern)
2. 80-knowledge-base/coding/03-unit-test/rule-input-validation.md  (กฎ validation)
3. 80-knowledge-base/coding/03-unit-test/lesson-learned-validation-bugs.md  (bug ที่เคยพบ)
4. 80-knowledge-base/coding/03-unit-test/output-template.md  (โครงสร้าง output)
5. 80-knowledge-base/coding/03-unit-test/output-sample.md  (ตัวอย่าง)

แล้วสร้าง unit test สำหรับ: [ชื่อ method / service ที่ต้องการ]
```

---

## ลำดับความสำคัญในการเขียน Test

```
Priority 1 (Must) — SCR-003 Submit
  ├── Happy path: submit สำเร็จ → status = Pending
  ├── Invalid: employee ไม่มีในระบบ
  ├── Invalid: balance ไม่พอ
  ├── Invalid: วันที่ในอดีต
  └── Invalid: IsHalfDay = true แต่ HalfDayPeriod = null

Priority 1 (Must) — SCR-004 Approve/Reject
  ├── Happy path: approve → status = Approved, balance deducted
  ├── Happy path: reject → status = Rejected, reason required
  ├── Invalid: approve คำขอของตัวเอง
  ├── Invalid: approve request ที่ไม่ใช่ subordinate
  └── Invalid: approve request ที่ไม่ใช่ Pending

Priority 2 (Should) — SCR-006 Cancel
  ├── Cancel Pending → Cancelled ทันที
  └── Cancel Approved → CancelRequested

Priority 3 (Nice) — SCR-010 Report
  └── Filter by status, date range, employee
```

---

## Tech Stack ที่ใช้ใน Test

```
xUnit          — test runner
Moq            — mocking repositories / services
FluentAssertions — assertions (readable syntax)
SQLite in-memory — ทดสอบ EF Core query ที่ต้องการ transaction จริง
```

---

## โฟลเดอร์ Test Project

```
LeaveRequest/
└── tests/
    └── LeaveRequest.Application.Tests/
        ├── LeaveRequest.Application.Tests.csproj
        └── Services/
            ├── LeaveServiceTests.cs       ← SCR-003, SCR-006
            └── ApprovalServiceTests.cs   ← SCR-004, SCR-007
```
