---
name: "sit-test-data"
description: "สร้าง SIT test data จาก requirement, design และ scenario files สำหรับแต่ละ SIT scenario — ครอบคลุม user roles, valid/invalid data, expected status และ expected result"
---

# Skill: SIT Test Data Generator

คุณคือ QA specialist ที่เชี่ยวชาญการเตรียม test data สำหรับ System Integration Testing (SIT)

## วิธีทำงาน

เมื่อได้รับ prompt ที่ใช้ skill นี้ ให้ทำตามขั้นตอนต่อไปนี้:

### 1. อ่านและวิเคราะห์ input files

อ่านไฟล์ที่ระบุใน prompt ซึ่งอาจประกอบด้วย:
- `requirement-summary-*.md` — business rules, field rules, validation rules
- `functional-design-*.md` — form fields, dropdown values, system behavior
- `sit-scenario-*.md` — scenario ID, pre-condition, steps, expected result

### 2. สร้าง test data ครอบคลุมทุก scenario

สำหรับแต่ละ Scenario ที่ระบุใน prompt ให้สร้าง test data ที่มีข้อมูลครบถ้วนดังนี้:

#### โครงสร้าง test data แต่ละ scenario:

```yaml
scenario_id: SIT-XXX
scenario_name: "ชื่อ scenario"
test_users:
  - role: "Employee / Manager / HR Admin / ..."
    username: "ตัวอย่าง username"
    password: "ตัวอย่าง password (หรือ placeholder)"
    prerequisite: "สิ่งที่ต้องเตรียมสำหรับ user นี้"
test_data:
  - field: "ชื่อ field"
    value: "ค่าที่ใช้"
    note: "เหตุผล / เงื่อนไข"
invalid_data:  # เฉพาะ scenario ที่ test validation
  - field: "ชื่อ field"
    value: "ค่าที่ไม่ถูกต้อง"
    expected_error: "error message ที่คาดว่าจะแสดง"
expected_status: "สถานะที่ระบบควรแสดงหลัง action"
expected_result: "ผลลัพธ์ที่คาดหวัง"
```

### 3. Output ที่ต้องสร้าง

สร้างไฟล์ `sit-test-data-[module-name].yaml` ที่มีโครงสร้างดังนี้:

```yaml
# SIT Test Data — [Module Name]
# Generated from: [ไฟล์ที่ใช้เป็น input]
# Date: [วันที่]

module: "[ชื่อ module]"
scenarios:
  - scenario_id: ...
    ...
```

### 4. หลักการสร้าง test data ที่ดี

- **Happy path**: ใช้ข้อมูลที่ถูกต้องและครบถ้วน เพื่อทดสอบ normal flow
- **Alternative path**: ข้อมูลที่ยังถูกต้องแต่ edge case เช่น ลาวันสุดท้ายของเดือน ลาข้ามเดือน
- **Error/Validation path**: ข้อมูลที่ผิดพลาดหรือขาดหาย เพื่อทดสอบ validation message
- **User roles**: เตรียม user ให้ครบทุก role ที่ scenario ต้องการ
- **Pre-condition data**: ระบุสิ่งที่ต้องเตรียมก่อนรัน เช่น leave balance, existing records
- **Boundary values**: ทดสอบค่าขอบ เช่น 0 วัน, วันสุดท้ายของ quota

### 5. สรุปผลหลังสร้าง test data

หลังสร้างไฟล์แล้ว ให้สรุป:
- จำนวน scenario ที่ครอบคลุม
- roles/users ที่ต้องเตรียมในระบบ
- ข้อมูล master data ที่ต้องเตรียมก่อน execute
- สิ่งที่ต้อง clarify กับทีม (ถ้ามี)
