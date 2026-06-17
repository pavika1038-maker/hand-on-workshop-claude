# Batch Functions — องค์ความรู้หลัก

## 1. คำจำกัดความ

Batch Functions คือ function ที่ทำงานตามรอบเวลา (scheduled) หรือ trigger โดยระบบ ไม่มี UI ให้ผู้ใช้โต้ตอบโดยตรง

## 2. ประเภทของ Batch Function

| ประเภท | คำอธิบาย | ตัวอย่าง |
|--------|---------|---------|
| Data Sync | Sync ข้อมูลระหว่างระบบ | Nightly employee sync จาก HRIS |
| Calculation | คำนวณข้อมูลตามรอบ | คำนวณ leave balance ทุกสิ้นวัน |
| Data Export | ส่งออกข้อมูลตามรอบ | ส่ง leave balance กลับ HRIS ทุกคืน |
| Cleanup | ลบหรือ archive ข้อมูลเก่า | Archive คำขอที่เกิน 2 ปี |
| Notification / API Caller | ส่ง notification หรือเรียก API ตามรอบ | ส่ง SMS แจ้งลูกค้าที่ consent อัปเดต |

## 3. โครงสร้างเอกสาร Batch Function

เอกสาร Batch Function มี 2 รูปแบบ:

### 3.1 รูปแบบย่อ (Simple — file sync, calculation)

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| 1. Overview | ข้อมูลทั่วไป (ID, Name, Type, Schedule, Duration) | Y |
| 2. Business Purpose | เหตุผลทางธุรกิจ | Y |
| 3. Process Flow | Step-by-step table (Action, Description, Error Handling) | Y |
| 4. Input / Output | Source/Destination data | Y |
| 5. Business Rules | กฎเกณฑ์ทางธุรกิจ | Y |
| 6. Error Handling & Retry | Error cases + retry + alert | Y |
| 7. Monitoring | Metrics + thresholds | Y |
| 8. Notes / Assumptions | สมมติฐาน | Y |
| Change Log | ประวัติการเปลี่ยนแปลง | Y |

### 3.2 รูปแบบเต็ม (Complex — API caller, config-driven, multi-table)

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| Document Header | Doc No, Project/System/Team metadata | Y |
| 1. Overview | ข้อมูลทั่วไป + System Overview Diagram (mermaid) | Y |
| 2. Business Purpose | เหตุผลทางธุรกิจ | Y |
| 3. Process Outline | Flowchart (mermaid TD) + Process Steps Summary | Y |
| 4. Process Description | Step-by-step detail (Initial → Get Data → Loop → End Log) | Y |
| 5. Input / Output | Source/Destination data | Y |
| 6. Error Handling & Retry | Error cases + retry + alert | Y |
| 7. Sample / Reference Data | ตัวอย่างข้อมูล (ถ้ามี) | เมื่อมี |
| 8. Monitoring | Metrics + thresholds | Y |
| 9. Notes / Assumptions | สมมติฐาน | Y |
| Change Log | ประวัติการเปลี่ยนแปลง | Y |

## 4. หลักการออกแบบ

| หลักการ | คำอธิบาย |
|---------|---------|
| Idempotent | Run ซ้ำได้โดยไม่เกิด side effect |
| Resumable | หาก fail กลางทาง สามารถ resume จากจุดที่ค้างได้ |
| Observable | ต้อง log start time, end time, record count, success/failure |
| Alertable | ต้องแจ้ง admin เมื่อ fail |
| Configurable | schedule, batch size, retry count, expiry period ต้อง config ได้ ไม่ hardcode |

## 5. Batch Execution Pattern

### 5.1 Simple Pattern (ETL)

```text
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ Schedule │───▶│ Extract  │───▶│ Transform│───▶│  Load    │
│ Trigger  │    │ (Read)   │    │ (Process)│    │ (Write)  │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
                     │               │               │
                     └───────────────┴───────────────┘
                              Logging & Monitoring
```

### 5.2 API Caller Pattern (Loop + Call API)

```text
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│ Write    │───▶│ Get Data │───▶│ Loop     │───▶│ Write    │
│ Start Log│    │ (Query)  │    │ Records  │    │ End Log  │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
                     │               │
                     │          ┌────┴────┐
                     │          │ Per Record:
                     │          │ 1. Insert log
                     │          │ 2. Call API
                     │          │ 3. Handle response
                     │          │ 4. Next record
                     │          └─────────┘
                     │
                Config-driven conditions
                (Config Master KEY = 'xx')
```

## 6. Process Log Pattern

ทุก batch ต้องเขียน Process Log ที่จุดเริ่มต้นและสิ้นสุด:

### Start Log

| Field | Value |
|---|---|
| Program ID | BAT[NNN] |
| Date/Time | System datetime |
| Title | Start process |
| Status | Successed |
| Src_01, Src_02, Ref_ID | blank |

### End Log

| Case | Title | Status | Message |
|---|---|---|---|
| Success | End process | Successed | Config Master KEY='09' → "INF_CODE - message" |
| Failed | End process | Failed | Error message of each case |

### Per-Record Log (API Caller)

| Case | Action |
|---|---|
| API success | Log: `[message]` |
| API failed | Log: `[Error_Flag] + " - " + [message]` |
| Exception | Log: "System exception error" + system error message |

## 7. Config-Driven Query Pattern

Batch ที่ซับซ้อนใช้ Config Master เป็นตัวกำหนดเงื่อนไข:

| Config KEY | Purpose | ตัวอย่าง |
|---|---|---|
| Filter conditions | กำหนด field name + condition value | KEY='62', VALUE2=field, VALUE3=condition |
| Error messages | ดึง error/info message | KEY='09', VALUE2='ERR0077' → VALUE3=message |
| Expiry control | กำหนดระยะเวลาหมดอายุ | KEY='64', configured by hour |
| Success message | ดึง success message | KEY='09', VALUE2='INF0004' → "Process successfully" |

### Query Logic Pattern

| Condition | Description |
|---|---|
| Record exists in Main Table but NOT in Detail Table | ข้อมูลที่ยังไม่ถูก process |
| Record exists in Log Table with no response AND expiry passed | ข้อมูลที่ส่งแล้วแต่ยังไม่ตอบกลับภายในเวลา |
| Additional conditions from Config Master | เงื่อนไขเพิ่มเติมจาก config (dynamic) |

## 8. API Caller Pattern

Batch ที่เรียก API ต่อ record:

### Sending Parameters Pattern

| Parameter Type | คำอธิบาย | ตัวอย่าง |
|---|---|---|
| Fix | ค่าคงที่ | appKey, F_Request_Type, F_Process_Type |
| From Source | ค่าจาก query result | PHONE_NO, EMAIL_ADDR, CUSTOMER_ID |
| From Config | ค่าจาก Config Master | REGISTER_BY (KEY='62') |
| Formatted | ค่าที่ต้อง format | Phone: 0857489334 → 66857489334 |
| Object Array | ข้อมูลซ้อน | F_Customer_Info: {ID, Revision, Reason, Remark} |
| Blank | ค่าว่าง | sessKey, F_Consent_HD_ID |

### API Return Handling Pattern

| Field | Description |
|---|---|
| success | true / false |
| message | Error code + message text |
| Error_Flag | blank = no error, 1 = validation error, 2 = system error |

### End-of-Record Logic

| Condition | Action |
|---|---|
| Records remain | Loop back to next record |
| No records remain | Write end-process log → End |

## 9. System Overview Diagram Pattern

Batch ที่ซับซ้อนควรมี System Overview Diagram (mermaid LR) แสดง:

| Component | คำอธิบาย | Mermaid Shape |
|---|---|---|
| Batch Process | กลุ่ม process หลัก | `subgraph` |
| Trigger | ตัว trigger batch | Rectangle ใน subgraph |
| Source DB | ฐานข้อมูลต้นทาง | `[(" ")]` cylinder |
| API Process | API ที่เรียก | Rectangle |
| Log DB | ฐานข้อมูล log | `[(" ")]` cylinder |
| External System | ระบบภายนอก (SMS Gateway, etc.) | Rectangle |
| End User | ผู้รับ notification | Rectangle |
| Related Process | process ที่เกี่ยวข้อง (consent submit, etc.) | Rectangle + dotted lines |

### Styling Convention

- Batch process subgraph: default
- API process: `fill:#EAD7A4,stroke:#333`
- External system: default
- Dotted lines: `.->` สำหรับ indirect/reference relationships
- Notes: `-.->` สำหรับ sample data annotations

## 10. Monitoring Standard

ทุก batch job ต้อง log ข้อมูลต่อไปนี้:

| ข้อมูล | คำอธิบาย | ตัวอย่าง |
|--------|---------|---------|
| Job ID | รหัส batch job | BAT-001-20260416-020000 |
| Start Time | เวลาเริ่มทำงาน | 2026-04-16 02:00:00 |
| End Time | เวลาเสร็จ | 2026-04-16 02:15:30 |
| Status | ผลลัพธ์ | Success / Failed / Partial |
| Records Processed | จำนวน record ที่ประมวลผล | 2,847 |
| Records Failed | จำนวน record ที่ fail | 3 |
| Error Detail | รายละเอียด error (ถ้ามี) | "ERR0077 - No data found" |
| Duration | ระยะเวลาทำงาน | 15 นาที 30 วินาที |

## 11. Best Practice

- ทุก batch ต้อง idempotent — run ซ้ำได้โดยไม่เกิดข้อมูลซ้ำ
- ใช้ staging table สำหรับรับข้อมูลก่อน merge เข้า production table
- Batch size ต้อง config ได้ (เช่น process ทีละ 1,000 records)
- ต้องมี reconciliation — เปรียบเทียบ record count ระหว่าง source และ destination
- ต้องมี dead letter / error queue สำหรับ record ที่ process ไม่สำเร็จ
- Schedule ต้องไม่ชนกับ maintenance window หรือ backup window
- ใช้ Config Master สำหรับ dynamic conditions แทน hardcode
- Expiry/retry period ต้อง config ได้ (Config Master KEY pattern)
- Error/Success messages ดึงจาก Config Master (centralized)
- Process Log ต้องเขียนทั้ง start และ end (success/failed)
- Per-record API call ต้อง log ทุก response (success + failed)
- System Overview Diagram (mermaid LR) สำหรับ batch ที่ซับซ้อน
- Process Outline Diagram (mermaid TD) สำหรับ decision flow
