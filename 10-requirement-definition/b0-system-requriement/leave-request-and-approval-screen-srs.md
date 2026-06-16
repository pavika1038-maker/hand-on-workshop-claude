---
title: "Screen SRS Document"
document_type: "Screen SRS"
version: "1.1"
language: "th"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
status: "Draft"
last_updated: "2026-06-16"
---

# Screen SRS Document: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)

## Change Log

| Version | Date | Section | Change Type | Description | Source |
|---------|------|---------|-------------|-------------|--------|
| 1.0 | 2026-06-16 | All | Created | สร้างเอกสารครั้งแรก — SF-001 ถึง SF-015 จาก SRS Summary baseline | SRS Summary v1.0 (BRD baseline) |
| 1.1 | 2026-06-16 | SF-003 §2.3.5, §2.3.7 | Added | เพิ่ม field `half_day_period` ใน Leave Request Fields Definition และ Screen Behavior | Meeting Note 2026-06-10 |

---

## 1. เอกสารอ้างอิงและขอบเขต

เอกสารนี้ลงรายละเอียด Screen Function ระดับหน้าจอและพฤติกรรมเชิงธุรกิจสำหรับระบบบริหารการลาและการอนุมัติ อ้างอิงจาก SRS Summary เป็น baseline หลัก ครอบคลุม Phase 1 (SF-001–SF-012) และ Phase 2 (SF-013–SF-015)

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
|-------|-------------|--------------|
| 1 | `a0-business-requirement/brd/leave-request-and-approval-brd.md` | ต้นทาง requirement เชิงธุรกิจ |
| 2 | `b0-system-requriement/leave-request-and-approval-srs-summary.md` | baseline ของ Screen Function Requirement |
| 3 | `91-project-asses/ascii-mockup/report/report-mockup-index.md` | แหล่งอ้างอิง mockup สำหรับ Report screens |
| 4 | `requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`, `v3.yaml` | เอกสารยืนยัน business rules |

### 1.2 Function Index

| Function ID | Function Name | Screen Name | Actor / User Role | Related Requirement IDs | Source Reference | หมายเหตุ |
|------------|--------------|------------|------------------|------------------------|----------------|---------|
| SF-001 | Login / Authentication | Login Page (SCR-001) | พนักงานทุกกลุ่ม, Manager, HR | SFR-001, TR-001, TR-008, NFR-004 | SRS §4.1 SFR-001, BRD BR-001 | |
| SF-002 | Leave Balance Dashboard | Leave Balance (SCR-002) | Employee, Outsource | SFR-002, VR-002, VR-003, VR-004, VR-008 | SRS §4.1 SFR-002, BRD BR-002, BR-007–BR-010 | |
| SF-003 | Submit Leave Request | Submit Leave (SCR-003) | Employee, Outsource | SFR-003, VR-001–VR-007, VR-011 | SRS §4.1 SFR-003, BRD BR-003–BR-007, BR-011 | |
| SF-004 | Manager Approval Inbox | Approval Inbox (SCR-004) | Line Manager | SFR-004, SFR-005, NFR-005 | SRS §4.1 SFR-004/005, BRD BR-012 | |
| SF-005 | Approve / Reject Action | Approval Inbox (SCR-004) | Line Manager | SFR-005, VR-010 | SRS §4.1 SFR-005, BRD BR-012, BR-013 | |
| SF-006 | Leave Request Status Tracking | Leave Request Detail (SCR-005) | Employee, Outsource, HR | SFR-006, SCR-005 | SRS §4.1 SFR-006, BRD BR-006 | |
| SF-007 | Cancel Leave — Pending | Cancel Leave (SCR-006) | Employee, Outsource | SFR-007, VR-009, VR-010 | SRS §4.1 SFR-007, BRD BR-014 | |
| SF-008 | Cancel Leave — Approved | Cancel Leave (SCR-006) | Employee, Outsource | SFR-008, VR-009 | SRS §4.1 SFR-008, BRD BR-015, NR-001, NR-002 | |
| SF-009 | Re-approve Cancel Request | Re-approve Cancel (SCR-007) | Line Manager | SFR-009, SFR-010, VR-012 | SRS §4.1 SFR-009/010, BRD BR-015, BR-016, BR-018 | |
| SF-010 | SLA Reminder & Escalation | (Background / System) | System | SFR-010, SIR-004, IF-005, NFR-011 | SRS §4.1 SFR-010, BRD BR-018, M3 (QA v3) | ไม่มี UI หลัก |
| SF-011 | HR Monitoring Dashboard | HR Monitoring (SCR-008) | HR | SFR-011, RFR-001 | SRS §4.1 SFR-011, BRD BR-007 | |
| SF-012 | Outsource Data Import | Outsource Import (SCR-009) | HR | SFR-012, SIR-003, VR-013 | SRS §4.1 SFR-012, BRD BR-020 | |
| SF-013 | Leave History & Audit Trail | Leave Detail (SCR-005) | HR, Employee | SFR-014, TR-009 | SRS §4.1 SFR-014, BRD BR-009 | Phase 2 |
| SF-014 | Leave Report Export | Leave Report (SCR-010) | HR | SFR-015, RFR-001, RFR-002 | SRS §4.1 SFR-015, BRD BR-010 | Phase 2 |
| SF-015 | Notification Log View | Notification Log (SCR-011) | HR | RFR-003, SFR-013 | SRS §4.2 RFR-003, BRD BR-019 | Phase 2 |

---

## 2. Detailed Function Specification

---

### 2.1 SF-001 Login / Authentication

#### 2.1.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-001 |
| Function Name | Login / Authentication |
| Description | ระบบตรวจสอบตัวตนผู้ใช้ก่อนอนุญาตให้เข้าใช้งาน รองรับพนักงานทุกกลุ่ม (ประจำ, Outsource, Manager, HR) |
| Business Purpose | ป้องกัน unauthorized access — ให้แต่ละ role เห็นเฉพาะข้อมูลที่ได้รับอนุญาต |
| Actor / User Role | พนักงานประจำ, Outsource, Line Manager, HR |
| Related Requirement IDs | SFR-001, NFR-004, NFR-005, TR-001, TR-007, TR-008 |
| Source Reference | SRS §4.1 SFR-001, BRD BR-001, QA-H6 |

#### 2.1.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Login Page (SCR-001) |
| Screen Description | หน้าแรกของระบบสำหรับยืนยันตัวตน ก่อนเข้าสู่ระบบหลัก |
| Navigation Inbound | URL โดยตรง / Redirect จากทุกหน้าเมื่อ session หมดอายุ |
| Navigation Outbound | Leave Balance Dashboard (SCR-002) เมื่อ Login สำเร็จ |
| Preconditions | ผู้ใช้มี account ในระบบ (พนักงานประจำ: sync จาก HRIS / Outsource: import แล้ว) |
| Postconditions | Session ถูกสร้าง, role ถูก identify, redirect ไปหน้าหลักตาม role |

#### 2.1.3 Mockup / UI Layout

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง |
| Mockup Version | — |
| Layout Description | หน้าจอ Login ประกอบด้วย Logo, ช่อง Username, ช่อง Password, ปุ่ม Login |
| Additional Notes | Authentication method ยังไม่ยืนยัน (SSO vs standalone) — ดู SRS §7 Open Issue |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.1.4 Tabs

ไม่มี

#### 2.1.5 Fields Definition

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | Default Value | เงื่อนไขการกรอก | Description | Sample Data |
|-----------|-------------|------------|---------|-------------|--------------|-------------|------------|
| username | ชื่อผู้ใช้ / Username | ข้อความ | Y | — | กรอกทุกครั้ง | รหัสพนักงาน หรือ email (ขึ้นกับ auth method ที่ยืนยัน) | emp001 |
| password | รหัสผ่าน / Password | Password (masked) | Y | — | กรอกทุกครั้ง | รหัสผ่านของผู้ใช้ | ข้อมูลไม่เพียงพอ (SSO อาจไม่มี field นี้) |

#### 2.1.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| Login | ยืนยันตัวตนและเข้าสู่ระบบ | Username และ Password กรอกครบ | ตรวจสอบ credential — สำเร็จ: สร้าง session + redirect SCR-002, ล้มเหลว: แสดง ERR-LGN-001 |
| Forgot Password | ขอ reset รหัสผ่าน | คลิกลิงก์ | ข้อมูลไม่เพียงพอ — ขึ้นกับ auth method |

#### 2.1.7 Screen Behavior

| สถานการณ์ | Trigger | Condition | Screen Behavior | หมายเหตุ |
|---------|--------|----------|----------------|---------|
| onLoad | เปิด URL ระบบ | — | แสดงหน้า Login, ล้าง session เก่า | |
| Login สำเร็จ | กด Login | Credential ถูกต้อง | ระบบระบุ role → redirect ไป SCR-002 | |
| Login ล้มเหลว | กด Login | Credential ผิด | แสดง ERR-LGN-001, ล้าง password field | |
| Session หมดอายุ | ใช้งานระบบอื่น | Session expired | Redirect กลับ SCR-001 พร้อม INF-LGN-001 | |

#### 2.1.8 Message List

##### Error Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| ERR-LGN-001 | Username หรือ Password ไม่ถูกต้อง | ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง กรุณาลองใหม่อีกครั้ง | Incorrect username or password. Please try again. |
| ERR-LGN-002 | Account ถูก lock | บัญชีถูกระงับชั่วคราว กรุณาติดต่อ HR | Your account has been temporarily locked. Please contact HR. |

##### Info Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| INF-LGN-001 | Session หมดอายุ | Session หมดอายุ กรุณา Login ใหม่ | Session expired. Please log in again. |

#### 2.1.9 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-001 | พนักงานทุกคนต้องมี account | ห้ามเข้าระบบโดยไม่มี account | BRD BR-001, SRS NFR-004 |
| BR-001-R | RBAC ตาม role | หลัง Login ระบบกำหนด role → แสดง menu และข้อมูลตาม role | SRS NFR-005 |

#### 2.1.10 Error Handling

| Error Type | Trigger Condition | System Behavior | User Message | Recovery |
|-----------|-----------------|----------------|-------------|---------|
| Validation | Credential ผิด | ไม่สร้าง session, ล้าง password | ERR-LGN-001 | กรอกใหม่ |
| System | ระบบ auth ล่ม | แสดง error page | "ระบบขัดข้องชั่วคราว กรุณาลองใหม่" | รอและ refresh |

#### 2.1.11 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
|-------|-----------|--------|
| Open Issue | Authentication method ยังไม่ยืนยัน (SSO vs standalone) | กระทบ field definition และ TR-008 |
| Assumption | พนักงาน Outsource ทุกคนมี email ที่ใช้ login ได้ | กระทบ VR-013 (import) และ SFR-001 |

---

### 2.2 SF-002 Leave Balance Dashboard

#### 2.2.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-002 |
| Function Name | Leave Balance Dashboard |
| Description | แสดงสิทธิ์วันลาคงเหลือของพนักงานแยกตามประเภทการลา คำนวณตามอายุงานและ employee_type |
| Business Purpose | ให้พนักงานตรวจสอบสิทธิ์ได้เองโดยไม่ต้องถาม HR |
| Actor / User Role | พนักงานประจำ, Outsource |
| Related Requirement IDs | SFR-002, VR-002, VR-003, VR-004, VR-008, SCR-002 |
| Source Reference | SRS §4.1 SFR-002, BRD BR-002, BR-007–BR-011, R6 (QA v2) |

#### 2.2.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Leave Balance Dashboard (SCR-002) |
| Screen Description | หน้าหลักหลัง login แสดงสิทธิ์วันลาคงเหลือแยกตามประเภท พร้อมลิงก์ไปยื่นคำขอลา |
| Navigation Inbound | SCR-001 (Login สำเร็จ), Header Navigation |
| Navigation Outbound | SCR-003 (Submit Leave Request), SCR-005 (Leave Request Detail) |
| Preconditions | Login สำเร็จ, มีข้อมูล employee ใน system |
| Postconditions | พนักงานเห็น balance ปัจจุบัน พร้อมดำเนินการต่อ |

#### 2.2.3 Mockup / UI Layout

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง |
| Layout Description | Card-based layout แสดงสิทธิ์แต่ละประเภทเป็น card, ปุ่ม "ยื่นคำขอลา", รายการคำขอล่าสุด |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.2.4 Tabs

ไม่มี

#### 2.2.5 Fields Definition (Display Only)

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | Default Value | เงื่อนไขการแสดง | Description | Sample Data |
|-----------|-------------|------------|---------|-------------|--------------|-------------|------------|
| leave_type | ประเภทการลา / Leave Type | ข้อความ (read-only) | Y | — | แสดงเฉพาะประเภทที่ employee_type มีสิทธิ์ | ชื่อประเภทการลา | ลาป่วย |
| entitled_days | สิทธิ์รวม / Total Entitlement | ตัวเลข (วัน) | Y | — | คำนวณจากอายุงาน | จำนวนวันสิทธิ์ทั้งหมด/ปี | 10 |
| used_days | ใช้ไปแล้ว / Used | ตัวเลข (วัน) | Y | 0 | — | วันที่ใช้ไปในปีนี้ | 3 |
| remaining_days | คงเหลือ / Remaining | ตัวเลข (วัน) | Y | — | = entitled + carry_forward − used | วันคงเหลือสุทธิ | 7 |
| carry_forward | สะสมจากปีก่อน / Carried Forward | ตัวเลข (วัน) | N | 0 | แสดงเฉพาะลาพักผ่อน, cap 30 วัน | วันสะสมจากปีที่แล้ว | 5 |
| reference_year | รอบปี / Year | ปี ค.ศ. | Y | ปีปัจจุบัน | — | รอบปีที่แสดง | 2026 |

#### 2.2.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ยื่นคำขอลา | ไปหน้า Submit Leave Request | คลิกปุ่ม | Navigate ไป SCR-003 |
| ดูประวัติคำขอ | ดูรายการคำขอในอดีต | คลิกลิงก์ / tab | แสดงรายการ Leave Request ของตนเอง |

#### 2.2.7 Screen Behavior

| สถานการณ์ | Trigger | Condition | Screen Behavior |
|---------|--------|----------|----------------|
| onLoad | เข้าหน้า | Login สำเร็จ | ดึง balance ตาม employee_id, คำนวณตาม employee_type + อายุงาน, แสดง card ตามประเภทที่มีสิทธิ์ |
| Outsource เข้าหน้า | onLoad | employee_type = Outsource | ซ่อน card ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท |
| Probation เข้าหน้า | onLoad | อายุงาน < 3 เดือน | แสดง card ลาพักผ่อนพร้อม label "ยังไม่มีสิทธิ์ (ช่วงทดลองงาน)" |

#### 2.2.8 Message List

##### Warning Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| WRN-BAL-001 | วันลาพักผ่อนคงเหลือ ≤ 2 วัน | สิทธิ์ลาพักผ่อนเหลือน้อย ({X} วัน) | Annual leave balance is low ({X} days remaining). |
| WRN-BAL-002 | วันสะสมใกล้ถึง cap 30 วัน | วันลาสะสมใกล้ถึงขีดสูงสุด (cap 30 วัน) | Carried-forward leave is near the 30-day cap. |

##### Info Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| INF-BAL-001 | อายุงาน < 3 เดือน | คุณอยู่ในช่วงทดลองงาน ยังไม่มีสิทธิ์ลาพักผ่อน | You are in probation period and not yet eligible for annual leave. |

#### 2.2.9 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-007 | อายุงาน < 3 เดือน ไม่มีสิทธิ์ลาพักผ่อน | แสดง balance = 0 พร้อม label probation | BRD BR-007, M2 (QA v3) |
| BR-008 | สิทธิ์ตามอายุงาน 5 tiers | คำนวณ entitled_days จาก hire_date | BRD BR-008, R6 (QA v2) |
| BR-009 | cap สะสม 30 วัน | carry_forward + entitled ≤ 30 สำหรับลาพักผ่อน | BRD BR-009 |
| BR-010 | ลากิจ 3 วัน/ปี | แสดง entitled_days = 3 สำหรับทุก employee_type | BRD BR-010, R1 |
| BR-011 | Outsource ไม่มีสิทธิ์ 4 ประเภท | ซ่อน card ตาม employee_type | BRD BR-011, R2 |

#### 2.2.10 Error Handling

| Error Type | Trigger Condition | System Behavior | User Message | Recovery |
|-----------|-----------------|----------------|-------------|---------|
| Integration | ดึง balance จาก HRIS ไม่ได้ | แสดงข้อมูลจาก local cache / แสดง error | "ไม่สามารถโหลดข้อมูลวันลาได้ กรุณา refresh" | Refresh หน้า |
| Data | ไม่พบข้อมูลพนักงาน | แสดง error page | "ไม่พบข้อมูลบัญชีของคุณ กรุณาติดต่อ HR" | ติดต่อ HR |

#### 2.2.11 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
|-------|-----------|--------|
| Open Issue | Carry-forward formula ยังไม่ยืนยัน | กระทบ remaining_days calculation |
| Assumption | ระบบคำนวณ balance real-time จากข้อมูลที่บันทึกในระบบ ไม่ใช่ดึงจาก HRIS ทุกครั้ง | กระทบ data sync design |

---

### 2.3 SF-003 Submit Leave Request

#### 2.3.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-003 |
| Function Name | Submit Leave Request |
| Description | พนักงานกรอกคำขอลา ระบบ validate หลายขั้นตอนก่อนบันทึก และ trigger notification |
| Business Purpose | แทนที่ใบลากระดาษ — ลดขั้นตอน ข้อมูลครบถ้วน มี validation อัตโนมัติ |
| Actor / User Role | พนักงานประจำ, Outsource |
| Related Requirement IDs | SFR-003, VR-001–VR-007, VR-011, SCR-003 |
| Source Reference | SRS §4.1 SFR-003, BRD BR-003–BR-007, BR-011, QA-H2 |

#### 2.3.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Submit Leave Request (SCR-003) |
| Screen Description | ฟอร์มยื่นคำขอลา — เลือกประเภทลา, วันที่, เหตุผล, แนบเอกสาร |
| Navigation Inbound | SCR-002 (ปุ่ม "ยื่นคำขอลา") |
| Navigation Outbound | SCR-002 (หลัง submit สำเร็จ หรือ Cancel), SCR-005 (ดูคำขอที่สร้าง) |
| Preconditions | Login สำเร็จ, employee มีสิทธิ์ลาอย่างน้อย 1 ประเภท |
| Postconditions | Leave Request สร้างแล้ว Status=Pending, Email แจ้ง Manager+HR |

#### 2.3.3 Mockup / UI Layout

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง |
| Layout Description | Form layout แนวตั้ง: Dropdown ประเภทลา, Date picker (from-to), คำนวณจำนวนวันอัตโนมัติ, Text area เหตุผล, File upload (conditional), ปุ่ม Submit / Cancel |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.3.4 Tabs

ไม่มี

#### 2.3.5 Fields Definition

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | Default Value | เงื่อนไขการกรอก | Description | Sample Data |
|-----------|-------------|------------|---------|-------------|--------------|-------------|------------|
| leave_type_id | ประเภทการลา / Leave Type | Dropdown (ตัวเลือก) | Y | — | แสดงเฉพาะประเภทที่ employee_type มีสิทธิ์ | เลือกประเภทการลา | ลาพักผ่อนประจำปี |
| start_date | วันที่เริ่มลา / Start Date | วันที่ (Date Picker) | Y | — | ≥ วันพรุ่งนี้ (ลาพักผ่อน), ≥ วันทำการถัดไป 3 วัน (ลากิจ), ≥ วันนี้ (ลาป่วย) | วันเริ่มต้นการลา | 2026-07-01 |
| end_date | วันที่สิ้นสุดลา / End Date | วันที่ (Date Picker) | Y | = start_date | ≥ start_date | วันสุดท้ายของการลา | 2026-07-03 |
| total_days | จำนวนวัน / Total Days | ตัวเลข (คำนวณอัตโนมัติ, read-only) | Y (auto) | — | คำนวณจาก start_date ถึง end_date (นับวันทำการ) — แสดง 0.5 เมื่อเลือก half-day | จำนวนวันลา | 3 |
| half_day_period | ช่วงเวลาลาครึ่งวัน / Half-Day Period | Dropdown (ครึ่งวันเช้า / ครึ่งวันบ่าย) | Conditional | — | แสดงและบังคับกรอกเมื่อ start_date = end_date และผู้ใช้เลือก "ลาครึ่งวัน" — ซ่อนเมื่อลามากกว่า 1 วัน | ระบุช่วงเวลา (เช้า/บ่าย) สำหรับการลาครึ่งวัน | ครึ่งวันเช้า (AM) |
| reason | เหตุผลการลา / Reason | ข้อความอิสระ (Textarea) | Y | — | — | เหตุผลการขอลา | ท่องเที่ยวประจำปีกับครอบครัว |
| medical_certificate | ใบรับรองแพทย์ / Medical Certificate | File Upload (PDF/JPG/PNG) | Conditional | — | บังคับเมื่อ leave_type = ลาป่วย AND total_days ≥ 3 วันทำการ | ไฟล์ใบรับรองแพทย์ | doctor_cert.pdf |

#### 2.3.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| Submit | ยืนยันคำขอลา | ทุก required field กรอกครบ, validation ผ่านทั้งหมด | บันทึก Leave Request (Status=Pending) → ส่ง Email แจ้ง Manager+HR → redirect SCR-005 |
| Cancel | ยกเลิกการกรอก | คลิกปุ่ม Cancel | แสดง confirm dialog → ถ้า confirm: กลับ SCR-002 โดยไม่บันทึก |

#### 2.3.7 Screen Behavior

| สถานการณ์ | Trigger | Condition | Screen Behavior |
|---------|--------|----------|----------------|
| onLoad | เปิดหน้า | — | โหลด dropdown ประเภทลาตาม employee_type, แสดง balance คงเหลือแต่ละประเภทเป็น hint |
| เปลี่ยน leave_type | onChange dropdown | — | ตรวจ eligibility (probation, Outsource restriction), อัปเดตกฎ validation, ซ่อน/แสดง field medical_certificate |
| เปลี่ยน start/end_date | onChange date | — | คำนวณ total_days อัตโนมัติ (นับเฉพาะวันทำการ), ตรวจ advance notice rule — ถ้า start_date = end_date: แสดง option "ลาครึ่งวัน" |
| เลือก "ลาครึ่งวัน" | onChange / toggle | start_date = end_date AND ผู้ใช้เลือก half-day | แสดง field `half_day_period` (Dropdown: ครึ่งวันเช้า/ครึ่งวันบ่าย) เป็น required, อัปเดต total_days = 0.5 |
| ยกเลิก "ลาครึ่งวัน" | onChange / toggle | — | ซ่อน field `half_day_period`, reset total_days = คำนวณปกติ (1 วัน) |
| start_date ≠ end_date | onChange date | — | ซ่อน field `half_day_period` โดยอัตโนมัติ, ล้างค่าที่เลือกไว้ |
| total_days ≥ 3 (ลาป่วย) | onChange total_days | leave_type = ลาป่วย AND total_days ≥ 3 | แสดง field medical_certificate พร้อม label "บังคับแนบ" |
| กด Submit | onClick | — | รัน validation ทั้งหมด (VR-001–VR-007, VR-011) ตามลำดับ — ถ้าผ่านทั้งหมด: บันทึก |

#### 2.3.8 Message List

##### Error Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| ERR-LR-001 | ประเภทลาที่ Outsource ไม่มีสิทธิ์ (VR-001) | ไม่มีสิทธิ์ลาประเภทนี้ กรุณาติดต่อบริษัทต้นสังกัด | You are not eligible for this leave type. Please contact your staffing agency. |
| ERR-LR-002 | สิทธิ์วันลาไม่เพียงพอ (VR-002) | สิทธิ์วันลาไม่เพียงพอ คงเหลือ {X} วัน | Insufficient leave balance. You have {X} days remaining. |
| ERR-LR-003 | อยู่ในช่วงทดลองงาน (VR-003) | ยังอยู่ในช่วงทดลองงาน ไม่มีสิทธิ์ลาพักผ่อน | You are in the probation period and not eligible for annual leave. |
| ERR-LR-004 | อายุงานไม่ถึง 1 ปี (VR-004) | อายุงานยังไม่ครบ 1 ปี ไม่มีสิทธิ์ลาพักผ่อนประจำปี | Your service period has not reached 1 year. Annual leave is not available yet. |
| ERR-LR-005 | ลาพักผ่อนไม่ได้แจ้งล่วงหน้า 1 วัน (VR-005) | ลาพักผ่อนต้องแจ้งล่วงหน้าอย่างน้อย 1 วัน | Annual leave requires at least 1 day advance notice. |
| ERR-LR-006 | ลากิจไม่ได้แจ้งล่วงหน้า 3 วันทำการ (VR-006) | ลากิจต้องแจ้งล่วงหน้าอย่างน้อย 3 วันทำการ | Personal leave requires at least 3 working days advance notice. |
| ERR-LR-007 | ลาป่วย ≥ 3 วัน ไม่มีใบรับรองแพทย์ (VR-007) | กรุณาแนบใบรับรองแพทย์สำหรับการลาป่วย 3 วันขึ้นไป | A medical certificate is required for sick leave of 3 or more consecutive working days. |
| ERR-LR-008 | ใช้สิทธิ์ครบ quota แล้ว (VR-011) | ใช้สิทธิ์ {ประเภทลา} ครบแล้ว ({X} วัน/ปี) สิทธิ์คงเหลือ 0 วัน | {Leave type} quota is exhausted ({X} days/year). No remaining balance. |

##### Success Message
| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) |
|-----------|-----------------|-----------------|-----------------|
| SUC-LR-001 | Submit สำเร็จ | ยื่นคำขอลาสำเร็จ อยู่ระหว่างรอการอนุมัติ | Leave request submitted successfully. Awaiting approval. |

#### 2.3.9 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-003 | ลาพักผ่อนแจ้งล่วงหน้า ≥ 1 วัน | VR-005: block submit หาก start_date ≤ วันนี้ | BRD BR-003, QA-H2 |
| BR-004 | ลากิจแจ้งล่วงหน้า ≥ 3 วันทำการ | VR-006: block submit หาก start_date < วันทำการที่ 4 นับจากวันนี้ | BRD BR-004 |
| BR-005 | ลาป่วยฉุกเฉินไม่ต้องแจ้งล่วงหน้า | ลาป่วย: ไม่มี advance notice validation | BRD BR-005 |
| BR-006 | ลาป่วย ≥ 3 วันทำการต่อเนื่อง ต้องแนบใบรับรองแพทย์ | VR-007: field medical_certificate เป็น required | BRD BR-006 |
| BR-007 | Probation < 3 เดือน ไม่มีสิทธิ์ลาพักผ่อน | VR-003: block submit | BRD BR-007, M2 (QA v3) |
| BR-011 | Outsource ไม่มีสิทธิ์ 4 ประเภท | VR-001: ซ่อน dropdown option, block หากเลือกได้ | BRD BR-011, R2 (QA v2) |

#### 2.3.10 Error Handling

| Error Type | Trigger Condition | System Behavior | User Message | Recovery |
|-----------|-----------------|----------------|-------------|---------|
| Validation | Validation ไม่ผ่าน | ไม่บันทึก, highlight field ที่ผิด | ERR-LR-001 ถึง ERR-LR-008 ตามกรณี | แก้ไข field แล้ว submit ใหม่ |
| Integration | Email แจ้ง Manager ส่งไม่สำเร็จ | บันทึก Request แล้ว, queue retry Email | INF: "บันทึกคำขอสำเร็จ — อาจมีความล่าช้าในการแจ้งหัวหน้างาน" | ระบบ retry อัตโนมัติ |
| System | ระบบล่มขณะ submit | แสดง error, ไม่บันทึก | "เกิดข้อผิดพลาด กรุณาลองใหม่" | Submit ใหม่ |

#### 2.3.11 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
|-------|-----------|--------|
| Open Issue | Max file size ของใบรับรองแพทย์ยังไม่ยืนยัน | กระทบ VR-007, IF-004 |
| Assumption | total_days นับวันทำการ (ไม่รวมวันหยุดราชการ/วันหยุดบริษัท) | ต้องมี holiday calendar ในระบบ |

---

### 2.4 SF-004 Manager Approval Inbox

#### 2.4.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-004 |
| Function Name | Manager Approval Inbox |
| Description | แสดงรายการคำขอลาที่รอการอนุมัติจากหัวหน้างาน พร้อม context ประวัติลาและสิทธิ์คงเหลือ |
| Business Purpose | ให้หัวหน้างานจัดการคำขอลาได้เร็วและมีข้อมูลประกอบการตัดสินใจครบถ้วน |
| Actor / User Role | Line Manager |
| Related Requirement IDs | SFR-004, SFR-005, NFR-005, SCR-004 |
| Source Reference | SRS §4.1 SFR-004/005, BRD BR-012, BR-013 |

#### 2.4.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Manager Approval Inbox (SCR-004) |
| Screen Description | Inbox รายการคำขอลารอ Approve — แสดงข้อมูลพนักงาน, ประเภทลา, วันที่, สิทธิ์คงเหลือ |
| Navigation Inbound | Header Navigation / Email notification link |
| Navigation Outbound | คลิกรายการ → SCR-005 (Detail), Action Approve/Reject ทำได้ใน inbox |
| Preconditions | Login เป็น Manager, มีคำขอรอ Approve อย่างน้อย 1 รายการ |
| Postconditions | คำขอถูก Approved หรือ Rejected, Email แจ้งพนักงาน + HR |

#### 2.4.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.4.4 Tabs

| Tab ID | Tab Name | Description | Actor | Default |
|--------|---------|------------|-------|---------|
| TAB-01 | รอดำเนินการ / Pending | คำขอที่ยังรอ Approve | Manager | Y |
| TAB-02 | ดำเนินการแล้ว / Processed | คำขอที่ Approve/Reject ไปแล้ว | Manager | N |

#### 2.4.5 Fields Definition (List View)

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | เงื่อนไขการแสดง | Description | Sample Data |
|-----------|-------------|------------|---------|--------------|-------------|------------|
| request_no | เลขคำขอ / Request No. | ข้อความ (read-only) | Y | — | หมายเลขอ้างอิงคำขอ | LR-2026-00123 |
| employee_name | ชื่อพนักงาน / Employee | ข้อความ (read-only) | Y | — | ชื่อผู้ยื่นคำขอ | สมชาย ใจดี |
| leave_type | ประเภทการลา / Leave Type | ข้อความ (read-only) | Y | — | ประเภทการลาที่ขอ | ลาพักผ่อนประจำปี |
| leave_dates | วันที่ลา / Leave Dates | วันที่ (read-only) | Y | — | ช่วงวันที่ลา | 1–3 ก.ค. 2026 (3 วัน) |
| remaining_balance | สิทธิ์คงเหลือ / Balance | ตัวเลข (read-only) | Y | — | วันลาคงเหลือ ณ ขณะนั้น | 7 วัน |
| submitted_date | วันที่ยื่น / Submitted | วันที่ (read-only) | Y | — | วัน-เวลาที่ยื่นคำขอ | 2026-06-20 09:30 |
| status | สถานะ / Status | Badge (read-only) | Y | — | Pending / Approved / Rejected | Pending |

#### 2.4.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| Approve | อนุมัติคำขอลา | คลิกปุ่ม Approve บน row หรือใน Detail | อัปเดต Status→Approved, ส่ง Email แจ้งพนักงาน+HR, refresh list |
| Reject | ปฏิเสธคำขอลา | คลิกปุ่ม Reject | เปิด dialog ให้กรอกเหตุผล (optional) → confirm → Status→Rejected, Email แจ้งพนักงาน+HR |
| ดูรายละเอียด | ดู Leave Request Detail | คลิก row / ลิงก์ | Navigate ไป SCR-005 |

#### 2.4.7 Screen Behavior

| สถานการณ์ | Trigger | Condition | Screen Behavior |
|---------|--------|----------|----------------|
| onLoad | เปิดหน้า | role = Manager | โหลดรายการ Pending requests ของทีมที่ Manager รับผิดชอบ |
| กด Approve | onClick | Status = Pending | ยืนยัน → บันทึก Approval → Email → อัปเดต list |
| กด Reject | onClick | Status = Pending | เปิด Reject dialog (เหตุผล optional) → confirm → บันทึก Rejection |

#### 2.4.8 Message List

##### Success Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| SUC-APR-001 | Approve สำเร็จ | อนุมัติคำขอลาของ {ชื่อพนักงาน} แล้ว | Leave request for {employee name} has been approved. |
| SUC-APR-002 | Reject สำเร็จ | ปฏิเสธคำขอลาของ {ชื่อพนักงาน} แล้ว | Leave request for {employee name} has been rejected. |

#### 2.4.9 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-012 | Approval 1 ระดับ (Line Manager เท่านั้น) | แสดงเฉพาะคำขอของทีมที่ Manager รับผิดชอบ | BRD BR-012, QA-H5 |
| BR-013 | เหตุผล Reject เป็น optional | dialog Reject มี textarea ไม่บังคับ | BRD BR-013, QA-M2 |

#### 2.4.10 Error Handling

| Error Type | Trigger Condition | System Behavior | User Message | Recovery |
|-----------|-----------------|----------------|-------------|---------|
| System | Email ส่งไม่สำเร็จหลัง Approve | บันทึก action แล้ว, retry email | "อนุมัติสำเร็จ — อาจมีความล่าช้าในการแจ้งพนักงาน" | Retry อัตโนมัติ |
| Concurrent | Manager อื่น Approve รายการเดียวกันพร้อมกัน | ตรวจ status ก่อน update — ถ้า Approved แล้ว: reject action | "คำขอนี้ถูกดำเนินการไปแล้ว" | Refresh list |

---

### 2.5 SF-005 Approve / Reject Action

ฟังก์ชันนี้ทำงานในหน้า SCR-004 Manager Approval Inbox — รายละเอียดอยู่ใน SF-004 Commands/Actions และ Screen Behavior ครบแล้ว

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-005 |
| Function Name | Approve / Reject Action |
| Related Requirement IDs | SFR-005, VR-010, BRD BR-012, BR-013 |
| Source Reference | ดู SF-004 |
| หมายเหตุ | รวมอยู่ใน SF-004 — ไม่แยก section ซ้ำ |

---

### 2.6 SF-006 Leave Request Status Tracking

#### 2.6.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-006 |
| Function Name | Leave Request Status Tracking |
| Description | พนักงานตรวจสอบสถานะและรายละเอียดคำขอลาของตนเอง |
| Business Purpose | ลดการสอบถาม HR — พนักงานรู้ผลเองผ่านระบบ |
| Actor / User Role | พนักงานประจำ, Outsource, HR (ดูแทน) |
| Related Requirement IDs | SFR-006, SCR-005 |
| Source Reference | SRS §4.1 SFR-006, BRD BR-006 |

#### 2.6.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Leave Request Detail (SCR-005) |
| Screen Description | หน้ารายละเอียดคำขอลาแต่ละรายการ — แสดงสถานะ, ข้อมูล, เหตุผล Reject (ถ้ามี), ปุ่มยกเลิก |
| Navigation Inbound | SCR-002 (ประวัติคำขอ), SCR-004 (Manager คลิก detail), Email notification link |
| Navigation Outbound | SCR-006 (Cancel), SCR-002 (กลับ) |
| Preconditions | Login สำเร็จ — employee เห็นเฉพาะคำขอของตนเอง |
| Postconditions | พนักงานรับทราบสถานะ / ดำเนินการยกเลิก |

#### 2.6.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.6.4 Fields Definition (Display)

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | เงื่อนไขการแสดง | Description |
|-----------|-------------|------------|--------------|-------------|
| request_no | เลขคำขอ / Request No. | ข้อความ (read-only) | เสมอ | หมายเลขอ้างอิง |
| leave_type | ประเภทการลา / Leave Type | ข้อความ (read-only) | เสมอ | ประเภทที่ขอ |
| leave_dates | ช่วงวันที่ / Dates | วันที่ (read-only) | เสมอ | วันเริ่ม–วันสิ้นสุด, จำนวนวัน |
| status | สถานะ / Status | Badge (color-coded) | เสมอ | Pending/Approved/Rejected/Cancelled/Cancel Requested |
| reason | เหตุผล (ที่ยื่น) / Reason | ข้อความ (read-only) | เสมอ | เหตุผลที่พนักงานระบุ |
| reject_reason | เหตุผลการปฏิเสธ / Reject Reason | ข้อความ (read-only) | เฉพาะ Status=Rejected และมีเหตุผล | เหตุผล Reject จาก Manager |
| approved_by | อนุมัติโดย / Approved By | ข้อความ (read-only) | เฉพาะ Approved/Rejected | ชื่อ Manager |
| action_date | วันที่ดำเนินการ / Action Date | วันที่ (read-only) | เฉพาะ Approved/Rejected | วัน-เวลาที่ Manager action |

#### 2.6.5 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ยกเลิกคำขอ | ไปหน้า Cancel Leave | Status = Pending หรือ Approved | Navigate ไป SCR-006 |
| กลับ | ไปหน้า Leave Balance | คลิก Back | Navigate ไป SCR-002 |

---

### 2.7 SF-007 Cancel Leave — Pending

#### 2.7.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-007 |
| Function Name | Cancel Leave Request — Pending |
| Description | พนักงานยกเลิกคำขอลาที่อยู่ในสถานะ Pending ได้ทันทีโดยไม่ต้องผ่านหัวหน้า |
| Business Purpose | ให้ความยืดหยุ่นแก่พนักงานก่อนที่หัวหน้าจะดำเนินการ |
| Actor / User Role | พนักงานประจำ, Outsource |
| Related Requirement IDs | SFR-007, VR-009, VR-010, SCR-006 |
| Source Reference | SRS §4.1 SFR-007, BRD BR-014 |

#### 2.7.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Cancel Leave Request (SCR-006) |
| Navigation Inbound | SCR-005 (ปุ่มยกเลิกคำขอ) |
| Navigation Outbound | SCR-002 (หลัง cancel สำเร็จ) |
| Preconditions | Status = Pending, Login เป็นเจ้าของคำขอ |
| Postconditions | Status→Cancelled ทันที, ไม่กระทบ balance |

#### 2.7.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.7.4 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ยืนยันยกเลิก | ยกเลิกคำขอ Pending ทันที | คลิก confirm | Status→Cancelled ทันที, SUC-CAN-001 แสดง, redirect SCR-002 |
| ยกเลิก (abort) | ไม่ดำเนินการ | คลิก "ไม่ยกเลิก" | กลับ SCR-005 โดยไม่เปลี่ยน Status |

#### 2.7.5 Message List

##### Warning Message (Confirm Dialog)
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| WRN-CAN-001 | กดปุ่มยกเลิกคำขอ | คุณต้องการยกเลิกคำขอลานี้ใช่หรือไม่? การยกเลิกจะมีผลทันที | Do you want to cancel this leave request? This action takes effect immediately. |

##### Success Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| SUC-CAN-001 | ยกเลิก Pending สำเร็จ | ยกเลิกคำขอลาสำเร็จ | Leave request has been cancelled. |

#### 2.7.6 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-014 | Pending → ยกเลิกเองได้ทันที ไม่ต้องแจ้งหัวหน้า | ไม่ต้อง route ไป Manager | BRD BR-014, R4 (QA v2) |
| BR-017 | ห้ามยกเลิก Rejected | ซ่อนปุ่มยกเลิกเมื่อ Status=Rejected | BRD VR-009 |

---

### 2.8 SF-008 Cancel Leave — Approved (Cancel Request Flow)

#### 2.8.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-008 |
| Function Name | Cancel Leave — Approved (Cancel Request Flow) |
| Description | พนักงานส่ง Cancel Request สำหรับคำขอที่ Approved แล้ว ระบบ route ไปยัง Manager พร้อม SLA 1 วันทำการ |
| Business Purpose | ควบคุมการยกเลิกหลัง Approve ผ่านกระบวนการ re-approve เพื่อป้องกันความเสียหาย |
| Actor / User Role | พนักงานประจำ, Outsource |
| Related Requirement IDs | SFR-008, VR-009, NR-001, NR-002, SCR-006 |
| Source Reference | SRS §4.1 SFR-008, BRD BR-015, NR-001, NR-002, R4 (QA v2) |

#### 2.8.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Cancel Leave Request (SCR-006) — Approved flow |
| Navigation Inbound | SCR-005 (ปุ่มยกเลิกคำขอ เมื่อ Status=Approved) |
| Preconditions | Status = Approved, Login เป็นเจ้าของคำขอ |
| Postconditions | Status→Cancel Requested, Email แจ้ง Manager+HR, SLA timer เริ่ม |

#### 2.8.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.8.4 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ส่งคำขอยกเลิก | ส่ง Cancel Request ไปยัง Manager | confirm dialog | Status→Cancel Requested, Email แจ้ง Manager+HR, SLA timer start |

#### 2.8.5 Message List

##### Warning Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| WRN-CAN-002 | กดยกเลิกคำขอ Approved | คำขอนี้ได้รับการอนุมัติแล้ว การยกเลิกต้องผ่านการอนุมัติจากหัวหน้างานภายใน 1 วันทำการ | This request is already approved. Cancellation requires manager re-approval within 1 working day. |

##### Success Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| SUC-CAN-002 | ส่ง Cancel Request สำเร็จ | ส่งคำขอยกเลิกแล้ว อยู่ระหว่างรอหัวหน้างานอนุมัติ (ภายใน 1 วันทำการ) | Cancellation request submitted. Awaiting manager approval within 1 working day. |

#### 2.8.6 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-015 | Approved → ต้อง re-approve ภายใน 1 วันทำการ | SLA timer เริ่มนับ, route ไป Manager | BRD BR-015, R4 (QA v2) |
| BR-017 | ห้าม Edit คำขอที่ Approved | ซ่อนปุ่ม Edit แสดงเฉพาะปุ่ม Cancel | BRD BR-017 |

---

### 2.9 SF-009 Re-approve Cancel Request (Manager)

#### 2.9.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-009 |
| Function Name | Re-approve Cancel Request |
| Description | หัวหน้างาน Approve/Reject Cancel Request — เมื่อ Approve: คืนวันลาอัตโนมัติ + Email แจ้งทุกฝ่าย |
| Business Purpose | ควบคุมการยกเลิกที่ผ่านการ Approve มาแล้ว และคืนวันลาอย่างถูกต้อง |
| Actor / User Role | Line Manager |
| Related Requirement IDs | SFR-009, SFR-010, VR-012, NFR-010, NR-001, NR-002 |
| Source Reference | SRS §4.1 SFR-009/010, BRD BR-015, BR-016, BR-018 |

#### 2.9.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Re-approve Cancel (SCR-007) |
| Navigation Inbound | SCR-004 (Tab: Cancel Requests) / Email notification link |
| Preconditions | Status = Cancel Requested, Login เป็น Manager ของพนักงาน, อยู่ในเวลา SLA |
| Postconditions | Approve: Status→Cancelled + balance คืน + Email / SLA หมด: ปุ่ม disabled + Escalate HR |

#### 2.9.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.9.4 Fields Definition (Display)

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | เงื่อนไขการแสดง | Description |
|-----------|-------------|------------|--------------|-------------|
| sla_countdown | เวลาคงเหลือ SLA / SLA Remaining | Countdown timer (HH:MM) | เสมอ (Cancel Requested) | เวลาที่เหลือก่อนหมด SLA 1 วันทำการ |
| cancel_request_date | วันที่ขอยกเลิก | วันที่ (read-only) | เสมอ | วันที่พนักงานส่ง Cancel Request |
| original_leave_detail | รายละเอียดคำขอเดิม | ข้อความ (read-only) | เสมอ | ประเภทลา, วันที่, จำนวนวัน |

#### 2.9.5 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| Approve การยกเลิก | อนุมัติ Cancel Request | SLA ยังไม่หมด | Status→Cancelled, balance คืนอัตโนมัติ, Email แจ้งพนักงาน+Manager+HR |
| Reject การยกเลิก | ปฏิเสธ Cancel Request | SLA ยังไม่หมด | Status→Approved (คงเดิม), Email แจ้งพนักงาน |

#### 2.9.6 Screen Behavior

| สถานการณ์ | Trigger | Condition | Screen Behavior |
|---------|--------|----------|----------------|
| SLA หมดเวลา | Timer event | SLA expired | ปุ่ม Approve/Reject disabled, แสดงสถานะ "Escalated ไปยัง HR", ซ่อน countdown |
| Approve สำเร็จ | onClick Approve | SLA ยังไม่หมด | คืนวันลาอัตโนมัติ + Email 3 recipients → SUC-REAPPR-001 |

#### 2.9.7 Message List

##### Success Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| SUC-REAPPR-001 | Re-approve สำเร็จ | อนุมัติการยกเลิกสำเร็จ วันลาได้รับการคืนให้พนักงานแล้ว | Cancellation approved. Leave balance has been restored to the employee. |

##### Info Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| INF-REAPPR-001 | SLA หมดเวลา | หมดเวลาดำเนินการ คำขอถูกส่งต่อให้ HR แล้ว | Action time has expired. This request has been escalated to HR. |

#### 2.9.8 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-016 | คืนวันลาอัตโนมัติเมื่อ Cancel Approved สำเร็จ | ระบบอัปเดต balance ทันทีหลัง Approve | BRD BR-016, NR-001 |
| BR-018 | SLA 1 วันทำการ: Reminder 4 ชม. + Escalate | Countdown แสดงบนหน้า, ปุ่ม disabled เมื่อหมด | BRD BR-018, M3 (QA v3) |
| VR-012 | SLA หมด → disabled + Escalated | ไม่อนุญาต action หลัง SLA | SRS VR-012 |

---

### 2.10 SF-010 SLA Reminder & Escalation (Background)

#### 2.10.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-010 |
| Function Name | SLA Reminder & Escalation |
| Description | Background job ส่ง Reminder Email 4 ชม. ก่อนหมด SLA และ Escalate Email ไปยัง HR เมื่อหมด SLA |
| Business Purpose | Enforce SLA 1 วันทำการอัตโนมัติ — ลด risk คำขอยกเลิกค้างไม่มีการดำเนินการ |
| Actor / User Role | System (ไม่มี UI หลัก) |
| Related Requirement IDs | SFR-010, SIR-004, IF-005, NFR-011, TR-004 |
| Source Reference | SRS §4.1 SFR-010, BRD BR-018, M3 (QA v3) |

#### 2.10.2 Screen Overview

ไม่มีหน้าจอหลัก — ทำงานเป็น background scheduled job ส่งผลทางอ้อมต่อ SCR-007 (SLA countdown/disabled) และ Email ที่ส่งออก

#### 2.10.3 System Behavior (ไม่ใช่ Screen Behavior)

| Event | Trigger | Condition | System Action |
|-------|--------|----------|--------------|
| SLA Reminder | Scheduler: (SLA deadline − 4 ชม.) | Status = Cancel Requested, SLA ยังไม่หมด | ส่ง Email Reminder ไปยัง Manager, log ใน Notification Log |
| SLA Escalation | Scheduler: เมื่อถึง SLA deadline | Status = Cancel Requested, Manager ยังไม่ action | ส่ง Escalation Email ไปยัง HR, อัปเดต Status indicator บน SCR-007, log |

#### 2.10.4 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-018 | Reminder 4 ชม. ก่อนหมด + Escalate ไป HR เมื่อหมด | Scheduler trigger 2 events ต่อ Cancel Request | BRD BR-018, M3 (QA v3) |

#### 2.10.5 Notes

| ประเภท | รายละเอียด | ผลกระทบ |
|-------|-----------|--------|
| Open Issue | SLA Escalate assignee ใน HR ยังไม่ระบุ | กระทบ Email recipient ของ Escalation |
| Constraint | Scheduler delay tolerance ≤ 15 นาที | กำหนดใน SRS NFR-011 |

---

### 2.11 SF-011 HR Monitoring Dashboard

#### 2.11.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-011 |
| Function Name | HR Monitoring Dashboard |
| Description | แสดงรายการคำขอลาทั้งองค์กร กรองตามหลายเกณฑ์ ให้ HR ติดตามแบบ real-time |
| Business Purpose | แทนที่ Excel control — HR เห็นข้อมูลรวมศูนย์ในระบบเดียว |
| Actor / User Role | HR |
| Related Requirement IDs | SFR-011, SCR-008 |
| Source Reference | SRS §4.1 SFR-011, BRD BR-007 |

#### 2.11.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | HR Monitoring Dashboard (SCR-008) |
| Navigation Inbound | Header Navigation (role = HR) |
| Preconditions | Login เป็น HR |
| Postconditions | HR เห็นภาพรวมคำขอลาทั้งองค์กร สามารถ drill down และ export ได้ |

#### 2.11.3 Mockup / UI Layout

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | อ้างอิง style จาก `91-project-asses/ascii-mockup/report/leave-monitoring-report/` (ใช้เป็นแนวทาง layout เท่านั้น) |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.11.4 Tabs

| Tab ID | Tab Name | Description | Default |
|--------|---------|------------|---------|
| TAB-01 | คำขอทั้งหมด / All Requests | รายการคำขอลาทั้งองค์กร | Y |
| TAB-02 | รอดำเนินการ / Pending | คำขอที่ยังรอ Manager | N |
| TAB-03 | Cancel Requested | คำขอที่รอ Re-approve | N |

#### 2.11.5 Fields Definition (Filter)

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | Description |
|-----------|-------------|------------|---------|-------------|
| filter_status | สถานะ / Status | Multi-select Dropdown | N | Pending/Approved/Rejected/Cancelled/Cancel Requested |
| filter_department | แผนก / Department | Dropdown | N | กรองตามแผนก |
| filter_employee_type | ประเภทพนักงาน / Employee Type | Dropdown (ประจำ/Outsource/ทั้งหมด) | N | กรองตาม employee_type |
| filter_leave_type | ประเภทการลา / Leave Type | Dropdown (7 ประเภท + ทั้งหมด) | N | กรองตามประเภทลา |
| filter_date_range | ช่วงวันที่ / Date Range | Date Range Picker | N | กรองตามวันที่ยื่น/วันที่ลา |

#### 2.11.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ค้นหา / Filter | กรองข้อมูลตาม criteria | กด Apply filter | Refresh list ตาม filter |
| ดูรายละเอียด | ดู Leave Request Detail | คลิก row | Navigate SCR-005 (HR view) |
| Export | Export รายงาน (Phase 2) | คลิก Export | ดาวน์โหลดไฟล์ตาม filter ปัจจุบัน |

---

### 2.12 SF-012 Outsource Data Import

#### 2.12.1 Function Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-012 |
| Function Name | Outsource Data Import |
| Description | HR upload Excel template import ข้อมูลพนักงาน Outsource พร้อม validate 7 required fields และแสดง error report |
| Business Purpose | Onboard ข้อมูล Outsource เข้าระบบโดยไม่ต้องกรอก manual ทีละคน |
| Actor / User Role | HR |
| Related Requirement IDs | SFR-012, SIR-003, VR-013, IF-003, SCR-009 |
| Source Reference | SRS §4.1 SFR-012, BRD BR-020, R3 (QA v2) |

#### 2.12.2 Screen Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Screen Name | Outsource Import (SCR-009) |
| Navigation Inbound | Header Navigation (role = HR) → Employee Management |
| Preconditions | Login เป็น HR, มีไฟล์ Excel template ที่เตรียมไว้ |
| Postconditions | ข้อมูล Outsource บันทึกในระบบ, แสดง import result (success/fail) |

#### 2.12.3 Mockup / UI Layout

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง screen ตัวอย่าง]
```

#### 2.12.4 Fields Definition

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required | เงื่อนไข | Description |
|-----------|-------------|------------|---------|---------|-------------|
| import_file | ไฟล์ Excel / Excel File | File Upload (.xlsx) | Y | รองรับ .xlsx เท่านั้น | Excel template ที่กรอกข้อมูล Outsource |

**Required fields ภายใน Excel template (7 fields):**

| Column | Field | Required | Validation |
|--------|-------|---------|-----------|
| A | ชื่อ-นามสกุล TH | Y | ไม่ว่าง |
| B | ชื่อ-นามสกุล EN | Y | ไม่ว่าง |
| C | รหัสพนักงาน | Y | unique, ไม่ซ้ำในระบบ |
| D | แผนก / ตำแหน่ง | Y | ไม่ว่าง |
| E | บริษัทต้นสังกัด | Y | ไม่ว่าง |
| F | Email (ใช้ Login) | Y | format email ถูกต้อง, unique |
| G | วันเริ่มงานใน ABC | Y | format วันที่ถูกต้อง, ≤ วันนี้ |
| H | รหัสหัวหน้างาน (Line Manager ID) | Y | ต้องมีใน system |

#### 2.12.5 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| Upload | อัปโหลดไฟล์ Excel | เลือกไฟล์ | Validate format ไฟล์ → แสดง preview records |
| Import | นำเข้าข้อมูลที่ validate ผ่าน | คลิก Import | Import records ที่ valid, แสดง result summary |
| Download Template | ดาวน์โหลด template | คลิกลิงก์ | ดาวน์โหลดไฟล์ template เปล่า |

#### 2.12.6 Message List

##### Error Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| ERR-IMP-001 | Field บังคับว่าง (VR-013) | แถวที่ {N}: {Field} ไม่สามารถเว้นว่างได้ | Row {N}: {Field} is required. |
| ERR-IMP-002 | Email ซ้ำ | แถวที่ {N}: Email นี้มีในระบบแล้ว | Row {N}: This email already exists in the system. |
| ERR-IMP-003 | Line Manager ไม่พบในระบบ | แถวที่ {N}: ไม่พบรหัสหัวหน้างาน {ID} ในระบบ | Row {N}: Manager ID {ID} not found. |
| ERR-IMP-004 | รูปแบบไฟล์ผิด | กรุณาอัปโหลดไฟล์ .xlsx เท่านั้น | Please upload .xlsx files only. |

##### Success Message
| Message ID | Trigger | Message Text (TH) | Message Text (EN) |
|-----------|---------|-----------------|-----------------|
| SUC-IMP-001 | Import สำเร็จ | นำเข้าข้อมูลสำเร็จ {X} รายการ (ไม่สำเร็จ {Y} รายการ) | Import completed: {X} records succeeded, {Y} failed. |

#### 2.12.7 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference |
|--------|-------------|-------------|----------------|
| BR-020 | Import via Excel template, 7 required fields | VR-013: validate ครบก่อน import | BRD BR-020, R3 (QA v2) |
| — | ไม่ import record ที่มีข้อผิดพลาด — import เฉพาะ valid records | Partial import: valid rows เข้า, invalid rows report | SRS VR-013 |

---

### 2.13 SF-013 Leave History & Audit Trail (Phase 2)

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-013 |
| Function Name | Leave History & Audit Trail |
| Phase | Phase 2 |
| Description | แสดงประวัติทุก action ของคำขอลา (สร้าง, อนุมัติ, ปฏิเสธ, ยกเลิก) พร้อม timestamp และผู้กระทำ |
| Related Requirement IDs | SFR-014, TR-009, SCR-005 |
| Source Reference | SRS §4.1 SFR-014, BRD BR-009 |
| หมายเหตุ | Phase 2 — รายละเอียดจะแตกเพิ่มเติมในรอบถัดไป |

---

### 2.14 SF-014 Leave Report Export (Phase 2)

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-014 |
| Function Name | Leave Report Export |
| Phase | Phase 2 |
| Description | HR export รายงานการลาเป็นไฟล์ ตามช่วงเวลา / หน่วยงาน / ประเภทพนักงาน |
| Related Requirement IDs | SFR-015, RFR-001, RFR-002, SCR-010 |
| Source Reference | SRS §4.1 SFR-015, BRD BR-010 |
| Mockup Reference | อ้างอิง style: `91-project-asses/ascii-mockup/report/leave-summary-report/` |
| หมายเหตุ | Phase 2 — Report template ยังไม่ยืนยัน (SRS §7 Open Issue) |

---

### 2.15 SF-015 Notification Log View (Phase 2)

| รายการ | รายละเอียด |
|-------|-----------|
| Function ID | SF-015 |
| Function Name | Notification Log View |
| Phase | Phase 2 |
| Description | HR ตรวจสอบ log การส่ง Email notification ทุกรายการ |
| Related Requirement IDs | RFR-003, SFR-013, SCR-011 |
| Source Reference | SRS §4.2 RFR-003, BRD BR-019 |
| หมายเหตุ | Phase 2 — ใช้ monitor KPI Email success rate ≥99% |

---

## 3. Cross-Function Traceability

| Function ID | Function Name | Related Requirement IDs | Mockup Reference | Source Reference |
|------------|--------------|------------------------|----------------|----------------|
| SF-001 | Login / Authentication | SFR-001, NFR-004, NFR-005, TR-001, TR-007, TR-008 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-001, BRD BR-001 |
| SF-002 | Leave Balance Dashboard | SFR-002, VR-002–VR-004, VR-008, SCR-002 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-002, BRD BR-007–BR-011 |
| SF-003 | Submit Leave Request | SFR-003, VR-001–VR-007, VR-011, SCR-003 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-003, BRD BR-003–BR-007, BR-011 |
| SF-004 | Manager Approval Inbox | SFR-004, SFR-005, NFR-005, SCR-004 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-004/005, BRD BR-012, BR-013 |
| SF-005 | Approve / Reject Action | SFR-005, VR-010 | — | รวมใน SF-004 |
| SF-006 | Leave Request Status Tracking | SFR-006, SCR-005 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-006, BRD BR-006 |
| SF-007 | Cancel Leave — Pending | SFR-007, VR-009, VR-010, SCR-006 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-007, BRD BR-014 |
| SF-008 | Cancel Leave — Approved | SFR-008, VR-009, NR-001, NR-002, SCR-006 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-008, BRD BR-015 |
| SF-009 | Re-approve Cancel Request | SFR-009, SFR-010, VR-012, NR-001, NR-002 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-009/010, BRD BR-015–BR-016, BR-018 |
| SF-010 | SLA Reminder & Escalation | SFR-010, SIR-004, IF-005, NFR-011, TR-004 | N/A (Background) | SRS §4.1 SFR-010, BRD BR-018, M3 (QA v3) |
| SF-011 | HR Monitoring Dashboard | SFR-011, SCR-008 | report-mockup-index.md (style reference) | SRS §4.1 SFR-011, BRD BR-007 |
| SF-012 | Outsource Data Import | SFR-012, SIR-003, VR-013, IF-003, SCR-009 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-012, BRD BR-020, R3 (QA v2) |
| SF-013 | Leave History & Audit Trail | SFR-014, TR-009, SCR-005 | ไม่มี mockup อ้างอิง | SRS §4.1 SFR-014, BRD BR-009 — Phase 2 |
| SF-014 | Leave Report Export | SFR-015, RFR-001, RFR-002, SCR-010 | leave-summary-report/ (style ref) | SRS §4.1 SFR-015, BRD BR-010 — Phase 2 |
| SF-015 | Notification Log View | RFR-003, SFR-013, SCR-011 | ไม่มี mockup อ้างอิง | SRS §4.2 RFR-003, BRD BR-019 — Phase 2 |

---

## 4. Source Reference

- `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md`
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-srs-summary.md`
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml`
- `91-project-asses/ascii-mockup/report/report-mockup-index.md` (style reference สำหรับ report screens)
