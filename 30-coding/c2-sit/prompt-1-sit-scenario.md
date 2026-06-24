# Prompt — สร้าง SIT Scenario

> Day 3 Session 2 — Section 1: สร้าง SIT Scenario (Hands-on)  
> วิธีใช้: เปิด Claude session ใหม่ → วาง prompt ด้านล่างทั้งหมด → แนบไฟล์ตามรายการ

---

## ไฟล์ที่ต้องแนบใน Claude Session

```
1. 10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md
2. 10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md
3. 20-system-design/b0-functional-design/leave-request-and-approval-sequence-diagram.md
4. 80-knowledge-base/testing/std-sit.md
5. 80-knowledge-base/testing/template-sit-scenario.md
6. 80-knowledge-base/testing/lesson-learned-sit-defects.md
```

---

## Prompt

```text
อ่านไฟล์ต่อไปนี้ก่อนสร้าง SIT scenario:

1. leave-request-and-approval-requirement-summary.md  — สรุป requirement ทุก flow ของระบบ
2. leave-request-and-approval-screen-srs.md           — SRS รายละเอียด FR, validation rule, business rule
3. leave-request-and-approval-sequence-diagram.md     — sequence diagram แสดง flow ระหว่าง Actor, API, DB
4. std-sit.md                                          — มาตรฐาน SIT: scope, defect severity, naming, test accounts
5. template-sit-scenario.md                            — template และตัวอย่าง SIT scenario table
6. lesson-learned-sit-defects.md                       — defect history และ edge case ที่มักพลาดใน SIT

จากนั้นสร้าง SIT scenario สำหรับระบบ Leave Request and Approval
บันทึกเป็นไฟล์ `30-coding/c2-sit/sit-scenario-leave-request.md`

---

โครงสร้างไฟล์ที่ต้องมี:

## Header
- ชื่อ module: Leave Request and Approval
- source artifacts ที่ใช้เป็น input (รายชื่อไฟล์ทั้ง 3 ด้านบน)
- วันที่สร้าง

## Scenario Table
| Scenario ID | Scenario Name | Business Flow | Pre-condition | Steps | Expected Result | Priority |

---

ครอบคลุม scenario ดังต่อไปนี้:

**Happy Path (ทุก flow หลัก):**
- Employee ยื่นคำขอลาสำเร็จ (Annual, Sick, Business leave)
- Manager อนุมัติคำขอลา → วันลาถูก deduct
- Manager ปฏิเสธคำขอลา → ระบุเหตุผล
- Employee ยกเลิกคำขอที่ยัง Pending → Cancelled ทันที
- Employee ขอยกเลิกคำขอที่ Approved → CancelRequested
- Manager อนุมัติการยกเลิก → วันลาคืน
- Manager ปฏิเสธการยกเลิก → คำขอกลับ Approved
- HR ดูรายการคำขอทั้งหมด

**Error / Validation Flow:**
- ยื่นลาเมื่อวันลาคงเหลือไม่เพียงพอ
- ยื่นลาโดยไม่ระบุข้อมูลบังคับ (วันที่ / ประเภทการลา)
- Reject โดยไม่ระบุเหตุผล
- Manager approve คำขอของตัวเอง (Unauthorized)
- Cancel คำขอที่ Cancelled แล้ว (Invalid transition)

**Edge Case จาก lesson-learned-sit-defects.md:**
- ตรวจ balance หลัง approve (DEF-001)
- ตรวจ Manager queue ของพนักงานที่ manager = EMP002 (DEF-002)
- ตรวจว่า UI แสดง rejection reason ให้ Employee เห็น (DEF-006)
- ตรวจว่า Cancel queue ของ Manager มี entry หลัง Employee ขอยกเลิก (DEF-004)

---

กฎสำหรับแต่ละ scenario:
- ใช้ Scenario ID รูปแบบ SIT-xxx (เริ่มที่ SIT-001)
- Priority: High / Medium / Low ตาม std-sit.md §7.1
- Pre-condition ต้องระบุ EmployeeId (EMP001/EMP002/EMP003) และสถานะของข้อมูลก่อนทดสอบ
- Steps ต้องระบุเป็นขั้นตอนที่ทำตามได้จริงบนหน้าจอ
- Expected Result ต้องระบุทั้ง UI state และ DB state (status, balance) ที่ควรเปลี่ยน
- ถ้า scenario มี dependency กับ scenario อื่น ให้ระบุใน Pre-condition เช่น "ต้องทำ SIT-001 ก่อน"
```
