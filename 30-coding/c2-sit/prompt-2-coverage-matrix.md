# Prompt — สร้าง SIT Coverage Matrix

> Day 3 Session 2 — Section 2: สร้าง Coverage Matrix (Hands-on)  
> วิธีใช้: เปิด Claude session ใหม่ → วาง prompt ด้านล่างทั้งหมด → แนบไฟล์ตามรายการ

---

## ไฟล์ที่ต้องแนบใน Claude Session

```
1. 10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md
2. 30-coding/c2-sit/sit-scenario-leave-request.md
3. 80-knowledge-base/testing/template-coverage-matrix.md
```

---

## Prompt

```text
อ่านไฟล์ต่อไปนี้ก่อนสร้าง Coverage Matrix:

1. leave-request-and-approval-screen-srs.md   — SRS ที่มีรายการ Functional Requirement (FR) ทั้งหมด
2. sit-scenario-leave-request.md              — SIT scenario ที่สร้างไว้แล้ว (SIT-001 เป็นต้นไป)
3. template-coverage-matrix.md                — template และตัวอย่าง coverage matrix

จากนั้นสร้าง SIT Coverage Matrix สำหรับระบบ Leave Request and Approval
บันทึกเป็นไฟล์ `30-coding/c2-sit/sit-coverage-matrix-leave-request.md`

---

โครงสร้างไฟล์ที่ต้องมี:

## Header
- Module: Leave Request and Approval
- วันที่สร้าง
- Source: leave-request-and-approval-screen-srs.md + sit-scenario-leave-request.md

## Coverage Table
| FR ID | Requirement | Scenario ID | Scenario Name | Coverage | หมายเหตุ |

สัญลักษณ์:
- ✅ = มี scenario ครอบคลุมแล้ว
- ❌ = ยังไม่มี scenario
- ⚠️ = มี scenario แต่ครอบคลุมบางส่วน
- N/A = ไม่อยู่ใน SIT scope

## Coverage Summary
| รายการ | จำนวน | % |
รวม FR ทั้งหมด, ✅, ❌, ⚠️, N/A

สรุป: ผ่าน / ไม่ผ่าน exit criteria พร้อมระบุ FR ที่ยังขาด scenario

---

กฎสำหรับการ map:
- ดึง FR ID และชื่อ requirement จาก SRS ทุกรายการ
- Map แต่ละ FR กับ Scenario ID จาก sit-scenario-leave-request.md
- ถ้า FR หนึ่งรองรับได้ด้วยหลาย scenario ให้ใส่ทุก Scenario ID คั่นด้วย comma
- ถ้าไม่มี scenario รองรับ ให้ใส่ ❌ และเพิ่มหมายเหตุว่าควรเพิ่ม scenario อะไร
- ถ้า FR อยู่นอก scope SIT (เช่น email notification จริง, performance) ให้ใส่ N/A พร้อมเหตุผล
```
