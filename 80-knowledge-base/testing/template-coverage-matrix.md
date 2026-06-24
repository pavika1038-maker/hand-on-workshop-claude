# Template — SIT Coverage Matrix

> ใช้ template นี้สร้างไฟล์ `sit-coverage-matrix-{module}.md`  
> วัตถุประสงค์: ตรวจสอบว่าทุก Functional Requirement มี SIT scenario รองรับ  
> อ้างอิง: std-sit.md

---

## Header Section

```md
# SIT Coverage Matrix — {Module Name}

**Module:** {ชื่อ module}  
**วันที่สร้าง:** {YYYY-MM-DD}  
**Source:** SRS + sit-scenario-{module}.md  
**Version:** 1.0
```

---

## Coverage Table

```md
| FR ID | Requirement | Scenario ID | Scenario Name | Coverage | หมายเหตุ |
| --- | --- | --- | --- | --- | --- |
| FR-001 | {ชื่อ requirement} | SIT-xxx | {ชื่อ scenario} | ✅ / ❌ / ⚠️ | {หมายเหตุถ้ามี} |
```

### สัญลักษณ์ Coverage

| สัญลักษณ์ | ความหมาย |
|----------|---------|
| ✅ | มี scenario ครอบคลุมแล้ว |
| ❌ | ยังไม่มี scenario — ต้องเพิ่ม |
| ⚠️ | มี scenario แต่ครอบคลุมบางส่วน — ต้องเพิ่ม step |
| N/A | ไม่อยู่ใน SIT scope (เช่น UI-only หรือ integration ภายนอก) |

---

## Summary Section

```md
## Coverage Summary

| รายการ | จำนวน | % |
|--------|-------|---|
| FR ทั้งหมด | {n} | 100% |
| มี scenario ครบ (✅) | {n} | {%} |
| ยังไม่มี scenario (❌) | {n} | {%} |
| บางส่วน (⚠️) | {n} | {%} |
| N/A | {n} | — |

**สรุป:** {ผ่าน / ไม่ผ่าน exit criteria} — FR ที่ยังไม่มี scenario: {รายการ}
```

---

## ตัวอย่างจริง (Leave Request Module)

```md
# SIT Coverage Matrix — Leave Request

| FR ID | Requirement | Scenario ID | Scenario Name | Coverage | หมายเหตุ |
| --- | --- | --- | --- | --- | --- |
| FR-001 | Employee ยื่นคำขอลาได้ | SIT-001 | Employee submits leave request successfully | ✅ | |
| FR-002 | ระบบตรวจสอบวันลาคงเหลือ | SIT-004 | System prevents submit when balance is insufficient | ✅ | |
| FR-003 | ระบบตรวจสอบข้อมูลบังคับ | SIT-005 | System prevents submit when required field is missing | ✅ | |
| FR-004 | Manager อนุมัติคำขอลาได้ | SIT-002 | Manager approves leave request | ✅ | |
| FR-005 | Manager ปฏิเสธคำขอลาได้พร้อมเหตุผล | SIT-003 | Manager rejects leave request with reason | ✅ | |
| FR-006 | Employee ยกเลิกคำขอ Pending ได้ทันที | SIT-006 | Employee cancels pending leave request | ✅ | |
| FR-007 | Employee ยกเลิกคำขอ Approved ต้องรอ Manager | SIT-007 | Employee requests cancel on approved leave | ✅ | |
| FR-008 | Manager อนุมัติการยกเลิก → วันลาคืน | SIT-008 | Manager approves cancel request | ✅ | |
| FR-009 | Manager ปฏิเสธการยกเลิก → คำขอกลับ Approved | SIT-009 | Manager rejects cancel request | ✅ | |
| FR-010 | HR ดูรายการคำขอทั้งหมดได้ | SIT-010 | HR views all leave requests | ✅ | |
| FR-011 | SLA escalate เมื่อ Manager ไม่อนุมัติภายยกำหนด | — | — | ❌ | ยังไม่มี scenario — ต้องเพิ่ม |
```
