# Prompt Part 2 ขั้นที่ 1 — Coverage Check

> ใช้สำหรับ: ตรวจสอบว่า test case ครอบคลุม method ทั้งหมดที่ design ระบุหรือยัง  
> วิธีใช้: Copy prompt ด้านล่าง → วางใน Claude session ใหม่

---

## Prompt

```text
อ่านไฟล์เหล่านี้ก่อนทำงาน:
1. method list จาก design spec: 20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md
2. test case list ที่สร้างไว้: 30-coding/c1-unit-test/test-case-list-merged.md
3. test code จริง: LeaveRequest/tests/LeaveRequest.Application.Tests/Services/LeaveRequestServiceTests.cs
4. source code: LeaveRequest/src/LeaveRequest.Application/Services/LeaveRequestService.cs

โปรดทำ Coverage Check และระบุผลลัพธ์ในรูปแบบตารางดังนี้:

---

### 1. Method Coverage — ครบทุก method ใน design หรือยัง

| Method | มีใน Design Spec | มี Test ใน TC List | มี Test Code จริง | หมายเหตุ |
|--------|-----------------|-------------------|-------------------|---------|

ให้ครอบคลุมทุก public method ของ LeaveRequestService, LeaveBalanceService, LeaveReportService

---

### 2. Test Type Coverage — แต่ละ method มีครบทุก type หรือยัง

| Method | HappyPath | ExceptionFlow | BusinessRule | NullEmpty | Boundary | หมายเหตุ |
|--------|-----------|---------------|--------------|-----------|----------|---------|

---

### 3. Test case ที่ขาดหาย (Coverage Gap)

ระบุ test case ที่ควรมีแต่ยังไม่มีใน TC list โดยแบ่งเป็น:

| Gap ID | Method | Scenario ที่ขาด | ประเภท | เหตุผลที่ควรมี |
|--------|--------|----------------|--------|--------------|

---

### 4. Test case ที่ซ้ำหรือ overlap กัน

| Test ID 1 | Test ID 2 | สิ่งที่ซ้ำกัน | แนะนำ |
|-----------|-----------|--------------|-------|

---

### 5. Method ที่ยังไม่มี test เลย

ระบุ method จาก source code ที่ไม่มีใน TC list เลย พร้อมระบุว่าควร test หรือข้ามได้ เหตุผลอะไร

---

### 6. สรุป Coverage Score (ประเมินเอง)

| หมวด | จำนวนที่ควรมี | จำนวนที่มีจริง | % coverage | สถานะ |
|------|--------------|---------------|-----------|-------|
| Method coverage | | | | |
| HappyPath coverage | | | | |
| ExceptionFlow coverage | | | | |
| BusinessRule coverage | | | | |
| KB/Boundary coverage | | | | |

---

Requirements:
- อ้างอิง Test ID จริงจาก test-case-list-merged.md (TC-SPEC-xxx หรือ TC-KB-xxx)
- ถ้า method ใดมี skip test ใน test code ให้ระบุด้วยว่า skip เพราะอะไร (TODO / not implemented / controller level)
- Gap ที่พบให้แยกชัดว่ามาจาก spec ที่ยังไม่ cover หรือมาจาก KB/programmer experience
```

---

## ไฟล์ที่ต้องอ่าน (สรุป)

| ไฟล์ | วัตถุประสงค์ |
|------|------------|
| `leave-request-and-approval-method-signature.md` | method list จาก design — ใช้เป็น baseline |
| `test-case-list-merged.md` | TC-SPEC + TC-KB ทั้ง 68 cases |
| `LeaveRequestServiceTests.cs` | test code จริงที่ generate แล้ว |
| `LeaveRequestService.cs` | source code — ตรวจ method ที่ยังไม่มี test |
