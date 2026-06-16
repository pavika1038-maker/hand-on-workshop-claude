---
title: "Report SRS Document"
document_type: "Report SRS Template"
version: "1.0"
language: "th"
---
# Report SRS Document: [ชื่อระบบ / ชื่อโมดูล]

## 1. เอกสารอ้างอิงและขอบเขต

- เอกสารนี้ใช้สำหรับลงรายละเอียด `Report Function` ในมุมมองการใช้งานรายงานและความต้องการเชิงธุรกิจ
- 1 เอกสารสามารถมีหลาย reports ได้ โดยให้แยก section ราย report ออกจากกัน
- หากรายละเอียดส่วนใดยังไม่พร้อม ให้คง placeholder ไว้หรือระบุ `ข้อมูลไม่เพียงพอ`

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
| --- | --- | --- |
| 1 | [BRD file path] | ต้นทาง requirement เชิงธุรกิจ |
| 2 | [SRS file path] | baseline ของ Report Function Requirement |
| 3 | [sample report / mockup / image path] | แหล่งอ้างอิงด้านรูปแบบรายงาน |
| 4 | [QA / policy / note] | เอกสารช่วยยืนยันรายละเอียดเพิ่มเติม |

### 1.2 Report Index

| Report ID | Report Name | Report Type | Actor / User Role | Frequency | Related Requirement IDs | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- | --- | --- | --- |
| RP-001 | [ชื่อรายงาน] | [รายละเอียด / สรุป / dashboard] | [Actor] | [เรียกดูเมื่อจำเป็น / รายวัน / รายเดือน] | [RFR-001] | [SRS / BRD / sample] | [ถ้ามี] |
| RP-002 | [ชื่อรายงาน] | [รายละเอียด / สรุป / dashboard] | [Actor] | [เรียกดูเมื่อจำเป็น / รายวัน / รายเดือน] | [RFR-002] | [SRS / BRD / sample] | [ถ้ามี] |

## 2. Detailed Report Specification

> หมายเหตุ:
> ให้คัดลอก section `2.x` นี้ซ้ำสำหรับทุก report ที่ต้องการลงรายละเอียด

### 2.1 [Report ID] [Report Name]

#### 2.1.1 Report Overview

| รายการ | รายละเอียด |
| --- | --- |
| Report ID | [RP-001] |
| Report Name | [ชื่อรายงาน] |
| Description | [คำอธิบายรายงาน] |
| Business Purpose | [วัตถุประสงค์เชิงธุรกิจ] |
| Actor / User Role | [Actor / role ที่เกี่ยวข้อง] |
| Frequency | [เรียกดูเมื่อจำเป็น / รายวัน / รายเดือน] |
| Related Requirement IDs | [RFR-xxx] |
| Source Reference | [SRS / BRD / QA / sample] |

#### 2.1.2 Report Description

| รายการ | รายละเอียด |
| --- | --- |
| Report Type | [รายละเอียด / สรุป / dashboard] |
| Output Format | [แสดงบนหน้าจอ / export เป็นไฟล์] |
| Sorting Method | [วิธีการจัดเรียงข้อมูล ถ้ามี] |
| Aggregation Method | [รวมยอด / นับจำนวน / ค่าเฉลี่ย / อื่น ๆ] |
| Additional Description | [คำอธิบายเพิ่มเติม] |

#### 2.1.3 Report Parameters (Filters)

| Parameter Name | Label (TH/EN) | Required (Y/N) | Default Value | Description | Sample Data |
| --- | --- | --- | --- | --- | --- |
| [parameter_name] | [TH / EN] | [Y/N] | [ค่าเริ่มต้น] | [คำอธิบาย parameter] | [ตัวอย่างข้อมูล] |
| [parameter_name] | [TH / EN] | [Y/N] | [ค่าเริ่มต้น] | [คำอธิบาย parameter] | [ตัวอย่างข้อมูล] |

#### 2.1.4 Report Layout / Structure

| ส่วนของรายงาน | รายละเอียด |
| --- | --- |
| Header | [อธิบายส่วนหัวรายงาน เช่น ชื่อรายงาน ช่วงวันที่ เงื่อนไขการค้นหา] |
| Detail | [อธิบายส่วนแสดงข้อมูลหลักของรายงาน] |
| Footer | [อธิบายส่วนสรุป เช่น รวมยอด จำนวนรายการ หรือหมายเหตุท้ายรายงาน] |

##### Displayed Columns / Data Items

| ลำดับ | รายการข้อมูลที่แสดง | Description | หมายเหตุ |
| --- | --- | --- | --- |
| 1 | [ชื่อรายการข้อมูล] | [คำอธิบาย] | [ถ้ามี] |
| 2 | [ชื่อรายการข้อมูล] | [คำอธิบาย] | [ถ้ามี] |

#### 2.1.5 Fields / Columns Definition

| Column Name | Label (TH/EN) | รูปแบบข้อมูล | Description |
| --- | --- | --- | --- |
| [column_name] | [TH / EN] | [วันที่ / ตัวเลข / จำนวนเงิน / ข้อความ] | [คำอธิบาย column] |
| [column_name] | [TH / EN] | [วันที่ / ตัวเลข / จำนวนเงิน / ข้อความ] | [คำอธิบาย column] |

#### 2.1.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
| --- | --- | --- | --- |
| [ดูรายงาน / ดาวน์โหลด / พิมพ์ / ส่งออกไฟล์] | [คำอธิบาย command] | [กดเมื่อใด / ใช้ได้เมื่อใด] | [ระบบทำอะไร] |
| [ดูรายงาน / ดาวน์โหลด / พิมพ์ / ส่งออกไฟล์] | [คำอธิบาย command] | [กดเมื่อใด / ใช้ได้เมื่อใด] | [ระบบทำอะไร] |

#### 2.1.7 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [ERR-001] | [error condition] | [ข้อความภาษาไทย] | [ข้อความภาษาอังกฤษ] | [ถ้ามี] |

##### Warning Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [WRN-001] | [warning condition] | [ข้อความภาษาไทย] | [ข้อความภาษาอังกฤษ] | [ถ้ามี] |

##### Info Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [INF-001] | [info condition] | [ข้อความภาษาไทย] | [ข้อความภาษาอังกฤษ] | [ถ้ามี] |

##### Success Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [SUC-001] | [success condition] | [ข้อความภาษาไทย] | [ข้อความภาษาอังกฤษ] | [ถ้ามี] |

#### 2.1.8 Business Rules

| Rule ID | Business Rule | Impact to Report | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [BR-001] | [ใครสามารถดูรายงานนี้ได้] | [ผลต่อการแสดงผลหรือการกรองข้อมูล] | [BRD / SRS / QA] | [ถ้ามี] |
| [BR-002] | [วิธีการคำนวณหรือเงื่อนไขการกรองข้อมูล] | [ผลต่อรายงาน] | [BRD / SRS / QA] | [ถ้ามี] |

#### 2.1.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
| --- | --- | --- | --- |
| Assumption | [ข้อสมมติ] | [ผลกระทบต่อการใช้งานรายงาน] | [ถ้ามี] |
| Constraint | [ข้อจำกัด] | [ผลกระทบต่อการใช้งานรายงาน] | [ถ้ามี] |

### 2.2 [Report ID] [Report Name]

> ให้คัดลอกโครงจาก `2.1` และปรับเลข section ต่อไปตามจำนวน reports จริง

## 3. Cross-Report Traceability

| Report ID | Report Name | Related Requirement IDs | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- |
| RP-001 | [ชื่อรายงาน] | [RFR-001] | [SRS / BRD / QA / sample] | |
| RP-002 | [ชื่อรายงาน] | [RFR-002] | [SRS / BRD / QA / sample] | |

## 4. Source Reference

- `[BRD file path]`
- `[SRS file path]`
- `[sample report / mockup / image path]`
- `[QA / policy / note]`
