---
title: "Report SRS Document"
document_type: "Report SRS"
version: "1.0"
language: "th"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
status: "Draft"
---

# Report SRS Document: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)

---

## 1. เอกสารอ้างอิงและขอบเขต

เอกสารนี้ลงรายละเอียด Report Function ในมุมมองการใช้งานรายงานและความต้องการเชิงธุรกิจ สำหรับระบบบริหารการลาและการอนุมัติ ABC Company อ้างอิงจาก SRS Summary เป็น baseline หลัก ครอบคลุม 3 รายงานซึ่งทั้งหมดอยู่ใน Phase 2

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
|-------|-------------|--------------|
| 1 | `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md` | ต้นทาง requirement เชิงธุรกิจ |
| 2 | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md` | baseline ของ Report Function Requirement (RFR-001–RFR-003) |
| 3 | `91-project-asses/ascii-mockup/report/report-mockup-index.md` | index แหล่งอ้างอิง mockup รายงาน (ไม่มีไฟล์ mockup จริงในโฟลเดอร์ย่อย) |
| 4 | `requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`, `v3.yaml` | เอกสารยืนยัน business rules |

### 1.2 Report Index

| Report ID | Report Name | Report Type | Actor / User Role | Frequency | Related Requirement IDs | Source Reference | หมายเหตุ |
|-----------|------------|------------|-----------------|-----------|------------------------|----------------|---------|
| RP-001 | Leave Summary Report | สรุป / รายละเอียด | HR | เรียกดูเมื่อจำเป็น (on-demand) | RFR-001, SFR-015, SCR-010 | SRS §4.2 RFR-001, BRD §3.1 BR-010, BRD §5.3.1.C KPI | Phase 2 |
| RP-002 | Leave Balance Report | รายละเอียดรายบุคคล | HR | เรียกดูเมื่อจำเป็น (on-demand) | RFR-002, SFR-015, SCR-010 | SRS §4.2 RFR-002, BRD §3.1 BR-010, BRD §6 LEAVE_BALANCE | Phase 2 |
| RP-003 | Notification Log Report | รายละเอียด Log | HR | เรียกดูเมื่อจำเป็น / รายวัน | RFR-003, SFR-013, SCR-011 | SRS §4.2 RFR-003, BRD BR-019, BRD §5.3.1.C KPI | Phase 2 |

---

## 2. Detailed Report Specification

---

### 2.1 RP-001 Leave Summary Report

#### 2.1.1 Report Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Report ID | RP-001 |
| Report Name | Leave Summary Report (รายงานสรุปการลา) |
| Description | รายงานสรุปการลาของพนักงานทั้งองค์กร แยกตามประเภทการลา / หน่วยงาน / ประเภทพนักงาน ตามช่วงเวลาที่ HR กำหนด |
| Business Purpose | ให้ HR และผู้บริหารเห็นภาพรวมการลาของทั้งองค์กร สนับสนุนการตัดสินใจด้านการบริหารบุคลากร แทนการรวม Excel จากหลายฝ่าย |
| Actor / User Role | HR |
| Frequency | เรียกดูเมื่อจำเป็น (on-demand) — คาดว่ารายเดือน/รายไตรมาส |
| Related Requirement IDs | RFR-001, SFR-015, SCR-010, NFR-009 |
| Source Reference | SRS §4.2 RFR-001, BRD §3.1 BR-010, BRD §5.3.1.C KPI, R1 (QA v2) |

#### 2.1.2 Report Description

| รายการ | รายละเอียด |
|-------|-----------|
| Report Type | สรุป (Summary) + รายละเอียดย่อย (Breakdown by department / leave type) |
| Output Format | แสดงบนหน้าจอ (SCR-010) + export เป็นไฟล์ (Excel หรือ PDF — รูปแบบไฟล์ยังไม่ยืนยัน ดู Notes) |
| Sorting Method | เรียงตาม แผนก → ประเภทการลา → จำนวนวันลา (descending) — ข้อมูลไม่เพียงพอสำหรับ sort order ที่แน่ชัด |
| Aggregation Method | นับจำนวนคำขอ (count), รวมจำนวนวันลา (sum), คำนวณ Approve/Reject rate (%) |
| Additional Description | รายงานนี้อ้างอิง KPI ที่กำหนดใน BRD §5.3.1.C ได้แก่ Adoption Rate ≥95% และ SLA compliance |

#### 2.1.3 Report Parameters (Filters)

| Parameter Name | Label (TH/EN) | Required (Y/N) | Default Value | Description | Sample Data |
|---------------|-------------|--------------|-------------|-------------|------------|
| date_from | วันที่เริ่มต้น / Date From | Y | วันแรกของเดือนปัจจุบัน | วันเริ่มต้นของช่วงที่ต้องการดูรายงาน | 2026-01-01 |
| date_to | วันที่สิ้นสุด / Date To | Y | วันสุดท้ายของเดือนปัจจุบัน | วันสิ้นสุดของช่วงที่ต้องการดูรายงาน | 2026-06-30 |
| department | แผนก / Department | N | ทั้งหมด (All) | กรองเฉพาะแผนกที่เลือก | IT, HR, Finance |
| employee_type | ประเภทพนักงาน / Employee Type | N | ทั้งหมด (All) | ประจำ / Outsource / ทั้งหมด | ประจำ |
| leave_type | ประเภทการลา / Leave Type | N | ทั้งหมด (All) | 7 ประเภทการลา หรือ ทั้งหมด | ลาพักผ่อนประจำปี |

#### 2.1.4 Report Layout / Structure

| ส่วนของรายงาน | รายละเอียด |
|-------------|-----------|
| Header | ชื่อรายงาน "รายงานสรุปการลา", ช่วงวันที่รายงาน, เงื่อนไข filter ที่เลือก (แผนก, ประเภทพนักงาน, ประเภทลา), วัน-เวลาที่ generate |
| Summary Strip | จำนวนคำขอทั้งหมด, จำนวนวันลาทั้งหมด, Approve rate (%), Reject rate (%), แยกตาม employee_type |
| Detail | ตารางสรุปรายการลาแยกตาม แผนก → ประเภทการลา พร้อมจำนวนคำขอและจำนวนวันรวม |
| Footer | รวมยอดทั้งหมด (Total), จำนวนพนักงานที่ลาในช่วงเวลา, หมายเหตุ |

##### Displayed Columns / Data Items

| ลำดับ | รายการข้อมูลที่แสดง | Description | หมายเหตุ |
|-------|------------------|-------------|---------|
| 1 | แผนก / Department | ชื่อหน่วยงาน | group header |
| 2 | ประเภทการลา / Leave Type | ชื่อประเภทการลา | |
| 3 | จำนวนคำขอ / Request Count | จำนวนคำขอลาทั้งหมดของประเภทนั้นในช่วงเวลา | |
| 4 | จำนวนวันลารวม / Total Leave Days | รวมวันลาทั้งหมดของประเภทนั้น | |
| 5 | อนุมัติ / Approved | จำนวนคำขอที่ Approved | |
| 6 | ปฏิเสธ / Rejected | จำนวนคำขอที่ Rejected | |
| 7 | ยกเลิก / Cancelled | จำนวนคำขอที่ Cancelled | |
| 8 | Approve Rate (%) | % Approved จากคำขอทั้งหมดของประเภทนั้น | คำนวณ: Approved / Total × 100 |

#### 2.1.5 Fields / Columns Definition

| Column Name | Label (TH/EN) | รูปแบบข้อมูล | Description |
|------------|-------------|------------|-------------|
| department | แผนก / Department | ข้อความ | ชื่อแผนก/หน่วยงาน |
| leave_type_name | ประเภทการลา / Leave Type | ข้อความ | ชื่อประเภทการลา (7 ประเภท) |
| request_count | จำนวนคำขอ / Count | ตัวเลขจำนวนนับ | จำนวนคำขอลาในช่วงเวลา |
| total_leave_days | รวมวันลา / Total Days | ตัวเลข (วัน) | ผลรวมจำนวนวันลาทั้งหมด |
| approved_count | อนุมัติ / Approved | ตัวเลขจำนวนนับ | จำนวนคำขอที่ Status=Approved |
| rejected_count | ปฏิเสธ / Rejected | ตัวเลขจำนวนนับ | จำนวนคำขอที่ Status=Rejected |
| cancelled_count | ยกเลิก / Cancelled | ตัวเลขจำนวนนับ | จำนวนคำขอที่ Status=Cancelled |
| approve_rate_pct | Approve Rate (%) | ตัวเลข ทศนิยม 1 ตำแหน่ง | % Approved = approved_count / request_count × 100 |

#### 2.1.6 Mockup / Report Layout Reference

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง |
| Mockup Index | `91-project-asses/ascii-mockup/report/report-mockup-index.md` — ระบุ style SAP/Dynamics/Odoo/Zoho แต่ไม่พบไฟล์ mockup ในโฟลเดอร์ย่อย |
| Style Reference | อ้างอิง style แนวทาง: `leave-summary-report/` ตาม report-mockup-index.md |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง]
```

#### 2.1.7 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ดูรายงาน / Generate | สร้างและแสดงรายงานตาม filter | กด Apply / Generate | คำนวณและแสดงผลรายงานตาม parameter ที่เลือก |
| Export Excel | ส่งออกรายงานเป็น Excel | คลิกปุ่ม Export Excel | ดาวน์โหลดไฟล์ .xlsx ตาม filter ปัจจุบัน |
| Export PDF | ส่งออกรายงานเป็น PDF | คลิกปุ่ม Export PDF | ดาวน์โหลดไฟล์ .pdf ตาม filter ปัจจุบัน |
| Reset Filter | ล้าง filter กลับค่าเริ่มต้น | คลิก Reset | คืนค่า filter เป็น default ทั้งหมด, ไม่ refresh รายงาน |

#### 2.1.8 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-RPT-001 | date_from > date_to | วันที่เริ่มต้นต้องน้อยกว่าหรือเท่ากับวันที่สิ้นสุด | Start date must be less than or equal to end date. | |
| ERR-RPT-002 | ไม่พบข้อมูลตาม filter | ไม่พบข้อมูลที่ตรงกับเงื่อนไขที่เลือก | No data found matching the selected criteria. | |
| ERR-RPT-003 | Export ล้มเหลว | ไม่สามารถส่งออกไฟล์ได้ กรุณาลองใหม่ | Unable to export file. Please try again. | |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-RPT-001 | กำลัง generate รายงาน | กำลังดึงข้อมูล กรุณารอสักครู่... | Generating report, please wait... | แสดงระหว่างโหลด |

##### Success Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| SUC-RPT-001 | Export สำเร็จ | ส่งออกไฟล์สำเร็จ | File exported successfully. | |

#### 2.1.9 Business Rules

| Rule ID | Business Rule | Impact to Report | Source Reference | หมายเหตุ |
|--------|-------------|----------------|----------------|---------|
| BR-010 | HR สามารถ export รายงานการลา | รายงานนี้ถูกเรียกโดย HR เท่านั้น (RBAC) | BRD §3.1 BR-010, SRS NFR-005 | |
| BR-011 | Outsource ไม่มีสิทธิ์ 4 ประเภทลา | เมื่อ filter employee_type=Outsource จะไม่แสดงแถวประเภทลา 4 ประเภทนั้น (0 records) | BRD BR-011, SRS VR-001 | |
| BR-010-R1 | ลากิจ 3 วัน/ปี ทั้งประจำและ Outsource | ควร grouping ถูกต้องในรายงาน | BRD BR-010, R1 (QA v2) | |
| NFR-005 | RBAC — HR เห็นข้อมูลทั้งองค์กร | รายงานแสดงข้อมูลทุกแผนก ไม่จำกัดตาม department | SRS NFR-005 | |

#### 2.1.10 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | Export file format (Excel / PDF / ทั้งคู่) ยังไม่ยืนยัน | กระทบ Commands, TR-006 | ดู SRS §7 Open Issue |
| Open Issue | รูปแบบ report template / column layout ยังไม่ยืนยัน | กระทบ Fields Definition และ Displayed Columns ทั้งหมด | HR ต้องยืนยันก่อน Phase 2 |
| Assumption | ข้อมูลคำนวณจาก Leave Request ที่บันทึกในระบบเท่านั้น (ไม่รวมข้อมูล offline) | รายงานสะท้อนเฉพาะคำขอที่ผ่านระบบ | |
| Constraint | รายงานนี้เป็น Phase 2 — รายละเอียดจะยืนยันก่อน sprint ของ Phase 2 | | |

---

### 2.2 RP-002 Leave Balance Report

#### 2.2.1 Report Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Report ID | RP-002 |
| Report Name | Leave Balance Report (รายงานสิทธิ์วันลาคงเหลือ) |
| Description | รายงานสิทธิ์วันลาคงเหลือของพนักงานทุกคน ณ วันที่ที่ HR ระบุ แยกตามประเภทการลา รายบุคคล |
| Business Purpose | ให้ HR ตรวจสอบ leave balance ของพนักงานทั้งองค์กร ณ เวลาใดก็ได้ — ใช้วางแผนอัตรากำลัง และตรวจสอบ compliance |
| Actor / User Role | HR |
| Frequency | เรียกดูเมื่อจำเป็น (on-demand) — คาดว่ารายเดือน / ต้นปี / ปลายปี |
| Related Requirement IDs | RFR-002, SFR-015, SCR-010 |
| Source Reference | SRS §4.2 RFR-002, BRD §3.1 BR-010, BRD §6 LEAVE_BALANCE, R6 (QA v2) |

#### 2.2.2 Report Description

| รายการ | รายละเอียด |
|-------|-----------|
| Report Type | รายละเอียดรายบุคคล (Detail by employee) — 1 แถว / 1 พนักงาน / 1 ประเภทลา |
| Output Format | แสดงบนหน้าจอ + export เป็นไฟล์ (format ยังไม่ยืนยัน — ดู Notes) |
| Sorting Method | เรียงตาม แผนก → ชื่อพนักงาน (A-Z) → ประเภทการลา — ข้อมูลไม่เพียงพอสำหรับ sort order ที่แน่ชัด |
| Aggregation Method | รวมยอด balance รายแผนก (subtotal) และรวมยอดทั้งหมด (grand total) |
| Additional Description | ใช้ as-of date ในการคำนวณ balance ณ วันนั้น (ไม่ใช่ปัจจุบันเสมอไป) |

#### 2.2.3 Report Parameters (Filters)

| Parameter Name | Label (TH/EN) | Required (Y/N) | Default Value | Description | Sample Data |
|---------------|-------------|--------------|-------------|-------------|------------|
| as_of_date | ดูข้อมูล ณ วันที่ / As-of Date | Y | วันนี้ (Today) | คำนวณ balance ณ วันที่ระบุ | 2026-06-30 |
| department | แผนก / Department | N | ทั้งหมด (All) | กรองเฉพาะแผนกที่เลือก | Finance |
| employee_type | ประเภทพนักงาน / Employee Type | N | ทั้งหมด (All) | ประจำ / Outsource / ทั้งหมด | Outsource |
| leave_type | ประเภทการลา / Leave Type | N | ทั้งหมด (All) | กรองเฉพาะประเภทลาที่เลือก | ลาพักผ่อนประจำปี |
| employee_id | รหัสพนักงาน / Employee ID | N | — | ค้นหาเฉพาะพนักงานคนเดียว | EMP001 |

#### 2.2.4 Report Layout / Structure

| ส่วนของรายงาน | รายละเอียด |
|-------------|-----------|
| Header | ชื่อรายงาน "รายงานสิทธิ์วันลาคงเหลือ", as-of date, เงื่อนไข filter, วัน-เวลาที่ generate |
| Detail | ตารางรายบุคคล: แผนก, รหัสพนักงาน, ชื่อ, employee_type, ประเภทลา, สิทธิ์ทั้งหมด, สะสม, ใช้ไป, คงเหลือ |
| Subtotal | รวมยอดต่อแผนก (สิทธิ์รวม, วันลารวม, วันคงเหลือรวม) |
| Footer | Grand Total ทั้งองค์กร |

##### Displayed Columns / Data Items

| ลำดับ | รายการข้อมูลที่แสดง | Description | หมายเหตุ |
|-------|------------------|-------------|---------|
| 1 | แผนก / Department | ชื่อหน่วยงาน | group header |
| 2 | รหัสพนักงาน / Employee ID | รหัสอ้างอิงพนักงาน | |
| 3 | ชื่อ-นามสกุล / Name | ชื่อพนักงาน | |
| 4 | ประเภทพนักงาน / Employee Type | ประจำ / Outsource | |
| 5 | ประเภทการลา / Leave Type | ชื่อประเภทลา | |
| 6 | สิทธิ์ทั้งหมด / Entitled Days | จำนวนวันสิทธิ์ตามอายุงาน/ประเภท | |
| 7 | วันสะสม / Carried Forward | วันสะสมจากปีก่อน (cap 30 วัน สำหรับลาพักผ่อน) | เฉพาะลาพักผ่อน |
| 8 | ใช้ไป / Used Days | จำนวนวันที่ใช้ไปแล้วในปีนี้ ณ as-of date | |
| 9 | คงเหลือ / Remaining Days | = Entitled + Carried Forward − Used | |

#### 2.2.5 Fields / Columns Definition

| Column Name | Label (TH/EN) | รูปแบบข้อมูล | Description |
|------------|-------------|------------|-------------|
| department | แผนก / Department | ข้อความ | ชื่อแผนก/หน่วยงาน |
| employee_id | รหัสพนักงาน / Employee ID | ข้อความ | รหัสอ้างอิงพนักงาน |
| employee_name | ชื่อ-นามสกุล / Name | ข้อความ | ชื่อเต็มภาษาไทย |
| employee_type | ประเภทพนักงาน / Type | ข้อความ (ประจำ/Outsource) | ประเภทพนักงาน |
| leave_type_name | ประเภทการลา / Leave Type | ข้อความ | ชื่อประเภทการลา |
| entitled_days | สิทธิ์ทั้งหมด / Entitled | ตัวเลข (วัน) | จำนวนวันสิทธิ์ตาม tier อายุงาน |
| carry_forward_days | สะสม / Carried Fwd | ตัวเลข (วัน) | วันสะสมจากปีก่อน (max 30 วัน) |
| used_days | ใช้ไป / Used | ตัวเลข (วัน) | วันที่ใช้ไปแล้ว ณ as-of date |
| remaining_days | คงเหลือ / Remaining | ตัวเลข (วัน) | = entitled + carry_forward − used |

#### 2.2.6 Mockup / Report Layout Reference

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง |
| Mockup Index | `91-project-asses/ascii-mockup/report/report-mockup-index.md` — ระบุ style สำหรับ Leave Request History Report ซึ่งใกล้เคียง แต่ไม่พบไฟล์ mockup จริง |
| Style Reference | อ้างอิงแนวทาง style: `leave-request-history-report/` ตาม report-mockup-index.md |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง]
```

#### 2.2.7 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ดูรายงาน / Generate | สร้างและแสดงรายงาน balance ณ as-of date | กด Apply / Generate | คำนวณ balance ณ วันที่ระบุ แสดงรายบุคคลตาม filter |
| Export Excel | ส่งออกรายงานเป็น Excel | คลิกปุ่ม Export Excel | ดาวน์โหลดไฟล์ .xlsx |
| Export PDF | ส่งออกรายงานเป็น PDF | คลิกปุ่ม Export PDF | ดาวน์โหลดไฟล์ .pdf |
| Reset Filter | ล้าง filter | คลิก Reset | คืนค่า default ทั้งหมด |

#### 2.2.8 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-RPT-004 | as_of_date อยู่ในอนาคต (> วันนี้) | ไม่สามารถดูข้อมูลในอนาคตได้ กรุณาเลือกวันที่ไม่เกินวันนี้ | Cannot view future data. Please select a date no later than today. | |
| ERR-RPT-005 | ไม่พบข้อมูลตาม filter | ไม่พบข้อมูลที่ตรงกับเงื่อนไขที่เลือก | No data found matching the selected criteria. | |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-RPT-002 | as_of_date เป็นวันที่ผ่านมา | ข้อมูลแสดง ณ วันที่ {as_of_date} — อาจแตกต่างจากปัจจุบัน | Data shown as of {as_of_date} — may differ from current balance. | |

#### 2.2.9 Business Rules

| Rule ID | Business Rule | Impact to Report | Source Reference | หมายเหตุ |
|--------|-------------|----------------|----------------|---------|
| BR-008 | สิทธิ์ลาพักผ่อนตาม tier อายุงาน 5 ระดับ | คำนวณ entitled_days จาก hire_date ณ as-of date | BRD BR-008, R6 (QA v2) | |
| BR-009 | cap สะสม 30 วัน (ลาพักผ่อน) | carry_forward_days แสดงสูงสุด 30 วัน | BRD BR-009 | |
| BR-007 | Probation < 3 เดือน ไม่มีสิทธิ์ลาพักผ่อน | แสดง entitled_days = 0 สำหรับพนักงาน probation | BRD BR-007, M2 (QA v3) | |
| BR-011 | Outsource ไม่มีสิทธิ์ 4 ประเภท | ไม่แสดงแถว 4 ประเภทลานั้นสำหรับ Outsource | BRD BR-011 | |
| NFR-005 | RBAC — HR เห็นข้อมูลทั้งองค์กร | ไม่จำกัด scope ตาม department ของ HR | SRS NFR-005 | |
| NFR-010 | Data Integrity | ตัวเลข remaining_days ต้องสอดคล้องกับ Leave Balance Dashboard ณ วันเดียวกัน | SRS NFR-010, BRD BR-016 | |

#### 2.2.10 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | Carry-forward calculation formula ยังไม่ยืนยัน (ดู SRS §7) | กระทบ carry_forward_days calculation | |
| Open Issue | Export file format ยังไม่ยืนยัน | กระทบ Commands | |
| Assumption | as_of_date ต้องไม่เกิน Today (ไม่รองรับ future projection) | จำกัด as_of_date ≤ Today | |
| Constraint | Phase 2 — รายละเอียดยืนยันก่อน sprint Phase 2 | | |

---

### 2.3 RP-003 Notification Log Report

#### 2.3.1 Report Overview

| รายการ | รายละเอียด |
|-------|-----------|
| Report ID | RP-003 |
| Report Name | Notification Log Report (รายงาน Log การแจ้งเตือน) |
| Description | รายงานแสดง log การส่ง Email notification ทั้งหมดของระบบ — ให้ HR ตรวจสอบว่า Email ส่งสำเร็จหรือล้มเหลว และ monitor KPI Email success rate ≥99% |
| Business Purpose | ให้ HR ตรวจสอบ delivery status ของ Email notification ทุก event — ใช้ monitor KPI และ troubleshoot กรณี notification ไม่ถึงมือผู้รับ |
| Actor / User Role | HR |
| Frequency | เรียกดูเมื่อจำเป็น (on-demand) หรือรายวัน เพื่อ monitor KPI Email success rate ≥99% |
| Related Requirement IDs | RFR-003, SFR-013, SCR-011, NFR-007 |
| Source Reference | SRS §4.2 RFR-003, BRD BR-019, BRD §5.3.1.C KPI (Email success ≥99%), R5 (QA v2) |

#### 2.3.2 Report Description

| รายการ | รายละเอียด |
|-------|-----------|
| Report Type | รายละเอียด Log (Detail Log) |
| Output Format | แสดงบนหน้าจอ (SCR-011) — export ข้อมูลไม่เพียงพอ (ดู Notes) |
| Sorting Method | เรียงตาม timestamp descending (ล่าสุดอยู่บน) |
| Aggregation Method | นับจำนวน Email ส่งสำเร็จ / ล้มเหลว, คำนวณ success rate (%) ในช่วงเวลา |
| Additional Description | รายงานนี้ใช้ monitor KPI Email notification success rate ≥99% ตาม BRD §5.3.1.C |

#### 2.3.3 Report Parameters (Filters)

| Parameter Name | Label (TH/EN) | Required (Y/N) | Default Value | Description | Sample Data |
|---------------|-------------|--------------|-------------|-------------|------------|
| date_from | วันที่เริ่มต้น / Date From | Y | วันนี้ (Today) | วันเริ่มต้นของช่วงที่ต้องการดู log | 2026-06-16 |
| date_to | วันที่สิ้นสุด / Date To | Y | วันนี้ (Today) | วันสิ้นสุดของช่วงที่ต้องการดู log | 2026-06-16 |
| event_type | ประเภท Event / Event Type | N | ทั้งหมด (All) | ประเภท notification event: ยื่นลา / Approve / Reject / Cancel / SLA Reminder / SLA Escalate | Approve |
| recipient_email | อีเมลผู้รับ / Recipient | N | — | กรองตาม email ของผู้รับ | manager@abc.com |
| delivery_status | สถานะการส่ง / Status | N | ทั้งหมด (All) | Success / Failed / Retry | Failed |

#### 2.3.4 Report Layout / Structure

| ส่วนของรายงาน | รายละเอียด |
|-------------|-----------|
| Header | ชื่อรายงาน "รายงาน Log การแจ้งเตือน", ช่วงวันที่, filter ที่เลือก, วัน-เวลาที่ generate |
| Summary Strip | Email ทั้งหมด, ส่งสำเร็จ, ล้มเหลว, Success Rate (%) ในช่วงเวลา |
| Detail | ตาราง log รายการ Email แต่ละ record เรียงตาม timestamp descending |
| Footer | จำนวนรายการ log ทั้งหมด |

##### Displayed Columns / Data Items

| ลำดับ | รายการข้อมูลที่แสดง | Description | หมายเหตุ |
|-------|------------------|-------------|---------|
| 1 | วัน-เวลาที่ส่ง / Sent At | Timestamp ที่ระบบส่ง Email | format: YYYY-MM-DD HH:MM:SS |
| 2 | ประเภท Event / Event Type | ชื่อ event ที่ trigger notification | เช่น "Leave Submitted", "Approved" |
| 3 | อ้างอิงคำขอ / Request No. | เลขคำขอลาที่เกี่ยวข้อง | |
| 4 | ชื่อพนักงาน (เจ้าของคำขอ) / Employee | ชื่อพนักงานที่คำขอเกี่ยวข้อง | |
| 5 | ผู้รับ / Recipient | Email address ของผู้รับ | |
| 6 | สถานะการส่ง / Status | Success / Failed / Retry | badge สีเขียว/แดง/เหลือง |
| 7 | จำนวนครั้งที่ retry / Retry Count | จำนวนครั้งที่ระบบ retry | 0 = ส่งสำเร็จครั้งแรก |
| 8 | สาเหตุล้มเหลว / Failure Reason | เหตุผลที่ Email ไม่ถึงผู้รับ (ถ้า Failed) | แสดงเฉพาะ Status=Failed |

#### 2.3.5 Fields / Columns Definition

| Column Name | Label (TH/EN) | รูปแบบข้อมูล | Description |
|------------|-------------|------------|-------------|
| sent_at | วัน-เวลาที่ส่ง / Sent At | datetime (YYYY-MM-DD HH:MM:SS) | timestamp ที่ระบบส่ง Email |
| event_type | ประเภท Event / Event Type | ข้อความ | Leave Submitted / Approved / Rejected / Cancelled / SLA Reminder / SLA Escalate |
| request_no | เลขคำขอ / Request No. | ข้อความ | หมายเลขอ้างอิง Leave Request |
| employee_name | ชื่อพนักงาน / Employee | ข้อความ | ชื่อเจ้าของคำขอ |
| recipient_email | ผู้รับ / Recipient | email | Email address ผู้รับ |
| delivery_status | สถานะ / Status | ข้อความ (Success/Failed/Retry) | สถานะการส่ง Email |
| retry_count | Retry / Retry Count | ตัวเลขจำนวนนับ | จำนวนครั้ง retry (0 = สำเร็จครั้งแรก) |
| failure_reason | สาเหตุล้มเหลว / Reason | ข้อความ | Error message จาก Email gateway (แสดงเฉพาะ Status=Failed) |

#### 2.3.6 Mockup / Report Layout Reference

| รายการ | รายละเอียด |
|-------|-----------|
| Mockup Reference | ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง |
| Mockup Index | `91-project-asses/ascii-mockup/report/report-mockup-index.md` — ระบุ style สำหรับ Leave Monitoring Report ซึ่งใกล้เคียงในเชิง layout แต่ไม่พบไฟล์ mockup จริง |
| Style Reference | อ้างอิงแนวทาง style: `leave-monitoring-report/` ตาม report-mockup-index.md |

```text
[ไม่มีข้อมูลที่มากเพียงพอ หรือ mockup อ้างอิงในการสร้าง Report ตัวอย่าง]
```

#### 2.3.7 Commands / Actions

| Name | Description | Trigger Condition | System Response |
|------|------------|-----------------|----------------|
| ดูรายงาน / Generate | แสดง log ตาม filter | กด Apply / Generate | ดึง log จาก notification table แสดงตาม filter |
| Refresh | อัปเดต log ใหม่ | คลิก Refresh | ดึงข้อมูล log ล่าสุดใหม่อีกครั้ง |
| Export | ส่งออก log (ถ้ามี) | คลิก Export | ข้อมูลไม่เพียงพอ — ดู Notes |

#### 2.3.8 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| ERR-LOG-001 | ไม่พบ log ตาม filter | ไม่พบรายการ log ที่ตรงกับเงื่อนไขที่เลือก | No notification log entries found for the selected criteria. | |

##### Warning Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| WRN-LOG-001 | Success Rate < 99% ในช่วงเวลาที่ดู | อัตราการส่ง Email ต่ำกว่า 99% ({rate}%) — กรุณาตรวจสอบ | Email success rate is below 99% ({rate}%) — please investigate. | ใช้ monitor KPI |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
|-----------|-----------------|-----------------|-----------------|---------|
| INF-LOG-001 | Success Rate ≥ 99% | อัตราการส่ง Email ในช่วงนี้อยู่ที่ {rate}% — เป็นไปตาม KPI | Email success rate is {rate}% — within KPI target. | |

#### 2.3.9 Business Rules

| Rule ID | Business Rule | Impact to Report | Source Reference | หมายเหตุ |
|--------|-------------|----------------|----------------|---------|
| BR-019 | ระบบส่ง Email notification ทุก event พร้อม log | รายงานนี้อ่านจาก notification_log ที่ระบบบันทึกทุก event | BRD BR-019, SRS SFR-013 | |
| NFR-007 | Email success rate ≥ 99% | WRN-LOG-001 alert เมื่อ rate < 99% — ช่วย HR monitor KPI | SRS NFR-007, BRD §5.3.1.C KPI | |
| NFR-005 | RBAC — HR เห็น log ทั้งองค์กร | แสดง log ทุก event, ทุกพนักงาน, ทุกแผนก | SRS NFR-005 | |
| SIR-002 | Email retry 3 ครั้ง | retry_count แสดงจำนวนครั้งที่ retry | SRS SIR-002, NFR-007 | |

#### 2.3.10 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
|-------|-----------|--------|---------|
| Open Issue | Export log เป็นไฟล์ (ต้องการหรือไม่) ยังไม่ยืนยัน | กระทบ Commands | |
| Open Issue | SLA Escalate assignee ใน HR ยังไม่ระบุ — กระทบ log recipient field | กระทบ recipient_email column | ดู SRS §7 Open Issue |
| Open Issue | Log retention period ยังไม่ยืนยัน | กระทบจำนวน log ที่ query ได้ | ดู SRS §7 Open Issue |
| Assumption | Notification Log บันทึกอัตโนมัติโดย SFR-013 (Email Notification Engine) | รายงานนี้ read-only จาก log table | |
| Constraint | Phase 2 — รายละเอียดยืนยันก่อน sprint Phase 2 | | |

---

## 3. Cross-Report Traceability

| Report ID | Report Name | Related Requirement IDs | Source Reference | หมายเหตุ |
|-----------|------------|------------------------|----------------|---------|
| RP-001 | Leave Summary Report | RFR-001, SFR-015, SCR-010, NFR-005, NFR-009 | SRS §4.2 RFR-001, BRD §3.1 BR-010, BRD §5.3.1.C KPI | Phase 2 — report template ยังไม่ยืนยัน |
| RP-002 | Leave Balance Report | RFR-002, SFR-015, SCR-010, NFR-005, NFR-010 | SRS §4.2 RFR-002, BRD §3.1 BR-010, BRD §6 LEAVE_BALANCE | Phase 2 — carry-forward formula ยังไม่ยืนยัน |
| RP-003 | Notification Log Report | RFR-003, SFR-013, SCR-011, NFR-007 | SRS §4.2 RFR-003, BRD BR-019, BRD §5.3.1.C KPI | Phase 2 — log retention ยังไม่ยืนยัน |

---

## 4. Source Reference

- `10-requirement-definition/a0-business-requirement/brd/leave-request-and-approval-brd.md`
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md`
- `10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml`
- `10-requirement-definition/a0-business-requirement/requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml`
- `91-project-asses/ascii-mockup/report/report-mockup-index.md` (index อ้างอิง style — ไม่พบไฟล์ mockup จริงในโฟลเดอร์ย่อย)
