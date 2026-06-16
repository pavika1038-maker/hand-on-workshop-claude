---
title: "Non-Functional / Technical SRS Document"
document_type: "Non-Functional / Technical SRS"
version: "1.0"
language: "th"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
status: "Draft"
---

# Non-Functional / Technical SRS Document: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)

## Change Log

| Version | Date | Section | Change Type | Description | Source |
|---------|------|---------|-------------|-------------|--------|
| 1.0 | 2026-04-16 | All | Created | สร้างเอกสารจาก SRS Summary v0.3 — ครอบคลุม NFR-001~004 และ TR-001~006 | SRS Summary v0.3 (BRD baseline) |

---

## 1. Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Document Name | Non-Functional / Technical SRS — ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval) |
| Description | เอกสารสรุปข้อกำหนดเชิงคุณภาพ (Non-Functional) และเชิงเทคนิค (Technical) สำหรับระบบบริหารการลาและการอนุมัติ ABC Company อ้างอิงจาก NFR-001–NFR-011 และ TR-001–TR-009 ใน SRS Summary |
| Business Purpose | ให้ทีม Architecture, Security, Operations, และ Test Planning มีข้อกำหนดที่ชัดเจนสำหรับการออกแบบและทดสอบระบบในเชิงคุณภาพ |
| Scope | ครอบคลุม Performance, Availability, Security, Usability, Technology Stack, Database, Integration, Reliability, Scalability, Monitoring, Deployment, Compliance — ยึด confirmed baseline จาก SRS Summary เท่านั้น ไม่เดา detail ที่ยังไม่มีหลักฐาน |

---

## 2. System Overview

| รายการ | รายละเอียด |
|-------|-----------|
| System Summary | ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval) ให้พนักงาน ABC Company (ประจำและ Outsource) ยื่นคำขอลา ตรวจสอบสิทธิ์ และติดตามสถานะผ่าน Web Browser แทนใบลากระดาษ พร้อม workflow อนุมัติโดย Line Manager และ HR monitoring รวมศูนย์ |
| System Type | Web Application (Browser-based) — ไม่ใช่ Mobile Native App |
| Primary User Groups | พนักงานประจำ, Outsource, Line Manager, HR |
| Source Reference | BRD §1 Business Objective, BRD §3.4 สถาปัตยกรรม, SRS SFR-001–SFR-015 |

---

## 3. Application Architecture (ภาพรวมโครงสร้างระบบ)

| รายการ | รายละเอียด |
|-------|-----------|
| Architecture Approach | ข้อมูลไม่เพียงพอ — Architecture style (Monolith / Microservices / Layered) ยังไม่ได้รับการยืนยันจากทีม IT / ทีม Architecture |
| Main Components | Frontend (Web Browser), Backend (Application Server), Database, External Systems: HRIS (IF-001), Email Gateway (IF-002), File Storage (IF-004), Internal SLA Scheduler (IF-005) |
| System Segmentation | Web Application ที่ integrate กับ HRIS เดิมโดยไม่ replace — HRIS ยังคงเป็น master สำหรับพนักงานประจำ |
| Source Reference | BRD §3.4, SRS TR-001, TR-002, SIR-001–SIR-005 |

> **หมายเหตุ:** รายละเอียด architecture (framework, runtime, server, container) ยังไม่ยืนยัน — ดู Section 17 Open Issues

---

## 4. Performance (ประสิทธิภาพของระบบ)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Response Time Target | หน้าจอหลักทุกหน้าต้องโหลดเสร็จ ≤ 3 วินาที (P95) ภายใต้ concurrent users ปกติ | NFR-001 | BRD §5.3.1.C KPI (ระยะเวลายื่นคำขอ ≤ 5 นาที) |
| Leave Balance Calculation | ข้อมูล leave balance ต้องแสดงผลใน ≤ 2 วินาทีหลัง login / เปิดหน้า Dashboard | NFR-002 | BRD §3.1 BR-002, BRD §5.3.1.A step 2 |
| Concurrent Users | ข้อมูลไม่เพียงพอ — จำนวน concurrent users ยังไม่ยืนยัน (ดู Section 17) | NFR-001 (partial) | SRS §7 Open Issue |
| Peak Usage Scenario | ข้อมูลไม่เพียงพอ — peak load scenario ยังไม่กำหนด | — | SRS §7 Open Issue |

> **Open Issue:** Concurrent Users / System Load ยังไม่มีข้อมูล — กระทบการกำหนด performance target ที่ถูกต้อง ต้องได้รับการยืนยันจาก HR/IT

---

## 5. Availability (ความพร้อมใช้งาน)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Required Availability Window | ระบบต้องพร้อมใช้งานตลอดเวลาทำการขององค์กร | NFR-003 | BRD §3.1 In Scope, BRD §1 Business Objective |
| SLA / Uptime Target | Availability ≥ 99% (excluding planned maintenance) ในช่วงเวลาทำการ | NFR-003 | BRD §3.1 In Scope |
| SLA Scheduler Availability | SLA Timer (IF-005) ต้องทำงานได้ 24/7 แม้นอกเวลาทำการ เพราะ SLA 1 วันทำการอาจ span ข้ามวัน | NFR-011, TR-004 | BRD BR-018, M3 (QA v3) |
| Downtime Handling Approach | ข้อมูลไม่เพียงพอ — Fallback / maintenance mode ยังไม่กำหนด — แนะนำให้วางแผน maintenance window นอกเวลาทำการ | — | SRS §7 Open Issue |

> **Open Issue:** SLA Uptime target (99%) และ planned maintenance window ยังไม่ได้รับการยืนยันจากทีม IT

---

## 6. Security (ความปลอดภัย)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Access Control Approach | Role-Based Access Control (RBAC): พนักงานเห็นเฉพาะข้อมูลตนเอง, Manager เห็นทีมที่รับผิดชอบ, HR เห็นทั้งองค์กร | NFR-005 | BRD §4 Actors, BRD §6 LEAVE_REQUEST |
| Sensitive Data Protection | ข้อมูลพนักงาน Outsource ได้รับการปกป้องเทียบเท่าพนักงานประจำ — เข้าถึงได้เฉพาะ HR และ Manager ที่รับผิดชอบ | NFR-006 | BRD §7.7 Outsource Onboarding, R3 (QA v2) |
| Authentication Approach | ข้อมูลไม่เพียงพอ — Authentication method (SSO / Active Directory / Username-Password) ยังไม่ได้รับการยืนยัน (ดู Section 17 Open Issues) | NFR-004, TR-008 | BRD BR-001, NFR-004 |
| Data Transmission Security | การรับส่งข้อมูลทุก request ต้องเข้ารหัสผ่าน HTTPS / TLS 1.2 หรือสูงกว่า | TR-007 | BRD §4 Actors (ทุก role เข้าถึงผ่าน Web) |
| Session Management | Session ต้องมี timeout ตามนโยบาย IT — ทุก request ต้องผ่าน authentication | NFR-004 | BRD BR-001, SFR-001 |
| Audit Trail | ระบบต้องเก็บ audit log ทุก action ในรูปแบบ immutable — query ได้ (Phase 2) | TR-009 | BRD §3.1 BR-009 |

> **Open Issue:** Authentication method ยังไม่ยืนยัน — SSO หรือ standalone auth กระทบ Screen SRS (SF-001), TR-008, NFR-004 — ต้องยืนยันกับทีม IT

---

## 7. Usability & Accessibility

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Usability Goal | ระบบต้องใช้ง่ายพอที่ผู้ใช้ทุกกลุ่มสามารถยื่นลาได้โดยไม่ต้องผ่านการฝึกอบรมเข้มข้น | NFR-009 | BRD §5.3.1.C KPI |
| Adoption KPI | Adoption rate ≥ 95% ภายใน 3 เดือนหลัง go-live (วัดจาก % คำขอลาที่ผ่านระบบ vs คำขอรวมทั้งหมด) | NFR-009 | BRD §5.3.1.C KPI (Adoption Rate ≥95%) |
| Supported User Types | พนักงานประจำ (ทุกระดับ), Outsource, Line Manager, HR — รองรับความหลากหลายของ tech-savvy level | NFR-008, NFR-009 | BRD §4 Actors |
| Accessibility Consideration | ข้อมูลไม่เพียงพอ — WCAG หรือมาตรฐาน accessibility อื่นยังไม่ระบุ | — | ดู Section 17 |

---

## 8. Responsive / Device Support

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Supported Devices | Desktop Browser และ Mobile Browser (Responsive Web) — ไม่พัฒนา Mobile Native App | NFR-008 | BRD §3.2 Out of Scope (ไม่ใช้ Mobile native) |
| Supported Browsers | Chrome, Edge, Safari รุ่นล่าสุด (Latest version at time of release) | NFR-008, TR-001 | BRD §1 Business Objective |
| Display Approach | Responsive Web Design — ปรับ layout ตามขนาดหน้าจออัตโนมัติ (Desktop/Tablet/Mobile) | NFR-008, TR-001 | BRD §3.4 สถาปัตยกรรม |
| Native App | ไม่มีแผนพัฒนา native app สำหรับ iOS/Android | — | BRD §3.2 Out of Scope |

---

## 9. Technology Stack (ภาพรวมเทคโนโลยี)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Platform | Web Application — HTML5/CSS3/JavaScript standard | TR-001 | BRD §3.4 สถาปัตยกรรม |
| Preferred Language | ข้อมูลไม่เพียงพอ — Programming language ยังไม่ยืนยัน | — | ดู Section 17 |
| Framework / Platform | ข้อมูลไม่เพียงพอ — Frontend/Backend framework ยังไม่ยืนยัน | — | ดู Section 17 |
| Email Integration | รองรับ SMTP server หรือ Cloud Email Gateway — ต้องรองรับ TLS, retry mechanism, delivery status | TR-003, SIR-002 | BRD BR-019, SRS §4.5 IF-002 |
| SLA Scheduler | Background job scheduler สำหรับ SLA timer — delay tolerance ≤ 15 นาที | TR-004, SIR-004 | BRD BR-018, M3 (QA v3) |
| File Handling | รองรับ upload: PDF, JPG, PNG — max size ยังไม่ยืนยัน | TR-005 | BRD BR-006 |
| Excel Import | รองรับ .xlsx import พร้อม validate 7 required fields | TR-006, SIR-003 | BRD BR-020 |
| Data Transmission | HTTPS / TLS 1.2 หรือสูงกว่า สำหรับทุก HTTP request | TR-007 | BRD §4 Actors |
| Authentication | ข้อมูลไม่เพียงพอ — SSO / Active Directory / standalone ยังไม่ยืนยัน | TR-008 | BRD BR-001, Open Issue |

> **Open Issue:** Technology Stack (Language, Framework, Runtime) ยังไม่ได้รับการยืนยัน — ต้องยืนยันก่อน Architecture Design phase

---

## 10. Database & Data Management

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Database Type | ข้อมูลไม่เพียงพอ — Relational / NoSQL ยังไม่ยืนยัน | — | ดู Section 17 |
| Data Storage Approach | ต้องเก็บ Leave Request, Leave Balance, Notification Log ในลักษณะที่ query ได้ — สนับสนุน RBAC ระดับ row | NFR-005, NFR-010 | BRD §6 Business Entity |
| Leave Balance Integrity | วันลาคงเหลือต้องอัปเดตอย่างถูกต้องทุกครั้งที่มีการ Approve หรือ Cancel (Approve สำเร็จ) — ห้าม inconsistency | NFR-010 | BRD BR-016, NR-001 |
| Audit Log Storage | Audit log ต้องอยู่ในรูปแบบ immutable — ห้ามลบหรือแก้ไข (Phase 2) | TR-009 | BRD §3.1 BR-009 |
| Audit Log Retention | ข้อมูลไม่เพียงพอ — Retention period ยังไม่ยืนยัน | TR-009 | SRS §7 Open Issue |
| Backup Approach | ข้อมูลไม่เพียงพอ — Backup policy ยังไม่กำหนด | — | ดู Section 17 |
| Long-term Data Retention | ข้อมูลไม่เพียงพอ — Data retention policy ยังไม่กำหนด | — | ดู Section 17 |

---

## 11. Integration Protocols (โปรโตคอลการเชื่อมต่อ)

| Protocol / Approach | Description | Use Case / Requirement | หมายเหตุ |
|--------------------|-------------|----------------------|---------|
| REST API (Candidate) | ข้อมูลไม่เพียงพอ — อาจใช้สำหรับ HRIS integration หากเปิด API | IF-001: HRIS → Leave App | ขึ้นกับ HRIS capability ที่ยังไม่ยืนยัน |
| SMTP / Email API | ส่ง Email notification ผ่าน SMTP server หรือ Cloud Email API พร้อม TLS และ retry | IF-002: Leave App → Email Gateway | TR-003, NFR-007 |
| File Transfer / File-based (Candidate) | ข้อมูลไม่เพียงพอ — อาจใช้สำหรับ HRIS integration แบบ batch file export | IF-001 (กรณี HRIS ไม่มี API) | ขึ้นกับ HRIS capability |
| Excel File Upload | HR upload .xlsx ผ่าน Web UI — ระบบ parse และ validate | IF-003: Outsource Import | TR-006, SFR-012 |
| File Storage API | Upload/Download ไฟล์ใบรับรองแพทย์ (PDF/JPG/PNG) — type ยังไม่ยืนยัน | IF-004: Medical Certificate | TR-005, SIR-005 |
| Internal Scheduler | Background timer สำหรับ SLA reminder/escalate — ไม่ใช่ external integration | IF-005: SLA Timer | TR-004, NFR-011 |
| Database Connection | Internal connection ระหว่าง Application Server และ Database | Internal | ประเภท DB ยังไม่ยืนยัน |

---

## 12. Reliability (ความเสถียรของระบบ)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Continuous Operation | SLA Scheduler (IF-005) ต้องทำงานได้ 24/7 ไม่หยุดนอกเวลาทำการ | TR-004, NFR-011 | BRD BR-018, M3 (QA v3) |
| Email Reliability | Email notification ต้องส่งสำเร็จ ≥ 99% — retry อย่างน้อย 3 ครั้งกรณีล้มเหลว | NFR-007, SIR-002 | BRD §5.3.1.C KPI (Email success ≥99%) |
| Data Integrity on Concurrent Update | หลัง Approve Cancel Request: วันลาต้องคืนถูกต้องทันที — ต้องป้องกัน race condition กรณี concurrent action | NFR-010 | BRD BR-016, NR-001 |
| Incident Handling Approach | ข้อมูลไม่เพียงพอ — Incident response process ยังไม่กำหนด | — | ดู Section 17 |
| Recovery Approach | ข้อมูลไม่เพียงพอ — RTO/RPO ยังไม่กำหนด | — | ดู Section 17 |

---

## 13. Scalability (การรองรับการขยายตัว)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Current Scale | ข้อมูลไม่เพียงพอ — จำนวนพนักงานรวม (ประจำ + Outsource) ยังไม่ระบุ กระทบการออกแบบ capacity | NFR-001, NFR-003 | SRS §7 Open Issue |
| Growth Handling Approach | ข้อมูลไม่เพียงพอ — Horizontal/Vertical scaling strategy ยังไม่กำหนด | — | ดู Section 17 |
| Future Expansion Direction | Phase 2 เพิ่ม Report Export, Audit Trail, Notification Log — ควรออกแบบ database schema รองรับตั้งแต่ Phase 1 | TR-009, RFR-001, RFR-002, RFR-003 | BRD §3.1 Phase 2 items |

---

## 14. Monitoring & Logging

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Email Delivery Monitoring | ต้อง log delivery status ทุก Email event (Success/Failed/Retry count) — HR ตรวจสอบผ่าน RP-003 Notification Log Report | NFR-007, SFR-013, RFR-003 | BRD BR-019, BRD §5.3.1.C KPI |
| SLA Scheduler Monitoring | ต้อง monitor scheduler lag — alert เมื่อ delay เกิน 15 นาที | NFR-011, TR-004 | BRD BR-018 |
| Audit Log | ต้องเก็บ log ทุก action (create/approve/reject/cancel) พร้อม timestamp และ user_id (Phase 2) | TR-009 | BRD §3.1 BR-009 |
| Important Event Logging | Leave Request สร้าง, Status เปลี่ยน, Cancel Request สร้าง, SLA trigger, Excel Import result, File Upload result | SFR-013 (implied) | BRD §7.6 Notification Events |
| Monitoring Approach | ข้อมูลไม่เพียงพอ — Monitoring tool (APM, log aggregator, dashboard) ยังไม่กำหนด | — | ดู Section 17 |
| Alerting Approach | ข้อมูลไม่เพียงพอ — Alert channel (email / Slack / PagerDuty) ยังไม่กำหนด | — | ดู Section 17 |

---

## 15. Deployment & Environment (ภาพรวม)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Environment Landscape | ข้อมูลไม่เพียงพอ — Dev / Test / UAT / Production environment structure ยังไม่กำหนด | — | ดู Section 17 |
| Deployment Approach | ข้อมูลไม่เพียงพอ — On-premise / Cloud / Hybrid ยังไม่ยืนยัน | — | ดู Section 17 |
| Deployment Constraint | ระบบต้องรองรับ Chrome, Edge, Safari รุ่นล่าสุด — ไม่ต้องการ plugin หรือ native app | TR-001, NFR-008 | BRD §3.2 Out of Scope |
| HRIS Integration Environment | ต้องมี environment ที่เชื่อมกับ HRIS (staging/production) เพื่อทดสอบ integration จริง | TR-002, SIR-001 | BRD §3.4 HRIS Integration |

---

## 16. Compliance / Policy (ถ้ามี)

| รายการ | รายละเอียด | Requirement ID | Source Reference |
|-------|-----------|--------------|----------------|
| Data Security Transmission | ต้องใช้ TLS 1.2+ สำหรับทุก request — ไม่อนุญาต HTTP plain | TR-007 | BRD §4 Actors |
| RBAC Compliance | ทุก user เห็นเฉพาะข้อมูลที่ตนมีสิทธิ์ — กำหนดตาม role: Employee, Manager, HR | NFR-005 | BRD §4 Actors |
| Leave Law Compliance | สิทธิ์วันลาต้องไม่ต่ำกว่าที่กฎหมายแรงงานกำหนด — ระบบต้องรองรับ quota ที่ HR กำหนด | BRD BR-002, BR-010 | BRD §7 Constraints |
| Data Privacy — Outsource | ข้อมูล Outsource ที่ import เข้าระบบต้องได้รับการปกป้องเทียบเท่าพนักงานประจำ | NFR-006 | BRD §7.7 |
| Audit Trail Immutability | Audit log ต้องไม่สามารถลบหรือแก้ไขได้ (Phase 2) | TR-009 | BRD §3.1 BR-009 |
| Organization Policy | ข้อมูลไม่เพียงพอ — นโยบาย IT security และ data governance ขององค์กร ABC Company ยังไม่ได้รับ | — | ดู Section 17 |

---

## 17. Assumptions / Constraints

### 17.1 Confirmed Assumptions (จาก SRS Summary §7)

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Assumption | ระบบใหม่ integrate กับ HRIS เดิม — ไม่ replace HRIS | กำหนดขอบเขต IF-001 ให้เป็น read-only จาก HRIS | BRD §3.4, QA-H6 |
| Assumption | พนักงานทุกคน (รวม Outsource) มี email address ที่ active | Email notification ทำงานได้ทุก event สำหรับทุก recipient | SRS §7 |
| Assumption | Line Manager ของพนักงานแต่ละคนมีข้อมูลใน HRIS หรือใน import field | Approval routing ทำงานถูกต้อง | SRS §7 |

### 17.2 Open Issues (ข้อมูลยังไม่เพียงพอ)

| Open Issue | รายละเอียด | ผลกระทบต่อ NFR/TR | สิ่งที่ต้องยืนยัน |
|-----------|-----------|-----------------|----------------|
| **Authentication Method** | SSO / Active Directory / standalone username-password ยังไม่ยืนยัน | NFR-004, TR-008, SF-001 | ทีม IT ยืนยัน identity provider |
| **HRIS Integration Pattern** | API / DB link / File-based export ยังไม่ยืนยัน | TR-002, SIR-001, IF-001 | ทีม IT + HRIS vendor ยืนยัน capability |
| **Email Server / Provider** | SMTP / SES / SendGrid / Exchange Online ยังไม่ระบุ | TR-003, SIR-002, IF-002 | ทีม IT ยืนยัน |
| **File Storage Type** | Local server / Cloud S3 / Azure Blob / DB blob ยังไม่ยืนยัน | TR-005, SIR-005, IF-004 | ทีม IT ยืนยัน |
| **Max File Size** | ใบรับรองแพทย์ — max upload size ยังไม่ยืนยัน | TR-005, VR-007 | HR + IT ยืนยัน |
| **Concurrent Users / Load** | จำนวนพนักงานรวมและ expected peak load ยังไม่มีข้อมูล | NFR-001, NFR-003 | HR / IT ยืนยันจำนวนพนักงาน |
| **Audit Log Retention Period** | ระยะเวลาเก็บ log ยังไม่กำหนด | TR-009 (Phase 2) | HR / Legal ยืนยัน |
| **Carry-forward Formula** | รู้ว่า cap = 30 วัน แต่ไม่ชัดว่าคำนวณยอดสะสมอย่างไร (pro-rata หรือเต็มจำนวน) | NFR-010, NFR-002 | HR ยืนยัน formula |
| **HR Email Recipient** | Individual email หรือ Distribution List สำหรับ notification | SFR-013, IF-002 | HR ยืนยัน |
| **SLA Escalate Assignee** | ตำแหน่ง/email ของ HR ที่รับ Escalation Email ยังไม่ระบุ | SFR-010, IF-002, IF-005 | HR ยืนยัน |
| **Technology Stack** | Programming language, Framework, Database type ยังไม่ยืนยัน | TR-001–TR-009 (indirect) | ทีม IT / ทีม Architecture ยืนยัน |
| **Backup & Recovery Policy** | Backup frequency, RTO, RPO ยังไม่กำหนด | NFR-003 (indirect) | ทีม IT ยืนยัน |
| **Deployment Model** | On-premise / Cloud / Hybrid ยังไม่ยืนยัน | TR-001–TR-009 | ทีม IT ยืนยัน |
| **Working Hours for SLA** | "1 วันทำการ" สำหรับ SLA นับอย่างไร (กี่ชั่วโมง, วันหยุดนักขัตฤกษ์รวมหรือไม่) | NFR-011, TR-004, IF-005 | HR ยืนยัน |

---

## 18. Non-Functional Requirement Summary (Traceability)

| Req ID | Category | Requirement Description | Measure / Acceptance Basis | Status | Source BRD Reference |
|--------|---------|------------------------|--------------------------|--------|---------------------|
| NFR-001 | Performance — Response Time | หน้าจอหลักโหลด ≤ 3 วินาที (P95) | Page load ≤ 3s (P95) — load volume ยังไม่ยืนยัน | Confirmed (partial) | BRD §5.3.1.C KPI |
| NFR-002 | Performance — Balance Calculation | Leave balance แสดงใน ≤ 2 วินาทีหลัง login | ≤ 2s | Confirmed | BRD §3.1 BR-002 |
| NFR-003 | Availability | ≥ 99% ในช่วงเวลาทำการ | Availability ≥ 99% (excluding planned maintenance) | Confirmed (partial) | BRD §3.1 In Scope |
| NFR-004 | Security — Authentication | ทุก request ต้องผ่าน authentication | Session required — method ยังไม่ยืนยัน | Open Issue | BRD BR-001 |
| NFR-005 | Security — Authorization | RBAC — Employee/Manager/HR เห็นข้อมูลตาม role | Employee A ไม่เห็นข้อมูล Employee B | Confirmed | BRD §4 Actors |
| NFR-006 | Security — Data Privacy | Outsource data ปกป้องเทียบเท่าพนักงานประจำ | เข้าถึงเฉพาะ HR + Manager ที่รับผิดชอบ | Confirmed | BRD §7.7, R3 (QA v2) |
| NFR-007 | Reliability — Notification | Email success rate ≥ 99%, retry ≥ 3 ครั้ง | Success rate ≥ 99% | Confirmed | BRD §5.3.1.C KPI, BR-019 |
| NFR-008 | Usability — Responsive Web | Chrome/Edge/Safari latest, Desktop+Mobile browser | ใช้งานได้บน 3 browsers, Desktop+Mobile | Confirmed | BRD §3.2 Out of Scope |
| NFR-009 | Usability — Adoption | Adoption rate ≥ 95% ภายใน 3 เดือน | ≥ 95% คำขอผ่านระบบ (vs total) ภายใน 3 เดือน | Confirmed | BRD §5.3.1.C KPI |
| NFR-010 | Data Integrity — Leave Balance | Balance อัปเดตถูกต้องทุกครั้งที่ Approve/Cancel | Approve: balance ลด, Cancel Approve: balance คืน — automated test | Confirmed | BRD BR-016, NR-001 |
| NFR-011 | SLA Compliance | SLA Reminder/Escalate ทำงาน delay ≤ 15 นาที | Scheduler delay ≤ 15 นาที — monitor log | Confirmed | BRD BR-018, M3 (QA v3) |

---

## 19. Technical Requirement Summary (Traceability)

| Req ID | Category | Technical Requirement Description | Constraint / Standard | Status | Source BRD Reference |
|--------|---------|----------------------------------|---------------------|--------|---------------------|
| TR-001 | Platform — Web Application | Web App รองรับ Chrome/Edge/Safari latest, HTML5 | ไม่ใช่ native app | Confirmed | BRD §3.4, §3.2 Out of Scope |
| TR-002 | Integration — HRIS | Integrate กับ HRIS เดิม — ไม่ replace | Pattern (API/Batch/File) ยังไม่ยืนยัน | Open Issue | BRD §3.4, QA-H6 |
| TR-003 | Integration — Email | SMTP/Cloud Email Gateway — TLS, retry, delivery log | Email provider ยังไม่ยืนยัน | Open Issue | BRD BR-019, SIR-002 |
| TR-004 | Scheduler / Timer | Background job สำหรับ SLA timer — 24/7, delay ≤ 15 นาที | Job delay ≤ 15 นาที | Confirmed | BRD BR-018, SIR-004 |
| TR-005 | File Upload | รองรับ PDF/JPG/PNG upload — max size ยังไม่ยืนยัน | Max size ยังไม่ยืนยัน | Open Issue (partial) | BRD BR-006, SIR-005 |
| TR-006 | Import — Excel | รองรับ .xlsx import — validate 7 required fields | .xlsx only, error report by row/field | Confirmed | BRD BR-020, SIR-003 |
| TR-007 | Security — Data Transmission | HTTPS ทุก request | TLS 1.2 หรือสูงกว่า | Confirmed | BRD §4 Actors |
| TR-008 | Authentication Method | ยังไม่ยืนยัน — SSO/AD/Password | ต้องยืนยันกับทีม IT | Open Issue | BRD BR-001, NFR-004 |
| TR-009 | Audit Log Storage | Immutable audit log, query ได้ (Phase 2) | Retention period ยังไม่ยืนยัน | Phase 2 / Open Issue | BRD §3.1 BR-009 |

---

## 20. Source Reference

- `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md`
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md`
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml`
