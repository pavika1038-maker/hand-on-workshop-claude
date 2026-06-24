# Prompt 2.1 — Unit Test Case ที่ Derive จาก Design Spec

> ใช้สำหรับ: สร้าง test case ที่อ้างอิง design spec โดยตรง  
> วิธีใช้: Copy prompt ด้านล่าง → วางใน Claude → แนบไฟล์ที่ระบุ

---

## ไฟล์ที่ต้องอ่านก่อนสร้าง test case

```
อ่านไฟล์เหล่านี้ทั้งหมดก่อน:

1. 20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md
2. 20-system-design/b0-functional-design/leave-request-and-approval-sequence-diagram.md
3. 10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md
4. LeaveRequest/src/LeaveRequest.Application/Services/LeaveService.cs
5. LeaveRequest/src/LeaveRequest.Application/Services/ApprovalService.cs
```

---

## Prompt

```text
อ่านเอกสารเหล่านี้ก่อนทำงาน:
1. method signature: 20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md
2. sequence diagram: 20-system-design/b0-functional-design/leave-request-and-approval-sequence-diagram.md
3. SRS screen: 10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md
4. source code:
   - LeaveRequest/src/LeaveRequest.Application/Services/LeaveService.cs
   - LeaveRequest/src/LeaveRequest.Application/Services/ApprovalService.cs

จากนั้นสร้าง unit test case ที่ derive จาก design spec โดยตรง สำหรับ method ต่อไปนี้:
- LeaveService.SubmitLeaveRequestAsync()
- LeaveService.CancelLeaveRequestAsync()
- ApprovalService.ApproveLeaveRequestAsync()
- ApprovalService.RejectLeaveRequestAsync()

Output เป็นตารางรูปแบบนี้:

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source (Spec Section) |
|---------|--------|----------|-------|-----------------|-----------|----------------------|

กฎของ Output:
- Test ID รูปแบบ: TC-SPEC-001, TC-SPEC-002, ...
- Test Type ใช้ค่า: HappyPath / AlternativeFlow / ExceptionFlow / BusinessRule
- Source ต้องระบุว่ามาจาก section ใดของ design spec เสมอ เช่น "SRS SCR-003 VR-001" หรือ "Sequence Diagram step 4"
- ครอบคลุม happy path ของทุก method
- ครอบคลุม exception ทุกข้อที่ design spec ระบุไว้
- ครอบคลุม business rule validation ตาม method signature
- ไม่ต้องสร้าง test case ที่ไม่มีอ้างอิงใน spec — แนวทางนี้เน้นเฉพาะสิ่งที่ design บอกไว้ชัดเจน
```

---

## ตัวอย่าง Output ที่ถูกต้อง

| Test ID | Method | Scenario | Input | Expected Output | Test Type | Source |
|---------|--------|----------|-------|-----------------|-----------|--------|
| TC-SPEC-001 | SubmitLeaveRequestAsync | ยื่นคำขอลาปกติ | EmployeeId=EMP001, LeaveTypeId=1, StartDate=วันนี้+1, EndDate=วันนี้+2, IsHalfDay=false | Status=Pending, LeaveRequestRef มีค่า | HappyPath | SRS SCR-003 Main Flow |
| TC-SPEC-002 | SubmitLeaveRequestAsync | วันลาคงเหลือไม่เพียงพอ | RemainingDays=1, DurationDays=3 | ErrorCode=INSUFFICIENT_BALANCE | ExceptionFlow | SRS SCR-003 VR-003 |
| TC-SPEC-003 | ApproveLeaveRequestAsync | Manager อนุมัติสำเร็จ | RequestId=valid, ApproverId=EMP002 (เป็น Manager ของ requester) | Status=Approved, ApprovedBy=EMP002 | HappyPath | Sequence Diagram SCR-004 step 3 |

---

## หมายเหตุ

- ผลลัพธ์จาก prompt นี้ใช้รวมกับผลลัพธ์จาก **prompt-2-2** ก่อนนำไปเขียน test code
- ถ้า design spec ไม่ระบุ behavior บางอย่างชัดเจน ให้ข้ามไป — อย่าเดา
