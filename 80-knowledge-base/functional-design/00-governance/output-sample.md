# Governance — Output Sample

> ตัวอย่างนี้แสดงให้เห็นว่าเอกสาร governance ที่สร้างจาก template หน้าตาเป็นอย่างไร

---

# Code Review Checklist

## 1. วัตถุประสงค์

เอกสารนี้กำหนด checklist สำหรับ code review ก่อน merge เข้า develop/main branch

## 2. ขอบเขต

ครอบคลุมทุก pull request ของ functional design implementation

## 3. กฎ / มาตรฐาน

| # | กฎ | รายละเอียด | ตัวอย่าง |
|---|-----|-----------|---------|
| 1 | Naming convention | ชื่อ class, method, variable ต้องตรงกับ coding standard | `GetLeaveBalance()` ไม่ใช่ `getlvbal()` |
| 2 | Unit test | ทุก business rule ต้องมี unit test | `LeaveBalanceCalculator_ShouldReturn_CorrectBalance()` |
| 3 | No hardcode | ห้าม hardcode ค่า config, connection string | ใช้ `appsettings.json` หรือ Key Vault |

## 4. ตัวอย่าง

PR #123 — เพิ่ม Leave Balance Calculation:
- ✅ Naming convention ถูกต้อง
- ✅ Unit test ครบ 5 cases
- ❌ พบ hardcode connection string → แก้ไขแล้วใน commit abc123

## 5. ข้อยกเว้น

- Hotfix ที่ต้อง deploy ด่วน สามารถ review หลัง merge ได้ แต่ต้อง review ภายใน 24 ชั่วโมง
