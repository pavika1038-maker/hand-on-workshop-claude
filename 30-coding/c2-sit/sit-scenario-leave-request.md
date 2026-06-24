# SIT Scenario — Leave Request and Approval

**Module:** Leave Request and Approval  
**วันที่สร้าง:** 2026-06-16  
**สร้างโดย:** AI-assisted (Claude)  
**Version:** 1.0

**Source Artifacts:**
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md`
- `20-system-design/b0-functional-design/leave-request-and-approval-sequence-diagram.md`
- `80-knowledge-base/testing/std-sit.md`
- `80-knowledge-base/testing/lesson-learned-sit-defects.md`

---

## Test Accounts

| Role | EmployeeId | ชื่อ | Manager |
|------|-----------|------|---------|
| Employee | EMP001 | สมชาย ใจดี | EMP002 |
| Manager | EMP002 | วิชัย รักงาน | — |
| HR | EMP003 | นันทา พร้อมใจ | EMP002 |

> ใช้ `X-Employee-Id` header แทน JWT (Mock Auth)

---

## Scenario Table

| Scenario ID | Scenario Name | Business Flow | Pre-condition | Steps | Expected Result | Priority |
|---|---|---|---|---|---|---|
| SIT-001 | Employee submits Annual Leave request successfully | Submit Leave Request (SF-003) | - EMP001 login สำเร็จ<br>- Annual Leave balance ≥ 3 วัน (EntitledDays=10, UsedDays=2, PendingDays=0)<br>- ไม่มีคำขอซ้อนทับใน 01-07-2026 ถึง 03-07-2026 | 1. EMP001 เข้าเมนู Leave Request<br>2. กด "ยื่นคำขอลา"<br>3. เลือกประเภท "ลาพักผ่อนประจำปี"<br>4. ระบุ Start Date: 2026-07-01, End Date: 2026-07-03<br>5. กรอกเหตุผล: "ท่องเที่ยวประจำปี"<br>6. กด Submit | **UI:** หน้าจอ redirect ไปหน้า Leave Request Detail แสดง Status = "Pending Approval"<br>**DB:** LeaveRequests.Status = "Pending", DurationDays = 3<br>**DB:** LeaveBalances.PendingDays เพิ่มจาก 0 → 3 (Remaining ลดจาก 8 → 5)<br>**Email:** Manager (EMP002) + HR (EMP003) ได้รับแจ้ง | High |
| SIT-002 | Employee submits Sick Leave request successfully | Submit Leave Request (SF-003) | - EMP001 login สำเร็จ<br>- ลาป่วยไม่เกิน 2 วัน (ไม่ต้องแนบใบแพทย์) | 1. EMP001 เข้าเมนู Leave Request → ยื่นคำขอลา<br>2. เลือกประเภท "ลาป่วย"<br>3. ระบุวันที่ 1 วัน (วันถัดไป)<br>4. กรอกเหตุผล: "ไม่สบาย"<br>5. ตรวจว่าไม่มี field แนบใบแพทย์ (≤ 2 วัน)<br>6. กด Submit | **UI:** Status = "Pending Approval"<br>**DB:** LeaveRequests สร้าง record ใหม่ Status=Pending, LeaveTypeId = Sick<br>**UI:** ไม่มี field medical_certificate แสดง (DurationDays < 3)<br>**Email:** Manager + HR ได้รับแจ้ง | High |
| SIT-003 | Employee submits Business Leave request successfully | Submit Leave Request (SF-003) | - EMP001 login สำเร็จ<br>- Business Leave balance ≥ 1 วัน (สิทธิ์ 3 วัน/ปี)<br>- Start Date ≥ 3 วันทำการนับจากวันนี้ | 1. EMP001 ยื่นคำขอลากิจ<br>2. เลือกประเภท "ลากิจส่วนตัว"<br>3. ระบุวันที่ล่วงหน้า ≥ 3 วันทำการ<br>4. กรอกเหตุผล<br>5. กด Submit | **UI:** Status = "Pending Approval"<br>**DB:** LeaveRequests สร้าง record ใหม่, DurationDays = 1<br>**DB:** LeaveBalances.PendingDays เพิ่มขึ้น | High |
| SIT-004 | Manager approves leave request and balance is deducted | Approve Leave Request (SF-005) | - ต้องทำ SIT-001 ก่อน → มีคำขอ Pending ของ EMP001<br>- EMP002 (Manager) login สำเร็จ | 1. EMP002 เข้า Approval Inbox (SCR-004)<br>2. ตรวจว่าคำขอของ EMP001 แสดงในรายการ<br>3. คลิกเลือกคำขอ → ดูรายละเอียด<br>4. กด "Approve"<br>5. ยืนยัน action | **UI:** สถานะเปลี่ยนเป็น "Approved" ทั้งในหน้า Manager และ Employee<br>**DB:** LeaveRequests.Status = "Approved", ApprovedBy = "EMP002"<br>**DB:** LeaveBalances.PendingDays ลดจาก 3 → 0, UsedDays เพิ่มจาก 2 → 5 (DEF-001)<br>**Email:** EMP001 + HR ได้รับแจ้งการอนุมัติ | High |
| SIT-005 | Manager rejects leave request with reason and Employee sees reason | Reject Leave Request (SF-005) | - มีคำขอ Pending ของ EMP001 (ทำ SIT-001 ใหม่ หรือสร้างข้อมูลเพิ่ม)<br>- EMP002 login สำเร็จ | 1. EMP002 เข้า Approval Inbox<br>2. เลือกคำขอของ EMP001<br>3. กด "Reject"<br>4. กรอกเหตุผล: "ช่วงนั้นคนน้อยเกินไป"<br>5. กด Confirm Reject<br>6. EMP001 login เข้าหน้า Leave History<br>7. ตรวจสอบว่าเห็น Rejection Reason | **UI Manager:** สถานะเปลี่ยนเป็น "Rejected"<br>**UI Employee (DEF-006):** คำขอแสดง Status = "Rejected" และ Rejection Reason = "ช่วงนั้นคนน้อยเกินไป" ชัดเจน<br>**DB:** LeaveRequests.Status = "Rejected", RejectedBy = "EMP002", RejectionReason ไม่ว่าง<br>**DB:** LeaveBalances.PendingDays คืนกลับ (ลดเท่ากับ DurationDays)<br>**Email:** EMP001 + HR ได้รับแจ้ง พร้อมเหตุผล | High |
| SIT-006 | Employee cancels Pending leave request immediately | Cancel Pending Leave (SF-007) | - มีคำขอ Status=Pending ของ EMP001<br>- EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Leave History / My Requests<br>2. เลือกคำขอที่ Status = "Pending"<br>3. กด "ยกเลิกคำขอ"<br>4. ยืนยัน Cancel | **UI:** สถานะเปลี่ยนเป็น "Cancelled" ทันที ไม่ต้องรอ Manager<br>**DB:** LeaveRequests.Status = "Cancelled" (EC-SIT-003)<br>**DB:** LeaveBalances.PendingDays คืนกลับทันที (ลดลงเท่ากับ DurationDays)<br>**Email:** ไม่มี notification (SFR-007 spec: no email required for Pending cancel) | High |
| SIT-007 | Employee requests cancel on Approved leave (creates CancelRequest) | Cancel Approved Leave (SF-008) | - มีคำขอ Status=Approved ของ EMP001 (ใช้ผลจาก SIT-004)<br>- EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Leave History<br>2. เลือกคำขอที่ Status = "Approved"<br>3. กด "ขอยกเลิก"<br>4. กรอกเหตุผล: "แผนเปลี่ยนกะทันหัน"<br>5. กด Confirm | **UI:** สถานะเปลี่ยนเป็น "Cancel Requested"<br>**DB:** LeaveRequests.Status = "CancelRequested" (DEF-004)<br>**DB:** CancelRequests มี record ใหม่ Status=Pending, LeaveRequestId ตรงกัน, SlaDeadline = UtcNow + 1 วันทำการ<br>**UI Manager:** Cancel queue ของ EMP002 มี entry ของ EMP001 แสดงอยู่<br>**Email:** EMP002 (Manager) + HR ได้รับแจ้ง | High |
| SIT-008 | Manager approves cancel request and balance is restored | Approve Cancel Request (SF-009) | - ต้องทำ SIT-007 ก่อน → มี CancelRequest Status=Pending<br>- EMP002 login สำเร็จ | 1. EMP002 เข้า Cancel Request Queue (SCR-007)<br>2. เลือก Cancel Request ของ EMP001<br>3. กด "Approve Cancel"<br>4. ยืนยัน action | **UI:** LeaveRequest สถานะเปลี่ยนเป็น "Cancelled"<br>**DB:** CancelRequests.Status = "Approved"<br>**DB:** LeaveRequests.Status = "Cancelled"<br>**DB (DEF-007):** LeaveBalances.UsedDays ลดลงเท่ากับ DurationDays, Remaining เพิ่มขึ้นกลับมา<br>**Email:** EMP001 + EMP002 + HR ได้รับแจ้ง | Medium |
| SIT-009 | Manager rejects cancel request and leave status reverts to Approved | Reject Cancel Request (SF-009) | - มี CancelRequest Status=Pending ของ EMP001 (สร้างชุดข้อมูลใหม่)<br>- EMP002 login สำเร็จ | 1. EMP002 เข้า Cancel Request Queue<br>2. เลือก Cancel Request ของ EMP001<br>3. กด "Reject Cancel"<br>4. กรอกเหตุผล<br>5. ยืนยัน | **UI (DEF-008):** LeaveRequest สถานะกลับเป็น "Approved" (ไม่ใช่ Cancelled)<br>**DB:** CancelRequests.Status = "Rejected"<br>**DB:** LeaveRequests.Status = "Approved" (restore)<br>**DB:** LeaveBalances ไม่เปลี่ยนแปลง (UsedDays คงเดิม)<br>**Email:** EMP001 + HR ได้รับแจ้ง | Medium |
| SIT-010 | HR views all leave requests in monitoring dashboard | HR Monitoring (SF-011) | - EMP001 มีคำขอลาอย่างน้อย 1 รายการ<br>- EMP003 (HR) login สำเร็จ | 1. EMP003 เข้าเมนู HR Monitoring Dashboard (SCR-008)<br>2. ตรวจว่าเห็นรายการคำขอของพนักงานทุกคน<br>3. ลองกรอง Status = "Pending"<br>4. ลองกรองตาม Department | **UI:** รายการแสดงคำขอของพนักงานทุกคนในองค์กร (ไม่จำกัดแค่ subordinate)<br>**UI:** Filter ตาม Status ทำงานถูกต้อง<br>**UI:** ข้อมูลแสดง EmployeeId, ชื่อ, ประเภทลา, วันที่, Status ครบ | Medium |
| SIT-011 | HR employee submits leave and appears in Manager queue (DEF-002) | Submit Leave + Manager Queue | - EMP003 (HR) ต้องมี ManagerId = "EMP002" ใน seed data<br>- EMP002 login สำเร็จ | 1. EMP003 ยื่นคำขอลาพักผ่อน 1 วัน<br>2. EMP002 เข้า Approval Inbox | **UI Manager:** คำขอของ EMP003 แสดงใน Approval Inbox ของ EMP002 (DEF-002)<br>**DB:** LeaveRequests.EmployeeId = "EMP003", Status = "Pending"<br>ไม่มีกรณีที่คำขอหายไปเพราะ ManagerId = null | High |
| SIT-012 | System prevents submit when Annual Leave balance is insufficient | Validation — Balance (SF-003) | - EMP001 มี Annual Leave Remaining = 1 วัน (ตั้งค่า seed data)<br>- EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Submit Leave Request<br>2. เลือกประเภท "ลาพักผ่อนประจำปี"<br>3. ระบุวันที่ 3 วัน<br>4. กด Submit | **UI (DEF-003):** ระบบแสดง error message "สิทธิ์วันลาไม่เพียงพอ คงเหลือ 1 วัน" ชัดเจนบนหน้าจอ (ไม่ใช่ spinner ค้าง)<br>**DB:** ไม่มี LeaveRequest record ใหม่ถูกสร้าง<br>**DB:** LeaveBalances.PendingDays ไม่เปลี่ยนแปลง | High |
| SIT-013 | System prevents submit when required fields are missing | Validation — Required Fields (SF-003) | - EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Submit Leave Request<br>2. ไม่เลือกประเภทการลา (ปล่อยว่าง)<br>3. กด Submit<br>4. ทดสอบซ้ำโดยเลือกประเภท แต่ไม่กรอกวันที่<br>5. ทดสอบซ้ำโดยกรอกครบแต่ไม่ใส่เหตุผล | **กรณี 1:** ระบบแสดง error "กรุณาเลือกประเภทการลา"<br>**กรณี 2:** ระบบแสดง error "กรุณาระบุวันที่"<br>**กรณี 3:** ระบบแสดง error "กรุณาระบุเหตุผล"<br>**ทุกกรณี:** ไม่มี API call ออกไป (Client-side validation ต้อง block ก่อน) | High |
| SIT-014 | System prevents reject without rejection reason | Validation — Reject Reason (SF-005) | - มีคำขอ Status=Pending ของ EMP001<br>- EMP002 login สำเร็จ | 1. EMP002 เข้า Approval Inbox<br>2. เลือกคำขอ → กด "Reject"<br>3. ปล่อย Rejection Reason ว่างเปล่า<br>4. กด Confirm | **UI:** ระบบ block การ Reject พร้อมแสดง error "กรุณาระบุเหตุผลการปฏิเสธ"<br>**DB:** LeaveRequests.Status ยังคงเป็น "Pending" ไม่เปลี่ยน<br>**API:** ถ้า bypass UI ส่ง API โดยตรง → HTTP 422 พร้อม errorCode = "REJECTION_REASON_REQUIRED" | High |
| SIT-015 | Manager cannot approve own leave request (Unauthorized) | Authorization — Self-Approve (SF-005) | - EMP002 (Manager) มีคำขอลาของตัวเองที่สถานะ Pending (EMP002 ไม่มี Manager ที่ต้องอนุมัติ)<br>- ทดสอบผ่าน API โดยตรง | 1. EMP002 login<br>2. ส่ง API POST /api/v1/leave-requests/{leaveRequestId}/approve โดยใช้ X-Employee-Id: EMP002 ในขณะที่ leaveRequestId เป็นของ EMP002 เอง<br>(หรือ EMP001 ลองอนุมัติคำขอของตัวเอง) | **API:** HTTP 403 หรือ 422 พร้อม errorCode = "FORBIDDEN"<br>**DB:** LeaveRequests.Status ไม่เปลี่ยนแปลง<br>**UI (ถ้า test ผ่าน Browser):** ปุ่ม Approve ไม่แสดงในหน้าคำขอของตัวเอง | Medium |
| SIT-016 | System prevents cancelling an already Cancelled request | Validation — Invalid Transition (SF-007, DEF-005) | - ต้องทำ SIT-006 ก่อน → มีคำขอ Status=Cancelled แล้ว<br>- EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Leave History<br>2. เลือกคำขอที่ Status = "Cancelled"<br>3. พยายามกด "ยกเลิกคำขอ" (ถ้า UI ยังแสดงปุ่ม)<br>4. ถ้า UI ซ่อนปุ่ม ให้ส่ง API โดยตรง: DELETE /api/v1/leave-requests/{id}/cancel | **UI:** ปุ่ม "ยกเลิก" ไม่แสดงสำหรับคำขอที่ Cancelled แล้ว<br>**API:** HTTP 422 พร้อม errorCode = "INVALID_STATUS"<br>**DB:** LeaveRequests.Status ไม่เปลี่ยนแปลง | Medium |
| SIT-017 | Balance is correct immediately after Manager approves (DEF-001 verification) | Balance Accuracy Post-Approve | - ต้องทำ SIT-004 ก่อน | 1. หลัง Manager approve (SIT-004)<br>2. EMP001 login ทันที<br>3. เข้าหน้า Leave Balance Dashboard (SCR-002)<br>4. Query DB: `SELECT EntitledDays, UsedDays, PendingDays FROM LeaveBalances WHERE EmployeeId='EMP001'` | **UI Employee:** หน้า Balance Dashboard แสดง Annual Leave คงเหลือ = 5 (= 10 - 5 used)<br>**UI:** UsedDays = 5 (ไม่ใช่ 2 เหมือนก่อน), PendingDays = 0<br>**DB:** ตรงกับ UI ทุก field (DEF-001) | High |
| SIT-018 | Balance is restored correctly after Manager approves cancel (DEF-007 verification) | Balance Restore Post-Cancel-Approve | - ต้องทำ SIT-008 ก่อน (Approve Cancel สำเร็จ) | 1. หลัง Manager approve cancel (SIT-008)<br>2. EMP001 login<br>3. เข้าหน้า Leave Balance Dashboard<br>4. Query DB: `SELECT UsedDays, PendingDays FROM LeaveBalances WHERE EmployeeId='EMP001'` | **UI:** Annual Leave Remaining เพิ่มขึ้นกลับมา (UsedDays ลดลง)<br>**DB:** LeaveBalances.UsedDays = 2 (กลับมาเท่าเดิมก่อน SIT-004), Remaining = 8<br>**DB:** LeaveRequests.Status = "Cancelled" (DEF-007) | Medium |
| SIT-019 | Half-day leave submit and balance deducted as 0.5 days | Half-Day Leave (SF-003) | - EMP001 มี Annual Leave balance ≥ 0.5 วัน<br>- EMP001 login สำเร็จ | 1. EMP001 เข้าหน้า Submit Leave Request<br>2. เลือก Annual Leave<br>3. ตั้ง Start Date = End Date (วันเดียวกัน)<br>4. เลือก option "ลาครึ่งวัน" (half-day toggle)<br>5. เลือก "ครึ่งวันเช้า (AM)"<br>6. กด Submit | **UI:** Field `half_day_period` แสดงเมื่อเลือก half-day, ซ่อนเมื่อยกเลิก<br>**UI:** Total Days แสดง = 0.5<br>**DB:** LeaveRequests.DurationDays = 0.5, IsHalfDay = true<br>**DB:** LeaveBalances.PendingDays เพิ่มขึ้น 0.5 | Medium |
| SIT-020 | Outsource employee cannot submit restricted leave type (VR-001) | Leave Type Restriction — Outsource | - มี Outsource Employee (EMP_OUT) ใน system<br>- EMP_OUT login สำเร็จ | 1. EMP_OUT เข้าหน้า Submit Leave Request<br>2. ตรวจว่า Dropdown ไม่แสดง "ลาคลอดบุตร", "ลาเพื่อทำหมัน", "ลารับราชการทหาร", "ลาอุปสมบท"<br>3. ถ้า bypass UI ส่ง API ด้วย leaveTypeId ของ ลาคลอดบุตร | **UI:** Dropdown ซ่อน 4 ประเภทที่ Outsource ไม่มีสิทธิ์ (BR-011)<br>**API:** HTTP 422 พร้อม errorCode = "VR-001" หรือ message แจ้ง restriction | Medium |

---

## Scenario Dependencies

```
SIT-001 (Submit Annual Leave)
  └─→ SIT-004 (Approve) ─→ SIT-017 (Verify Balance)
        └─→ SIT-007 (Cancel Approved)
              └─→ SIT-008 (Approve Cancel) ─→ SIT-018 (Verify Balance Restore)
              └─→ SIT-009 (Reject Cancel)

SIT-001 (Submit Annual Leave)
  └─→ SIT-005 (Reject) [ต้องสร้าง Pending ใหม่ถ้า SIT-001 ถูก approve แล้ว]

SIT-001 (Submit Annual Leave)
  └─→ SIT-006 (Cancel Pending)
        └─→ SIT-016 (Double-cancel guard)
```

---

## Coverage Matrix

| SRS Function | Scenario(s) ที่ครอบคลุม | Priority |
|---|---|---|
| SF-001 Login | (สมมติผ่านแล้ว — ใช้ Mock Auth) | — |
| SF-002 Leave Balance Dashboard | SIT-017, SIT-018 (ตรวจ balance post-action) | High |
| SF-003 Submit Leave Request | SIT-001, SIT-002, SIT-003, SIT-012, SIT-013, SIT-019, SIT-020 | High |
| SF-004 Manager Approval Inbox | SIT-004, SIT-011 | High |
| SF-005 Approve / Reject | SIT-004, SIT-005, SIT-014, SIT-015 | High |
| SF-006 Leave Request Status | SIT-005 (rejection reason visible), SIT-006 | High |
| SF-007 Cancel Pending Leave | SIT-006, SIT-016 | High |
| SF-008 Cancel Approved Leave | SIT-007 | High |
| SF-009 Re-approve Cancel | SIT-008, SIT-009 | Medium |
| SF-010 SLA Reminder | (ไม่อยู่ใน SIT scope — std-sit.md §7.2) | Low |
| SF-011 HR Monitoring | SIT-010, SIT-011 | Medium |

---

## Lesson Learned Coverage

| Defect | Scenario ที่ตรวจ | วิธีตรวจ |
|---|---|---|
| DEF-001: Balance ไม่ deduct หลัง Approve | SIT-004, SIT-017 | Query DB + ดู UI Balance Dashboard |
| DEF-002: Manager Queue ว่างเมื่อ ManagerId = null | SIT-011 | ให้ HR (EMP003) ยื่นลา แล้วตรวจ EMP002's queue |
| DEF-003: UI ไม่แสดง error (spinner ค้าง) | SIT-012 | ตรวจว่า error message แสดงชัดเจน ไม่ใช่ spinner |
| DEF-004: CancelRequest ไม่ถูก INSERT | SIT-007 | Query CancelRequests table + ตรวจ Manager cancel queue |
| DEF-005: Cancel ซ้ำได้ | SIT-016 | ลอง cancel คำขอที่ Cancelled แล้ว |
| DEF-006: Rejection Reason ไม่แสดงให้ Employee เห็น | SIT-005 (step 6-7) | EMP001 เข้า history → ตรวจว่าเห็น reason |
| DEF-007: Balance ไม่คืนหลัง Approve Cancel | SIT-008, SIT-018 | Query DB + ดู UI balance ก่อน/หลัง |
| DEF-008: Status ไม่กลับ Approved หลัง Reject Cancel | SIT-009 | ตรวจ LeaveRequests.Status หลัง reject cancel |
