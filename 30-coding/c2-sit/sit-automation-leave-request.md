# SIT Browser Automation Draft — Leave Request and Approval

**Module:** Leave Request and Approval  
**วันที่สร้าง:** 2026-06-16  
**อ้างอิง Scenario:** `30-coding/c2-sit/sit-scenario-leave-request.md`  
**อ้างอิง Test Data:** `30-coding/c2-sit/sit-test-data-leave-request.yaml`  
**Version:** 1.0

> **Draft Note:** Automation นี้เป็น draft สำหรับ Antigravity Browser Integration  
> Element selector ที่ระบุอิงจาก SRS field label และ button text — ต้องยืนยันกับ dev ก่อน execute จริง

---

## Pre-execution Checklist

ตรวจสอบทุกข้อก่อนเริ่มรัน automation:

- [ ] Application ขึ้นที่ `http://localhost:5000` และ respond ปกติ
- [ ] SQLite DB seed data พร้อม: EMP001, EMP002, EMP003 ใน Employees table
- [ ] EMP001.ManagerId = EMP002, EMP003.ManagerId = EMP002
- [ ] LeaveBalances seed: EMP001 Annual remaining=10, Sick=30, Business=3
- [ ] LeaveTypes seed: id=1 Annual (notice 1 วัน), id=2 Sick, id=3 Personal (notice 3 วัน)
- [ ] ตรวจ holiday calendar: 2026-07-01 ถึง 2026-07-03 เป็นวันทำการ
- [ ] Mock Auth active: ระบบรับ `X-Employee-Id` header หรือ mock login form พร้อมใช้
- [ ] NotificationLogs table ว่างเปล่า (reset ก่อนรัน เพื่อตรวจ notification แม่นยำ)

---

## SIT-001 — Employee submits leave request successfully

**Business Flow:** SF-003 Submit Leave Request — Happy Path  
**Priority:** High  
**Dependency:** ไม่มี (รันได้อิสระ แต่ต้องรันก่อน SIT-002)  
**SRS Trace:** SFR-003, VR-001, VR-002, VR-003, VR-005, BR-003

### Pre-condition Setup

- [ ] Identity = EMP001 (สมชาย ใจดี) ผ่าน Mock Auth
- [ ] Annual Leave remaining = 10 วัน (ตาม initial seed)
- [ ] ไม่มีคำขอลาของ EMP001 ซ้อนทับช่วง 2026-07-01 ถึง 2026-07-03

### Automation Steps

| Step | Action | Target / Path | Input / Value | Checkpoint |
|------|--------|---------------|---------------|------------|
| 1 | `navigate` | `http://localhost:5000` | — | หน้าโหลดสำเร็จ ไม่มี 404/500 |
| 2 | `screenshot` | — | — | 📸 **EVD-001** หน้าแรกก่อน login |
| 3 | `fill` | ช่อง "ชื่อผู้ใช้ / Username" | `EMP001` | ค่าปรากฏใน field |
| 4 | `click` | ปุ่ม "Login" หรือ "เข้าสู่ระบบ" | — | — |
| 5 | `wait` | หน้า Leave Balance Dashboard (SCR-002) | max 5s | URL เปลี่ยน หรือ heading แสดง |
| 6 | `assert` | heading หน้า หรือ breadcrumb | contains "สิทธิ์วันลา" หรือ "Leave Balance" | ✅ Login สำเร็จ เข้าหน้า Dashboard |
| 7 | `assert` | card "ลาพักผ่อนประจำปี" | remaining = 10 | ✅ Balance initial ถูกต้อง (ก่อน submit) |
| 8 | `screenshot` | — | — | 📸 **EVD-002** Balance Dashboard ก่อน submit (Annual=10) |
| 9 | `click` | ปุ่ม "ยื่นคำขอลา" | — | — |
| 10 | `wait` | หน้า Submit Leave Request (SCR-003) | max 3s | Form แสดง |
| 11 | `assert` | form heading | contains "ยื่นคำขอลา" หรือ "Leave Request" | ✅ หน้า form แสดงถูก |
| 12 | `select` | dropdown "ประเภทการลา / Leave Type" | "ลาพักผ่อนประจำปี" | ค่าถูก set ใน dropdown |
| 13 | `assert` | hint หรือ label ใต้ dropdown | contains "คงเหลือ 10 วัน" หรือ "10 days remaining" | ✅ balance hint แสดงถูก |
| 14 | `assert` | field "medical_certificate" / "ใบรับรองแพทย์" | ไม่แสดง (hidden) | ✅ Annual Leave ไม่บังคับแนบเอกสาร |
| 15 | `fill` | field "วันที่เริ่มลา / Start Date" | `2026-07-01` | วันที่ปรากฏใน input |
| 16 | `fill` | field "วันที่สิ้นสุดลา / End Date" | `2026-07-03` | วันที่ปรากฏใน input |
| 17 | `assert` | field "จำนวนวัน / Total Days" (read-only) | `3` | ✅ คำนวณอัตโนมัติถูกต้อง |
| 18 | `assert` | field "ช่วงเวลาลาครึ่งวัน / Half-Day Period" | ไม่แสดง (hidden) | ✅ start_date ≠ end_date ไม่แสดง half-day |
| 19 | `fill` | field "เหตุผลการลา / Reason" (textarea) | `ท่องเที่ยวประจำปีกับครอบครัว` | ข้อความปรากฏใน textarea |
| 20 | `screenshot` | — | — | 📸 **EVD-003** Form ครบ ก่อนกด Submit |
| 21 | `click` | ปุ่ม "Submit" หรือ "ยื่นคำขอ" | — | — |
| 22 | `wait` | redirect ไปหน้า Leave Request Detail (SCR-005) | max 5s | URL เปลี่ยน หรือ element ใหม่แสดง |
| 23 | `assert` | Status badge | "Pending Approval" หรือ "รอการอนุมัติ" | ✅ Status ถูกต้อง |
| 24 | `assert` | ประเภทการลาที่แสดง | "ลาพักผ่อนประจำปี" | ✅ Leave Type ถูกบันทึก |
| 25 | `assert` | ช่วงวันที่ที่แสดง | contains "1" และ "3 ก.ค." หรือ "Jul" | ✅ วันที่ถูกบันทึก |
| 26 | `assert` | จำนวนวันที่แสดง | "3 วัน" หรือ "3 days" | ✅ DurationDays ถูก |
| 27 | `assert` | เลขคำขอ / Request No. | pattern "LR-2026-xxxxx" แสดง | ✅ LeaveRequestRef สร้างแล้ว |
| 28 | `screenshot` | — | — | 📸 **EVD-004** หน้า Detail Status=Pending (บันทึก Request Ref) |
| 29 | `navigate` | กลับหน้า Leave Balance Dashboard | คลิก "กลับ" หรือ navigate ไป SCR-002 | — |
| 30 | `assert` | card "ลาพักผ่อนประจำปี" — Pending | `3` | ✅ PendingDays เพิ่มขึ้น (DEF-001 pre-check) |
| 31 | `assert` | card "ลาพักผ่อนประจำปี" — Remaining | `7` | ✅ Remaining ลดลง = 10 - 3 |
| 32 | `screenshot` | — | — | 📸 **EVD-005** Balance Dashboard หลัง submit (Pending=3, Remaining=7) |

### Evidence Summary — SIT-001

| Evidence ID | Step | สิ่งที่ capture | วัตถุประสงค์ |
|-------------|------|----------------|-------------|
| EVD-001 | 2 | หน้าแรกก่อน login | baseline |
| EVD-002 | 8 | Balance ก่อน submit (Annual=10) | เปรียบเทียบก่อน/หลัง |
| EVD-003 | 20 | Form ครบก่อนกด Submit | ยืนยัน input |
| EVD-004 | 28 | Detail page Status=Pending + Request Ref | ผลลัพธ์หลัก |
| EVD-005 | 32 | Balance หลัง submit (Pending=3, Remaining=7) | DEF-001 pre-check |

### Expected Final State — SIT-001

- URL: `http://localhost:5000/leave-balance` หรือ `/leave-requests` (SCR-002)
- UI: card Annual Leave แสดง PendingDays=3, Remaining=7
- DB Check (optional):
  ```sql
  SELECT Status, DurationDays, LeaveTypeId, CreatedAt
  FROM LeaveRequests
  WHERE EmployeeId = 'EMP001'
  ORDER BY CreatedAt DESC LIMIT 1;
  -- Expected: Status='Pending', DurationDays=3.0, LeaveTypeId=1

  SELECT UsedDays, PendingDays
  FROM LeaveBalances
  WHERE EmployeeId = 'EMP001' AND LeaveTypeId = 1;
  -- Expected: UsedDays=2, PendingDays=3
  ```

---

## SIT-002 — Manager approves leave request

**Business Flow:** SF-004 Manager Approval Inbox + SF-005 Approve Action  
**Priority:** High  
**Dependency:** ต้องรัน **SIT-001 ผ่านก่อน** — ต้องมี LeaveRequest Status=Pending ของ EMP001 อยู่ใน DB  
**SRS Trace:** SFR-004, SFR-005, VR-010, BR-012, DEF-001 check

### Pre-condition Setup

- [ ] SIT-001 ผ่านแล้ว — มี LeaveRequest (Status=Pending, EmployeeId=EMP001) ใน DB
- [ ] Identity = EMP002 (วิชัย รักงาน) ผ่าน Mock Auth
- [ ] EMP001.ManagerId = EMP002 (คำขอจึงปรากฏใน Inbox ของ EMP002)
- [ ] บันทึก LeaveRequestId จาก EVD-004 ของ SIT-001 ไว้ก่อน

### Automation Steps — Phase A: Manager Approves

| Step | Action | Target / Path | Input / Value | Checkpoint |
|------|--------|---------------|---------------|------------|
| 1 | `navigate` | `http://localhost:5000` | — | หน้าโหลดสำเร็จ |
| 2 | `fill` | ช่อง "ชื่อผู้ใช้ / Username" | `EMP002` | ค่าปรากฏใน field |
| 3 | `click` | ปุ่ม "Login" หรือ "เข้าสู่ระบบ" | — | — |
| 4 | `wait` | Dashboard โหลด | max 5s | Login สำเร็จ |
| 5 | `assert` | role indicator หรือ menu | contains "Approval" หรือ "อนุมัติ" | ✅ Login เป็น Manager สำเร็จ |
| 6 | `navigate` | Approval Inbox (SCR-004) | คลิก menu "Approval Inbox" หรือ "รออนุมัติ" | — |
| 7 | `wait` | รายการ Approval Inbox โหลด | max 5s | — |
| 8 | `assert` | Tab ที่ active | "รอดำเนินการ / Pending" | ✅ default tab ถูก |
| 9 | `assert` | รายการใน Inbox | contains "สมชาย ใจดี" | ✅ คำขอของ EMP001 ปรากฏ (DEF-002 check) |
| 10 | `assert` | รายการของ EMP001 — ประเภทลา | "ลาพักผ่อนประจำปี" | ✅ leave type ถูก |
| 11 | `assert` | รายการของ EMP001 — วันที่ | contains "1 ก.ค." หรือ "Jul 1" | ✅ วันที่ถูก |
| 12 | `assert` | รายการของ EMP001 — สิทธิ์คงเหลือ | "7 วัน" หรือ "7 days" | ✅ balance แสดงถูก (pending หักแล้ว) |
| 13 | `screenshot` | — | — | 📸 **EVD-006** Approval Inbox ก่อน Approve (เห็น request ของ EMP001) |
| 14 | `click` | ปุ่ม "Approve" บน row ของ EMP001 | — | — |
| 15 | `wait` | การเปลี่ยน Status หรือ dialog confirm | max 3s | — |
| 16 | `assert` | Status ของ request EMP001 | "Approved" หรือ "อนุมัติแล้ว" | ✅ Status เปลี่ยนแล้ว |
| 17 | `screenshot` | — | — | 📸 **EVD-007** Approval Inbox หลัง Approve (Status=Approved) |

### Automation Steps — Phase B: ตรวจ Balance ฝั่ง Employee (DEF-001 Check)

> ⚠️ **DEF-001 Critical Check:** Balance ต้องอัปเดตทันทีหลัง Approve — ต้อง switch เป็น EMP001

| Step | Action | Target / Path | Input / Value | Checkpoint |
|------|--------|---------------|---------------|------------|
| 18 | `navigate` | `http://localhost:5000` (logout แล้ว login ใหม่) | — | หน้า Login |
| 19 | `fill` | ช่อง Username | `EMP001` | — |
| 20 | `click` | ปุ่ม "Login" | — | — |
| 21 | `wait` | หน้า Leave Balance Dashboard | max 5s | — |
| 22 | `assert` | card "ลาพักผ่อนประจำปี" — Used | `5` (UsedDays เพิ่มจาก 2 → 5) | ✅ **DEF-001**: balance deducted ถูกต้อง |
| 23 | `assert` | card "ลาพักผ่อนประจำปี" — Pending | `0` (PendingDays ลดจาก 3 → 0) | ✅ PendingDays cleared |
| 24 | `assert` | card "ลาพักผ่อนประจำปี" — Remaining | `7` (= 12 - 5 - 0) | ✅ Remaining ถูกต้อง |
| 25 | `screenshot` | — | — | 📸 **EVD-008** Balance ฝั่ง Employee หลัง Approve (Used=5, Pending=0, Remaining=7) |
| 26 | `navigate` | หน้า Leave Request History หรือ My Requests | — | รายการคำขอของ EMP001 |
| 27 | `assert` | Status ของคำขอล่าสุด | "Approved" หรือ "อนุมัติแล้ว" | ✅ EMP001 เห็น Status=Approved |
| 28 | `screenshot` | — | — | 📸 **EVD-009** Leave History ฝั่ง Employee (Status=Approved) |

### Automation Steps — Phase C: ตรวจ Notification (Optional)

| Step | Action | Target / Path | Input / Value | Checkpoint |
|------|--------|---------------|---------------|------------|
| 29 | `navigate` | `/api/v1/notifications` หรือ DB query | — | — |
| 30 | `assert` | NotificationLogs | มี event "Approved" ส่งถึง EMP001 | ✅ Employee ได้รับ notification |
| 31 | `assert` | NotificationLogs | มี event "Approved" ส่งถึง EMP003 (HR) | ✅ HR ได้รับ notification |

### Evidence Summary — SIT-002

| Evidence ID | Step | สิ่งที่ capture | วัตถุประสงค์ |
|-------------|------|----------------|-------------|
| EVD-006 | 13 | Approval Inbox ก่อน Approve (เห็น EMP001 request) | DEF-002 check + baseline |
| EVD-007 | 17 | Manager view หลัง Approve (Status=Approved) | ผลลัพธ์ Manager side |
| EVD-008 | 25 | Balance ฝั่ง Employee (Used=5, Pending=0, Remaining=7) | **DEF-001 critical check** |
| EVD-009 | 28 | Leave History EMP001 Status=Approved | Employee view ถูก |

### Expected Final State — SIT-002

- UI (Manager): คำขอของ EMP001 ย้ายจาก Tab "Pending" ไป Tab "Processed"
- UI (Employee): Leave History แสดง Status="Approved"
- UI (Employee): Balance Dashboard: UsedDays=5, PendingDays=0, Remaining=7
- DB Check (optional):
  ```sql
  SELECT Status, ApprovedBy, ApprovalDate
  FROM LeaveRequests
  WHERE EmployeeId = 'EMP001' AND Status = 'Approved';
  -- Expected: Status='Approved', ApprovedBy='EMP002', ApprovalDate NOT NULL

  SELECT UsedDays, PendingDays
  FROM LeaveBalances
  WHERE EmployeeId = 'EMP001' AND LeaveTypeId = 1;
  -- Expected: UsedDays=5, PendingDays=0
  ```

---

## Evidence Folder Structure

```
30-coding/c2-sit/evidence/
├── SIT-001/
│   ├── EVD-001_homepage-before-login.png
│   ├── EVD-002_balance-before-submit_annual-10.png
│   ├── EVD-003_form-complete-before-submit.png
│   ├── EVD-004_detail-status-pending_request-ref.png
│   └── EVD-005_balance-after-submit_pending-3-remaining-7.png
└── SIT-002/
    ├── EVD-006_approval-inbox-before-approve.png
    ├── EVD-007_approval-inbox-after-approve_status-approved.png
    ├── EVD-008_employee-balance-after-approve_used-5-pending-0.png
    └── EVD-009_employee-history-status-approved.png
```

> **ชื่อไฟล์:** ตาม std-sit.md §5.2 — `evidence-{scenario-id}-{step}-{pass|fail}.png`

---

## Global Assumptions

| # | Assumption | Impact ถ้าผิด |
|---|-----------|--------------|
| A1 | Mock Auth ทำงานผ่าน login form ที่รับ EmployeeId โดยตรง (ไม่ใช่ JWT จริง) | ต้องปรับ step 3–4 ถ้า auth mechanism ต่างออกไป |
| A2 | URL base = `http://localhost:5000` | ปรับ URL ถ้า port หรือ host ต่างกัน |
| A3 | Approval Inbox เข้าได้จาก menu navigation (ไม่ต้องพิมพ์ URL ตรง) | ปรับ step 6 ถ้า URL ตรงพร้อมใช้ |
| A4 | ปุ่ม "Approve" แสดงบน row ของ Inbox โดยตรง (ไม่ต้องคลิกเข้าหน้า Detail ก่อน) | ถ้าต้อง navigate ไป Detail ก่อน ให้เพิ่ม step: click row → assert detail → click Approve |
| A5 | Balance card แสดง UsedDays, PendingDays, Remaining แยกชัดเจน | ถ้า UI merge หรือ label ต่าง ให้ปรับ assert value |
| A6 | "Total Days" คำนวณเฉพาะวันทำการ — holiday calendar seed พร้อม | ถ้า 2026-07-01~03 มีวันหยุด Total Days จะ < 3 |
| A7 | Logout/switch identity ทำได้โดยไปหน้า Login ใหม่ (session ใหม่) | ถ้ามี session persist ต้อง explicit logout ก่อน |

---

## Dependency Notes

```
SIT-001 (Submit) ── ต้องผ่านก่อน ──▶ SIT-002 (Approve)

ถ้า SIT-001 fail:
  └─ SIT-002 จะ block ทันที (ไม่มี Pending request ให้ Approve)
  └─ ให้ fix SIT-001 ก่อน แล้วรัน SIT-002 ใหม่
```

---

## Items to Clarify กับ Dev/BA ก่อน Execute

| # | ประเด็น | ผลกระทบ |
|---|---------|---------|
| Q1 | ปุ่ม Approve อยู่ใน **row โดยตรง** หรือต้องคลิกเข้า Detail ก่อน? | กำหนดจำนวน step ใน SIT-002 Phase A |
| Q2 | หลัง Approve แล้ว list refresh **ทันที** หรือต้อง reload? | กำหนด `wait` condition ใน step 16 |
| Q3 | URL pattern ของ Approval Inbox คือ `/manager/inbox` หรืออื่น? | ปรับ step 6 ใน SIT-002 |
| Q4 | Login form รับ field ชื่ออะไร? — `username` / `email` / `employee_id`? | ปรับ step 3 ทั้งสอง scenario |
| Q5 | Balance Dashboard แสดง PendingDays แยกต่างหาก หรือรวมใน Remaining? | กำหนด assert value ใน SIT-001 step 30-31 |
| Q6 | Logout ทำผ่าน menu "ออกจากระบบ" หรือเพียง navigate กลับ login page? | ปรับ step 18 ใน SIT-002 Phase B |
