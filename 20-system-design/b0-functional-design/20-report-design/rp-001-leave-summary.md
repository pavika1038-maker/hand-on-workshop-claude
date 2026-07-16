---
function_id: "RP-001"
function_name: "Leave Summary Report"
category: "Report"
report_type: "Summary"
version: "1.0"
status: "Draft"
author: "report-design-agent (Claude)"
last_updated: "2026-07-09"
---

# RP-001 — Leave Summary Report

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | RP-001 |
| Report Name | Leave Summary Report (รายงานสรุปการลา) |
| Category | Report |
| Report Type | Summary + รายละเอียดย่อย (Breakdown by department / leave type) |
| Description | รายงานสรุปการลาของพนักงานทั้งองค์กร แยกตามประเภทการลา / หน่วยงาน / ประเภทพนักงาน ตามช่วงเวลาที่ HR กำหนด แสดงบนหน้าจอ SCR-010 และ export เป็นไฟล์ได้ |
| Actor / User Role | HR |
| Frequency | On-demand — คาดว่ารายเดือน/รายไตรมาส (Report SRS §2.1.1) |
| Related Screen | SCR-010 (HR Report Screen) |
| Related Requirement IDs | RFR-001, SFR-015, SCR-010, NFR-005, NFR-009 |
| Source Reference | Report SRS §2.1 (RP-001), SRS §4.2 RFR-001, BRD §3.1 BR-010, BRD §5.3.1.C KPI, R1 (QA v2) |
| Version | 1.0 |
| Created By | report-design-agent (2026-07-09) |
| Updated By | — |

## 2. Business Purpose

ให้ HR และผู้บริหารเห็นภาพรวมการลาของทั้งองค์กร สนับสนุนการตัดสินใจด้านการบริหารบุคลากร แทนการรวม Excel จากหลายฝ่าย และใช้ติดตาม KPI ที่กำหนดใน BRD §5.3.1.C (Adoption Rate ≥95%, SLA compliance) (Source: Report SRS §2.1.1, BRD §3.1 BR-010)

## 3. Report Parameters (Filters)

| No | Parameter | Label (TH/EN) | Type | Required | Default | Validation | DB Mapping | Description |
| :---: | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | date_from | วันที่เริ่มต้น / Date From | Date Picker | Y | วันแรกของเดือนปัจจุบัน | ≤ date_to มิฉะนั้น ERR-RPT-001 (Report SRS §2.1.8) | `LeaveRequests.StartDate` (DATE) | วันเริ่มต้นของช่วงที่ต้องการดูรายงาน — map เป็น `LeaveReportFilterDto.StartDate` |
| 2 | date_to | วันที่สิ้นสุด / Date To | Date Picker | Y | วันสุดท้ายของเดือนปัจจุบัน | ≥ date_from | `LeaveRequests.StartDate` (DATE) | วันสิ้นสุดของช่วง — map เป็น `LeaveReportFilterDto.EndDate` (เงื่อนไขการเทียบช่วง ดู §9 + Assumption) |
| 3 | department | แผนก / Department | Dropdown (single) | N | ทั้งหมด (All) | ต้องเป็นค่าที่มีใน `Employees.Department` | `Employees.Department` NVARCHAR(200) | กรองเฉพาะแผนกที่เลือก — map เป็น `LeaveReportFilterDto.Department` |
| 4 | employee_type | ประเภทพนักงาน / Employee Type | Dropdown | N | ทั้งหมด (All) | ค่า: All / ประจำ (1) / Outsource (2) | `Employees.EmployeeType` TINYINT (1=Regular, 2=Outsource) | map เป็น `LeaveReportFilterDto.EmployeeType` |
| 5 | leave_type | ประเภทการลา / Leave Type | Dropdown | N | ทั้งหมด (All) | ต้องเป็น 1 ใน 7 ประเภทใน `LeaveTypes` | `LeaveRequests.LeaveTypeId` TINYINT → `LeaveTypes.LeaveTypeId` | map เป็น `LeaveReportFilterDto.LeaveTypeId` |

## 4. Report Layout

| ส่วน | รายละเอียด |
| --- | --- |
| Header | ชื่อรายงาน "รายงานสรุปการลา", ช่วงวันที่รายงาน, เงื่อนไข filter ที่เลือก (แผนก, ประเภทพนักงาน, ประเภทลา), วัน-เวลาที่ generate (Report SRS §2.1.4) |
| Summary | Summary Strip: จำนวนคำขอทั้งหมด, จำนวนวันลาทั้งหมด, Approve rate (%), Reject rate (%) — แยกตาม employee_type (Regular / Outsource) |
| Detail | ตารางสรุปแยกตาม แผนก (group header) → ประเภทการลา พร้อมจำนวนคำขอ, วันลารวม, Approved/Rejected/Cancelled, Approve Rate |
| Footer | รวมยอดทั้งหมด (Grand Total), จำนวนพนักงานที่ลาในช่วงเวลา (distinct employee), หมายเหตุ |

### Columns Definition

| No | Column | Label (TH/EN) | Data Type | Format | DB Mapping | Description |
| :---: | --- | --- | --- | --- | --- | --- |
| 1 | department | แผนก / Department | Text | — | `Employees.Department` NVARCHAR(200) | ชื่อแผนก/หน่วยงาน — group header (Report SRS §2.1.5) |
| 2 | leave_type_name | ประเภทการลา / Leave Type | Text | — | `LeaveTypes.TypeNameTh` NVARCHAR (คู่ `TypeNameEn` สำหรับ EN) | ชื่อประเภทการลา (7 ประเภท) |
| 3 | request_count | จำนวนคำขอ / Count | Integer | #,##0 | คำนวณ: `COUNT(LeaveRequests.LeaveRequestId)` | จำนวนคำขอลาทั้งหมดของ แผนก+ประเภท ในช่วงเวลา (ทุก status — ดู Assumption) |
| 4 | total_leave_days | รวมวันลา / Total Days | Decimal | #,##0.0 | คำนวณ: `SUM(LeaveRequests.DurationDays)` DECIMAL(10,2) | ผลรวมวันลา — DurationDays รองรับครึ่งวัน (0.5) |
| 5 | approved_count | อนุมัติ / Approved | Integer | #,##0 | คำนวณ: `COUNT WHERE Status = 2` | จำนวนคำขอ Status=Approved |
| 6 | rejected_count | ปฏิเสธ / Rejected | Integer | #,##0 | คำนวณ: `COUNT WHERE Status = 3` | จำนวนคำขอ Status=Rejected |
| 7 | cancelled_count | ยกเลิก / Cancelled | Integer | #,##0 | คำนวณ: `COUNT WHERE Status = 4` | จำนวนคำขอ Status=Cancelled |
| 8 | approve_rate_pct | Approve Rate (%) | Decimal | #,##0.0 "%" | คำนวณ: `approved_count / request_count × 100` | % Approved จากคำขอทั้งหมดของประเภทนั้น (Report SRS §2.1.5); request_count = 0 → แสดง "—" (Assumption) |

## 5. Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | `report-mockup-index.md` ระบุ style reference โฟลเดอร์ `leave-summary-report/` (SAP/Dynamics/Odoo/Zoho) **แต่ไฟล์ mockup ไม่มีอยู่จริงใน repository** (ตรงกับ Report SRS §2.1.6 ที่ระบุว่าไม่พบไฟล์) — ASCII ด้านล่างเป็น Assumption |
| Layout Description | ตาม Report SRS §2.1.4: Header + Summary Strip + Detail (group by แผนก) + Footer |

### Filter Screen

```text
┌─ รายงานสรุปการลา (SCR-010) ─────────────────────────────────────────────┐
│ วันที่เริ่มต้น * [2026-01-01 📅]   วันที่สิ้นสุด * [2026-06-30 📅]              │
│ แผนก [ทั้งหมด ▾]   ประเภทพนักงาน [ทั้งหมด ▾]   ประเภทการลา [ทั้งหมด ▾]        │
│                                                                         │
│        [ ดูรายงาน ]  [ Export Excel ]  [ Export PDF ]  [ Reset ]          │
└─────────────────────────────────────────────────────────────────────────┘
```

### Report Output

```text
┌─────────────────────────────────────────────────────────────────────────┐
│ รายงานสรุปการลา — 01/01/2026 ถึง 30/06/2026                                │
│ Filter: แผนก=ทั้งหมด | ประเภทพนักงาน=ทั้งหมด | ประเภทลา=ทั้งหมด                  │
│ Generated: 09/07/2026 14:30 โดย HR001                                    │
├─ Summary Strip ─────────────────────────────────────────────────────────┤
│ คำขอทั้งหมด: 452   วันลารวม: 1,238.5   Approve: 91.2%   Reject: 5.3%       │
│   • ประจำ: 398 คำขอ / 1,105.0 วัน     • Outsource: 54 คำขอ / 133.5 วัน      │
├─ Detail ────────────────────────────────────────────────────────────────┤
│ แผนก: Information Technology                                             │
│  ประเภทการลา         คำขอ   วันรวม   อนุมัติ  ปฏิเสธ  ยกเลิก  Approve%       │
│  ลาพักผ่อนประจำปี      45    120.5     41      2      2     91.1          │
│  ลาป่วย               30     85.0     28      1      1     93.3          │
│  ...                                                                    │
│ แผนก: Human Resources                                                    │
│  ...                                                                    │
├─ Footer ────────────────────────────────────────────────────────────────┤
│ Grand Total: 452 คำขอ | 1,238.5 วัน | พนักงานที่ลา 187 คน                    │
│ หมายเหตุ: ข้อมูลจากคำขอที่บันทึกในระบบเท่านั้น                    หน้า 1/3       │
└─────────────────────────────────────────────────────────────────────────┘
```

## 6. Commands / Actions

| No | Command | Type | Default State | Trigger Condition | System Response |
| :---: | --- | --- | --- | --- | --- |
| 1 | ดูรายงาน / Generate | Button | Enable | date_from + date_to กรอกครบและ valid | แสดง INF-RPT-001 ระหว่างโหลด → query ตาม §9 → แสดงรายงานบน SCR-010; service method ฝั่ง query ยังไม่มีใน Method Signature (ดู Assumption — เสนอ `IReportService.GetLeaveSummaryAsync()`) |
| 2 | Export Excel | Button | Enable เมื่อ generate รายงานแล้ว | คลิก | เรียก `IReportService.ExportLeaveReportAsync(filter)` ด้วย `Format = ReportFormat.Excel` → download `.xlsx` → SUC-RPT-001 |
| 3 | Export PDF | Button | Enable เมื่อ generate รายงานแล้ว | คลิก | เรียก `IReportService.ExportLeaveReportAsync(filter)` ด้วย `Format = ReportFormat.Pdf` → download `.pdf` → SUC-RPT-001 (รูปแบบไฟล์ยังเป็น Open Issue — ดู §14) |
| 4 | Reset Filter | Button | Enable | คลิก | คืนค่า filter ทั้งหมดเป็น default — ไม่ refresh รายงาน (Report SRS §2.1.7) |

## 7. Sorting Logic

| ระดับ | Field | Direction | Description |
| :---: | --- | --- | --- |
| 1 | `Employees.Department` | ASC | กลุ่มหลักตาม SRS "แผนก → ประเภทการลา → จำนวนวันลา" (Report SRS §2.1.2) |
| 2 | total_leave_days (`SUM(DurationDays)`) | DESC | ภายในแผนก เรียงประเภทลาที่ใช้วันมากสุดก่อน — SRS ระบุ descending แต่ "ข้อมูลไม่เพียงพอสำหรับ sort order ที่แน่ชัด" → baseline นี้เป็น Assumption |
| 3 | `LeaveTypes.LeaveTypeId` | ASC | Tiebreak เมื่อวันรวมเท่ากัน (Assumption — เอกสารนี้) |

## 8. Summary Logic

### Per Group Summary

| Group By | Summary Field | สูตร | Description |
| --- | --- | --- | --- |
| `Employees.Department` | request_count, total_leave_days | SUM ของทุกประเภทลาในแผนก | Subtotal ต่อแผนก (group footer) |
| `Employees.EmployeeType` | request_count, total_leave_days, approve_rate, reject_rate | COUNT / SUM / % | Summary Strip แยก ประจำ vs Outsource (Report SRS §2.1.4) |

### Grand Total

| Summary Field | สูตร | Description |
| --- | --- | --- |
| จำนวนคำขอทั้งหมด | `COUNT(LeaveRequestId)` | ทุก record ที่ผ่าน filter |
| จำนวนวันลาทั้งหมด | `SUM(DurationDays)` | รวมทุกแผนก/ประเภท |
| Approve rate (%) | `COUNT(Status=2) / COUNT(*) × 100` | ทศนิยม 1 ตำแหน่ง |
| Reject rate (%) | `COUNT(Status=3) / COUNT(*) × 100` | ทศนิยม 1 ตำแหน่ง |
| จำนวนพนักงานที่ลา | `COUNT(DISTINCT EmployeeId)` | Footer (Report SRS §2.1.4) |

## 9. Query / Data Source Logic

```text
Table: LeaveRequests (LR)
  JOIN Employees (E)  ON LR.EmployeeId = E.EmployeeId
  JOIN LeaveTypes (LT) ON LR.LeaveTypeId = LT.LeaveTypeId

WHERE LR.StartDate BETWEEN @date_from AND @date_to        -- เงื่อนไขเทียบช่วง: Assumption ดู §14
  AND LR.IsDeleted = 0
  AND E.IsDeleted = 0
  AND (@department IS NULL OR E.Department = @department)
  AND (@employee_type IS NULL OR E.EmployeeType = @employee_type)
  AND (@leave_type IS NULL OR LR.LeaveTypeId = @leave_type)

GROUP BY E.Department, LT.LeaveTypeId, LT.TypeNameTh
  → COUNT(LR.LeaveRequestId)                        AS request_count
  → SUM(LR.DurationDays)                            AS total_leave_days
  → COUNT(CASE WHEN LR.Status = 2 THEN 1 END)       AS approved_count
  → COUNT(CASE WHEN LR.Status = 3 THEN 1 END)       AS rejected_count
  → COUNT(CASE WHEN LR.Status = 4 THEN 1 END)       AS cancelled_count

ORDER BY E.Department ASC, SUM(LR.DurationDays) DESC, LT.LeaveTypeId ASC
```

- Service method: Export ใช้ `IReportService.ExportLeaveReportAsync(LeaveReportFilterDto filter)` (Method Signature §4.11 — Phase 2, SRS Trace: RFR-001, SFR-015, BR-010) รับ filter เป็น `LeaveReportFilterDto` (StartDate, EndDate, Department, EmployeeType, LeaveTypeId, Format) — ส่วน method สำหรับแสดงบนหน้าจอยังไม่มีใน Method Signature (Assumption §14)
- Pagination / row limit: — ไม่ระบุใน SRS; รายงานเป็น aggregate (สูงสุด ~แผนก × 7 ประเภท rows) จึงไม่จำเป็นต้อง paginate (Assumption)

## 10. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-RP001-001 | รายงานนี้เรียกได้เฉพาะ HR (RBAC) — HR เห็นข้อมูลทั้งองค์กร ไม่จำกัดตามแผนก | Endpoint enforce `[Authorize(Policy="HrOnly")]` ที่ Backend; ไม่ filter ตาม department ของผู้เรียก | BRD §3.1 BR-010, SRS NFR-005, Report SRS §2.1.9 |
| BR-RP001-002 | Outsource ไม่มีสิทธิ์ 4 ประเภทลา — filter employee_type=Outsource จะไม่มีแถวประเภทลาเหล่านั้น | เป็นผลจากข้อมูลจริง (ไม่มี record) — ไม่ต้อง hardcode ซ่อนแถว; แถว 0 record ไม่แสดง | BRD BR-011, SRS VR-001, Report SRS §2.1.9 |
| BR-RP001-003 | ลากิจ 3 วัน/ปี ใช้ได้ทั้งประจำและ Outsource — ต้อง group ให้ถูกต้อง | ลากิจปรากฏใน breakdown ของทั้ง 2 employee_type | BRD BR-010, R1 (QA v2), Report SRS §2.1.9 |
| BR-RP001-004 | ข้อมูลคำนวณจาก Leave Request ที่บันทึกในระบบเท่านั้น (ไม่รวม offline) | Query จากตาราง `LeaveRequests` อย่างเดียว — ระบุหมายเหตุใน Footer | Report SRS §2.1.10 Assumption |

## 11. Message List

### Error Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| ERR-RPT-001 | date_from > date_to (Report SRS §2.1.8) | วันที่เริ่มต้นต้องน้อยกว่าหรือเท่ากับวันที่สิ้นสุด | Start date must be less than or equal to end date. |
| ERR-RPT-002 | ไม่พบข้อมูลตาม filter | ไม่พบข้อมูลที่ตรงกับเงื่อนไขที่เลือก | No data found matching the selected criteria. |
| ERR-RPT-003 | Export ล้มเหลว | ไม่สามารถส่งออกไฟล์ได้ กรุณาลองใหม่ | Unable to export file. Please try again. |

### Info / Success Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| INF-RPT-001 | กำลัง generate รายงาน (แสดงระหว่างโหลด) | กำลังดึงข้อมูล กรุณารอสักครู่... | Generating report, please wait... |
| SUC-RPT-001 | Export สำเร็จ | ส่งออกไฟล์สำเร็จ | File exported successfully. |

## 12. Database Tables Reference

| Table Name | Alias | Description |
| --- | --- | --- |
| LeaveRequests | LR | SELECT (aggregate): COUNT / SUM(DurationDays) / COUNT by Status — filter ตามช่วงวันที่ + IsDeleted=0 |
| Employees | E | JOIN เพื่อ group ตาม `Department` และ filter `EmployeeType` |
| LeaveTypes | LT | JOIN เพื่อแสดง `TypeNameTh` / `TypeNameEn` และ filter `LeaveTypeId` |

## 13. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| Validation error | date_from > date_to | ไม่ query — แสดง error ที่ filter, focus ที่ date_from | ERR-RPT-001 | แก้ไขช่วงวันที่ |
| No data | Query คืน 0 record | แสดงข้อความแทนตาราง — ปุ่ม Export disabled | ERR-RPT-002 | ปรับ filter ให้กว้างขึ้น |
| Export error | `ExportLeaveReportAsync` throw / stream ล้มเหลว | ไม่ download ไฟล์, log error พร้อม CorrelationId | ERR-RPT-003 | ลองใหม่; ถ้าซ้ำแจ้ง IT |
| System error | Backend ล่ม / query timeout (HTTP 5xx) | แสดง error ตาม global error handling ของ SPA, log | "ระบบขัดข้องชั่วคราว กรุณาลองใหม่" (pattern เดียวกับ Screen SRS — Assumption) | รอและลองใหม่ |
| Authorization error | ผู้ใช้ที่ไม่ใช่ HR เรียก endpoint ตรง | HTTP 403 — Audit log Access Denied (Security Architecture §9.2) | — (Frontend ไม่แสดง menu นี้ให้ role อื่น) | — |

## 14. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Open Issue (จาก SRS) | Export file format (Excel / PDF / ทั้งคู่) ยังไม่ยืนยัน — เอกสารนี้ใส่ทั้ง 2 ปุ่มเป็น baseline ตาม `ReportFormat { Excel, Pdf }` ใน Method Signature | ตัดปุ่มออกเมื่อ HR ยืนยัน format เดียว; กระทบ TR-006 |
| Open Issue (จาก SRS) | Report template / column layout ยังไม่ยืนยัน — ต้อง confirm ก่อน Phase 2 sprint | กระทบ §4 Columns Definition ทั้งหมด |
| Open Issue (จาก SRS) | Sort order ที่แน่ชัด — SRS ระบุ "ข้อมูลไม่เพียงพอ" | Baseline §7 ต้องให้ HR ยืนยัน |
| Assumption (จาก SRS) | ข้อมูลคำนวณจากคำขอที่บันทึกในระบบเท่านั้น (ไม่รวม offline) | Footer มีหมายเหตุกำกับ |
| Assumption (เอกสารนี้) | เงื่อนไขช่วงวันที่ใช้ `LeaveRequests.StartDate BETWEEN date_from AND date_to` — SRS ไม่ระบุว่านับคำขอที่ span คร่อมช่วงอย่างไร (ทางเลือก: overlap ช่วงลา ↔ ช่วงรายงาน) | หากต้องการนับแบบ overlap ต้องเปลี่ยน WHERE และวิธีคิด DurationDays บางส่วน — ต้อง confirm กับ HR |
| Assumption (เอกสารนี้) | request_count นับคำขอ **ทุก status** (รวม Pending/CancelRequested/Escalated) — SRS แสดง breakdown เฉพาะ Approved/Rejected/Cancelled จึงอาจมี count ไม่ครบยอด (Total ≠ 5+6+7) | ถ้า HR ต้องการนับเฉพาะ 3 status ให้เพิ่ม WHERE Status IN (2,3,4) |
| Assumption (เอกสารนี้) | approve_rate_pct เมื่อ request_count = 0 แสดง "—" (ป้องกัน divide by zero) | — |
| Assumption (เอกสารนี้) | Method Signature มีเฉพาะ `ExportLeaveReportAsync` (export) — method สำหรับแสดงผลบนหน้าจอ SCR-010 ยังไม่มี เสนอเพิ่ม `IReportService.GetLeaveSummaryAsync(LeaveReportFilterDto)` คืน DTO สำหรับ render | ต้องเพิ่มใน Method Signature ก่อน implement |
| Assumption (เอกสารนี้) | ไม่มี pagination — ผลลัพธ์เป็น aggregate ต่อ แผนก×ประเภทลา (ไม่เกินหลักร้อยแถว) | หากองค์กรมีแผนกจำนวนมาก พิจารณา lazy render ฝั่ง UI |
| Assumption (เอกสารนี้) | ASCII mockup ใน §5 วาดขึ้นเอง — `report-mockup-index.md` อ้างโฟลเดอร์ `leave-summary-report/` แต่ไฟล์ไม่มีอยู่จริงใน repo | รอ mockup จริง / HR ยืนยัน layout |
| Assumption (เอกสารนี้) | Summary Strip แยกตาม employee_type ใช้ query เดียวกัน group เพิ่มด้วย `E.EmployeeType` | — |
| Constraint (จาก SRS) | รายงานนี้เป็น **Phase 2** — รายละเอียดยืนยันก่อน sprint ของ Phase 2 | เอกสารนี้เป็น baseline สำหรับ Phase 2 planning |

## Change Log

| Version | Date | Author | Change Type | Description | Remark |
| --- | --- | --- | --- | --- | --- |
| 1.0 | 2026-07-09 | report-design-agent (Claude) | Created | สร้างเอกสารครั้งแรกจาก Report SRS v1.0 (§2.1 RP-001), Data Architecture (LeaveRequests/Employees/LeaveTypes DDL), Method Signature §4.11 (`IReportService.ExportLeaveReportAsync`, `LeaveReportFilterDto`), report-mockup-index.md | Generated ตาม template report-design-agent |

### สรุปการเปลี่ยนแปลงสำคัญ

| ช่วง Version | การเปลี่ยนแปลง | ผลกระทบ |
| --- | --- | --- |
| 1.0 | Baseline แรก | — |
