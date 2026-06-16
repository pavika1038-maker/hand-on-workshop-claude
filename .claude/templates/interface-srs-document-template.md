---
title: "Interface SRS Document"
document_type: "Interface SRS Template"
version: "1.0"
language: "th"
---
# Interface SRS Document: [ชื่อระบบ / ชื่อโมดูล]

## 1. เอกสารอ้างอิงและขอบเขต

- เอกสารนี้ใช้สำหรับลงรายละเอียด `Interface / Integration` ในมุมมองการเชื่อมต่อข้อมูลเชิงธุรกิจระหว่างระบบ
- 1 เอกสารสามารถมีหลาย interfaces ได้ โดยให้แยก section ราย interface ออกจากกัน
- หากรายละเอียดส่วนใดยังไม่พร้อม ให้คง placeholder ไว้หรือระบุ `ข้อมูลไม่เพียงพอ`

### 1.1 เอกสารอ้างอิง

| ลำดับ | เอกสารอ้างอิง | บทบาทของเอกสาร |
| --- | --- | --- |
| 1 | [BRD file path] | ต้นทาง requirement เชิงธุรกิจ |
| 2 | [SRS file path] | baseline ของ System Integration Requirement |
| 3 | [sample file / mapping note / image path] | แหล่งอ้างอิงด้านข้อมูลหรือ flow |
| 4 | [QA / policy / note] | เอกสารช่วยยืนยันรายละเอียดเพิ่มเติม |

### 1.2 Interface Index

| Interface ID | Interface Name | Source System | Destination System | Integration Type | Frequency | Related Requirement IDs | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| IF-001 | [ชื่อ interface] | [ระบบต้นทาง] | [ระบบปลายทาง] | [ส่งข้อมูล / รับข้อมูล / แลกเปลี่ยนข้อมูล] | [real-time / รายวัน / ตามรอบเวลา] | [SIR-001, TR-001] | [SRS / BRD / note] | [ถ้ามี] |
| IF-002 | [ชื่อ interface] | [ระบบต้นทาง] | [ระบบปลายทาง] | [ส่งข้อมูล / รับข้อมูล / แลกเปลี่ยนข้อมูล] | [real-time / รายวัน / ตามรอบเวลา] | [SIR-002, TR-002] | [SRS / BRD / note] | [ถ้ามี] |

## 2. Detailed Interface Specification

> หมายเหตุ:
> ให้คัดลอก section `2.x` นี้ซ้ำสำหรับทุก interface ที่ต้องการลงรายละเอียด

### 2.1 [Interface ID] [Interface Name]

#### 2.1.1 Interface Overview

| รายการ | รายละเอียด |
| --- | --- |
| Interface ID | [IF-001] |
| Interface Name | [ชื่อ interface] |
| Description | [คำอธิบายการเชื่อมต่อ] |
| Business Purpose | [วัตถุประสงค์เชิงธุรกิจ] |
| Source System | [ระบบต้นทาง] |
| Destination System | [ระบบปลายทาง] |
| Related Requirement IDs | [SIR-xxx, TR-xxx] |
| Source Reference | [SRS / BRD / QA / note] |

#### 2.1.2 Interface Description

| รายการ | รายละเอียด |
| --- | --- |
| Integration Type | [ส่งข้อมูล / รับข้อมูล / แลกเปลี่ยนข้อมูล] |
| Operation Type | [ทำงานอัตโนมัติ / ผู้ใช้สั่งงาน] |
| Frequency | [real-time / รายวัน / ตามรอบเวลา] |
| Additional Description | [คำอธิบายเพิ่มเติม] |

#### 2.1.3 Data Scope (ขอบเขตข้อมูล)

| รายการ | รายละเอียด |
| --- | --- |
| Data Scope Summary | [ข้อมูลอะไรบ้างที่ถูกส่งหรือรับ] |
| Data Condition | [เงื่อนไขของข้อมูล เช่น ส่งเฉพาะข้อมูลใหม่ / เฉพาะสถานะที่กำหนด] |
| Exclusion | [กรณีที่ไม่ต้องส่งหรือไม่ต้องรับข้อมูล] |

#### 2.1.4 Data Structure (ข้อมูลที่แลกเปลี่ยน)

| Data Name | Description | Sample Data | Required (Y/N) |
| --- | --- | --- | --- |
| [data_name] | [คำอธิบายข้อมูล] | [ตัวอย่างข้อมูล] | [Y/N] |
| [data_name] | [คำอธิบายข้อมูล] | [ตัวอย่างข้อมูล] | [Y/N] |

#### 2.1.5 Trigger / Timing

| Trigger Scenario | Description | Timing / Frequency | หมายเหตุ |
| --- | --- | --- | --- |
| [เมื่อผู้ใช้ทำรายการ] | [อธิบายเหตุการณ์ที่ทำให้เริ่มเชื่อมต่อ] | [real-time / ทันที] | [ถ้ามี] |
| [ตามเวลาที่กำหนด] | [อธิบายเหตุการณ์ที่ทำให้เริ่มเชื่อมต่อ] | [รายวัน / ตามรอบเวลา] | [ถ้ามี] |
| [เมื่อมีการเปลี่ยนแปลงข้อมูล] | [อธิบายเหตุการณ์ที่ทำให้เริ่มเชื่อมต่อ] | [ตาม event] | [ถ้ามี] |

#### 2.1.6 Expected Result

| สถานการณ์ | Expected Result | หมายเหตุ |
| --- | --- | --- |
| [เมื่อ interface ทำงานสำเร็จ] | [ข้อมูลถูกส่งไปยังระบบปลายทาง / ระบบปลายทางใช้งานข้อมูลได้] | [ถ้ามี] |
| [เมื่อมีผลลัพธ์เพิ่มเติม] | [อธิบายผลลัพธ์ที่คาดหวัง] | [ถ้ามี] |

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

| Rule ID | Business Rule | Impact to Interface | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- |
| [BR-001] | [เงื่อนไขการส่งข้อมูล] | [ผลต่อการเชื่อมต่อ] | [BRD / SRS / QA] | [ถ้ามี] |
| [BR-002] | [ข้อจำกัดของข้อมูล / กรณีที่ไม่ต้องส่งข้อมูล] | [ผลต่อการเชื่อมต่อ] | [BRD / SRS / QA] | [ถ้ามี] |

#### 2.1.9 Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ | หมายเหตุ |
| --- | --- | --- | --- |
| Assumption | [ข้อสมมติ] | [ผลกระทบต่อการใช้งาน interface] | [ถ้ามี] |
| Constraint | [ข้อจำกัด] | [ผลกระทบต่อการใช้งาน interface] | [ถ้ามี] |

### 2.2 [Interface ID] [Interface Name]

> ให้คัดลอกโครงจาก `2.1` และปรับเลข section ต่อไปตามจำนวน interfaces จริง

## 3. Cross-Interface Traceability

| Interface ID | Interface Name | Related Requirement IDs | Source Reference | หมายเหตุ |
| --- | --- | --- | --- | --- |
| IF-001 | [ชื่อ interface] | [SIR-001, TR-001] | [SRS / BRD / QA / note] | |
| IF-002 | [ชื่อ interface] | [SIR-002, TR-002] | [SRS / BRD / QA / note] | |

## 4. Source Reference

- `[BRD file path]`
- `[SRS file path]`
- `[sample file / mapping note / image path]`
- `[QA / policy / note]`
