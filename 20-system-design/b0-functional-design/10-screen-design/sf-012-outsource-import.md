---
function_id: "SF-012"
function_name: "Outsource Data Import"
category: "Screen"
screen_type: "Search & Action Form"
version: "1.0"
status: "Draft"
author: "screen-design-agent (Claude)"
last_updated: "2026-07-12"
---

# SF-012 — Outsource Data Import

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | SF-012 |
| Function Name | Outsource Data Import |
| Category | Screen |
| Screen Type | Search & Action Form (upload → validate/preview → confirm import — ดู Assumption §13 เรื่องการเลือก Screen Type) |
| Description | HR upload ไฟล์ Excel template เพื่อนำเข้าข้อมูลพนักงาน Outsource เข้าระบบ ระบบ validate 7 required fields ทุกแถว แสดง preview พร้อมสถานะ valid/invalid ก่อนยืนยันนำเข้าเฉพาะแถวที่ valid |
| Actor / User Role | HR |
| Related Requirement IDs | SFR-012, SIR-003, VR-013, IF-003, SCR-009 |
| Source Reference | Screen SRS §2.12 (SF-012), Interface SRS §2.3 (IF-003), SRS §4.1 SFR-012, BRD BR-020, BR-011, R3 (QA v2) |
| Version | 1.0 |
| Created By | screen-design-agent (2026-07-12) |
| Updated By | — |

## 2. Business Purpose

Onboard ข้อมูลพนักงาน Outsource เข้าระบบ Leave App ได้รวดเร็วโดยไม่ต้องกรอกข้อมูลทีละคน เนื่องจาก Outsource ไม่ได้เป็นพนักงานตรงของ ABC Company จึงไม่ได้อยู่ใน HRIS (ไม่ใช้ IF-001 Employee Master Sync) — ต้องใช้ Excel Import (IF-003) แทน พร้อม validate ข้อมูลให้ครบถ้วนก่อนนำเข้าเพื่อลดข้อผิดพลาดจากการกรอกมือ (Source: Screen SRS §2.12.1, Interface SRS §2.3.1, BRD BR-020)

## 3. Screen Overview

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | Outsource Import (SCR-009) |
| Menu Path | Main Menu > Employee Management (Header Navigation, role = HR) > Outsource Import |
| Navigation Inbound | Header Navigation (role = HR) → Employee Management (Screen SRS §2.12.2) |
| Navigation Outbound | ไม่มี — หลัง Import แสดงผลลัพธ์ (success/fail summary) อยู่หน้าเดิม เพื่อให้ HR ตรวจ error report ต่อได้ |
| Preconditions | Login เป็น HR (SF-001), มีไฟล์ Excel template ที่เตรียมไว้ (หรือ Download Template ก่อน) |
| Postconditions | Record ที่ valid ถูกบันทึกเป็น Employee ใหม่ (EmployeeType = Outsource), ImportLogs ถูกบันทึกทุกครั้ง (สำเร็จหรือไม่), แสดง import result (success/fail count + error report) |

### Related Screens

| Screen ID | Screen Name | Description |
| --- | --- | --- |
| SCR-008 | HR Monitoring Dashboard | หน้าจอ HR อื่นที่เข้าถึงผ่าน Header Navigation เดียวกัน (role = HR) — ไม่ใช่ navigation โดยตรง (Assumption) |

### Screen Flow

```text
Header Navigation (role = HR)
  └── Employee Management
        └── SF-012 Outsource Import (SCR-009)
              ├── [Download Template] → ดาวน์โหลดไฟล์ (ไม่เปลี่ยนหน้า)
              ├── [Upload] → แสดง Preview (อยู่หน้าเดิม)
              └── [Import] → แสดง Import Result (อยู่หน้าเดิม)
```

```mermaid
flowchart LR
    A[Header Navigation — role HR]
    B[SF-012 Outsource Import SCR-009]

    A -->|Employee Management| B

    style B fill:#EAD7A4,stroke:#333
    style A stroke-dasharray: 5 5
```

## 4. Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | — (Screen SRS §2.12.3 และ Cross-Function Traceability ระบุ "ไม่มี mockup อ้างอิง" — ASCII ด้านล่างเป็น Assumption ของเอกสารนี้) |
| Layout Description | ส่วนบน: ปุ่ม Download Template + ปุ่ม Upload (เลือกไฟล์ .xlsx) ส่วนกลาง: ตาราง Preview แสดงทุกแถวพร้อมสถานะ Valid/Invalid และ error message ส่วนล่าง: ปุ่ม Import (enable เมื่อมีอย่างน้อย 1 แถว valid) และผลลัพธ์หลัง Import |

```text
+----------------------------------------------------------------------+
| [LOGO]  Leave Management System        User: [HR_ID]  [HR_NAME]     |
+----------------------------------------------------------------------+
| Menu >> Employee Management >> Outsource Import (SCR-009)            |
+----------------------------------------------------------------------+
| นำเข้าข้อมูลพนักงาน Outsource (Outsource Data Import)                 |
|                                                                      |
| [ ดาวน์โหลด Template ]        [ Choose File... ]  [ Upload ]          |
|                                                                      |
| Preview (แสดงหลัง Upload)                                            |
| +------------------------------------------------------------------+ |
| | แถว | ชื่อ-สกุล TH | รหัสพนักงาน | Email          | สถานะ | ข้อผิดพลาด | |
| | 2   | สมหญิง...    | OUT001      | somying@xyz.co | ✓ Valid |  —      | |
| | 3   | สมชาย...     | OUT002      | somchai@xyz.co | ✗ Invalid | Email ซ้ำ | |
| +------------------------------------------------------------------+ |
| สรุป: ทั้งหมด 2 แถว — Valid 1, Invalid 1                              |
|                                                                      |
|                                      [ Import ]   [ ยกเลิก ]          |
|                                                                      |
| ผลลัพธ์ (แสดงหลัง Import): นำเข้าข้อมูลสำเร็จ 1 รายการ (ไม่สำเร็จ 1)       |
+----------------------------------------------------------------------+
```

## 5. Fields Definition

### 5.1 Upload Section

| No | Field Name | Label (TH/EN) | Type | Length | Required | Default | Validation | DB Mapping | Description |
| :---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | import_file | ไฟล์ Excel / Excel File | File Upload | — | Y | — | รองรับเฉพาะ `.xlsx` (TR-006), Content-Type = `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` — ผิดรูปแบบ: ERR-IF003-006 | — (ไฟล์ไม่ persist เป็น column — parse แล้วประมวลผลทันที) | ไฟล์ Excel template ที่ HR เตรียมข้อมูล Outsource |

### 5.2 Excel Template Fields (7 Required Fields — Interface SRS §2.3.4)

| No | Field Name | Label (TH/EN) | Type | Length | Required | Default | Validation | DB Mapping (Employees) | Description |
| :---: | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | name_th (Col A) | ชื่อ-นามสกุล TH / Name (Thai) | Text | 200 | Y | — | ไม่ว่าง — ว่าง: ERR-IF003-001 | `FullNameTh` (NVARCHAR(200)) | ชื่อ-นามสกุลภาษาไทย |
| 2 | name_en (Col B) | ชื่อ-นามสกุล EN / Name (English) | Text | 200 | Y | — | ไม่ว่าง — ว่าง: ERR-IF003-001 | `FullNameEn` (NVARCHAR(200)) | ชื่อ-นามสกุลภาษาอังกฤษ |
| 3 | employee_id (Col C) | รหัสพนักงาน / Employee ID | Text | 20 | Y | — | ไม่ว่าง, unique ในระบบ — ซ้ำ: ERR-IF003-002 | `EmployeeId` (NVARCHAR(20), PK) | รหัสพนักงาน Outsource เช่น "OUT001" |
| 4 | department_position (Col D) | แผนก / ตำแหน่ง / Department / Position | Text | 200 | Y | — | ไม่ว่าง — ว่าง: ERR-IF003-001 | `Department` (NVARCHAR(200)) — ดู Assumption §13 เรื่อง Position | แผนกและตำแหน่งของ Outsource (Excel เป็น 1 field รวม) |
| 5 | agency_company (Col E) | บริษัทต้นสังกัด / Agency Company | Text | 200 | Y | — | ไม่ว่าง — ว่าง: ERR-IF003-001 | `AgencyCompany` (NVARCHAR(200)) | ชื่อบริษัท Outsource vendor ต้นสังกัด |
| 6 | email (Col F) | Email (ใช้ Login) | Text (Email) | 200 | Y | — | Format email ถูกต้อง + unique ในระบบ — ผิด/ซ้ำ: ERR-IF003-003 | `Email` (NVARCHAR(200), UNIQUE) | ใช้ login (Entra ID SSO) และรับ notification |
| 7 | abc_start_date (Col G) | วันเริ่มงานใน ABC / ABC Start Date | Date | — | Y | — | Format วันที่ถูกต้อง (YYYY-MM-DD) และ ≤ วันนี้ — ผิด: ERR-IF003-005 | `AbcStartDate` (DATE) + `HireDate` (DATE, ดู Assumption §13) | วันที่เริ่มงานที่ ABC Company — ใช้คำนวณอายุงานของ Outsource (SF-002 §13) |
| 8 | line_manager_id (Col H) | รหัสหัวหน้างาน / Line Manager ID | Text | 20 | Y | — | ต้องมีอยู่ในระบบ (Employees) — ไม่พบ: ERR-IF003-004 | `ManagerId` (NVARCHAR(20), FK → Employees) | รหัสพนักงาน ABC ที่เป็นหัวหน้างานของ Outsource รายนี้ |

**System-Set Field (ไม่กรอกใน Excel):** `EmployeeType` = 2 (Outsource) ถูกตั้งค่าอัตโนมัติทุก record ที่ import ผ่าน (BR-011)

## 6. Commands / Actions

| No | Command | Type | Default State | Trigger Condition | System Response |
| :---: | --- | --- | --- | --- | --- |
| 1 | Download Template | Link/Button | Enable | คลิกลิงก์ | ดาวน์โหลดไฟล์ template Excel เปล่า (static asset) — ไม่เรียก service |
| 2 | Upload | Button | Enable | เลือกไฟล์แล้วคลิก Upload | Validate นามสกุลไฟล์ + Content-Type (TR-006) → parse ทุกแถว → validate 7 fields ต่อแถว → แสดง preview พร้อมสถานะ valid/invalid ต่อแถว (ดู Assumption §13 เรื่อง preview endpoint) |
| 3 | Import | Button | Disable จนกว่ามีอย่างน้อย 1 แถว valid ใน preview | คลิก Import | เรียก `IImportService.ImportOutsourceEmployeesAsync(hrEmployeeId, excelStream, fileName)` → บันทึกเฉพาะแถว valid เป็น Employee (EmployeeType=Outsource) → บันทึก ImportLogs → แสดงผลลัพธ์ SUC-IF003-001 |

## 7. Screen Behavior

### 7.1 Initial Screen (onLoad)

- แสดงปุ่ม Download Template และช่องเลือกไฟล์ Upload ว่าง — ไม่มี preview, ปุ่ม Import disabled
- ไม่มีการโหลดข้อมูลจาก server (หน้าจอเริ่มต้นว่างเปล่า)

### 7.2 Click "Download Template"

- ดาวน์โหลดไฟล์ Excel template เปล่า (มี header ตาม §5.2) — ไม่มี business validation

### 7.3 เลือกไฟล์ + Click "Upload"

#### 7.3.1 Validation (File-level — ตามลำดับใน `ImportOutsourceEmployeesAsync`, Method Signature §4.9)

| ลำดับ | Validation | Requirement | Error Message |
| :---: | --- | --- | --- |
| 1 | นามสกุลไฟล์ = `.xlsx` เท่านั้น | TR-006 | ERR-IF003-006 |
| 2 | Content-Type = spreadsheetml.sheet | TR-006 | ERR-IF003-006 |

- ไฟล์ผ่าน file-level validation → parse แต่ละแถว → รัน row-level validation (ตาราง §7.4.1) → แสดงผลเป็น preview พร้อมสถานะต่อแถว (ไม่ commit ข้อมูลใด ๆ ในขั้นตอนนี้ — ดู Assumption §13 เรื่อง preview vs import 2-step)

### 7.4 Click "Import"

#### 7.4.1 Validation (Row-level — ตามลำดับใน `ImportOutsourceEmployeesAsync`)

| ลำดับ | Validation | Requirement | Error Message |
| :---: | --- | --- | --- |
| 1 | 7 required fields ครบทุก field ไม่ว่าง | VR-013 | ERR-IF003-001 |
| 2 | email format ถูกต้อง (regex) | VR-013 | ERR-IF003-003 |
| 3 | email unique ในระบบ | VR-013 | ERR-IF003-003 |
| 4 | employee_id unique ในระบบ | VR-013 | ERR-IF003-002 |
| 5 | line_manager_id ต้องมีในระบบ | VR-013 | ERR-IF003-004 |
| 6 | abc_start_date format ถูกต้องและ ≤ วันนี้ | VR-013 | ERR-IF003-005 |

- แถวที่ fail validation ข้อใดข้อหนึ่ง: skip ทั้งแถว (ไม่ import บางส่วนของแถว), รวมอยู่ใน error report — แถวอื่นที่ valid ยัง import ต่อ (Interface SRS §2.3.2, §2.3.6)
- ถ้าทุกแถว fail validation ทั้งหมด: ระบบ throw `ExcelImportValidationException` — ไม่ import เลย แสดง error report ครบทุกแถว (Interface SRS §2.3.6)

#### 7.4.2 Insert / Update (DB Transaction)

```text
BEGIN TRANSACTION
  FOREACH validRow IN previewRows WHERE Status = Valid:
    UPSERT Employees
      (EmployeeId = validRow.employee_id,
       EmployeeCode = validRow.employee_id,   -- ดู Assumption §13 (EmployeeCode ไม่มีใน template)
       FullNameTh = validRow.name_th, FullNameEn = validRow.name_en,
       Department = validRow.department_position, Position = NULL,  -- ดู Assumption §13
       Email = validRow.email,
       HireDate = validRow.abc_start_date,    -- ดู Assumption §13 (NOT NULL constraint)
       ManagerId = validRow.line_manager_id,
       EmployeeType = 2,                       -- Outsource — BR-011
       AgencyCompany = validRow.agency_company,
       AbcStartDate = validRow.abc_start_date,
       IsActive = 1,
       CreatedBy = hrEmployeeId)

  INSERT ImportLogs
    (ImportedBy = hrEmployeeId, FileName = fileName,
     TotalRecords, SuccessRecords, FailedRecords,
     ErrorDetails = JSON [{"row": N, "field": "...", "error": "..."}, ...])
COMMIT

AFTER COMMIT: — ไม่มี notification event ระบุใน SRS สำหรับ import (ต่างจาก SF-003 ที่ trigger IF-002)
```

- สำเร็จ: แสดง SUC-IF003-001 พร้อมจำนวน success/fail และลิงก์ดู error report (ถ้ามี)

## 8. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-SF012-001 | Import เฉพาะแถว (record) ที่ valid ทั้งแถว — แถวที่มี error ถูก skip ทั้งแถว ไม่ import บางส่วนของแถว | `ImportOutsourceEmployeesAsync` commit เฉพาะ valid rows ในทรานแซคชันเดียว | BRD BR-020, VR-013, Interface SRS §2.3.2 |
| BR-SF012-002 | EmployeeType ของทุก record ที่ import ผ่าน IF-003 ถูก set = 2 (Outsource) โดยอัตโนมัติ | ไม่มี field ให้ HR กรอก employee_type ใน template | BRD BR-011, Interface SRS §2.3.8 |
| BR-SF012-003 | รองรับเฉพาะไฟล์ `.xlsx` | ไฟล์ format อื่น (.xls, .csv) block ก่อน parse | TR-006, Interface SRS §2.3.9 Constraint |
| BR-SF012-004 | 7 required fields ต้องครบทุกแถวที่จะ import ได้ | field ว่างใดๆ ทำให้ทั้งแถว invalid | VR-013, BRD BR-020 |
| BR-SF012-005 | ทุกครั้งที่ import (สำเร็จหรือไม่) ต้องบันทึก ImportLogs | Audit trail ของการ import ทุกครั้ง | Method Signature §4.9 (`IImportLogRepository.AddAsync` เสมอ) |

```text
Upload ไฟล์
│
├── นามสกุล/Content-Type ผิด → ERR-IF003-006 (block ทั้งไฟล์ ไม่ parse)
│
└── ผ่าน file-level validation → parse ทีละแถว
      │
      ├── แถว valid ครบ 7 fields + unique + manager พบ + วันที่ถูกต้อง → สถานะ Valid
      │     └── กด Import → บันทึกเป็น Employee (EmployeeType=2)
      │
      └── แถวใดแถวหนึ่ง fail → สถานะ Invalid + error message ต่อแถว
            └── กด Import → แถวนี้ถูก skip (ไม่บันทึก) แต่แถว valid อื่นยัง import
                  └── ถ้าทุกแถว Invalid → ไม่ import เลย (ExcelImportValidationException)
```

## 9. Message List

### Error Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| ERR-IF003-001 | Field บังคับว่าง (VR-013) | แถวที่ {N}: {Field} ไม่สามารถเว้นว่างได้ | Row {N}: {Field} is required and cannot be empty. |
| ERR-IF003-002 | รหัสพนักงานซ้ำในระบบ | แถวที่ {N}: รหัสพนักงาน {employee_id} มีในระบบแล้ว | Row {N}: Employee ID {employee_id} already exists in the system. |
| ERR-IF003-003 | Email format ผิดหรือซ้ำในระบบ | แถวที่ {N}: Email {email} ไม่ถูกต้องหรือมีในระบบแล้ว | Row {N}: Email {email} is invalid or already exists. |
| ERR-IF003-004 | Line Manager ไม่พบในระบบ | แถวที่ {N}: ไม่พบรหัสหัวหน้างาน {manager_id} ในระบบ | Row {N}: Manager ID {manager_id} not found in the system. |
| ERR-IF003-005 | วันที่ format ผิดหรือเป็นอนาคต | แถวที่ {N}: วันที่เริ่มงานไม่ถูกต้อง (ต้องเป็น YYYY-MM-DD และไม่เกินวันนี้) | Row {N}: Invalid start date (must be YYYY-MM-DD and not a future date). |
| ERR-IF003-006 | รูปแบบไฟล์ผิด | กรุณาอัปโหลดไฟล์ .xlsx เท่านั้น | Please upload .xlsx files only. |

### Success / Info Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| SUC-IF003-001 | Import สำเร็จ (มีอย่างน้อย 1 แถว valid) | นำเข้าข้อมูลสำเร็จ {X} รายการ (ไม่สำเร็จ {Y} รายการ — ดู error report) | Import completed: {X} records succeeded, {Y} failed — see error report. |

## 10. Popup / Sub-screen Definition

### 10.1 Import Result Summary (แสดงหลังคลิก Import)

| No | Field Name | Label | Data Source | Description |
| :---: | --- | --- | --- | --- |
| 1 | total_records | ทั้งหมด | `ImportResultDto.TotalRecords` | จำนวนแถวทั้งหมดในไฟล์ |
| 2 | success_records | สำเร็จ | `ImportResultDto.SuccessRecords` | จำนวนแถวที่ import สำเร็จ |
| 3 | failed_records | ไม่สำเร็จ | `ImportResultDto.FailedRecords` | จำนวนแถวที่ถูก skip |
| 4 | error_list | รายการ error | `ImportResultDto.Errors` (List of `ImportErrorDto`: RowNumber, Field, Message) | ตารางแถวที่ error พร้อมเหตุผล — ดาวน์โหลดเป็น error report ได้ (ดู Assumption §13) |

## 11. Database Tables Reference

| Table Name | Alias | Description |
| --- | --- | --- |
| Employees | — | UPSERT เฉพาะแถวที่ valid — `EmployeeType = 2` บังคับ, ตรวจ unique `EmployeeId`/`Email`, ตรวจ `ManagerId` มีอยู่จริง (FK) |
| ImportLogs | — | INSERT ทุกครั้งหลังกด Import (ไม่ว่าสำเร็จทั้งหมด/บางส่วน/ล้มเหลวทั้งหมด) — `ErrorDetails` เก็บเป็น JSON |

## 12. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| Validation error (row-level) | Field ว่าง/format ผิด/ซ้ำ/manager ไม่พบ ต่อแถว (§7.4.1) | Skip เฉพาะแถวนั้น, รวมใน error list — แถว valid อื่นยัง import ต่อ | ERR-IF003-001 ถึง ERR-IF003-005 ตามกรณี | แก้ไขแถวใน Excel แล้ว Upload ใหม่ |
| File format error | นามสกุล/Content-Type ไม่ใช่ .xlsx (§7.3.1) | Block ทั้งไฟล์ ไม่ parse | ERR-IF003-006 | เลือกไฟล์ .xlsx แล้ว Upload ใหม่ |
| All-rows-invalid error | ทุกแถวใน validation ไม่ผ่าน (`ExcelImportValidationException`) | ไม่ import เลย, แสดง error report ครบทุกแถว | รวม ERR-IF003-001–005 ตามแถว | แก้ไขไฟล์ทั้งหมดแล้วลองใหม่ |
| System error | Parse ไฟล์ล้มเหลว (ไฟล์เสีย) หรือ backend error ขณะ import | Rollback ทั้ง transaction, ไม่บันทึกใด ๆ | "เกิดข้อผิดพลาด กรุณาลองใหม่" | Upload ไฟล์ใหม่ |

## 13. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Open Issue (จาก SRS) | ระบบ handle การ update Outsource ที่เปลี่ยน Manager/แผนกอย่างไร (re-import ทั้งไฟล์ หรือแก้ไขในระบบโดยตรง) ยังไม่ระบุใน BRD | Interface SRS §2.3.9 — ต้อง confirm workflow ก่อน implement edit/update flow ของ Outsource |
| Open Issue (จาก SRS) | Max file size ของไฟล์ Excel import ยังไม่ระบุ | กระทบ upload validation ที่ frontend/backend — ต้อง confirm |
| Assumption (จาก SRS) | `Employees.HireDate` เป็น NOT NULL ตาม Data Architecture DDL — สำหรับ Outsource เอกสารนี้ตั้งค่า `HireDate = AbcStartDate` เพื่อไม่ violate constraint (สอดคล้องกับ Assumption ของ SF-002 ที่ใช้ `AbcStartDate` คำนวณอายุงาน Outsource) | ต้อง confirm กับทีม Dev/DBA ว่าการ mirror ค่าไปยัง HireDate เหมาะสมหรือควรอนุญาต NULL สำหรับ Outsource |
| Assumption (เอกสารนี้) | `Employees.EmployeeCode` เป็น NOT NULL + UNIQUE ตาม DDL แต่ Excel template ไม่มี field นี้แยกจาก employee_id — เอกสารนี้ตั้งค่า `EmployeeCode = employee_id` จนกว่าจะยืนยัน business rule ที่ต่างกัน | ต้อง confirm กับ HR/BA ว่า EmployeeCode ของ Outsource ควรมี format ต่างจาก EmployeeId หรือไม่ |
| Assumption (เอกสารนี้) | Excel column D "แผนก / ตำแหน่ง" เป็น field เดียวใน template แต่ตาราง `Employees` มี `Department` และ `Position` แยกกัน — เอกสารนี้ map ค่าทั้งหมดเข้า `Department`, `Position = NULL` | ต้อง confirm กับ HR ว่าต้องแยกแผนก/ตำแหน่งด้วย delimiter หรือไม่ |
| Assumption (เอกสารนี้) | Method Signature §4.9 มีเฉพาะ `ImportOutsourceEmployeesAsync()` ซึ่ง validate+import ในคำสั่งเดียว (synchronous) แต่ Screen SRS/Interface SRS ต้องการ UX แบบ 2 ขั้นตอน (Upload = preview ก่อน, Import = ยืนยันภายหลัง) — เอกสารนี้ assume ว่าต้องมี preview mechanism เพิ่มเติม (เช่น dry-run parameter หรือ client-side parse) ที่ยังไม่ระบุใน Method Signature | ต้องเพิ่ม parameter/endpoint แยกสำหรับ preview ก่อน implement — confirm กับทีม Dev |
| Assumption (เอกสารนี้) | Screen SRS §2.12.6 มีชุด Message ID ของตัวเอง (ERR-IMP-001–004, SUC-IMP-001) ซึ่งดูซ้ำซ้อนกับ ERR-IF003-001–006/SUC-IF003-001 ใน Interface SRS §2.3.7 — เอกสารนี้ใช้ชุด IF003 เป็นหลักเพราะครอบคลุมกว่า (ตรงกับ validation ใน Method Signature §4.9 ทุกข้อ) | ต้องให้ BA ยืนยันเพื่อเลิกใช้ชุดใดชุดหนึ่งใน SRS รอบถัดไป |
| Note | ไม่มี mockup อ้างอิงใน SRS — ASCII ใน §4 และ preview/result popup ใน §10 เป็น Assumption ของเอกสารนี้ทั้งหมด | ต้องให้ UX/Business review ก่อนถือเป็น final layout |
| Note | Service method หลัก: `IImportService.ImportOutsourceEmployeesAsync()` (Method Signature §4.9) — ใช้เป็น contract ระหว่าง UI กับ backend | — |

## Change Log

| Version | Date | Author | Change Type | Description | Remark |
| --- | --- | --- | --- | --- | --- |
| 1.0 | 2026-07-12 | screen-design-agent (Claude) | Created | สร้างเอกสารครั้งแรกจาก Screen SRS v1.1 (§2.12 SF-012), Interface SRS §2.3 (IF-003), Data Architecture Design (Employees/ImportLogs DDL), Method Signature §4.9 (`IImportService.ImportOutsourceEmployeesAsync`) | Generated ตาม template screen-design-agent |

### สรุปการเปลี่ยนแปลงสำคัญ

| ช่วง Version | การเปลี่ยนแปลง | ผลกระทบ |
| --- | --- | --- |
| 1.0 | Baseline แรก | — |
