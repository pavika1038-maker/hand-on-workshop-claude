# Report Functions — Output Template

> เลือกรูปแบบที่เหมาะสม:
> - **รูปแบบ A (ย่อ)** — สำหรับรายงานภายในที่ไม่ซับซ้อน (Summary, Monitoring)
> - **รูปแบบ B (เต็ม)** — สำหรับรายงานที่ต้องส่งมอบลูกค้าหรือรายงานที่ซับซ้อน (Transaction Detail, Formal Spec)

---

## รูปแบบ A — Internal / Simple Report

```markdown
---
function_id: "RPT-[NNN]"
function_name: "[Report Name]"
category: "Report"
report_type: "[Transaction Detail / Monitoring / Summary / Dashboard / Export]"
version: "1.0"
status: "Draft"
author: ""
last_updated: ""
---

# RPT-[NNN] — [Report Name]

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | RPT-[NNN] |
| Report Name | [ชื่อรายงาน] |
| Category | Report |
| Report Type | [Transaction Detail / Monitoring / Summary / Dashboard / Export] |
| Description | [อธิบายรายงาน] |
| Actor / User Role | [ผู้ใช้งาน] |
| Frequency | [เรียกดูเมื่อจำเป็น / รายวัน / รายเดือน] |
| Related Requirement IDs | [RFR-xxx] |

## 2. Business Purpose

[ทำไมรายงานนี้ถึงมีอยู่]

## 3. Report Parameters (Filters)

| No | Parameter | Label (TH/EN) | Type | Required | Default | Description |
| :---: | --- | --- | --- | --- | --- | --- |
| 1 | | | Dropdown | | | |
| 2 | | | Date Range | | | |

## 4. Report Layout

| ส่วน | รายละเอียด |
| --- | --- |
| Header | [ชื่อรายงาน, filter criteria, print info] |
| Summary | [ยอดรวม, KPI] |
| Detail | [ตารางข้อมูลหลัก] |
| Footer | [Totals, Last Refresh, Page No.] |

### Columns Definition

| Column | Label (TH/EN) | Data Type | Description |
| --- | --- | --- | --- |
| | | | |

## 5. Mockup / UI Layout

### Filter Screen

```text
[ASCII mockup ของหน้าจอกำหนดเงื่อนไข]
+----------------------------------------------------------------------+
| [Report Name]                                                        |
+----------------------------------------------------------------------+
| Filter 1 : [ Dropdown ▼ ]                                           |
| Filter 2 : [ Dropdown ▼ ]                                           |
| From Date : [ Date 📅 ]        To Date : [ Date 📅 ]               |
| [ ] Include Cancelled                                                |
|                                                                      |
|                          [ Preview ] [ Excel ] [ Editor ]            |
+----------------------------------------------------------------------+
```

### Report Output (PDF/Excel)

```text
[ASCII mockup ของ report output]
+----------------------------------------------------------------------+
| [Company Name]                                                       |
| [Report Name]                                                        |
+----------------------------------------------------------------------+
| Print Date: dd/mm/yyyy HH:MM    Printed By: [User]                  |
| Filters: [Filter Criteria Summary]                                   |
+----------------------------------------------------------------------+
| No | Col1 | Col2 | Col3 | ... | Total |                             |
|----|------|------|------|-----|-------|                             |
| 1  | ...  | ...  | ...  | ... | ...   |                             |
+----------------------------------------------------------------------+
| Summary Per Group: ...                                               |
+----------------------------------------------------------------------+
| Grand Total: ...                                                     |
+----------------------------------------------------------------------+
```

## 6. Commands / Actions

| Command | Description | Trigger | Response |
| --- | --- | --- | --- |
| Execute / Preview | แสดงรายงาน | กรอก filter แล้วกด | แสดงผลตามเงื่อนไข |
| Export Excel | ส่งออก Excel | กดปุ่ม | ดาวน์โหลดไฟล์ Excel |
| Export PDF | ส่งออก PDF | กดปุ่ม | ดาวน์โหลดไฟล์ PDF |

## 7. Sorting Logic

| ระดับ | Field | Direction | Description |
| :---: | --- | --- | --- |
| 1 | [Group field] | ASC | กลุ่มหลัก |
| 2 | [Sub-group field] | ASC | กลุ่มย่อย |
| 3 | [Detail field] | ASC/DESC | ภายในกลุ่ม |

## 8. Summary Logic

### Per Group Summary

| Column | Aggregation | Description |
| --- | --- | --- |
| [จำนวน] | COUNT | จำนวนรายการ |
| [ยอดรวม] | SUM | ยอดรวม |

### Grand Total

| Column | Aggregation | Description |
| --- | --- | --- |
| [จำนวน] | COUNT | จำนวนรายการทั้งหมด |
| [ยอดรวม] | SUM | ยอดรวมทั้งหมด |

## 9. Business Rules

| Rule ID | Business Rule | Impact | Source |
| --- | --- | --- | --- |
| BR-RPT[NNN]-001 | [อธิบาย rule] | [ผลกระทบ] | [Reference] |

## 10. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| | | |

## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | | | Created | สร้างเอกสารครั้งแรก |
```

---

## รูปแบบ B — Formal / Software Report Specification

```markdown
# Software Report Specification
## [Report Name]

---

## Document Information

- **Document Title:** Spec – [Report Name]
- **Purpose:** [วัตถุประสงค์ของเอกสาร]
- **System:** [ชื่อระบบ]
- **Company:** [ชื่อบริษัท]
- **Version:** V0.01

---

## Approval & Sign-off

### Certification
ขอรับรองว่าสามารถให้ทีมพัฒนาได้ตามเอกสารฉบับนี้

| Item | Value |
|---|---|
| Name | |
| Organization | |
| Signature | |
| Date | |

### Review By
- **[Reviewer Name]** ([Role])

### Approve By
- **[Approver Name]** ([Role])

---

## Change History

| Date | Author | Version | Reason for Change | Reference Section |
|---|---|---|---|---|
| | | V0.01 | Initial version | All |

---

## 1. Introduction

### 1.1 Overview
[อธิบายภาพรวมของรายงาน — ครอบคลุมอะไรบ้าง]

### 1.2 Business Background
[อธิบายที่มาทางธุรกิจ — ทำไมต้องมีรายงานนี้]

### 1.3 Purpose
1. [วัตถุประสงค์ข้อ 1]
2. [วัตถุประสงค์ข้อ 2]
3. [วัตถุประสงค์ข้อ 3]

---

## 2. Overview Scope

[อธิบาย scope ของรายงาน — ทำงานบนระบบอะไร, ดึงข้อมูลจากไหน]

> ⚠️ หมายเหตุด้าน Performance
> [ระบุหมายเหตุด้าน performance ถ้ามี หรือระบุว่าไม่มี]

---

## 3. Summary Requirements and Expectations

### 3.1 Report Invocation
- [วิธีเรียกใช้รายงาน — กรองตามอะไร]

### 3.2 Data Display Options
- [ตัวเลือกการแสดงข้อมูล — ทุกสาขา / เลือกสาขา / Multi-select]

> ขึ้นกับ **สิทธิ์ผู้ใช้งาน**

### 3.3 Filter Conditions
- [รายการ filter ทั้งหมด]

Screen Outline:

Name: [ชื่อรายงาน]
Menu: [Menu Path]

```text
[ASCII mockup ของ filter screen]
+----------------------------------------------------------------------+
| [Report Name]                                                        |
+----------------------------------------------------------------------+
| Field 1 : [ Dropdown ▼ ]                                    (1)     |
| Field 2 : [ Dropdown ▼ ]                                    (2)     |
| Field 3 : [ Dropdown ▼ ]                                    (3)     |
| Sort By  : [ Dropdown ▼ ]                                    (4)     |
| From Date : [ Date 📅 ]        To Date : [ Date 📅 ]        (5)     |
| [ ] Include Cancelled                                        (6)     |
|                                                                      |
|                          [ Preview ] [ Excel ] [ Editor ]            |
+----------------------------------------------------------------------+
```

### 3.4 PDF Sorting Logic
1. [Sort Level 1]
2. [Sort Level 2]
3. [Sort Level 3]
4. [Sort Level 4]

### 3.5 Output
- [รูปแบบ output — PDF, Excel]
- [ระดับ summary — ตามสาขา, ตามแผนก, รวมทั้งหมด]

---

## 4. Business Flow

- [อธิบายผลกระทบต่อ business flow หรือระบุว่า **ไม่มีการเปลี่ยนแปลง Business Flow**]

---

## 5. Menu & Function Overview

| Menu | Description | Type |
|---|---|---|
| [Menu Path] | [คำอธิบาย] | Custom Screen |

---

## 6. Functional Requirement Details

### 6.1 Screen: [Screen Name]

**Menu Path:**
[Full Menu Path]

**Purpose:**
[วัตถุประสงค์ของหน้าจอ]

---

### 6.2 Filter Fields Specification

| No | Field | Type | Mandatory | Default |
|---|---|---|---|---|
| 1 | [Field Name] | Dropdown | Yes | [Default Value] |
| 2 | [Field Name] | Dropdown | Yes | All |
| 3 | [Field Name] | Dropdown | No | All |
| 4 | [Field Name] | Dropdown (Single) | Yes | [Default Value] |
| 5 | From – To Date | Date | Yes | Current Date |
| 6 | [Toggle Field] | Toggle | No | Yes |
| 7 | Preview | Button | - | - |
| 8 | Export Excel | Button | - | - |
| 9 | Editor | Button | - | - |

---

## 7. Report Requirement – PDF / Excel

PDF Outline:

```text
[ASCII mockup ของ PDF output]
+=======================================================================+
| [Company Name]                                                        |
| [Report Name]                                                         |
+=======================================================================+

เงื่อนไข:
วันที่พิมพ์ : dd/mm/yyyy HH:MM          พิมพ์โดย : [User]

เงื่อนไขรายงาน:
[Filter 1] : [Value]    [Filter 2] : [Value]    [Filter 3] : [Value]
ตั้งแต่วันที่ : dd/mm/yyyy   ถึงวันที่ : dd/mm/yyyy

+----+------+----------------+------------+----------+-----------+------+
| No | Col1 | Col2           | Col3       | ...      | Total     | Flag |
+----+------+----------------+------------+----------+-----------+------+
| 1  | ...  | ...            | ...        | ...      | ...       |      |
| 2  | ...  | ...            | ...        | ...      | ...       |      |
+----+------+----------------+------------+----------+-----------+------+

หมายเหตุ: [เงื่อนไขการแสดงข้อมูล]


สรุปตาม[กลุ่ม]:
+------+--------+---------+-----------+-----------+
| กลุ่ม | จำนวน | ยอดรวม  | ...       | หมายเหตุ |
+------+--------+---------+-----------+-----------+
| ...  | ...    | ...     | ...       |           |
+------+--------+---------+-----------+-----------+


สรุปรวมทั้งหมด:
+--------+---------+-----------+-----------+
| จำนวน | ยอดรวม  | ...       | ...       |
+--------+---------+-----------+-----------+
| ...    | ...     | ...       | ...       |
+--------+---------+-----------+-----------+

หมายเหตุ: [หมายเหตุเพิ่มเติม เช่น ตัวเลขสีแดง = รายการยกเลิก]
```

### 7.1 Header
- Report Name
- Company Name
- Branch Name
- Date Range
- Print Date / Time
- Printed By
- Page No.
- [Remark / Special Notes]

---

### 7.2 Detail Section
Displayed per [grouping] including:
- [Column Group 1 — Document Info]
- [Column Group 2 — Entity Info]
- [Column Group 3 — Amount Breakdown]
- [Column Group 4 — Status Flags]

---

### 7.3 Footer Summary Logic

**Per [Group]**
- [Summary Item 1]
- [Summary Item 2]
- [Summary Item 3]

**Grand Total**
- [Total Item 1]
- [Total Item 2]
```
