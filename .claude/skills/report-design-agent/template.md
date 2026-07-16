<!--
Template สำหรับ Report Design Document (1 RP = 1 ไฟล์)
- Copy โครงสร้างนี้ทั้งหมด แล้วแทนที่ [placeholder] ด้วยข้อมูลจริง
- ห้ามลบ / สลับ / เปลี่ยนชื่อ section — section ที่ไม่เกี่ยวข้องให้ใส่ "— ไม่มี" พร้อมเหตุผลสั้น ๆ
- คอลัมน์ในตารางห้ามตัดออก ถ้าไม่มีข้อมูลให้ใส่ "—"
- อ้างอิงจาก 80-knowledge-base/functional-design/03-report-functions/output-template.md รูปแบบ A
  ปรับให้ตรงกับโปรเจกต์นี้ (RP prefix, DB Mapping, Query Logic, Message List, Exception Handling)
-->
---
function_id: "RP-[NNN]"
function_name: "[Report Name]"
category: "Report"
report_type: "[Transaction Detail / Monitoring / Summary / Dashboard / Export]"
version: "1.0"
status: "Draft"
author: "report-design-agent (Claude)"
last_updated: "[YYYY-MM-DD]"
---

# RP-[NNN] — [Report Name]

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | RP-[NNN] |
| Report Name | [ชื่อรายงาน] |
| Category | Report |
| Report Type | [ประเภทรายงาน] |
| Description | [อธิบายรายงาน 1-2 ประโยค] |
| Actor / User Role | [ผู้ใช้งาน] |
| Frequency | [on-demand / รายวัน / รายเดือน] |
| Related Screen | [SCR-xxx หน้าจอที่เรียก report นี้ หรือ —] |
| Related Requirement IDs | [RFR-xxx, SFR-xxx] |
| Source Reference | [Report SRS §x.x (RP-NNN), SRS §x.x, BRD BR-xxx] |
| Version | 1.0 |
| Created By | report-design-agent ([YYYY-MM-DD]) |
| Updated By | — |

## 2. Business Purpose

[เหตุผลทางธุรกิจที่รายงานนี้มีอยู่ — ใครใช้ ตัดสินใจเรื่องอะไร — ปิดท้ายด้วย (Source: ...)]

## 3. Report Parameters (Filters)

| No | Parameter | Label (TH/EN) | Type | Required | Default | Validation | DB Mapping | Description |
| :---: | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | [param] | [TH / EN] | [Dropdown/Date Range/...] | [Y/N] | [ค่า หรือ —] | [กฎ + source] | [`Table.Column` หรือ —] | [คำอธิบาย] |

## 4. Report Layout

| ส่วน | รายละเอียด |
| --- | --- |
| Header | [ชื่อรายงาน, filter criteria, print date/by] |
| Summary | [ยอดรวม, KPI หรือ —] |
| Detail | [ตารางข้อมูลหลัก] |
| Footer | [Grand total, Last Refresh, Page No.] |

### Columns Definition

| No | Column | Label (TH/EN) | Data Type | Format | DB Mapping | Description |
| :---: | --- | --- | --- | --- | --- | --- |
| 1 | [column] | [TH / EN] | [type] | [format เช่น dd/mm/yyyy, #,##0.0] | [`Table.Column` (type)] | [คำอธิบาย + ที่มา ถ้าเป็นค่าคำนวณระบุสูตร] |

## 5. Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | [path จาก report-mockup-index.md — ถ้าไม่มีให้ระบุว่า ASCII ด้านล่างเป็น Assumption] |
| Layout Description | [อธิบาย layout จาก SRS] |

### Filter Screen

```text
[ASCII mockup ของหน้าจอกำหนดเงื่อนไข]
```

### Report Output

```text
[ASCII mockup ของ report output — header, columns, summary, footer]
```

## 6. Commands / Actions

| No | Command | Type | Default State | Trigger Condition | System Response |
| :---: | --- | --- | --- | --- | --- |
| 1 | Preview / Execute | Button | [Enable/Disable] | [เงื่อนไข] | [รวมชื่อ service method ที่เรียก เช่น `IXxxService.XxxAsync()`] |
| 2 | Export Excel | Button | [Enable/Disable] | [เงื่อนไข] | [พฤติกรรม] |

## 7. Sorting Logic

| ระดับ | Field | Direction | Description |
| :---: | --- | --- | --- |
| 1 | [field] | [ASC/DESC] | [กลุ่มหลัก / เหตุผล + source ถ้า SRS ระบุ] |

## 8. Summary Logic

### Per Group Summary

<!-- ถ้าไม่มี group summary: "— ไม่มี" -->

| Group By | Summary Field | สูตร | Description |
| --- | --- | --- | --- |
| [field] | [field] | [SUM/COUNT/AVG] | [คำอธิบาย] |

### Grand Total

| Summary Field | สูตร | Description |
| --- | --- | --- |
| [field] | [SUM/COUNT] | [คำอธิบาย] |

## 9. Query / Data Source Logic

<!-- pseudo query ระดับ design — ใช้ชื่อตาราง/คอลัมน์จริงจาก Data Architecture -->

```text
Table: [Main Table]
  JOIN [Related Table] (Alias) ON [Join Condition]

WHERE [Base Conditions — รวม filter จาก section 3]
  [AND IsDeleted = 0 ฯลฯ]

GROUP BY [ถ้ามี]
ORDER BY [ตาม section 7]
```

- Service method: [`IXxxService.XxxAsync()` จาก Method Signature §x.x หรือ Assumption ถ้าไม่มี]
- Pagination / row limit: [ระบุ หรือ —]

## 10. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-RP[NNN]-001 | [กฎ] | [ผลต่อ query/column/summary] | [RFR-xxx, BRD BR-xxx] |

## 11. Message List

### Error Messages

<!-- Message ที่ SRS กำหนดแล้ว: ใช้ ID ตาม SRS (เช่น ERR-RPT-001) — Message ใหม่: ERR-RP[NNN]-nnn -->

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| [ID] | [เงื่อนไข] | [TH] | [EN] |

### Info / Success Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| [ID] | [เงื่อนไข] | [TH] | [EN] |

## 12. Database Tables Reference

| Table Name | Alias | Description |
| --- | --- | --- |
| [Table] | — | [ใช้ทำอะไรใน report นี้: SELECT + เงื่อนไข] |

## 13. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| Validation error | [filter ไม่ถูกต้อง] | [พฤติกรรม] | [message id] | [วิธีแก้] |
| No data | [ไม่พบข้อมูลตาม filter] | [พฤติกรรม] | [message id] | [วิธีแก้] |
| Export error | [export ล้มเหลว] | [พฤติกรรม] | [message id] | [วิธีแก้] |
| System error | [ระบบล่ม / timeout] | [พฤติกรรม] | [message id] | [วิธีแก้] |

## 14. Notes / Assumptions

<!-- ทุกสิ่งที่เติมเองนอกเหนือจาก SRS ต้องอยู่ที่นี่ — แยกประเภท: Open Issue (จาก SRS) / Assumption (จาก SRS) / Assumption (เอกสารนี้) / Note -->

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| [ประเภท] | [รายละเอียด] | [ผลกระทบ + สิ่งที่ต้อง confirm] |

## Change Log

| Version | Date | Author | Change Type | Description | Remark |
| --- | --- | --- | --- | --- | --- |
| 1.0 | [YYYY-MM-DD] | report-design-agent (Claude) | Created | สร้างเอกสารครั้งแรกจาก [Report SRS vX.X (§x.x RP-NNN), input อื่นที่ใช้] | Generated ตาม template report-design-agent |

### สรุปการเปลี่ยนแปลงสำคัญ

| ช่วง Version | การเปลี่ยนแปลง | ผลกระทบ |
| --- | --- | --- |
| 1.0 | Baseline แรก | — |
