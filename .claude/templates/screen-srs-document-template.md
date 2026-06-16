---
title: "Screen SRS Document"
document_type: "Screen SRS Template"
version: "2.0"
language: "th"
---
# Screen SRS Document: [ชื่อระบบ / ชื่อโมดูล]

## 1. เอกสารอ้างอิงและขอบเขต

- เอกสารนี้ใช้สำหรับลงรายละเอียด `Screen Function` ในมุมมองการใช้งานหน้าจอและพฤติกรรมเชิงธุรกิจ
- 1 เอกสารสามารถมีหลาย functions ได้ โดยให้แยก section ราย function ออกจากกัน
- หากรายละเอียดส่วนใดยังไม่พร้อม ให้คง placeholder ไว้หรือระบุ `ข้อมูลไม่เพียงพอ`

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
| --- | --- | --- |
| 1 | [BRD file path] | ต้นทาง requirement เชิงธุรกิจ |
| 2 | [SRS file path] | baseline ของ Screen Function Requirement |
| 3 | [Mockup / Figma / image path] | แหล่งอ้างอิงด้าน UI |
| 4 | [QA / policy / note] | เอกสารช่วยยืนยันรายละเอียดเพิ่มเติม |

### 1.2 Function Index

| Function ID | Function Name | Screen Name | Actor / User Role | Related Requirement IDs | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- | --- | --- |
| SF-001 | [ชื่อ function] | [ชื่อหน้าจอ] | [Actor] | [SFR-001, SCR-001] | [SRS / BRD / mockup] | [ถ้ามี] |
| SF-002 | [ชื่อ function] | [ชื่อหน้าจอ] | [Actor] | [SFR-002, SCR-002] | [SRS / BRD / mockup] | [ถ้ามี] |

## 2. Detailed Function Specification

> หมายเหตุ:
> ให้คัดลอก section `2.x` นี้ซ้ำสำหรับทุก function ที่ต้องการลงรายละเอียด

### 2.1 [Function ID] [Function Name]

#### 2.1.1 Function Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | [SF-001] |
| Function Name | [ชื่อ function] |
| Description | [อธิบายการทำงานของ function] |
| Business Purpose | [วัตถุประสงค์เชิงธุรกิจ] |
| Actor / User Role | [Actor / role ที่เกี่ยวข้อง] |
| Related Requirement IDs | [SFR-xxx, SCR-xxx, VR-xxx] |
| Source Reference | [SRS / BRD / QA / mockup] |

#### 2.1.2 Screen Overview

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | [ชื่อหน้าจอ] |
| Screen Description | [คำอธิบายหน้าจอ] |
| Navigation Inbound | [เข้าหน้านี้จากหน้าไหน / action ใด] |
| Navigation Outbound | [ออกไปหน้าไหนได้บ้าง] |
| Preconditions | [เงื่อนไขก่อนใช้งาน] |
| Postconditions | [ผลลัพธ์หลังใช้งาน] |

#### 2.1.3 Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | [Figma link / image / file path / attachment] |
| Mockup Version | [version / date] |
| Layout Description | [อธิบาย layout หรือการจัดวางข้อมูลของหน้าจอโดยรวม] |
| Additional Notes | [ข้อสังเกตเพิ่มเติมถ้ามี] |

```text
[วาง placeholder สำหรับ mockup screen หรือระบุ reference ที่ใช้]
```

#### 2.1.4 Tabs

> หากไม่มี tab ให้ระบุ `ไม่มี`

| Tab ID | Tab Name | Description | Actor / User Role | Default Tab (Y/N) | หมายเหตุ |
| --- | --- | --- | --- | --- | --- |
| TAB-01 | [ชื่อ tab] | [คำอธิบาย] | [Actor / role] | [Y/N] | [ถ้ามี] |
| TAB-02 | [ชื่อ tab] | [คำอธิบาย] | [Actor / role] | [Y/N] | [ถ้ามี] |

#### 2.1.5 Fields Definition

| Field Name | Label (TH/EN) | รูปแบบข้อมูล | Required (Y/N) | Default Value | เงื่อนไขการกรอกข้อมูล | Description | Sample Data |
| --- | --- | --- | --- | --- | --- | --- | --- |
| [field_name] | [TH / EN] | [ข้อความ / ตัวเลข / วันที่ / ตัวเลือก] | [Y/N] | [ค่าเริ่มต้น] | [เช่น กรอกเมื่อ..., เลือกได้เฉพาะ..., อ่านอย่างเดียวเมื่อ...] | [คำอธิบาย field] | [ตัวอย่างข้อมูล] |
| [field_name] | [TH / EN] | [ข้อความ / ตัวเลข / วันที่ / ตัวเลือก] | [Y/N] | [ค่าเริ่มต้น] | [เช่น กรอกเมื่อ..., เลือกได้เฉพาะ..., อ่านอย่างเดียวเมื่อ...] | [คำอธิบาย field] | [ตัวอย่างข้อมูล] |

#### 2.1.6 Commands / Actions

| Name | Description | Trigger Condition | System Response |
| --- | --- | --- | --- |
| [Add / Edit / Delete / Submit / Approve] | [คำอธิบาย command] | [เงื่อนไขที่สั่งงานได้] | [ระบบตอบสนองอย่างไร] |
| [Add / Edit / Delete / Submit / Approve] | [คำอธิบาย command] | [เงื่อนไขที่สั่งงานได้] | [ระบบตอบสนองอย่างไร] |

#### 2.1.7 Screen Behavior

| สถานการณ์ / Event | Trigger | Condition | Screen Behavior | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [เมื่อเปิดหน้าจอ / onLoad] | [เปิดหน้าจอ] | [ถ้ามี] | [ระบบแสดงข้อมูลอะไร หรือเตรียมข้อมูลอะไรให้ผู้ใช้] | [ถ้ามี] |
| [เมื่อกดปุ่ม / onClick] | [กดปุ่ม / link / row] | [ถ้ามี] | [ระบบตอบสนองอย่างไร] | [ถ้ามี] |
| [เมื่อแก้ไขข้อมูล / onChange] | [เปลี่ยนค่า field] | [ถ้ามี] | [ระบบเปลี่ยนข้อมูล คำนวณ หรือแสดงผลอย่างไร] | [ถ้ามี] |

#### 2.1.8 Message List

##### Error Message

| Message ID | Trigger Condition | Message Text (TH) | Message Text (EN) | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [ERR-001] | [validation fail / system error] | [ข้อความภาษาไทย] | [ข้อความภาษาอังกฤษ] | [ถ้ามี] |

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

#### 2.1.9 Business Rules

| Rule ID | Business Rule | System Impact | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [BR-001] | [เงื่อนไขทางธุรกิจที่เกี่ยวข้อง] | [ส่งผลต่อ field / command / event / message อย่างไร] | [BRD / SRS / QA] | [ถ้ามี] |
| [BR-002] | [เงื่อนไขทางธุรกิจที่เกี่ยวข้อง] | [ส่งผลต่อ field / command / event / message อย่างไร] | [BRD / SRS / QA] | [ถ้ามี] |

#### 2.1.10 Error Handling

| Error Type | Trigger Condition | System Behavior | User Message | Recovery / Next Action |
| --- | --- | --- | --- | --- |
| [Validation / System / Integration] | [เกิดกรณีใด] | [ระบบจัดการอย่างไร] | [ข้อความที่ผู้ใช้เห็น] | [ผู้ใช้งานควรทำอย่างไรต่อ] |
| [Validation / System / Integration] | [เกิดกรณีใด] | [ระบบจัดการอย่างไร] | [ข้อความที่ผู้ใช้เห็น] | [ผู้ใช้งานควรทำอย่างไรต่อ] |

#### 2.1.11 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
| --- | --- | --- | --- |
| Assumption | [ข้อสมมติ] | [ผลกระทบต่อ design / dev / test] | [ถ้ามี] |
| Constraint | [ข้อจำกัด] | [ผลกระทบต่อ design / dev / test] | [ถ้ามี] |

### 2.2 [Function ID] [Function Name]

> ให้คัดลอกโครงจาก `2.1` และปรับเลข section ต่อไปตามจำนวน functions จริง

## 3. Cross-Function Traceability

| Function ID | Function Name | Related Requirement IDs | Mockup Reference | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- | --- |
| SF-001 | [ชื่อ function] | [SFR-001, SCR-001, VR-001] | [mockup ref] | [SRS / BRD / QA] | |
| SF-002 | [ชื่อ function] | [SFR-002, SCR-002] | [mockup ref] | [SRS / BRD / QA] | |

## 4. Source Reference

- `[BRD file path]`
- `[SRS file path]`
- `[Mockup / Figma / image path]`
- `[QA / policy / note]`
