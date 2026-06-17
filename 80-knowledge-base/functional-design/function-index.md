# Function Index

> เอกสารนี้เป็น **Single Source of Truth** สำหรับ scope ของ functional design ทั้งหมด
> ทุก function ต้องลงทะเบียนที่นี่ก่อนเริ่มเขียนเอกสาร

## สรุปภาพรวม

| Category | Prefix | จำนวน Function | Folder |
|----------|--------|---------------|--------|
| Common | COM | 0 | `01-common-functions/` |
| Screen | SCR | 0 | `02-screen-functions/` |
| Report | RPT | 0 | `03-report-functions/` |
| Interface | INT | 0 | `04-interface-functions/` |
| Batch | BAT | 0 | `05-batch-functions/` |

## Function Registry

| ID | Function Name | Category | Description | Priority | Status | Owner | Document |
|----|--------------|----------|------------|----------|--------|-------|----------|
| — | ยังไม่มี function ลงทะเบียน | — | — | — | — | — | — |

> **วิธีใช้:**
> 1. เพิ่มแถวใหม่ในตาราง Function Registry เมื่อมี function ใหม่
> 2. กรอก ID ตาม naming convention (ดู `00-governance/naming-convention.md`)
> 3. สร้างไฟล์เอกสารตาม template (ดู `00-governance/document-template.md`)
> 4. ใส่ relative link ในคอลัมน์ Document
> 5. อัปเดต Status เมื่อเอกสารเปลี่ยนสถานะ

## ตัวอย่าง Function Registry (สำหรับอ้างอิง)

| ID | Function Name | Category | Description | Priority | Status | Owner | Document |
|----|--------------|----------|------------|----------|--------|-------|----------|
| COM-001 | Login | Common | User authentication และแสดงสิทธิ์ตามบทบาท | High | Draft | BA | `01-common-functions/com-001-login.md` |
| COM-002 | User Profile | Common | แสดงและแก้ไขข้อมูลส่วนตัวของผู้ใช้ | Medium | Draft | BA | `01-common-functions/com-002-user-profile.md` |
| COM-003 | Menu Navigation | Common | โครงสร้างเมนูหลักและการนำทาง | High | Draft | BA | `01-common-functions/com-003-menu-navigation.md` |
| COM-004 | Home Dashboard | Common | หน้าจอหลักแสดงภาพรวมตามบทบาท | High | Draft | BA | `01-common-functions/com-004-home-dashboard.md` |
| SCR-001 | Create Leave Request | Screen | สร้างคำขอลาใหม่ | High | Draft | BA | `02-screen-functions/scr-001-create-request.md` |
| SCR-002 | Approval Screen | Screen | หน้าจออนุมัติ/ปฏิเสธคำขอ | High | Draft | BA | `02-screen-functions/scr-002-approval-screen.md` |
| SCR-003 | Search Screen | Screen | ค้นหาและกรองคำขอ | Medium | Draft | BA | `02-screen-functions/scr-003-search-screen.md` |
| RPT-001 | Leave Summary | Report | รายงานสรุปการลา | Medium | Draft | BA | `03-report-functions/rpt-001-leave-summary.md` |
| RPT-002 | Usage Dashboard | Report | Dashboard แสดงภาพรวมการใช้สิทธิ์ | Low | Draft | BA | `03-report-functions/rpt-002-usage-dashboard.md` |
| INT-001 | HR API Inbound | Interface | รับข้อมูลพนักงานจาก HRIS | High | Draft | SA | `04-interface-functions/int-001-hr-api-inbound.md` |
| INT-002 | Payroll Export | Interface | ส่งข้อมูลการลาไปยังระบบ Payroll | Medium | Draft | SA | `04-interface-functions/int-002-payroll-export.md` |
| BAT-001 | Nightly Sync | Batch | Sync ข้อมูลพนักงานจาก HRIS ทุกคืน | High | Draft | SA | `05-batch-functions/bat-001-nightly-sync.md` |
| BAT-002 | Leave Balance Calc | Batch | คำนวณสิทธิ์วันลาคงเหลือ | High | Draft | SA | `05-batch-functions/bat-002-leave-balance-calc.md` |

> **หมายเหตุ:** ตารางตัวอย่างด้านบนเป็นเพียงตัวอย่างสำหรับอ้างอิงรูปแบบ ให้ลบออกเมื่อเริ่มใช้งานจริง
