---
title: "Interface SRS Document"
document_type: "Interface SRS"
version: "1.0"
language: "th"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
created_date: "2026-06-16"
last_updated: "2026-06-16"
status: "Draft"
---

# Interface SRS Document: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)

## Change Log

| Version | Date | Section | Change Type | Description | Source |
|---------|------|---------|-------------|-------------|--------|
| 1.0 | 2026-04-16 | All | Created | สร้างเอกสารจาก SRS Summary v0.3 — ครอบคลุม 3 interfaces (IF-001 ถึง IF-003) | SRS Summary v0.3 (BRD baseline) |

---

## 1. เอกสารอ้างอิงและขอบเขต

เอกสารนี้ลงรายละเอียด Interface / Integration ในมุมมองการเชื่อมต่อข้อมูลเชิงธุรกิจระหว่างระบบ สำหรับระบบบริหารการลาและการอนุมัติ ABC Company อ้างอิงจาก SRS Summary เป็น baseline หลัก ครอบคลุม 5 interfaces (IF-001–IF-005)

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
|-------|-------------|--------------|
| 1 | `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md` | ต้นทาง requirement เชิงธุรกิจ |
| 2 | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md` | baseline ของ System Integration Requirement (SIR-001–SIR-005) และ Interface List (IF-001–IF-005) |
| 3 | `requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml` | QA v2 — ยืนยัน integration rules (R3, R5) |
| 4 | `requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml` | QA v3 — ยืนยัน SLA timer (M3) |

### 1.2 Interface Index

| Interface ID | Interface Name | Source System | Destination System | Integration Type | Frequency | Related Requirement IDs | Source Reference | หมายเหตุ |
|-------------|---------------|--------------|------------------|----------------|-----------|------------------------|----------------|---------|
| IF-001 | Employee Master Sync | HRIS (Legacy) | Leave Web App | รับข้อมูล (Inbound) | Scheduled Batch (รายวัน) หรือ Realtime — ยังไม่ยืนยัน | SIR-001, SFR-001, SFR-002, TR-002 | SRS §4.3 SIR-001, §4.5 IF-001, BRD §3.4, QA-H6 | Integration pattern ยังไม่ยืนยัน |
| IF-002 | Email Notification | Leave Web App | Email Gateway (SMTP/Cloud) | ส่งข้อมูล (Outbound) | Event-driven (ทุก status change) | SIR-002, SFR-013, NFR-007, TR-003 | SRS §4.3 SIR-002, §4.5 IF-002, BRD BR-019, R5 (QA v2) | ต้อง retry และ log |
| IF-003 | Excel Import — Outsource | HR (Browser Upload) | Leave Web App | รับข้อมูล (Inbound) | Manual Batch (HR trigger) | SIR-003, SFR-012, VR-013, TR-006 | SRS §4.3 SIR-003, §4.5 IF-003, BRD BR-020, R3 (QA v2) | |
| IF-004 | File Upload — Medical Certificate | Employee (Browser) | File Storage | ส่งข้อมูล (Inbound) | Event-driven (เมื่อ submit ลาป่วย ≥ 3 วัน) | SIR-005, SFR-003, VR-007, TR-005 | SRS §4.3 SIR-005, §4.5 IF-004, BRD BR-006, BRD §5.3.1.B UC-02 | storage type ยังไม่ยืนยัน |
| IF-005 | SLA Timer Event | Leave Web App (Scheduler) | Leave Web App (Notification Engine) | แลกเปลี่ยนข้อมูล (Internal) | Time-based (อัตโนมัติ) | SIR-004, SFR-010, NFR-011, TR-004 | SRS §4.3 SIR-004, §4.5 IF-005, BRD BR-018, M3 (QA v3) | |

---

## 2. Detailed Interface Specification

---

### 2.1 IF-001 Employee Master Sync

#### 2.1.1 Interface Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Interface ID | IF-001 |
| Interface Name | Employee Master Sync |
| Description | ดึงข้อมูล Employee Master Data ของพนักงานประจำจาก HRIS เดิม มายังระบบ Leave Web App เพื่อใช้สร้าง account, ระบุ line_manager และคำนวณ leave entitlement |
| Business Purpose | ให้ข้อมูลพนักงานประจำในระบบ Leave App สอดคล้องกับ HRIS — ไม่ duplicate ข้อมูล และไม่ต้อง manage master data ใหม่ |
| Source System | HRIS (Legacy) |
| Destination System | Leave Web App |
| Related Requirement IDs | SIR-001, SFR-001, SFR-002, TR-002, NFR-005 |
| Source Reference | SRS §4.3 SIR-001, §4.5 IF-001, BRD §3.4 HRIS Integration, QA-H6 |

#### 2.1.2 Interface Description

| รายการ | รายละเอียด |
|-------|-----------|
| Integration Type | รับข้อมูล (Inbound) — Leave App เป็นผู้ดึงข้อมูลจาก HRIS |
| Operation Type | ทำงานอัตโนมัติ (Scheduled Batch หรือ Realtime API — ยังไม่ยืนยัน) |
| Frequency | Scheduled daily sync หรือ on-demand เมื่อ login (ขึ้นกับ integration pattern ที่ยืนยัน) |
| Additional Description | ข้อมูล Outsource ไม่ได้มาจาก HRIS — ใช้ Excel Import (IF-003) แทน / ระบบ Leave App integrate กับ HRIS แต่ไม่ replace HRIS |

#### 2.1.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
|-------|-----------|
| Data Scope Summary | ข้อมูลพนักงานประจำ: employee_id, ชื่อ-นามสกุล (TH/EN), แผนก, ตำแหน่ง, email, วันเริ่มงาน (hire_date), line_manager_id, สถานะการจ้างงาน |
| Data Condition | ส่งเฉพาะพนักงานประจำ (employee_type = ประจำ) ที่ยังปฏิบัติงานอยู่ (active) |
| Exclusion | ไม่รวม Outsource (ใช้ IF-003 แทน), ไม่รวมพนักงานที่ลาออก/เกษียณแล้ว |

#### 2.1.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
|-----------|-------------|------------|---------------|
| employee_id | รหัสพนักงาน — ใช้เป็น key ระบุตัวตน | EMP001 | Y |
| name_th | ชื่อ-นามสกุลภาษาไทย | สมชาย ใจดี | Y |
| name_en | ชื่อ-นามสกุลภาษาอังกฤษ | Somchai Jaidee | Y |
| department | ชื่อแผนก/หน่วยงาน | Information Technology | Y |
| position | ตำแหน่งงาน | Software Engineer | Y |
| email | Email address สำหรับ login และรับ notification | somchai@abc.com | Y |
| hire_date | วันเริ่มงาน — ใช้คำนวณอายุงานและ leave entitlement | 2022-01-15 | Y |
| line_manager_id | รหัสหัวหน้างาน — ใช้ route approval | EMP050 | Y |
| employment_status | สถานะการจ้างงาน (Active/Inactive) | Active | Y |

#### 2.1.5 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
|----------------|-------------|------------------|---------|
| Scheduled Sync | ระบบดึงข้อมูลพนักงานทั้งหมดจาก HRIS ตามรอบเวลาที่กำหนด | รายวัน (Daily Batch) — เวลาที่แน่ชัดยังไม่ยืนยัน | pattern นี้ใช้หาก HRIS รองรับ file export หรือ DB extract |
| On-demand (Login) | ดึงข้อมูลเมื่อพนักงาน login ครั้งแรก หรือเมื่อข้อมูล stale | Real-time เมื่อ login (หาก HRIS มี API) | pattern นี้ขึ้นกับ HRIS API capability |
| Manual Trigger (HR) | HR สั่ง sync ข้อมูลเมื่อมีการเปลี่ยนแปลงโครงสร้างองค์กร | On-demand — ข้อมูลไม่เพียงพอ | |

#### 2.1.6 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
|---------|----------------|---------|
| Sync สำเร็จ | ข้อมูลพนักงานประจำใน Leave App ถูกต้องตรงกับ HRIS — สามารถ login และดู leave balance ได้ | |
| พนักงานใหม่ใน HRIS | account ถูกสร้างใน Leave App อัตโนมัติหลัง sync | |
| พนักงานลาออก | status ใน Leave App เปลี่ยน → Inactive, ไม่สามารถ login ได้ | |
| Sync ล้มเหลว | Leave App ใช้ข้อมูล cache เดิม, แจ้ง admin / log error | |

#### 2.1.7 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-IF001-001 | เชื่อมต่อ HRIS ไม่ได้ | ไม่สามารถดึงข้อมูลพนักงานจาก HRIS ได้ กรุณาติดต่อทีม IT | Unable to sync employee data from HRIS. Please contact IT. | แสดงต่อ Admin/HR |
| ERR-IF001-002 | ข้อมูลพนักงานจาก HRIS ไม่ครบถ้วน (missing required fields) | ข้อมูลพนักงาน {employee_id} จาก HRIS ไม่สมบูรณ์ — ข้ามการ import รายการนี้ | Employee {employee_id} data from HRIS is incomplete — skipped. | Log สำหรับ admin |

##### Warning Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| WRN-IF001-001 | Sync ล่าช้าเกินกำหนด | ข้อมูลพนักงานอาจไม่ใช่ล่าสุด (sync ครั้งล่าสุด: {datetime}) | Employee data may not be up to date (last sync: {datetime}). | แสดงบน HR Dashboard |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-IF001-001 | Sync สำเร็จ | ซิงค์ข้อมูลพนักงานสำเร็จ จำนวน {N} รายการ ({datetime}) | Employee sync completed: {N} records updated ({datetime}). | Log สำหรับ admin |

#### 2.1.8 Business Rules

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
|--------|-------------|-------------------|----------------|---------|
| BR-001 | พนักงานประจำต้องมี account ในระบบ | IF-001 ต้องส่งข้อมูลพนักงานประจำทุกคนที่ active | BRD BR-001, SRS SFR-001 | |
| BR-008 | สิทธิ์ลาพักผ่อนขึ้นกับอายุงาน | hire_date เป็นข้อมูลสำคัญ — หาก HRIS ส่งค่าผิดกระทบ leave entitlement ทั้งหมด | BRD BR-008 | |
| BR-001-HRIS | HRIS integrate แต่ไม่ replace | Leave App อ่านข้อมูลจาก HRIS เท่านั้น — ห้าม write กลับไป HRIS | BRD §3.4 HRIS Integration, QA-H6 | |
| NFR-005 | RBAC ต้องระบุ line_manager ถูกต้อง | line_manager_id จาก HRIS ใช้ route approval — ข้อมูลผิดกระทบ approval flow | SRS NFR-005 | |

#### 2.1.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | Integration pattern (API / DB link / File-based export) ยังไม่ยืนยัน | กระทบ design ของ IF-001 ทั้งหมด | ทีม IT / HRIS vendor ต้องยืนยัน capability |
| Open Issue | Frequency และ timing ของ sync ยังไม่ยืนยัน | กระทบ data freshness และ SLA ของ Leave App | |
| Assumption | HRIS เป็นแหล่งข้อมูล master สำหรับพนักงานประจำ — Leave App ไม่แก้ไขข้อมูลพนักงาน | หาก HRIS ข้อมูลผิด Leave App จะผิดตาม | |
| Constraint | Outsource ไม่อยู่ใน HRIS — ใช้ IF-003 Excel Import แทน | ห้าม mix data source สำหรับ Outsource | |

---

### 2.2 IF-002 Email Notification

#### 2.2.1 Interface Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Interface ID | IF-002 |
| Interface Name | Email Notification |
| Description | ส่ง Email notification อัตโนมัติทุก event ที่เกี่ยวกับ Leave Request ไปยัง recipients ที่กำหนด (พนักงาน, Manager, HR) พร้อม retry และ log delivery status |
| Business Purpose | แจ้งผลการดำเนินการ (ยื่น/Approve/Reject/Cancel/SLA) ให้ทุกฝ่ายรับทราบทันที แทนการแจ้งด้วยตนเอง |
| Source System | Leave Web App |
| Destination System | Email Gateway (SMTP Server / Cloud Email Service — ยังไม่ยืนยัน provider) |
| Related Requirement IDs | SIR-002, SFR-013, NFR-007, TR-003, IF-005 |
| Source Reference | SRS §4.3 SIR-002, §4.5 IF-002, BRD BR-019, BRD §7.6 Notification Events, R5 (QA v2) |

#### 2.2.2 Interface Description

| รายการ | รายละเอียด |
|-------|-----------|
| Integration Type | ส่งข้อมูล (Outbound) — Leave App ส่ง Email ออกไปยัง Email Gateway |
| Operation Type | ทำงานอัตโนมัติ (Event-driven) — trigger ทุกครั้งที่ Status ของ Leave Request เปลี่ยน |
| Frequency | Real-time event-driven — trigger ต่อ 1 event |
| Additional Description | ต้องรองรับ retry อย่างน้อย 3 ครั้งกรณีส่งไม่สำเร็จ และ log delivery status ทุกครั้ง เพื่อ monitor KPI Email success rate ≥99% |

#### 2.2.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
|-------|-----------|
| Data Scope Summary | Event type, request_id, request detail, recipient list (พนักงาน/Manager/HR), เหตุผล (ถ้ามี), SLA info (สำหรับ Reminder/Escalate) |
| Data Condition | ส่ง Email ทุก event ที่กำหนดใน Notification Events ตาม BRD §7.6 — ไม่มีเงื่อนไขข้ามหรือเลือก event |
| Exclusion | ไม่ส่งสำหรับ background operation ที่ไม่เปลี่ยน status (เช่น เปิดดู Leave Detail) |

#### 2.2.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
|-----------|-------------|------------|---------------|
| event_type | ประเภท event ที่ trigger notification | "Leave Submitted" | Y |
| request_id | หมายเลขอ้างอิง Leave Request | LR-2026-00123 | Y |
| recipient_list | รายชื่อ email ผู้รับ (1 หรือหลาย recipients) | [manager@abc.com, hr@abc.com] | Y |
| employee_name | ชื่อพนักงานเจ้าของคำขอ | สมชาย ใจดี | Y |
| leave_type | ประเภทการลา | ลาพักผ่อนประจำปี | Y |
| leave_dates | ช่วงวันที่ลา | 1–3 ก.ค. 2026 (3 วัน) | Y |
| action_reason | เหตุผล Reject / Cancel (ถ้ามี) | ข้อมูลไม่เพียงพอ (optional) | N |
| sla_deadline | เวลา deadline SLA (สำหรับ Reminder/Escalate) | 2026-06-17 17:00 | Conditional |
| notification_log_id | ID สำหรับ track ผลการส่งใน log | NL-2026-00456 | Y (ระบบสร้างอัตโนมัติ) |

#### 2.2.5 Notification Events — Recipients Matrix

| Event | พนักงาน (เจ้าของคำขอ) | Line Manager | HR | หมายเหตุ |
|-------|----------------------|------------|-----|---------|
| Leave Submitted (ยื่นคำขอ) | ✓ (confirm) | ✓ (action required) | ✓ | BRD §7.6, R5 (QA v2) |
| Leave Approved | ✓ | — | ✓ | BRD §7.6 |
| Leave Rejected | ✓ (พร้อมเหตุผลถ้ามี) | — | ✓ | BRD BR-013 |
| Cancel Request Submitted (Approved leave) | — | ✓ (action required) | ✓ | BRD BR-015, NR-002 |
| Cancel Approved (Cancellation) | ✓ | ✓ | ✓ | BRD NR-002 |
| Cancel Rejected | ✓ | — | ✓ | BRD NR-002 |
| SLA Reminder (4 ชม. ก่อนหมด) | — | ✓ | — | BRD BR-018, M3 (QA v3) |
| SLA Escalated (หมดเวลา) | — | — | ✓ | BRD BR-018, M3 (QA v3) |

#### 2.2.6 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
|----------------|-------------|------------------|---------|
| Leave Request Status Change | พนักงาน submit / Manager Approve / Manager Reject | Real-time ทันทีที่ action สำเร็จ | |
| Cancel Request Submitted | พนักงานส่ง Cancel Request สำหรับ Approved leave | Real-time ทันทีที่บันทึก Cancel Request | |
| SLA Reminder | 4 ชั่วโมงก่อนหมด SLA 1 วันทำการ | Time-based (เรียก IF-005) | |
| SLA Escalation | เมื่อถึง SLA deadline Manager ยังไม่ action | Time-based (เรียก IF-005) | |

#### 2.2.7 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
|---------|----------------|---------|
| Email ส่งสำเร็จ | Email ถึง recipients ตรงเวลา, delivery_status = Success, บันทึกลง notification_log | |
| Email ส่งล้มเหลว | ระบบ retry อัตโนมัติ (≥3 ครั้ง), บันทึก retry_count และ failure_reason ลง log | |
| หลัง retry ครบ 3 ครั้งยังล้มเหลว | delivery_status = Failed, บันทึก log, HR ตรวจสอบผ่าน RP-003 | NFR-007: success rate ≥99% |

#### 2.2.8 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-IF002-001 | Email ส่งล้มเหลวหลัง retry ครบแล้ว | ไม่สามารถส่ง Email แจ้ง {recipient} ได้ — กรุณาตรวจสอบใน Notification Log | Unable to send email to {recipient} — please check Notification Log. | Log สำหรับ admin/HR |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-IF002-001 | Email ส่งสำเร็จ (log entry) | Email {event_type} ส่งสำเร็จไปยัง {recipient} ({datetime}) | Email {event_type} delivered to {recipient} ({datetime}). | บันทึกใน notification_log |

#### 2.2.9 Business Rules

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
|--------|-------------|-------------------|----------------|---------|
| BR-019 | HR รับ Email notification ทุก event | recipients ทุก event ต้องรวม HR | BRD BR-019, R5 (QA v2) | |
| BR-013 | เหตุผล Reject เป็น optional — แต่ถ้ามีต้องส่งในอีเมล | Email Rejected ต้องรวม action_reason หากมีค่า | BRD BR-013 | |
| NFR-007 | Email success rate ≥ 99% | ต้องมี retry ≥ 3 ครั้ง และ log ทุก delivery status | SRS NFR-007 | |
| NR-002 | Event "Cancellation Approved" เป็น event ใหม่ — ส่งให้ทั้ง 3 ฝ่าย | recipients = [พนักงาน, Manager, HR] | BRD §8 NR-002 | |

#### 2.2.10 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | Email server / provider (SMTP / SES / SendGrid / Exchange Online) ยังไม่ยืนยัน | กระทบ TR-003, configuration ของ retry/TLS | ทีม IT ยืนยัน |
| Open Issue | HR recipient — Individual email หรือ Distribution List ยังไม่ยืนยัน | กระทบ recipient_list config | R5 แนะนำ group แต่ยังไม่ confirm |
| Open Issue | SLA Escalate recipient ใน HR ยังไม่ระบุตำแหน่ง/email | กระทบ Escalation event recipient | HR ยืนยัน |
| Assumption | พนักงานทุกคน (รวม Outsource) มี email address ที่ active | หาก email ไม่ active: delivery ล้มเหลว retry ครบ → log Failed | |
| Constraint | Email ต้องส่งผ่าน TLS 1.2 หรือสูงกว่า (TR-007) | กระทบ SMTP config | |

---

### 2.3 IF-003 Excel Import — Outsource

#### 2.3.1 Interface Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Interface ID | IF-003 |
| Interface Name | Excel Import — Outsource Onboarding |
| Description | HR upload ไฟล์ Excel template ผ่าน Web UI เพื่อ import ข้อมูลพนักงาน Outsource เข้าระบบ พร้อม validate 7 required fields และแสดง error report |
| Business Purpose | Onboard ข้อมูล Outsource เข้าระบบ Leave App ได้รวดเร็ว โดยไม่ต้องกรอก manual ทีละคน และไม่ผ่าน HRIS เนื่องจาก Outsource ไม่ได้เป็นพนักงานตรงของ ABC Company |
| Source System | HR (Browser Upload — SCR-009) |
| Destination System | Leave Web App (Database) |
| Related Requirement IDs | SIR-003, SFR-012, VR-013, TR-006, IF-003 |
| Source Reference | SRS §4.3 SIR-003, §4.5 IF-003, BRD BR-020, BRD §7.7, R3 (QA v2) |

#### 2.3.2 Interface Description

| รายการ | รายละเอียด |
|-------|-----------|
| Integration Type | รับข้อมูล (Inbound) — HR upload Excel → Leave App อ่านและ validate → บันทึก Database |
| Operation Type | ผู้ใช้สั่งงาน (Manual Batch) — HR เป็นผู้ trigger |
| Frequency | ทุกต้นไตรมาสหรือเมื่อมีการเปลี่ยนแปลงรายชื่อ Outsource |
| Additional Description | ระบบ validate ก่อน import ทุกครั้ง — import เฉพาะ record ที่ valid ทั้งหมด (ไม่ partial import row ที่มีข้อผิดพลาด) |

#### 2.3.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
|-------|-----------|
| Data Scope Summary | ข้อมูลพนักงาน Outsource: 7 required fields ตาม Excel template ที่กำหนด |
| Data Condition | import เฉพาะ record ที่ผ่าน validation ครบทุก field — record ที่มีข้อผิดพลาดถูก skip พร้อมแสดง error report |
| Exclusion | ไม่รวมข้อมูลพนักงานประจำ (ใช้ IF-001 แทน), ไม่รวม field นอกเหนือจาก 7 required fields ที่กำหนด |

#### 2.3.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
|-----------|-------------|------------|---------------|
| name_th | ชื่อ-นามสกุลภาษาไทย | สมหญิง รักงาน | Y |
| name_en | ชื่อ-นามสกุลภาษาอังกฤษ | Somying Rakngarn | Y |
| employee_id | รหัสพนักงาน — ต้อง unique, ไม่ซ้ำในระบบ | OUT001 | Y |
| department_position | แผนก / ตำแหน่ง | IT / Tester | Y |
| agency_company | บริษัทต้นสังกัด (Outsource vendor) | XYZ Staffing Co., Ltd. | Y |
| email | Email address สำหรับ login และ notification — format valid, unique ในระบบ | somying@xyz.co.th | Y |
| abc_start_date | วันที่เริ่มงานที่ ABC Company — format ถูกต้อง, ≤ วันนี้ | 2026-01-01 | Y |
| line_manager_id | รหัสหัวหน้างาน — ต้องมีอยู่ใน system (พนักงาน ABC Company) | EMP050 | Y |

#### 2.3.5 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
|----------------|-------------|------------------|---------|
| HR Upload Template | HR เลือกไฟล์และ upload ผ่าน SCR-009 | Manual — เมื่อ HR trigger | |
| Validate & Preview | ระบบ validate ไฟล์และแสดง preview ก่อน import | Real-time หลัง upload | |
| Import Confirmed | HR กด "Import" เพื่อยืนยันการนำเข้า | Manual — หลัง preview | เฉพาะ valid records |

#### 2.3.6 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
|---------|----------------|---------|
| ทุก record ผ่าน validation | import สำเร็จทั้งหมด, SUC-IMP-001, ข้อมูล Outsource พร้อมใช้งาน | |
| มี record ที่ validation ไม่ผ่าน | import เฉพาะ valid records, แสดง error report ระบุ row/field ที่ผิด | |
| ไฟล์ผิด format (.xls / .csv / etc.) | แสดง ERR-IMP-004, ไม่ import | |
| ทุก record validation ไม่ผ่าน | ไม่ import เลย, แสดง error report ครบทุก row | |

#### 2.3.7 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-IF003-001 | Field บังคับว่าง (VR-013) | แถวที่ {N}: {Field} ไม่สามารถเว้นว่างได้ | Row {N}: {Field} is required and cannot be empty. | |
| ERR-IF003-002 | รหัสพนักงานซ้ำในระบบ | แถวที่ {N}: รหัสพนักงาน {employee_id} มีในระบบแล้ว | Row {N}: Employee ID {employee_id} already exists in the system. | |
| ERR-IF003-003 | Email format ผิดหรือซ้ำในระบบ | แถวที่ {N}: Email {email} ไม่ถูกต้องหรือมีในระบบแล้ว | Row {N}: Email {email} is invalid or already exists. | |
| ERR-IF003-004 | Line Manager ไม่พบในระบบ | แถวที่ {N}: ไม่พบรหัสหัวหน้างาน {manager_id} ในระบบ | Row {N}: Manager ID {manager_id} not found in the system. | |
| ERR-IF003-005 | วันที่ format ผิดหรือเป็นอนาคต | แถวที่ {N}: วันที่เริ่มงานไม่ถูกต้อง (ต้องเป็น YYYY-MM-DD และไม่เกินวันนี้) | Row {N}: Invalid start date (must be YYYY-MM-DD and not a future date). | |
| ERR-IF003-006 | รูปแบบไฟล์ผิด | กรุณาอัปโหลดไฟล์ .xlsx เท่านั้น | Please upload .xlsx files only. | |

##### Success Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| SUC-IF003-001 | Import สำเร็จ | นำเข้าข้อมูลสำเร็จ {X} รายการ (ไม่สำเร็จ {Y} รายการ — ดู error report) | Import completed: {X} records succeeded, {Y} failed — see error report. | |

#### 2.3.8 Business Rules

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
|--------|-------------|-------------------|----------------|---------|
| BR-020 | Outsource onboard ผ่าน Excel template — 7 required fields | validate ครบก่อน import ทุกครั้ง | BRD BR-020, R3 (QA v2) |  |
| BR-011 | Outsource employee_type ถูกกำหนดอัตโนมัติหลัง import | ระบบ set employee_type = Outsource ให้ทุก record ที่ import ผ่าน IF-003 | BRD BR-011 | |
| VR-013 | Validation rule: 7 required fields ครบ, email unique, manager_id ต้องมีในระบบ | block import record ที่ fail validation | SRS VR-013 | |
| NFR-006 | ข้อมูล Outsource ปกป้องเทียบเท่าพนักงานประจำ | หลัง import: ข้อมูลเข้าถึงได้เฉพาะ HR และ Manager ที่รับผิดชอบ | SRS NFR-006 | |

#### 2.3.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | ระบบ handle update กรณี Outsource เปลี่ยน Manager / แผนก อย่างไร (re-import หรือ edit ในระบบ) | กระทบ workflow update ข้อมูล Outsource | ยังไม่ระบุใน BRD |
| Assumption | HR ใช้ Excel template ที่กำหนดเท่านั้น — ไม่รองรับ format อื่น | หาก HR ส่งไฟล์ต่าง format: ERR-IF003-006 | |
| Constraint | รองรับ .xlsx เท่านั้น (TR-006) | ไม่รองรับ .xls, .csv, .ods | |

---

### 2.4 IF-004 File Upload — Medical Certificate

#### 2.4.1 Interface Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Interface ID | IF-004 |
| Interface Name | File Upload — Medical Certificate |
| Description | พนักงานอัปโหลดไฟล์ใบรับรองแพทย์ผ่าน Web Browser เมื่อยื่นลาป่วย ≥ 3 วันทำการต่อเนื่อง ไฟล์ถูกบันทึกใน File Storage และ link กับ Leave Request นั้น |
| Business Purpose | จัดเก็บเอกสารยืนยันลาป่วยระยะยาว เพื่อให้ Manager และ HR ตรวจสอบประกอบการอนุมัติ |
| Source System | Employee (Browser — SCR-003 Submit Leave Request) |
| Destination System | File Storage (type ยังไม่ยืนยัน) |
| Related Requirement IDs | SIR-005, SFR-003, VR-007, TR-005 |
| Source Reference | SRS §4.3 SIR-005, §4.5 IF-004, BRD BR-006, BRD §5.3.1.B UC-02 |

#### 2.4.2 Interface Description

| รายการ | รายละเอียด |
|-------|-----------|
| Integration Type | ส่งข้อมูล (Inbound) — Browser ส่งไฟล์ไป File Storage |
| Operation Type | ผู้ใช้สั่งงาน (Manual Upload) — trigger เมื่อพนักงาน submit |
| Frequency | Event-driven — trigger เมื่อพนักงาน submit ลาป่วย ≥ 3 วันทำการต่อเนื่อง |
| Additional Description | ไฟล์ต้องเชื่อมกับ leave_request_id ที่เกี่ยวข้อง เพื่อ retrieve ได้เมื่อ Manager/HR ต้องการดู |

#### 2.4.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
|-------|-----------|
| Data Scope Summary | ไฟล์ใบรับรองแพทย์ (PDF / JPG / PNG) พร้อม metadata: leave_request_id, employee_id, upload_timestamp |
| Data Condition | บังคับเมื่อ leave_type = ลาป่วย AND total_days ≥ 3 วันทำการต่อเนื่อง (VR-007) |
| Exclusion | ไม่บังคับสำหรับลาป่วย < 3 วัน หรือประเภทลาอื่น |

#### 2.4.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
|-----------|-------------|------------|---------------|
| file_content | ไฟล์ใบรับรองแพทย์ | (binary) | Y |
| file_name | ชื่อไฟล์ | doctor_cert_20260701.pdf | Y |
| file_type | ประเภทไฟล์ | PDF / JPG / PNG | Y |
| file_size | ขนาดไฟล์ (bytes) | ข้อมูลไม่เพียงพอ (max size ยังไม่ยืนยัน) | Y |
| leave_request_id | ID ของ Leave Request ที่เชื่อมกับไฟล์ | LR-2026-00123 | Y |
| employee_id | รหัสพนักงานผู้ upload | EMP001 | Y |
| upload_timestamp | วัน-เวลาที่ upload | 2026-06-16 09:30:00 | Y (ระบบสร้างอัตโนมัติ) |

#### 2.4.5 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
|----------------|-------------|------------------|---------|
| Submit Leave (ลาป่วย ≥ 3 วัน) | พนักงาน submit คำขอลาป่วยที่ total_days ≥ 3 วันทำการต่อเนื่อง | Real-time เมื่อ submit Leave Request (SCR-003) | VR-007: บังคับแนบก่อน Submit ได้ |

#### 2.4.6 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
|---------|----------------|---------|
| Upload สำเร็จ | ไฟล์บันทึกใน File Storage, link กับ leave_request_id, Leave Request Submit สำเร็จ | |
| Upload ล้มเหลว (network error) | Leave Request ไม่บันทึก, แสดง error, พนักงาน retry | |
| ไฟล์ประเภทไม่รองรับ | แสดง ERR-IF004-001, block submit | |
| ไฟล์ใหญ่เกิน limit | แสดง ERR-IF004-002, block submit (เมื่อ max size ยืนยันแล้ว) | |

#### 2.4.7 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-IF004-001 | ประเภทไฟล์ไม่รองรับ | กรุณาอัปโหลดเฉพาะไฟล์ PDF, JPG, หรือ PNG เท่านั้น | Please upload PDF, JPG, or PNG files only. | |
| ERR-IF004-002 | ขนาดไฟล์เกิน limit | ไฟล์มีขนาดใหญ่เกิน {max_size} MB กรุณาลดขนาดไฟล์ | File size exceeds {max_size} MB. Please reduce the file size. | max_size ยังไม่ยืนยัน |
| ERR-IF004-003 | Upload ล้มเหลว (network/storage error) | ไม่สามารถอัปโหลดไฟล์ได้ กรุณาลองใหม่อีกครั้ง | File upload failed. Please try again. | |

#### 2.4.8 Business Rules

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
|--------|-------------|-------------------|----------------|---------|
| BR-006 | ลาป่วย ≥ 3 วันทำการต่อเนื่อง ต้องแนบใบรับรองแพทย์ | VR-007: block submit SCR-003 หากไม่แนบไฟล์ | BRD BR-006, BRD §5.3.1.B UC-02 | |
| TR-005 | รองรับ PDF, JPG, PNG — Max size ยังไม่ยืนยัน | validate file type ก่อน upload | SRS TR-005 | |
| NFR-005 | เฉพาะ Manager และ HR เข้าถึงไฟล์ใบรับรองแพทย์ | File Storage ต้องมี access control | SRS NFR-005, NFR-006 | |

#### 2.4.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | File Storage type (local server / cloud S3 / Azure Blob / DB blob) ยังไม่ยืนยัน | กระทบ design ของ IF-004 ทั้งหมด | ทีม IT ยืนยัน |
| Open Issue | Max file size ยังไม่ยืนยัน | กระทบ ERR-IF004-002 และ UX | HR และ IT ยืนยัน |
| Assumption | ไฟล์ถูก link กับ Leave Request — ถ้า Leave Request ถูกยกเลิก ไฟล์ยังคงอยู่ใน storage (retention policy ยังไม่กำหนด) | | |

---

### 2.5 IF-005 SLA Timer Event

#### 2.5.1 Interface Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Interface ID | IF-005 |
| Interface Name | SLA Timer Event |
| Description | Internal scheduler ของ Leave Web App จัดการ SLA countdown สำหรับ Cancel Request — trigger Reminder Email 4 ชั่วโมงก่อนหมด SLA และ Escalation Email เมื่อหมด SLA 1 วันทำการ |
| Business Purpose | Enforce SLA 1 วันทำการสำหรับ re-approve Cancel Request โดยอัตโนมัติ ลด risk คำขอค้างโดยไม่ได้รับการดำเนินการ |
| Source System | Leave Web App (Scheduler / Timer Engine) |
| Destination System | Leave Web App (Notification Engine → IF-002) |
| Related Requirement IDs | SIR-004, SFR-010, NFR-011, TR-004, VR-012 |
| Source Reference | SRS §4.3 SIR-004, §4.5 IF-005, BRD BR-018, BRD §5.3.1.B UC-05, M3 (QA v3) |

#### 2.5.2 Interface Description

| รายการ | รายละเอียด |
|-------|-----------|
| Integration Type | แลกเปลี่ยนข้อมูล (Internal) — ภายใน Leave Web App เดียวกัน |
| Operation Type | ทำงานอัตโนมัติ (Time-based Scheduler) — ไม่ต้องผู้ใช้สั่งงาน |
| Frequency | ทำงานต่อเนื่อง — ตรวจ SLA deadline ตาม interval ที่กำหนด (delay tolerance ≤ 15 นาที ตาม NFR-011) |
| Additional Description | ทำงานได้แม้ระหว่าง non-business hours เพราะ SLA 1 วันทำการอาจ span ข้ามวัน |

#### 2.5.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
|-------|-----------|
| Data Scope Summary | Cancel Request ที่ Status = "Cancel Requested" และ SLA ยังไม่หมด / กำลังจะหมด |
| Data Condition | ตรวจเฉพาะ Cancel Request ที่ยังรอ Manager action (Status = Cancel Requested, Manager ยังไม่ Approve/Reject) |
| Exclusion | ไม่ trigger สำหรับ Cancel Request ที่ Approved/Rejected แล้ว, หรือ Leave Request ที่ไม่ใช่ Approved |

#### 2.5.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
|-----------|-------------|------------|---------------|
| cancel_request_id | ID ของ Cancel Request ที่กำลัง monitor | CR-2026-00045 | Y |
| leave_request_id | ID ของ Leave Request ต้นทาง | LR-2026-00123 | Y |
| sla_deadline | วัน-เวลา deadline SLA (1 วันทำการหลัง Cancel Request) | 2026-06-17 17:00:00 | Y |
| event_type | ประเภท event: SLA_REMINDER หรือ SLA_ESCALATE | SLA_REMINDER | Y |
| manager_id | รหัส Manager ที่ต้อง action | EMP050 | Y |
| employee_id | รหัสพนักงานเจ้าของคำขอ | EMP001 | Y |

#### 2.5.5 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
|----------------|-------------|------------------|---------|
| Cancel Request สร้างแล้ว | Scheduler เริ่ม monitor SLA ทันทีที่บันทึก Cancel Request | Real-time — เริ่มนับทันทีหลัง Status = Cancel Requested | SLA = 1 วันทำการจาก เวลาที่บันทึก |
| SLA Reminder | 4 ชั่วโมงก่อนถึง SLA deadline — trigger IF-002 ส่ง Reminder Email ไปยัง Manager | Time-based: (SLA_deadline − 4 ชม.) | M3 (QA v3) |
| SLA Escalation | เมื่อถึง SLA deadline และ Manager ยังไม่ action | Time-based: SLA_deadline (หรือทันทีหลังเลย) | trigger IF-002 ส่ง Escalation Email ไปยัง HR |
| Manager Action ก่อนหมด SLA | Manager Approve หรือ Reject Cancel Request ก่อน deadline | Event-driven — cancel timer สำหรับ request นั้น | ไม่ต้อง trigger Reminder/Escalate |

#### 2.5.6 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
|---------|----------------|---------|
| Reminder trigger สำเร็จ | IF-002 ส่ง Reminder Email ไปยัง Manager ทันเวลา (±15 นาที) | NFR-011: delay ≤ 15 นาที |
| Escalation trigger สำเร็จ | IF-002 ส่ง Escalation Email ไปยัง HR, status indicator บน SCR-007 เปลี่ยน → "Escalated", ปุ่ม Approve/Reject disabled | VR-012 |
| Manager action ก่อน SLA | Scheduler cancel timer สำหรับ request นั้น — ไม่มี Reminder/Escalation | |
| Scheduler หยุดทำงานกะทันหัน | หลัง recover: ตรวจ SLA deadline ที่ผ่านมา → trigger Escalation สำหรับ request ที่หมดเวลา | |

#### 2.5.7 Message List

##### Warning Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| WRN-IF005-001 | Scheduler delay เกิน 15 นาที (monitor log) | SLA Scheduler มีความล่าช้าผิดปกติ — กรุณาตรวจสอบระบบ | SLA Scheduler delay detected — please check system health. | Log/admin alert |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-IF005-001 | SLA Reminder trigger สำเร็จ (log) | Reminder Email สำหรับ {cancel_request_id} ส่งแล้ว ({datetime}) | SLA Reminder for {cancel_request_id} triggered ({datetime}). | Log entry |
| INF-IF005-002 | SLA Escalation trigger สำเร็จ (log) | Escalation Email สำหรับ {cancel_request_id} ส่งแล้ว ไปยัง HR ({datetime}) | SLA Escalation for {cancel_request_id} triggered to HR ({datetime}). | Log entry |

#### 2.5.8 Business Rules

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
|--------|-------------|-------------------|----------------|---------|
| BR-018 | SLA 1 วันทำการ: Reminder 4 ชม. ก่อน + Escalate ไป HR เมื่อหมด | 2 trigger events ต่อ 1 Cancel Request | BRD BR-018, M3 (QA v3) | |
| VR-012 | SLA หมด → ปุ่ม Approve/Reject disabled, Status = Escalated | Escalation trigger ต้อง update status ใน SCR-007 ด้วย | SRS VR-012 | |
| NFR-011 | Scheduler delay tolerance ≤ 15 นาที | ต้องมี monitoring สำหรับ scheduler lag | SRS NFR-011 | |
| TR-004 | Scheduler ทำงานได้แม้ non-business hours | design ให้ 24/7 — ไม่หยุดช่วงนอกเวลาทำการ | SRS TR-004 | |

#### 2.5.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | SLA Escalate assignee ใน HR ยังไม่ระบุ (ตำแหน่ง/email) | กระทบ recipient ของ Escalation Email ที่ IF-002 ส่ง | HR ยืนยัน |
| Open Issue | SLA "1 วันทำการ" นับจากเวลาที่ Cancel Request บันทึก — working hours definition ยังไม่ชัดเจน | กระทบ SLA_deadline calculation | HR ยืนยัน working hours สำหรับ SLA |
| Assumption | Scheduler เป็น internal component ของ Leave Web App — ไม่ใช่ third-party scheduler | |  |
| Constraint | delay tolerance ≤ 15 นาที (NFR-011) — Scheduler ต้อง run ทุก ≤ 15 นาที | กระทบ infrastructure / compute resource | |

---

## 3. Cross-Interface Traceability

| Interface ID | Interface Name | Related Requirement IDs | Source Reference | หมายเหตุ |
|-------------|---------------|------------------------|----------------|---------|
| IF-001 | Employee Master Sync | SIR-001, SFR-001, SFR-002, TR-002, NFR-005 | SRS §4.3 SIR-001, §4.5 IF-001, BRD §3.4, QA-H6 | Integration pattern (API/Batch/File) ยังไม่ยืนยัน |
| IF-002 | Email Notification | SIR-002, SFR-013, NFR-007, TR-003, TR-007 | SRS §4.3 SIR-002, §4.5 IF-002, BRD BR-019, R5 (QA v2) | Email provider / retry config ยังไม่ยืนยัน |
| IF-003 | Excel Import — Outsource | SIR-003, SFR-012, VR-013, TR-006 | SRS §4.3 SIR-003, §4.5 IF-003, BRD BR-020, R3 (QA v2) | |
| IF-004 | File Upload — Medical Certificate | SIR-005, SFR-003, VR-007, TR-005 | SRS §4.3 SIR-005, §4.5 IF-004, BRD BR-006, BRD §5.3.1.B UC-02 | File storage type + max size ยังไม่ยืนยัน |
| IF-005 | SLA Timer Event | SIR-004, SFR-010, NFR-011, TR-004, VR-012 | SRS §4.3 SIR-004, §4.5 IF-005, BRD BR-018, M3 (QA v3) | SLA assignee + working hours definition ยังไม่ยืนยัน |

---

## 4. Source Reference

- `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md`
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md`
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml`
