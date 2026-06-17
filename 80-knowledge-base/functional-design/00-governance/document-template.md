# Functional Design — Document Template

## วิธีใช้

1. Copy template ด้านล่างไปสร้างไฟล์ใหม่
2. เปลี่ยน `[Function ID]` และ `[Function Name]` ให้ตรงกับ function ที่ออกแบบ
3. กรอกข้อมูลทุก section เท่าที่มีข้อมูล
4. Section ที่ไม่เกี่ยวข้องให้ระบุ `ไม่เกี่ยวข้อง` ห้ามลบ section ออก
5. อัปเดต Function Index หลังสร้างเอกสารเสร็จ

---

## Template

```markdown
---
function_id: "[TYPE]-[NNN]"
function_name: "[Function Name]"
category: "[Common / Screen / Report / Interface / Batch]"
version: "1.0"
status: "Draft"
author: ""
last_updated: ""
---

# [Function ID] — [Function Name]

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | [TYPE]-[NNN] |
| Function Name | [ชื่อ function] |
| Category | [Common / Screen / Report / Interface / Batch] |
| Description | [อธิบายสิ่งที่ function ทำ] |
| Actor / User Role | [ผู้ใช้งาน เช่น Employee, Manager, HR, System] |
| Related Requirement IDs | [SFR-xxx, RFR-xxx, SIR-xxx] |
| Source Reference | [SRS document / BRD section] |

## 2. Business Purpose

[อธิบายว่าทำไม function นี้ถึงมีอยู่ ตอบโจทย์ธุรกิจอะไร]

## 3. Process Flow

[อธิบาย step-by-step flow ของ function]

### 3.1 Flow Diagram (ถ้ามี)

```text
[วาง ASCII flow diagram หรือ reference ไปยัง Mermaid/draw.io]
```

### 3.2 Step-by-Step

| Step | Actor | Action | System Response | หมายเหตุ |
| --- | --- | --- | --- | --- |
| 1 | [ผู้ใช้/ระบบ] | [ทำอะไร] | [ระบบตอบสนองอย่างไร] | |
| 2 | | | | |

## 4. Inputs / Outputs

### 4.1 Inputs

| Input Name | Data Type | Required | Description | Source |
| --- | --- | --- | --- | --- |
| [field_name] | [type] | [Y/N] | [คำอธิบาย] | [มาจากไหน] |

### 4.2 Outputs

| Output Name | Data Type | Description | Destination |
| --- | --- | --- | --- |
| [field_name] | [type] | [คำอธิบาย] | [ส่งไปไหน] |

## 5. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-[ID]-001 | [เงื่อนไขทางธุรกิจ] | [ผลกระทบต่อ function] | [SRS / BRD] |

## 6. UI / Screen (ถ้าเกี่ยวข้อง)

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | [ชื่อหน้าจอ] |
| Navigation Inbound | [เข้ามาจากไหน] |
| Navigation Outbound | [ออกไปไหน] |
| Mockup Reference | [path ไปยัง mockup file] |

### Fields Definition

| Field Name | Label (TH/EN) | Data Type | Required | Default | Description |
| --- | --- | --- | --- | --- | --- |
| [field] | [TH / EN] | [type] | [Y/N] | [default] | [คำอธิบาย] |

### Commands / Actions

| Command | Description | Trigger Condition | System Response |
| --- | --- | --- | --- |
| [ปุ่ม/action] | [คำอธิบาย] | [เงื่อนไข] | [ระบบทำอะไร] |

## 7. Integration (ถ้าเกี่ยวข้อง)

| รายการ | รายละเอียด |
| --- | --- |
| Integration Type | [API / File / Message Queue] |
| Source System | [ระบบต้นทาง] |
| Destination System | [ระบบปลายทาง] |
| Protocol | [REST / SFTP / Service Bus] |
| Trigger | [เมื่อใดเรียกใช้] |

## 8. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| [กรณี error] | [เกิดเมื่อใด] | [ระบบทำอะไร] | [ข้อความที่แสดง] | [ผู้ใช้ทำอะไรต่อ] |

## 9. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Assumption | [ข้อสมมติ] | [ผลกระทบ] |
| Constraint | [ข้อจำกัด] | [ผลกระทบ] |
| Open Issue | [ประเด็นที่ต้องยืนยัน] | [ผลกระทบ] |
```
