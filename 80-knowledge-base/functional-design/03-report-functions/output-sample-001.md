# Report Functions — Output Sample

# RPT-001 — Leave Summary Report

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | RPT-001 |
| Report Name | Leave Summary Report |
| Category | Report |
| Report Type | Summary |
| Description | สรุปข้อมูลการลาตามหน่วยงาน ประเภทการลา และช่วงเวลา |
| Actor / User Role | HR |
| Frequency | เรียกดูเมื่อจำเป็น |
| Related Requirement IDs | RFR-002, SFR-009 |

## 2. Business Purpose

ให้ HR เห็นภาพรวมการลาจากระบบกลางแทนการรวบรวมจากหลายช่องทาง

## 3. Report Parameters (Filters)

| Parameter | Label (TH/EN) | Required | Default | Description |
| --- | --- | --- | --- | --- |
| leave_type | ประเภทการลา / Leave Type | N | All Types | กรองตามประเภท |
| date_range | ช่วงเวลา / Date Range | N | เดือนปัจจุบัน | กรองตามช่วงเวลา |
| department | หน่วยงาน / Department | N | All Departments | กรองตามแผนก |

## 4. Report Layout

| ส่วน | รายละเอียด |
| --- | --- |
| Header | ชื่อรายงาน, filter criteria |
| Summary | Total Requests, Total Days, Approved, Pending, Rejected |
| Detail | ตารางสรุปตามหน่วยงาน แยกตามประเภทการลา |
| Footer | Totals, Last Refresh |

### Columns Definition

| Column | Label (TH/EN) | Data Type | Description |
| --- | --- | --- | --- |
| department | หน่วยงาน / Department | ข้อความ | มิติหลักในการสรุป |
| annual_leave | ลาพักร้อน / Annual Leave | ตัวเลขทศนิยม | จำนวนวันลาพักร้อน |
| sick_leave | ลาป่วย / Sick Leave | ตัวเลขทศนิยม | จำนวนวันลาป่วย |
| personal_leave | ลากิจ / Personal Leave | ตัวเลขทศนิยม | จำนวนวันลากิจ |
| request_count | จำนวนคำขอ / Requests | จำนวนเต็ม | จำนวนคำขอรวม |

## 5. Mockup / UI Layout

```text
+-----------------------------------------------------------------+
| Leave Summary Report                       [Execute] [Export]   |
+-----------------------------------------------------------------+
| Leave Type: [All Types] Date: [01-30 Apr] Dept: [All Depts]    |
+-----------------------------------------------------------------+
| Summary: Requests=58  Days=91.5  Approved=41  Pending=12       |
+-----------------------------------------------------------------+
| Department  | Annual | Sick  | Personal | Other | Requests      |
|-------------|--------|-------|----------|-------|---------------|
| Finance     | 14.0   | 6.0   | 3.0      | 1.0   | 12            |
| HR          | 10.0   | 4.0   | 2.0      | 0.0   | 9             |
| Sales       | 18.5   | 7.0   | 5.0      | 2.0   | 16            |
+-----------------------------------------------------------------+
| Totals: 58 requests  91.5 days  Last Refresh: 16 Apr 26 17:55  |
+-----------------------------------------------------------------+
```

## 6. Commands / Actions

| Command | Description | Trigger | Response |
| --- | --- | --- | --- |
| Execute | แสดงรายงาน | กรอก filter แล้วกด | แสดงผลสรุปตามเงื่อนไข |
| Export | ส่งออก Excel/PDF | กดปุ่ม Export | ดาวน์โหลดไฟล์ |

## 7. Business Rules

| Rule ID | Business Rule | Impact | Source |
| --- | --- | --- | --- |
| BR-RPT001-001 | เฉพาะ HR เท่านั้นที่ดูรายงานนี้ได้ | จำกัด access ตาม role | TR-006 |
| BR-RPT001-002 | ข้อมูลต้องมาจากระบบกลาง | ผลสรุปต้องตรงกับข้อมูลจริง | RFR-002 |

## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | 2026-04-16 | BA Team | Created | สร้างเอกสารครั้งแรก |
