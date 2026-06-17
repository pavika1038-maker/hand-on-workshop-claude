# Common Functions — Output Sample

> ตัวอย่างนี้แสดงให้เห็นว่าเอกสาร Common Function ที่สร้างจาก template หน้าตาเป็นอย่างไร

---

---
function_id: "COM-001"
function_name: "Login"
category: "Common"
version: "1.0"
status: "Draft"
author: "BA Team"
last_updated: "2026-04-16"
---

# COM-001 — Login

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | COM-001 |
| Function Name | Login |
| Category | Common |
| Description | ระบบรองรับการเข้าสู่ระบบของผู้ใช้ตามบทบาท และแสดงสิทธิ์การใช้งานตามบทบาทของแต่ละกลุ่ม |
| Actor / User Role | All Users (Employee, Outsource Employee, Manager, HR) |
| Related Requirement IDs | SFR-001, TR-006, SCR-001 |
| Source Reference | SRS Summary 4.1 SFR-001, 4.4 SCR-001 |

## 2. Business Purpose

ให้ผู้ใช้เข้าสู่ระบบและเห็นสิทธิ์การใช้งานตามบทบาทก่อนเข้าใช้งานระบบ ป้องกันการเข้าถึงโดยไม่ได้รับอนุญาต

## 3. Process Flow

### 3.1 Step-by-Step

| Step | Actor | Action | System Response | หมายเหตุ |
| --- | --- | --- | --- | --- |
| 1 | User | เปิดหน้า Login | ระบบแสดงฟอร์ม Login (User ID, Password, Language) | |
| 2 | User | กรอก User ID และ Password | — | |
| 3 | User | กดปุ่ม Log In | ระบบตรวจสอบข้อมูลกับ AD | |
| 4 | System | ตรวจสอบสำเร็จ | ระบบแสดง Role Access Overview และนำไปยังหน้าจอ default ตามบทบาท | Employee → Dashboard, HR → HR Worklist |
| 5 | System | ตรวจสอบไม่สำเร็จ | ระบบแสดงข้อความ error | ล็อกหลังผิด 5 ครั้ง |

## 4. UI / Screen

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | Login / Role Access |
| Navigation Inbound | เปิดระบบ / URL ของระบบ |
| Navigation Outbound | Login สำเร็จ → Dashboard (Employee/Outsource) หรือ HR Worklist (HR) หรือ Approval Worklist (Manager) |

### Fields Definition

| Field Name | Label (TH/EN) | Data Type | Required | Default | Description |
| --- | --- | --- | --- | --- | --- |
| user_id | รหัสผู้ใช้ / User ID | ข้อความ | Y | — | รหัสผู้ใช้สำหรับเข้าสู่ระบบ |
| password | รหัสผ่าน / Password | ข้อความ (masked) | Y | — | รหัสผ่านของผู้ใช้ |
| language | ภาษา / Language | ตัวเลือก | N | TH | ภาษาที่ใช้แสดงผล (TH/EN) |

### Commands / Actions

| Command | Description | Trigger Condition | System Response |
| --- | --- | --- | --- |
| Log In | เข้าสู่ระบบ | กรอก User ID + Password ครบ | ตรวจสอบกับ AD แล้วนำไปยังหน้าจอ default |
| Clear | ล้างข้อมูล | กดปุ่ม | ล้างฟอร์ม |
| Cancel | ยกเลิก | กดปุ่ม | ปิดหน้าจอ |

## 5. Business Rules

| Rule ID | Business Rule | Impact | Source Reference |
| --- | --- | --- | --- |
| BR-COM001-001 | ระบบต้องแยกสิทธิ์ตามบทบาท Employee, Outsource, Manager, HR | แสดงเมนูและ function ตามบทบาท | TR-006 |
| BR-COM001-002 | ล็อกบัญชีหลังกรอกรหัสผ่านผิด 5 ครั้ง ล็อกนาน 30 นาที | ป้องกัน brute force | ISO 27001 Annex A 5.17 |
| BR-COM001-003 | Session timeout หลังไม่มีการใช้งาน 30 นาที | ป้องกัน unauthorized access | ISO 27001 |

## 6. Exception Handling

| Error Case | Trigger Condition | System Behavior | User Message | Recovery |
| --- | --- | --- | --- | --- |
| กรอกไม่ครบ | User ID หรือ Password ว่าง | ไม่ดำเนินการ | กรุณากรอกรหัสผู้ใช้และรหัสผ่าน | กรอกให้ครบ |
| ข้อมูลผิด | User ID หรือ Password ไม่ตรง | ไม่อนุญาต login | รหัสผู้ใช้หรือรหัสผ่านไม่ถูกต้อง | ตรวจสอบแล้วลองใหม่ |
| บัญชีถูกล็อก | ผิด 5 ครั้งติดต่อกัน | ล็อก 30 นาที | บัญชีถูกระงับ กรุณาติดต่อผู้ดูแลระบบ | รอ 30 นาที หรือติดต่อ admin |

## 7. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Assumption | ใช้ AD Login ผ่าน Microsoft Entra ID | ต้องมี AD account ก่อนใช้งาน |
| Assumption | ข้อมูลผู้ใช้ sync จาก HRIS | ผู้ใช้ใหม่ต้องรอ sync ก่อน login ได้ |

## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | 2026-04-16 | BA Team | Created | สร้างเอกสารครั้งแรก |
