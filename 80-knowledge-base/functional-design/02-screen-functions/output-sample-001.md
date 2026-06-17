# Screen Functions — Output Sample

> ตัวอย่างเอกสาร Screen Function ที่สร้างจาก template

---

# SCR-001 — Create Leave Request

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | SCR-001 |
| Function Name | Create Leave Request |
| Category | Screen |
| Screen Type | Create Form |
| Description | หน้าจอให้พนักงานกรอกและ submit คำขอลา พร้อมตรวจสอบสิทธิ์และเงื่อนไขก่อน submit |
| Actor / User Role | Employee, Outsource Employee |
| Related Requirement IDs | SFR-003, SFR-004, SFR-005, VR-001~005, SCR-003 |
| Source Reference | SRS Summary 4.1, 4.4, 4.6 |

## 2. Business Purpose

ให้พนักงานยื่นคำขอลาผ่านระบบแทนการใช้กระดาษหรือ email พร้อมตรวจสอบสิทธิ์วันลาและเงื่อนไขอัตโนมัติ

## 3. Screen Overview

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | Leave Request Form |
| Menu Path | Leave > Create Request |
| Navigation Inbound | Dashboard → กดปุ่ม "Create Leave" |
| Navigation Outbound | Submit สำเร็จ → Dashboard หรือ Request Detail |
| Preconditions | ผู้ใช้ login สำเร็จ และมีสิทธิ์ยื่นคำขอลา |
| Postconditions | คำขอถูกบันทึก สถานะ Draft หรือ Pending Manager Approval |

## 4. Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | *(ยังไม่มี mockup ใน workspace — สร้างเพิ่มหรืออ้างอิงจาก hand-on-ws01 ได้)* |
| Layout Description | 4 ส่วน: Employee Data, Leave Details, Balance Check, Approval & Validation |

## 5. Fields Definition

| Field Name | Label (TH/EN) | Data Type | Required | Default | Validation | Description |
| --- | --- | --- | --- | --- | --- | --- |
| leave_type | ประเภทการลา / Leave Type | ตัวเลือก | Y | — | Outsource เห็นเฉพาะลาป่วย/ลาพักร้อน | ประเภทการลา |
| start_date | วันที่เริ่มลา / Start Date | วันที่ | Y | — | ต้องไม่เป็นอดีต | วันที่เริ่มต้น |
| end_date | วันที่สิ้นสุด / End Date | วันที่ | Y | — | ต้อง >= start_date | วันที่สิ้นสุด |
| reason | เหตุผล / Reason | ข้อความ | Y | — | — | เหตุผลในการลา |
| contact_during | ช่องทางติดต่อ / Contact | ข้อความ | Y | — | — | เบอร์โทรระหว่างลา |
| attachment | เอกสารแนบ / Attachment | ไฟล์ | ลาป่วย ≥ 3 วัน = Y | — | PDF/JPG/PNG, ≤ 5MB, ≤ 3 ไฟล์ | ใบรับรองแพทย์ |

## 6. Commands / Actions

| Command | Description | Trigger Condition | System Response |
| --- | --- | --- | --- |
| Save Draft | บันทึกฉบับร่าง | กรอกข้อมูลบางส่วน | บันทึกสถานะ Draft |
| Validate | ตรวจสอบเงื่อนไข | กดปุ่ม | ตรวจสอบ required fields, balance, attachment |
| Submit | ยื่นคำขอ | ผ่าน validation ทั้งหมด | เช็ค Production Planning → route ไป Manager |

## 7. Screen Behavior

| Event | Trigger | Condition | Behavior | หมายเหตุ |
| --- | --- | --- | --- | --- |
| onLoad | เปิดหน้าจอ | — | แสดง Employee Data อัตโนมัติ, สถานะ = Draft | |
| onChange | เปลี่ยน leave_type | — | ดึง Balance Check มาแสดง | Outsource ถูกจำกัดตาม VR-003 |
| onChange | เปลี่ยน start/end date | — | คำนวณจำนวนวันลา | |
| onClick | กด Submit | ผ่าน validation | เช็ค PP → route → สถานะ = Pending Manager | |

## 8. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-SCR001-001 | ต้องกรอกข้อมูลหลักครบก่อน submit | block submit ถ้าไม่ครบ | VR-001 |
| BR-SCR001-002 | ตรวจสอบสิทธิ์วันลาคงเหลือ | block submit ถ้าไม่พอ | VR-002 |
| BR-SCR001-003 | ลาป่วย ≥ 3 วัน ต้องแนบใบรับรองแพทย์ | block submit ถ้าไม่แนบ | VR-004 |

## 9. Message List

### Error Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| ERR-SCR001-001 | กรอกไม่ครบ | กรุณากรอกข้อมูลให้ครบถ้วน | Please complete all required fields |
| ERR-SCR001-002 | สิทธิ์ไม่พอ | สิทธิ์วันลาคงเหลือไม่เพียงพอ | Insufficient leave balance |

### Success Messages

| Message ID | Trigger | Message (TH) | Message (EN) |
| --- | --- | --- | --- |
| SUC-SCR001-001 | submit สำเร็จ | ยื่นคำขอลาเรียบร้อยแล้ว | Leave request submitted successfully |

## 10. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| PP timeout | Production Planning ไม่ตอบกลับ | retry 2 ครั้ง แล้ว block | ไม่สามารถตรวจสอบตารางงานได้ กรุณาลองใหม่ | ลองใหม่ภายหลัง |

## 11. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Assumption | ระบบสร้าง Request No อัตโนมัติ | — |

## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | 2026-04-16 | BA Team | Created | สร้างเอกสารครั้งแรก |
