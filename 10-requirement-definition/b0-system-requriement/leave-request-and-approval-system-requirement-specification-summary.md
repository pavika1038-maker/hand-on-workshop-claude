---
title: "System Requirement Specification Summary"
document_type: "SRS Summary"
version: "1.0"
language: "th"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
status: "Draft"
source_brd: "10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md"
---

# System Requirement Specification Summary: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)
### ABC Company

---

## 1. วัตถุประสงค์ของเอกสาร

- เอกสารนี้แปลง Business Requirement จาก BRD ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval) ให้เป็น System Requirement ในระดับ Summary พร้อม traceability กลับสู่ BRD ทุกรายการ
- ระบบรองรับ business objective หลักจาก BRD ได้แก่: พนักงานทุกกลุ่มยื่นลาและตรวจสอบสิทธิ์ผ่าน Web ได้ด้วยตนเอง, หัวหน้างานอนุมัติผ่านระบบ, และ HR monitoring รวมศูนย์แทน Excel
- ขอบเขตของเอกสารนี้ครอบคลุม requirement ในระดับ summary สำหรับทุก module/screen/integration ที่ได้จาก BRD — เป็น baseline ก่อนแตกเป็น SRS รายละเอียดของแต่ละ module

---

## 2. ขอบเขตและแหล่งอ้างอิง

### 2.1 ขอบเขตของเอกสาร

- ครอบคลุม Phase 1 (High Priority) และ Phase 2 (Medium Priority) ตามที่กำหนดใน BRD Section 3.1
- แปลง BRD Business Rules (BR-001–BR-020) และ Use Cases (UC-01–UC-08) ให้เป็น Functional Requirement ระดับ system
- แยก requirement เป็น Functional (Screen, Report, Integration), Non-Functional, และ Technical
- ไม่ครอบคลุม implementation detail, physical database design, หรือ API specification

### 2.2 เอกสารอ้างอิงหลัก

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
|-------|-------------|--------------|
| 1 | `brd/leave-request-and-approval-brd.md` | BRD baseline หลักสำหรับการแปลง SRS Summary ทั้งหมด |
| 2 | `req-summary/leave-request-and-approval-requirement-summary.md` | Confirmed business baseline — ใช้ยืนยัน scope และ business rules |
| 3 | `requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml` | QA v3 (3/3 Closed) — authority สูงสุดสำหรับ design decisions |
| 4 | `requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml` | QA v2 (6/6 Closed) — authority สำหรับ leave rules และ Outsource policy |
| 5 | `requirement-validation/requirement-data-quality-analysis-qa-list.yaml` | QA v1 (12/12 Closed) — authority สำหรับ scope, actors, workflow |

---

## 3. หลักการแปลงและ Traceability

### 3.1 หลักการแปลง BRD เป็น SRS Summary

- แปลง Business Rule (BR-00x) → Functional Requirement (SFR/VR) โดยระบุ screen/process ที่เกี่ยวข้อง
- แปลง Business Use Case (UC-0x) → Validation Rule Matrix (VR) และ Screen Function Requirement (SFR)
- แปลง Business Entity → Interface List (IF) และ Screen data fields
- หากข้อมูลจาก BRD ไม่พอให้สรุป ระบุ `ข้อมูลไม่เพียงพอ` และจัดอยู่ใน Section 7 Open Issues
- คงระดับเอกสารไว้ในเชิง summary — ไม่ลงถึง API spec, DB schema, หรือ code design

### 3.2 หลักการ Traceability

- ทุก requirement มี Requirement ID (SFR-xxx, RFR-xxx, SIR-xxx, NFR-xxx, TR-xxx, VR-xxx)
- ทุก requirement มี Source BRD Reference ระบุถึง Section / Rule / Use Case
- Screen List และ Interface List อ้าง Related Requirement IDs เพื่อ trace ย้อนกลับได้
- Traceability Matrix (Section 8) สรุป mapping ทุก BRD item → SRS Requirement ID

---

## 4. Functional Requirement

### 4.1 Screen Function Requirement

> รวม requirement เพิ่มเติมจาก NR-001–NR-003 (BRD Section 8) ในหมวดนี้ตามประเภทของ requirement

| Req ID | Module / Screen | System Requirement Description | Input / Output Summary | Source BRD Reference | หมายเหตุ |
|--------|----------------|-------------------------------|----------------------|---------------------|---------|
| SFR-001 | Login / Authentication | ระบบต้องรองรับการ Login สำหรับพนักงานทุกกลุ่ม (ประจำและ Outsource) ผ่าน Web Browser | Input: username/password (หรือ SSO) — Output: session authenticated, redirect ไปหน้าหลัก | BRD §3.1 In Scope, BR-001, BRD §4 Actor |  |
| SFR-002 | Leave Balance Dashboard | ระบบต้องแสดงสิทธิ์วันลาคงเหลือแยกตามประเภทการลา (7 ประเภท) แบบ near real-time | Input: employee_id — Output: ตารางสิทธิ์คงเหลือ (ประเภทลา, สิทธิ์ทั้งหมด, ใช้ไป, คงเหลือ, สะสม) | BRD §3.1 BR-002, BRD BR-008, BRD BR-009, BRD §6 LEAVE_BALANCE | Outsource ต้องแสดงเฉพาะประเภทที่มีสิทธิ์ |
| SFR-003 | Leave Request Form | ระบบต้องให้พนักงานยื่นคำขอลาโดยระบุประเภทการลา วันที่เริ่ม-สิ้นสุด เหตุผล และแนบเอกสาร | Input: ประเภทลา, วันที่, เหตุผล, ไฟล์แนบ — Output: Leave Request บันทึก Status=Pending, trigger notification | BRD §5.3.1.A To-Be flow, BR-003, BR-005, BR-006, BRD §6 LEAVE_REQUEST |  |
| SFR-004 | Manager Approval Inbox | ระบบต้องแสดงรายการคำขอลาที่รอการอนุมัติของหัวหน้างาน พร้อมข้อมูลประวัติลาและสิทธิ์คงเหลือของพนักงาน | Input: manager_id — Output: รายการ Pending requests, ข้อมูล employee context, ปุ่ม Approve/Reject | BRD §5.3.1.A To-Be flow, BR-012, BRD §4 Actor (Manager) |  |
| SFR-005 | Approve / Reject Action | ระบบต้องให้หัวหน้างาน Approve หรือ Reject คำขอลา พร้อม optional เหตุผล และอัปเดตสถานะทันที | Input: request_id, action (Approve/Reject), reason (optional) — Output: Status updated, Email notification triggered | BRD BR-012, BR-013, BRD §5.3.1.B UC-08 |  |
| SFR-006 | Leave Request Status Tracking | ระบบต้องให้พนักงานตรวจสอบสถานะคำขอลาทุกรายการของตนเอง ย้อนหลังได้ตามช่วงเวลา | Input: employee_id, date range filter — Output: รายการคำขอพร้อม status, timestamp, เหตุผล (ถ้ามี) | BRD §3.1 BR-006, BRD §5.3.1.A |  |
| SFR-007 | Cancel Leave Request — Pending | ระบบต้องให้พนักงานยกเลิกคำขอที่อยู่ในสถานะ Pending ได้ทันที โดยไม่ต้องผ่านหัวหน้างาน | Input: request_id — Output: Status→Cancelled ทันที, no email required | BRD BR-014, BRD §5.1 Cancel Flow กรณี 1 |  |
| SFR-008 | Cancel Leave Request — Approved | ระบบต้องให้พนักงานส่ง Cancel Request สำหรับคำขอที่ Approved แล้ว และจัดการ re-approve flow พร้อม SLA | Input: request_id — Output: Status→Cancel Requested, Email แจ้ง Manager+HR, SLA timer เริ่มนับ | BRD BR-015, NR-001, NR-002, BRD §5.1 Cancel Flow กรณี 2 |  |
| SFR-009 | Re-approve Cancel Request (Manager) | ระบบต้องให้หัวหน้างาน Approve การยกเลิก พร้อม trigger คืนวันลาอัตโนมัติและ notification | Input: cancel_request_id, action=Approve — Output: Status→Cancelled, leave balance restored, Email แจ้งทุกฝ่าย | BRD BR-015, BR-016, NR-001, NR-002 |  |
| SFR-010 | SLA Reminder & Escalation | ระบบต้องส่ง Reminder Email ก่อนหมด SLA 4 ชม. และ Escalate ไปยัง HR เมื่อหมด SLA 1 วันทำการ | Input: SLA timer event — Output: Reminder Email → Manager (4 ชม. ก่อน), Escalate Email → HR (เมื่อหมด SLA) | BRD BR-018, BRD §5.3.1.B UC-05, M3 (QA v3) |  |
| SFR-011 | HR Monitoring Dashboard | ระบบต้องให้ HR ดูรายการคำขอลาทั้งหมดในระบบ กรองตามสถานะ / หน่วยงาน / ประเภทพนักงาน / วันที่ | Input: filter criteria — Output: รายการคำขอ, summary counts, export option | BRD §3.1 BR-007, BRD §4 Actor (HR) |  |
| SFR-012 | Outsource Data Import | ระบบต้องให้ HR import ข้อมูลพนักงาน Outsource จาก Excel template พร้อม validation และ error report | Input: Excel file (7 required fields) — Output: import result summary (success/fail records), data บันทึกในระบบ | BRD BR-020, BRD §7.7, R3 (QA v2) |  |
| SFR-013 | Email Notification Engine | ระบบต้องส่ง Email notification อัตโนมัติทุก event ตาม recipients ที่กำหนด พร้อม log ผลการส่ง | Input: event trigger (ยื่นลา/Approve/Reject/Cancel/SLA) — Output: Email ส่งไปยัง recipients ที่กำหนด, Notification Log บันทึก | BRD BR-019, R5 (QA v2), BRD §7.6 Notification Events |  |
| SFR-014 | Leave History & Audit Trail | ระบบต้องเก็บและแสดงประวัติทุก action (สร้าง, อนุมัติ, ปฏิเสธ, ยกเลิก) พร้อม timestamp และผู้กระทำ | Input: request_id หรือ employee_id — Output: history log เรียงตาม timestamp | BRD §3.1 BR-009 (Phase 2) | Phase 2 |
| SFR-015 | Leave Report Export | ระบบต้องให้ HR export รายงานการลาเป็นไฟล์ ตามช่วงเวลา / หน่วยงาน / ประเภทพนักงาน | Input: filter (date range, department, employee_type) — Output: รายงานไฟล์ (Excel หรือ PDF) | BRD §3.1 BR-010 (Phase 2) | Phase 2 |

---

### 4.2 Report Function Requirement

| Req ID | Report Name | System Requirement Description | Filter / Output Summary | Source BRD Reference | หมายเหตุ |
|--------|------------|-------------------------------|------------------------|---------------------|---------|
| RFR-001 | Leave Summary Report | ระบบต้องสร้างรายงานสรุปการลาของพนักงานทั้งองค์กร แยกตามประเภทการลา / หน่วยงาน / ประเภทพนักงาน | Filter: ช่วงวันที่, แผนก, employee_type (ประจำ/Outsource), ประเภทการลา — Output: จำนวนวันลา, จำนวนคำขอ, สถิติ Approve/Reject rate | BRD §3.1 BR-010, BRD §5.3.1.C KPI | Phase 2 — รายละเอียด report template ยังไม่ยืนยัน (ดู Section 7) |
| RFR-002 | Leave Balance Report | ระบบต้องสร้างรายงานสิทธิ์วันลาคงเหลือของพนักงานทุกคน ณ วันที่ระบุ | Filter: ช่วงวันที่ (as-of date), แผนก, employee_type — Output: ตารางสิทธิ์รายบุคคล (ได้รับ, ใช้ไป, คงเหลือ, สะสม) | BRD §3.1 BR-010, BRD §6 LEAVE_BALANCE | Phase 2 |
| RFR-003 | Notification Log Report | ระบบต้องแสดง log การส่ง Email notification ทั้งหมด สำหรับ HR ตรวจสอบ | Filter: event type, date range, recipient — Output: รายการ Email (event, recipient, timestamp, status สำเร็จ/ล้มเหลว) | BRD BR-019, BRD §5.3.1.C KPI (Email success rate ≥99%) | Phase 2 — ใช้ monitor KPI notification |

---

### 4.3 System Integration Requirement

| Req ID | Integration Target | System Requirement Description | Direction / Trigger | Source BRD Reference | หมายเหตุ |
|--------|-------------------|-------------------------------|--------------------|--------------------|---------|
| SIR-001 | HRIS (Legacy) | ระบบต้อง integrate กับ HRIS เดิมเพื่อดึงข้อมูล Employee Master Data (พนักงานประจำ) | Inbound / Realtime หรือ Scheduled Batch — trigger เมื่อ login หรือตามรอบที่กำหนด | BRD §3.4 HRIS Integration, BRD §6 EMPLOYEE, QA-H6 | Outsource ไม่ได้มาจาก HRIS — ใช้ Excel import แทน (SIR-003) |
| SIR-002 | Email Service (SMTP / Email Gateway) | ระบบต้อง integrate กับ Email Service เพื่อส่ง notification ทุก event โดยอัตโนมัติ พร้อมรับ delivery status | Outbound / Event-driven — trigger ทุกครั้งที่ status เปลี่ยน | BRD BR-019, BRD §5.3.1.A To-Be Flow, R5 (QA v2) | ต้องรองรับ retry กรณีส่งไม่สำเร็จ เพื่อให้ KPI ≥99% |
| SIR-003 | Excel Import (Outsource Onboarding) | ระบบต้องรองรับการ import ข้อมูล Outsource จาก Excel template ผ่าน UI — validate 7 required fields พร้อม error report | Inbound / Manual Batch — HR trigger เมื่อต้องการ onboard / update ข้อมูล (ทุกต้นไตรมาสหรือเมื่อมีการเปลี่ยนแปลง) | BRD BR-020, BRD §7.7, R3 (QA v2) |  |
| SIR-004 | SLA Scheduler / Timer Engine | ระบบต้องมี internal scheduler สำหรับจัดการ SLA countdown ของ Cancel Request — trigger Reminder 4 ชม. ก่อนหมด และ Escalate เมื่อหมดเวลา | Internal / Time-based — ทำงานอัตโนมัติหลังจากบันทึก Cancel Request | BRD BR-018, BRD §5.3.1.B UC-05, M3 (QA v3) |  |
| SIR-005 | File Storage (เอกสารแนบ) | ระบบต้อง integrate กับ File Storage เพื่อบันทึกไฟล์แนบ (ใบรับรองแพทย์) และ retrieve เมื่อต้องการ | Inbound (upload) / Outbound (retrieve) / Event-driven — trigger เมื่อพนักงาน submit คำขอลาป่วย ≥ 3 วัน | BRD BR-006, BRD §5.3.1.B UC-02 | ข้อมูลไม่เพียงพอสำหรับ storage type — ดู Section 7 |

---

### 4.4 Screen List

| Screen ID | Screen Name | Primary Users | Purpose Summary | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
|-----------|------------|--------------|----------------|------------------------|---------------------|---------|
| SCR-001 | Login Page | พนักงานประจำ, Outsource, Manager, HR | Authentication เข้าสู่ระบบ | SFR-001, TR-001, TR-002, NFR-004 | BRD BR-001, BRD §4 Actors | รองรับทุก role |
| SCR-002 | Leave Balance Dashboard | พนักงานประจำ, Outsource | ตรวจสอบสิทธิ์วันลาคงเหลือแยกตามประเภท | SFR-002, VR-007, VR-008 | BRD BR-002, BRD §6 LEAVE_BALANCE | แสดงเฉพาะประเภทที่มีสิทธิ์ตาม employee_type |
| SCR-003 | Submit Leave Request | พนักงานประจำ, Outsource | ยื่นคำขอลา — เลือกประเภท วันที่ เหตุผล แนบเอกสาร | SFR-003, VR-001 ถึง VR-006, VR-009 | BRD §5.3.1.A To-Be flow, BR-003–BR-007, BR-011 | Validation หลายขั้นตอนก่อน submit |
| SCR-004 | Manager Approval Inbox | หัวหน้างาน (Line Manager) | รายการคำขอลารอ Approve/Reject พร้อม context | SFR-004, SFR-005, VR-010 | BRD §5.3.1.A, BRD §4 Actor (Manager), BR-012 |  |
| SCR-005 | Leave Request Detail | พนักงาน, Manager, HR | ดูรายละเอียดคำขอลาแต่ละรายการ — history, status, เอกสาร | SFR-006, SFR-014 | BRD §3.1 BR-006, BR-009 | Phase 2: แสดง audit trail |
| SCR-006 | Cancel Leave Request | พนักงานประจำ, Outsource | ยกเลิกคำขอลา (Pending ทันที / Approved ผ่าน re-approve flow) | SFR-007, SFR-008, VR-011 | BRD BR-014, BR-015, §5.1 Cancel Flow |  |
| SCR-007 | Re-approve Cancel (Manager) | หัวหน้างาน (Line Manager) | Approve/Reject Cancel Request จากพนักงาน พร้อมเห็น SLA countdown | SFR-009, SFR-010, VR-012 | BRD BR-015, BR-018, NR-001, NR-002 |  |
| SCR-008 | HR Monitoring Dashboard | HR | ดูรายการคำขอทั้งองค์กร กรองข้อมูล export รายงาน | SFR-011, SFR-015, RFR-001, RFR-002 | BRD §3.1 BR-007, BR-010 | Phase 2: export report |
| SCR-009 | Outsource Import (HR) | HR | Upload Excel template import ข้อมูลพนักงาน Outsource พร้อมดู error log | SFR-012, SIR-003, VR-013 | BRD BR-020, BRD §7.7, R3 (QA v2) |  |
| SCR-010 | Leave Report (Phase 2) | HR | Export รายงานการลา ตาม filter | SFR-015, RFR-001, RFR-002 | BRD §3.1 BR-010 | Phase 2 |
| SCR-011 | Notification Log (Phase 2) | HR | ตรวจสอบ Email notification log | RFR-003, SFR-013 | BRD BR-019, BRD §5.3.1.C KPI | Phase 2 |

---

### 4.5 Interface List

| Interface ID | Interface Name | Source System | Target System | Direction / Pattern | Data Summary | Trigger / Frequency | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
|-------------|---------------|--------------|--------------|--------------------|-----------|--------------------|------------------------|---------------------|---------|
| IF-001 | Employee Master Sync | HRIS (Legacy) | Leave Web App | Inbound / Scheduled Batch หรือ Realtime API | employee_id, ชื่อ, แผนก, ตำแหน่ง, email, วันเริ่มงาน, line_manager_id | Scheduled (daily sync) หรือ on-demand เมื่อ login | SIR-001, SFR-001, SFR-002 | BRD §3.4, BRD §6 EMPLOYEE, QA-H6 | รายละเอียด integration pattern (API vs Batch) ยังไม่ยืนยัน — ดู Section 7 |
| IF-002 | Email Notification | Leave Web App | Email Gateway (SMTP / SES) | Outbound / Event-driven | recipient list, subject, body (event type, request detail, reason), notification_log_id | Triggered ทุก event (ยื่น, Approve, Reject, Cancel, SLA Reminder, Escalate) | SIR-002, SFR-013, BR-019 | BRD BR-019, BRD §7.6 Notification Events, R5 (QA v2) | ต้องรองรับ retry และ log delivery status |
| IF-003 | Excel Import — Outsource | HR (Browser Upload) | Leave Web App | Inbound / Manual Batch | ชื่อ TH/EN, รหัสพนักงาน, แผนก/ตำแหน่ง, บริษัทต้นสังกัด, email, วันเริ่มงาน, line_manager_id | Manual trigger โดย HR — ทุกต้นไตรมาสหรือเมื่อมีการเปลี่ยนแปลง | SIR-003, SFR-012, VR-013 | BRD BR-020, BRD §7.7, R3 (QA v2) |  |
| IF-004 | File Upload — Medical Certificate | Employee (Browser) | File Storage | Inbound / On-demand | ไฟล์ใบรับรองแพทย์ (PDF/Image), request_id reference | Triggered เมื่อพนักงาน submit ลาป่วย ≥ 3 วันทำการต่อเนื่อง | SIR-005, SFR-003, VR-005 | BRD BR-006, BRD §5.3.1.B UC-02 | storage type ยังไม่ยืนยัน |
| IF-005 | SLA Timer Event | Leave Web App (Scheduler) | Leave Web App (Notification Engine) | Internal / Time-based | cancel_request_id, SLA deadline, event_type (reminder/escalate) | Auto-triggered: Reminder ที่ (SLA deadline − 4 ชม.), Escalate เมื่อถึง SLA deadline | SIR-004, SFR-010, BR-018 | BRD BR-018, BRD §5.3.1.B UC-05, M3 (QA v3) |  |

---

### 4.6 Validation Rule Matrix

| Validation ID | Related Screen / Process | Validation Rule Description | Expected Result / Error Handling | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
|--------------|------------------------|---------------------------|--------------------------------|------------------------|---------------------|---------|
| VR-001 | SCR-003 Submit Leave Request | ตรวจสอบว่า employee มีสิทธิ์ลาประเภทที่เลือก (Outsource ห้ามเลือกลาคลอด/ทำหมัน/รับราชการ/อุปสมบท) | Pass: ดำเนินการต่อ — Fail: แสดง error "ไม่มีสิทธิ์ลาประเภทนี้ กรุณาติดต่อบริษัทต้นสังกัด" | SFR-003, SFR-002 | BRD BR-011, NR-003, BRD §5.3.1.B UC-07 |  |
| VR-002 | SCR-003 Submit Leave Request | ตรวจสอบวันลาคงเหลือ — สิทธิ์คงเหลือต้อง ≥ จำนวนวันที่ขอลา (ยกเว้นลาป่วยที่ไม่มี quota) | Pass: ดำเนินการต่อ — Fail: แสดง error "สิทธิ์วันลาไม่เพียงพอ คงเหลือ X วัน" | SFR-003, SFR-002 | BRD BR-002, BRD §6 LEAVE_BALANCE |  |
| VR-003 | SCR-003 Submit Leave Request | ตรวจสอบ probation period: พนักงานอายุงาน < 3 เดือน ไม่สามารถยื่นลาพักผ่อนได้ | Pass: ดำเนินการต่อ — Fail: แสดง error "ยังอยู่ในช่วงทดลองงาน ไม่มีสิทธิ์ลาพักผ่อน" | SFR-003 | BRD BR-007, M2 (QA v3) |  |
| VR-004 | SCR-003 Submit Leave Request | ตรวจสอบ annual quota: อายุงาน < 1 ปี (แต่ > 3 เดือน) ไม่มีสิทธิ์ลาพักผ่อน | Pass: ดำเนินการต่อ — Fail: แสดง error "อายุงานยังไม่ครบ 1 ปี ไม่มีสิทธิ์ลาพักผ่อนประจำปี" | SFR-003, SFR-002 | BRD BR-008 (ช่วง < 1 ปี = 0 วัน), R6 (QA v2) |  |
| VR-005 | SCR-003 Submit Leave Request | ตรวจสอบการแจ้งล่วงหน้า ลาพักผ่อน: วันเริ่มลาต้อง ≥ วันถัดไปจากวันยื่น | Pass: ดำเนินการต่อ — Fail: แสดง error "ลาพักผ่อนต้องแจ้งล่วงหน้าอย่างน้อย 1 วัน" | SFR-003 | BRD BR-003, QA-H2 (Business User) |  |
| VR-006 | SCR-003 Submit Leave Request | ตรวจสอบการแจ้งล่วงหน้า ลากิจ: วันเริ่มลาต้อง ≥ 3 วันทำการหลังจากวันยื่น | Pass: ดำเนินการต่อ — Fail: แสดง error "ลากิจต้องแจ้งล่วงหน้าอย่างน้อย 3 วันทำการ" | SFR-003 | BRD BR-004, ABC-Leave-Workflow-Thai-QA.md |  |
| VR-007 | SCR-003 Submit Leave Request | ตรวจสอบใบรับรองแพทย์: ลาป่วย ≥ 3 วันทำการต่อเนื่อง ต้องแนบไฟล์ | Pass: ดำเนินการต่อ — Fail: แสดง error "กรุณาแนบใบรับรองแพทย์สำหรับการลาป่วยตั้งแต่ 3 วันขึ้นไป" พร้อม block submit | SFR-003, IF-004 | BRD BR-006, BRD §5.3.1.B UC-02 |  |
| VR-008 | SCR-003 Submit Leave Request | ตรวจสอบ leave balance cap: สิทธิ์สะสมรวมต้องไม่เกิน 30 วัน (ใช้ในการคำนวณ balance แสดงผล) | ระบบคำนวณ balance cap อัตโนมัติ — แสดงยอดสุทธิสูงสุด 30 วัน แม้สะสมเกิน | SFR-002 | BRD BR-009, R6 (QA v2) |  |
| VR-009 | SCR-006 Cancel Leave Request | ตรวจสอบ Eligibility การยกเลิก: ห้ามยกเลิกคำขอที่อยู่ในสถานะ Rejected | Pass: ดำเนินการต่อ — Fail: ซ่อนปุ่มยกเลิก แสดง "คำขอนี้ถูก Reject แล้ว กรุณายื่นคำขอใหม่" | SFR-007, SFR-008 | BRD BR-014, BR-015, §5.1 Cancel Flow |  |
| VR-010 | SCR-003 / SCR-005 | ห้ามแก้ไข (Edit) คำขอที่อยู่ในสถานะ Approved — ระบบต้องไม่แสดงปุ่ม Edit | ไม่แสดงปุ่ม Edit เมื่อ status = Approved — แสดง "ต้องการแก้ไข กรุณายกเลิกแล้วยื่นใหม่" | SFR-003 | BRD BR-017, R4 (QA v2) |  |
| VR-011 | SCR-006 Cancel Leave Request | ตรวจสอบ Annual Leave Quota (กิจ/พักผ่อน): พนักงานใช้สิทธิ์ครบ quota ไม่สามารถยื่นลาประเภทนั้นได้อีก | Pass: ดำเนินการต่อ — Fail: แสดง error "ใช้สิทธิ์ [ประเภทลา] ครบแล้ว (X วัน/ปี) — สิทธิ์คงเหลือ 0 วัน" | SFR-002, SFR-003 | BRD BR-010, R1 (QA v2) |  |
| VR-012 | SCR-007 Re-approve Cancel (Manager) | ตรวจสอบ SLA: หากหมดเวลา 1 วันทำการแล้ว ปุ่ม Approve/Reject ต้องถูก disabled — สถานะ Escalated แสดงให้ HR | Manager screen แสดง SLA countdown — เมื่อหมดเวลา: ปุ่ม disabled, status = Escalated, แจ้ง HR อัตโนมัติ | SFR-009, SFR-010, IF-005 | BRD BR-018, M3 (QA v3) |  |
| VR-013 | SCR-009 Outsource Import | ตรวจสอบ Excel template: 7 required fields ต้องครบ, email format valid, line_manager_id ต้องมีในระบบ | Pass: import สำเร็จ — Fail: แสดง error report ระบุ row และ field ที่ผิดพลาด, ไม่ import record ที่มีข้อผิดพลาด | SFR-012, SIR-003 | BRD BR-020, BRD §7.7, R3 (QA v2) |  |

---

## 5. Non-Functional Requirement

> รวม requirement เพิ่มเติมจาก BRD §5.3.1.C Process Flow KPI ในหมวดนี้

| Req ID | Category | Requirement Description | Measure / Acceptance Basis | Source BRD Reference | หมายเหตุ |
|--------|---------|------------------------|--------------------------|---------------------|---------|
| NFR-001 | Performance — Response Time | หน้าจอทุกหน้าต้องโหลดเสร็จภายในเวลาที่กำหนด ภายใต้ load ปกติ | Page load ≤ 3 วินาที (P95) ภายใต้ concurrent users ปกติขององค์กร | BRD §5.3.1.C KPI (ระยะเวลายื่นคำขอ ≤ 5 นาที) | Load volume ยังไม่ยืนยัน — ดู Section 7 |
| NFR-002 | Performance — Leave Balance Calculation | ระบบต้องคำนวณและแสดง leave balance ได้ทันทีเมื่อพนักงานเปิดหน้า Dashboard | Balance แสดงผลใน ≤ 2 วินาทีหลัง login | BRD §3.1 BR-002, BRD §5.3.1.A step 2 |  |
| NFR-003 | Availability | ระบบต้องพร้อมใช้งานตลอดเวลาทำการ โดยมี downtime ที่ยอมรับได้ต่ำ | Availability ≥ 99% (excluding planned maintenance) ในช่วงเวลาทำการ | BRD §3.1 In Scope, BRD §1 Business Objective | SLA uptime ยังไม่ยืนยัน — ดู Section 7 |
| NFR-004 | Security — Authentication | ระบบต้องมีการ authenticate ผู้ใช้ก่อนเข้าใช้งานทุกครั้ง ป้องกัน unauthorized access | ทุก request ต้องผ่าน authentication — session timeout ตามนโยบาย IT | BRD BR-001, BRD §4 Actors | Authentication method (SSO / Password) ยังไม่ยืนยัน — ดู Section 7 |
| NFR-005 | Security — Authorization | ระบบต้องควบคุมสิทธิ์ตาม role: พนักงานเห็นเฉพาะข้อมูลตนเอง, Manager เห็นทีมตัวเอง, HR เห็นทั้งองค์กร | Role-based access control (RBAC) — พนักงาน A ไม่สามารถดูคำขอของพนักงาน B | BRD §4 Actors, BRD §6 LEAVE_REQUEST |  |
| NFR-006 | Security — Data Privacy | ข้อมูลพนักงาน Outsource ที่ import เข้าระบบต้องได้รับการปกป้องเทียบเท่าพนักงานประจำ | ข้อมูล Outsource เข้าถึงได้เฉพาะ HR และหัวหน้างานที่รับผิดชอบ | BRD §7.7 Outsource Onboarding, R3 (QA v2) |  |
| NFR-007 | Reliability — Notification | Email notification ต้องส่งสำเร็จในอัตราสูง พร้อม retry กรณีล้มเหลว | Email success rate ≥ 99% — retry อย่างน้อย 3 ครั้งกรณีล้มเหลว | BRD §5.3.1.C KPI (Email success ≥99%), BR-019 |  |
| NFR-008 | Usability — Web Responsive | ระบบต้องใช้งานได้บน Web Browser ทั้ง Desktop และ Mobile (responsive design) ไม่ต้องพัฒนา native app | ใช้งานได้บน Chrome / Edge / Safari รุ่นล่าสุด ทั้ง Desktop และ Mobile browser | BRD §3.2 Out of Scope (ไม่ใช้ Mobile native), BRD §1 Business Objective |  |
| NFR-009 | Usability — Adoption | ระบบต้องออกแบบให้ใช้ง่ายพอที่ผู้ใช้ทุกกลุ่มสามารถยื่นลาได้โดยไม่ต้องผ่านการฝึกอบรมเข้มข้น | Adoption rate ≥ 95% ภายใน 3 เดือนหลัง go-live (วัดจาก % คำขอที่ผ่านระบบ) | BRD §5.3.1.C KPI (Adoption Rate ≥95%) |  |
| NFR-010 | Data Integrity — Leave Balance | วันลาคงเหลือต้องอัปเดตอย่างถูกต้องทุกครั้งที่มีการอนุมัติหรือยกเลิกคำขอ — ห้ามมี inconsistency | หลัง Approve: balance ลด, หลัง Cancel Approved: balance คืน อัตโนมัติ — ตรวจสอบ consistency ด้วย automated test | BRD BR-016, NR-001 |  |
| NFR-011 | SLA Compliance | ระบบต้อง enforce SLA 1 วันทำการสำหรับ re-approve Cancel Request และ trigger action อัตโนมัติ | SLA Reminder ส่งได้ทุกครั้งก่อนครบ 4 ชม., Escalate ทำงานเมื่อ SLA หมด — delay tolerance ≤ 15 นาที | BRD BR-018, BRD §5.3.1.C KPI (SLA ≤1 วันทำการ), M3 (QA v3) |  |

---

## 6. Technical Requirement

| Req ID | Category | Technical Requirement Description | Constraint / Standard | Source BRD Reference | หมายเหตุ |
|--------|---------|----------------------------------|---------------------|---------------------|---------|
| TR-001 | Platform — Web Application | ระบบต้องพัฒนาเป็น Web Application ที่รันบน Web Browser ไม่ใช่ Desktop หรือ Mobile Native App | รองรับ Chrome / Edge / Safari รุ่นล่าสุด, HTML5/CSS3/JavaScript standard | BRD §3.4 สถาปัตยกรรม, BRD §3.2 Out of Scope |  |
| TR-002 | Integration — HRIS | ระบบต้อง integrate กับ HRIS เดิมเพื่อดึง Employee Master Data — ไม่ replace HRIS | Integration approach (API / DB link / File-based) ต้องยืนยันกับทีม IT — ดู Section 7 | BRD §3.4, SIR-001, QA-H6 | รายละเอียด integration ยังไม่ยืนยัน |
| TR-003 | Integration — Email | ระบบต้องส่ง Email ผ่าน SMTP server หรือ Cloud Email Gateway ขององค์กร | ต้องรองรับ TLS, retry mechanism, delivery status tracking | BRD BR-019, SIR-002 | Email server / provider ยังไม่ยืนยัน — ดู Section 7 |
| TR-004 | Scheduler / Timer | ระบบต้องมี background job scheduler สำหรับจัดการ SLA timer ของ Cancel Request | Job delay tolerance ≤ 15 นาที — scheduler ต้องทำงานได้แม้ระหว่าง non-business hours | BRD BR-018, SIR-004, NFR-011 |  |
| TR-005 | File Upload | ระบบต้องรองรับการ upload ไฟล์ใบรับรองแพทย์ — กำหนด file type และ max size | รองรับ PDF, JPG, PNG — Max file size ข้อมูลไม่เพียงพอ (ดู Section 7) | BRD BR-006, SIR-005, IF-004 | file size limit ยังไม่ยืนยัน |
| TR-006 | Import — Excel | ระบบต้องรองรับการ import Excel file ตาม template ที่กำหนด พร้อม validate และ report error | Excel format: .xlsx, 7 required columns ตาม template — error report ระบุ row / field | BRD BR-020, SIR-003, IF-003 |  |
| TR-007 | Security — Data Transmission | การรับส่งข้อมูลทุก request ต้องเข้ารหัสผ่าน HTTPS | TLS 1.2 หรือสูงกว่า | BRD §4 Actors (ทุก role เข้าถึงผ่าน Web), NFR-004 |  |
| TR-008 | Authentication Method | ข้อมูลไม่เพียงพอสำหรับระบุ authentication method ที่แน่ชัด (SSO / Active Directory / Username-Password) | ต้องยืนยันกับทีม IT ว่า ABC Company ใช้ SSO หรือ standalone auth — ดู Section 7 | BRD BR-001, NFR-004 | Open Issue |
| TR-009 | Audit Log Storage | ระบบต้องเก็บ audit log ทุก action ในรูปแบบที่ query ได้ และ immutable (ห้ามลบหรือแก้ไข) | Log retention period ข้อมูลไม่เพียงพอ (ดู Section 7) | BRD §3.1 BR-009 (Phase 2) | Phase 2 |

---

## 7. Assumptions / Open Issues

| ประเภท | รายการ | ผลกระทบต่อ SRS | สิ่งที่ต้องยืนยันเพิ่ม |
|-------|--------|--------------|---------------------|
| Open Issue | **HRIS Integration Pattern** — ยังไม่ทราบว่า HRIS เดิมเปิด API, รองรับ DB link, หรือใช้ File-based export | กระทบ IF-001, SIR-001, TR-002 — integration approach แตกต่างกันมาก | ทีม IT / HRIS vendor ต้องยืนยัน API capability ของ HRIS |
| Open Issue | **Authentication Method** — ไม่ชัดว่าใช้ SSO, Active Directory, หรือ standalone username/password | กระทบ TR-008, SFR-001, NFR-004 | ยืนยันกับทีม IT ว่ามี SSO / corporate identity provider หรือไม่ |
| Open Issue | **จำนวนวันลาประเภทพิเศษ** (ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท) — ไม่มีใน BRD | กระทบ VR-001, SFR-002 — ระบบไม่สามารถ validate quota ได้หากไม่มีข้อมูล | ขอข้อมูลจาก HR พร้อม reference กฎหมายแรงงาน |
| Open Issue | **Email Server / Provider** — ยังไม่ระบุ Email Gateway ที่ใช้ | กระทบ SIR-002, IF-002, TR-003 | ยืนยัน SMTP server / Cloud provider (SES, SendGrid, Exchange Online ฯลฯ) กับทีม IT |
| Open Issue | **File Storage Type** — สำหรับเอกสารแนบใบรับรองแพทย์ | กระทบ SIR-005, IF-004, TR-005 | ยืนยันว่าใช้ local file server, cloud storage (S3, Azure Blob), หรือ DB blob |
| Open Issue | **Max File Size & Format** — ใบรับรองแพทย์ | กระทบ VR-007, TR-005 | HR และ IT ยืนยัน accepted format และ size limit |
| Open Issue | **Carry-forward Calculation Formula** — รู้ว่า cap = 30 วัน แต่ไม่ชัดว่า formula คำนวณยอดสะสมอย่างไร | กระทบ SFR-002, VR-008 — balance แสดงผลอาจผิดพลาด | HR ยืนยัน formula: ยกยอดเต็มจำนวน หรือ pro-rata |
| Open Issue | **Concurrent Users / System Load** — ไม่มีข้อมูลจำนวน users | กระทบ NFR-001, NFR-003 — ไม่สามารถกำหนด performance target ที่ถูกต้อง | HR / IT ยืนยันจำนวนพนักงานรวมและ expected peak load |
| Open Issue | **Audit Log Retention Period** — ยังไม่ระบุ | กระทบ TR-009 (Phase 2) | ยืนยันกับ HR / Legal ว่าต้องเก็บ log นานเท่าใด |
| Open Issue | **HR Email: Individual vs Distribution List** — R5 แนะนำ email group แต่ยังไม่ confirm | กระทบ SFR-013, IF-002 — กระทบ email recipient configuration | ยืนยันกับ HR ว่าต้องการรับ email เป็น individual หรือ distribution group |
| Open Issue | **SLA Escalate Assignee ใน HR** — M3 ระบุ Escalate ไปยัง HR แต่ไม่ระบุตำแหน่ง/email | กระทบ SFR-010, IF-002 — ไม่ทราบ recipient ของ Escalation email | HR ระบุชื่อตำแหน่งหรือ email ที่รับ Escalation |
| Open Issue | **Report Template** (Phase 2) — BR-010 ระบุประเภท filter เท่านั้น ไม่มี report format | กระทบ RFR-001, RFR-002, SFR-015, SCR-010 | HR ยืนยัน report format, columns, และ export file type |
| Assumption | ระบบใหม่ integrate กับ HRIS เดิม — ไม่ replace HRIS | หากสมมติฐานผิด กระทบ scope ทั้งหมด | ยืนยันกับ Business Owner ก่อน design phase |
| Assumption | พนักงานทุกคนมี email สำหรับรับ notification | หากพนักงานบางกลุ่มไม่มี email กระทบ notification design | HR ยืนยันว่า Outsource ทุกคนมี email ในระบบ |
| Assumption | Line Manager ของพนักงานแต่ละคนมีข้อมูลใน HRIS หรือ import field | หากไม่มีข้อมูล approver กระทบ routing ของ Approval | ยืนยัน data availability ก่อน system design |

---

## 8. Traceability Matrix

| BRD Reference | BRD Requirement Summary | SRS Summary Requirement ID | Requirement Type | หมายเหตุ |
|--------------|------------------------|--------------------------|----------------|---------|
| BRD §3.1 BR-001 | พนักงานทุกคนมี account เข้าระบบได้ (ประจำ + Outsource) | SFR-001, NFR-004, TR-008 | Functional, Non-Functional, Technical |  |
| BRD §3.1 BR-002 | พนักงานเห็นสิทธิ์วันลาคงเหลือแยกตามประเภท | SFR-002, VR-002, VR-008, SCR-002 | Functional |  |
| BRD §3.1 BR-003 | พนักงานยื่นคำขอลาผ่านระบบ | SFR-003, SCR-003 | Functional |  |
| BRD §3.1 BR-004 | หัวหน้างาน Approve/Reject ผ่านระบบ | SFR-004, SFR-005, SCR-004, NFR-005 | Functional, Non-Functional |  |
| BRD §3.1 BR-005 | ระบบส่ง Email แจ้งพนักงานเมื่อ Approve/Reject | SFR-013, SIR-002, IF-002, NFR-007 | Functional, Non-Functional |  |
| BRD §3.1 BR-006 | พนักงานตรวจสอบสถานะคำขอลา | SFR-006, SCR-005 | Functional |  |
| BRD §3.1 BR-007 | HR monitoring รายการคำขอทั้งหมด | SFR-011, SCR-008 | Functional |  |
| BRD §3.1 BR-008 | ระบบรองรับ employee_type แยกประจำ/Outsource | SFR-002, VR-001, NR-003, NFR-005 | Functional, Non-Functional |  |
| BRD §3.1 BR-009 | Audit Trail (Phase 2) | SFR-014, TR-009 | Functional, Technical | Phase 2 |
| BRD §3.1 BR-010 | HR export รายงานการลา (Phase 2) | SFR-015, RFR-001, RFR-002, SCR-010 | Functional | Phase 2 |
| BRD Business Rule BR-001 | พนักงานทุกคนต้องมี account | SFR-001, TR-008 | Functional, Technical |  |
| BRD Business Rule BR-003 | ลาพักผ่อนแจ้งล่วงหน้า ≥ 1 วัน | VR-005, SFR-003 | Functional |  |
| BRD Business Rule BR-004 | ลากิจแจ้งล่วงหน้า ≥ 3 วันทำการ | VR-006, SFR-003 | Functional |  |
| BRD Business Rule BR-005 | ลาป่วยฉุกเฉินไม่ต้องแจ้งล่วงหน้า | VR-006 (exemption), SFR-003 | Functional |  |
| BRD Business Rule BR-006 | ลาป่วย ≥ 3 วันต้องแนบใบรับรองแพทย์ | VR-007, SFR-003, IF-004, SIR-005 | Functional |  |
| BRD Business Rule BR-007 | Probation < 3 เดือน ไม่มีสิทธิ์ลาพักผ่อน | VR-003, SFR-002 | Functional |  |
| BRD Business Rule BR-008 | ตารางสิทธิ์ลาพักผ่อนตามอายุงาน 5 tiers | VR-004, SFR-002, NFR-010 | Functional, Non-Functional |  |
| BRD Business Rule BR-009 | cap วันสะสม 30 วัน | VR-008, SFR-002 | Functional |  |
| BRD Business Rule BR-010 | สิทธิ์ลากิจ 3 วัน/ปี (ประจำและ Outsource) | VR-011, SFR-002 | Functional |  |
| BRD Business Rule BR-011 | Outsource ไม่มีสิทธิ์ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท | VR-001, SFR-002, NR-003 | Functional |  |
| BRD Business Rule BR-012 | Approval 1 ระดับ (Line Manager เท่านั้น) | SFR-004, SFR-005, NFR-005 | Functional, Non-Functional |  |
| BRD Business Rule BR-013 | Reject reason optional แสดงใน Email | SFR-005, SFR-013 | Functional |  |
| BRD Business Rule BR-014 | Cancel Pending — ยกเลิกเองทันที | SFR-007, VR-009, SCR-006 | Functional |  |
| BRD Business Rule BR-015 | Cancel Approved — ต้อง re-approve | SFR-008, SFR-009, VR-009 | Functional |  |
| BRD Business Rule BR-016 | คืนวันลาอัตโนมัติเมื่อ cancel Approved สำเร็จ | SFR-009, NFR-010, NR-001 | Functional, Non-Functional |  |
| BRD Business Rule BR-017 | ห้าม Edit คำขอที่ Approved | VR-010, SFR-003 | Functional |  |
| BRD Business Rule BR-018 | SLA Reminder 4 ชม. + Escalate เมื่อหมด | SFR-010, SIR-004, IF-005, NFR-011, TR-004 | Functional, Non-Functional, Technical |  |
| BRD Business Rule BR-019 | HR รับ Email notification ทุก event | SFR-013, SIR-002, IF-002, NFR-007 | Functional, Non-Functional |  |
| BRD Business Rule BR-020 | Outsource import via Excel template | SFR-012, SIR-003, IF-003, VR-013 | Functional |  |
| BRD §5.3.1.B UC-01 | ลาป่วยฉุกเฉิน | VR-006 exemption, SFR-003 | Functional |  |
| BRD §5.3.1.B UC-02 | ลาป่วย ≥ 3 วัน แนบใบรับรองแพทย์ | VR-007, SFR-003, IF-004 | Functional |  |
| BRD §5.3.1.B UC-03 | ยกเลิก Pending | SFR-007, VR-009 | Functional |  |
| BRD §5.3.1.B UC-04 | ยกเลิก Approved — re-approve flow | SFR-008, SFR-009, VR-009 | Functional |  |
| BRD §5.3.1.B UC-05 | SLA หมด — Escalate | SFR-010, SIR-004, IF-005, VR-012, NFR-011 | Functional, Non-Functional |  |
| BRD §5.3.1.B UC-06 | Probation period check | VR-003, SFR-002 | Functional |  |
| BRD §5.3.1.B UC-07 | Outsource ยื่นลาประเภทที่ไม่มีสิทธิ์ | VR-001, SFR-002 | Functional |  |
| BRD §5.3.1.B UC-08 | Reject พร้อมเหตุผล | SFR-005, SFR-013 | Functional |  |
| BRD §8 NR-001 | คืนวันลาอัตโนมัติเมื่อ Cancel Approved สำเร็จ | SFR-009, NFR-010 | Functional, Non-Functional |  |
| BRD §8 NR-002 | Email event: Cancellation Approved | SFR-013, IF-002 | Functional |  |
| BRD §8 NR-003 | employee_type ควบคุมสิทธิ์ลา Outsource | VR-001, SFR-002, NR-003 | Functional |  |
| BRD §5.3.1.C KPI — Notification ≥99% | NFR (implied from KPI) | NFR-007, TR-003 | Non-Functional |  |
| BRD §5.3.1.C KPI — Adoption ≥95% | NFR (implied from KPI) | NFR-009 | Non-Functional |  |
| BRD §3.4 สถาปัตยกรรม Web App / HRIS Integration | System architecture requirement | TR-001, TR-002, SIR-001 | Technical |  |

---

## 9. Source Reference

- `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md`
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-report-v3.md`
- `10-requirement-definition/a0-business-requirement/raw-extracted/Leave-Management-Request-and-Approval-Business-User.yaml`

---

*เอกสารนี้จัดทำโดยแปลงจาก BRD ที่ยืนยันแล้ว (21/21 QA Closed) — ทุก requirement ใน Section 4–6 สามารถ trace กลับสู่ BRD ได้ผ่าน Traceability Matrix (Section 8)*
