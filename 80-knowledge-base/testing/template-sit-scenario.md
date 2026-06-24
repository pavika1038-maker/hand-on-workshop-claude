# Template — SIT Scenario

> ใช้ template นี้สร้างไฟล์ `sit-scenario-{module}.md`  
> อ้างอิง: std-sit.md §6.1, §6.2

---

## Header Section

```md
# SIT Scenario — {Module Name}

**Module:** {ชื่อ module เช่น Leave Request}  
**วันที่สร้าง:** {YYYY-MM-DD}  
**สร้างโดย:** {ชื่อ / AI-assisted}  
**Version:** 1.0  

**Source Artifacts:**
- {path/to/srs.md หรือ section ใน SRS}
- {path/to/functional-design.md}
- {path/to/sequence-diagram.md}
```

---

## Scenario Table

```md
| Scenario ID | Scenario Name | Business Flow | Pre-condition | Steps | Expected Result | Priority |
| --- | --- | --- | --- | --- | --- | --- |
| SIT-001 | {ชื่อ scenario} | {ชื่อ flow} | {เงื่อนไขก่อนทดสอบ} | 1. {step 1}<br>2. {step 2}<br>3. {step 3} | {ผลลัพธ์ที่คาดหวัง} | High/Medium/Low |
```

### คำอธิบายแต่ละ column

| Column | คำอธิบาย | ตัวอย่าง |
|--------|---------|---------|
| **Scenario ID** | รหัส unique (SIT-xxx) | SIT-001 |
| **Scenario Name** | ชื่อ scenario ในรูป "{Actor} {action} {object}" | Employee submits leave request successfully |
| **Business Flow** | ชื่อ flow ที่ครอบคลุม | Create Leave Request |
| **Pre-condition** | สภาวะก่อนทดสอบ (login, data พร้อม) | Employee login สำเร็จ มีวันลาคงเหลือ ≥ 1 วัน |
| **Steps** | ขั้นตอนที่ต้องทำตามลำดับ | 1. เข้าเมนู Leave Request 2. กรอกข้อมูล 3. กด Submit |
| **Expected Result** | ผลลัพธ์ที่ถูกต้องของระบบ | สถานะเปลี่ยนเป็น Pending, ข้อมูลบันทึกใน DB |
| **Priority** | High / Medium / Low (อ้างอิงจาก std-sit.md §7.1) | High |

---

## แนวทางแบ่ง Scenario Type

### Happy Path
- 1 scenario ต่อ 1 business flow หลัก
- ครอบคลุมตั้งแต่ start จนถึง success state
- Pre-condition: ข้อมูลถูกต้องครบถ้วน

### Alternative Path
- กรณีที่ flow เดิมมีทางเลือก เช่น Cancel Pending vs Cancel Approved
- ใช้ scenario ID ถัดกัน เช่น SIT-004, SIT-005

### Error / Rejection Flow
- Validation error (กรอกข้อมูลไม่ครบ / ผิดรูปแบบ)
- Business rule violation (balance ไม่พอ, unauthorized)
- Expected Result ต้องระบุ error message ที่ควรแสดง

---

## ตัวอย่างจริง (Leave Request Module)

```md
| Scenario ID | Scenario Name | Business Flow | Pre-condition | Steps | Expected Result | Priority |
| --- | --- | --- | --- | --- | --- | --- |
| SIT-001 | Employee submits leave request successfully | Create Leave Request | Employee (EMP001) login สำเร็จ มีวันลา Annual ≥ 3 วัน | 1. เข้าเมนู Leave Request<br>2. เลือกประเภท Annual Leave<br>3. กรอกวันที่ลา 3 วัน และเหตุผล<br>4. กด Submit | สถานะ Pending Approval, วันลา PendingDays +3, แสดงใน Manager queue | High |
| SIT-002 | Manager approves leave request | Approve Leave Request | มีคำขอ SIT-001 ที่สถานะ Pending, Manager (EMP002) login สำเร็จ | 1. Manager เข้า Approval Queue<br>2. เลือกคำขอของ EMP001<br>3. กด Approve | สถานะ Approved, UsedDays +3, PendingDays -3, Employee เห็นผล | High |
| SIT-003 | Manager rejects leave request with reason | Reject Leave Request | มีคำขอ Pending, Manager login สำเร็จ | 1. Manager เข้า Approval Queue<br>2. เลือกคำขอ<br>3. กรอกเหตุผลปฏิเสธ<br>4. กด Reject | สถานะ Rejected, PendingDays กลับคืน, เหตุผลแสดงให้ Employee เห็น | High |
| SIT-004 | System prevents submit when balance is insufficient | Validation — Balance | Employee login สำเร็จ มีวันลา Annual เหลือ 1 วัน | 1. เข้าเมนู Leave Request<br>2. เลือก Annual 3 วัน<br>3. กด Submit | ระบบแสดง error "วันลาคงเหลือไม่เพียงพอ" ไม่บันทึกคำขอ | High |
```
