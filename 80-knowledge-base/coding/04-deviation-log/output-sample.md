# Deviation Log Sample — SubmitLeaveRequest (SCR-001)

> ตัวอย่าง deviation log หลัง generate code สำหรับ use case "ยื่นคำร้องขอลา"

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Module | Leave Request Submission |
| Function ID | SCR-001 |
| Design Source | leave-request-and-approval-method-signature.md, leave-request-and-approval-sequence-diagram.md |
| Code File(s) | LeaveService.cs, LeaveRequestRepository.cs, LeaveRequestsController.cs |
| Review Date | 2026-06-17 |
| Reviewed By | AI-generated, reviewed by team |

---

## 2. Deviation Log

| # | ประเภท | Design Spec | Actual Code | เหตุผล | การตัดสินใจ |
|---|--------|-------------|-------------|--------|------------|
| 1 | Extra | ไม่ได้ระบุ `GetOverlappingAsync` ใน method signature | เพิ่ม method นี้ใน `ILeaveRequestRepository` | จำเป็นสำหรับตรวจ VR-003 (ใบลาซ้ำซ้อน) | Accept |
| 2 | Logic | VR-002 check อยู่ใน sequence ก่อน BeginTransaction | implement ก่อน `BeginTransactionAsync()` เช่นกัน | Fail fast ก่อน open transaction ประหยัด resource | Accept |
| 3 | Method | `SubmitLeaveRequestAsync` return `LeaveRequestResponse` | reload entity หลัง save เพื่อ populate navigation props | EF Core ไม่ return navigation property หลัง AddAsync | Accept |
| 4 | Missing | Sequence diagram ระบุ publish event ไปยัง Azure Service Bus หลัง approve | ยังไม่ implement ใน phase นี้ | อยู่นอก scope ของ SCR-001 (เป็น async notification) | Defer → Sprint 2 |
| 5 | Structure | method `MapToResponse` ไม่ได้ระบุใน design | เพิ่มเป็น private static method ใน LeaveService | จำเป็นสำหรับ mapping entity → DTO ตาม coding standard | Accept |

---

## 3. Action Required

| # | Action | Owner | Due |
|---|--------|-------|-----|
| 1 | อัปเดต `ILeaveRequestRepository` interface ใน method-signature.md ให้ระบุ `GetOverlappingAsync` | Designer | Sprint 1 |
| 2 | สร้าง NotificationService + Service Bus integration สำหรับ item #4 | Dev | Sprint 2 |

---

## 4. Summary

- Deviation ทั้งหมด: 5 รายการ
- Accept (ยอมรับ): 4
- Fix (แก้ code): 0
- Defer (รอ Sprint ถัดไป): 1
- ต้องอัปเดต Design ไหม: **Yes** → `leave-request-and-approval-method-signature.md` (เพิ่ม `GetOverlappingAsync`)
