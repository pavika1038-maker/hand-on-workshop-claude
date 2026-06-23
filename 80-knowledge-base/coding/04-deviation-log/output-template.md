# Deviation Log Template

> ใช้บันทึกจุดที่ source code เบี่ยงเบนจาก design specification  
> กรอกหลัง generate code แต่ละ function และใช้ใน Part 7 Review

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Module | {feature_name} |
| Function ID | {function_id} เช่น SCR-001 |
| Design Source | {method_signature_file}, {sequence_diagram_file} |
| Code File(s) | {file_paths} |
| Review Date | {date} |
| Reviewed By | {reviewer} |

---

## 2. Deviation Log

| # | ประเภท | Design Spec | Actual Code | เหตุผล | การตัดสินใจ |
|---|--------|-------------|-------------|--------|------------|
| 1 | {type} | {design_expected} | {code_actual} | {reason} | Accept / Fix / Defer |

**ประเภท Deviation:**
- `Method` — signature เปลี่ยน (parameter, return type, method name)
- `Logic` — flow หรือ business rule ต่างจาก sequence diagram
- `Structure` — วาง code ผิด layer หรือ class
- `Missing` — method/validation ที่ design ระบุแต่ code ไม่มี
- `Extra` — code เพิ่มสิ่งที่ design ไม่ได้ระบุ

---

## 3. Action Required

| # | Action | Owner | Due |
|---|--------|-------|-----|
| 1 | {action description} | Dev / Designer | {date} |

---

## 4. Summary

- Deviation ทั้งหมด: {n} รายการ
- Accept (ยอมรับ): {n}
- Fix (แก้ code): {n}
- Defer (รอ sprint ถัดไป): {n}
- ต้องอัปเดต Design ไหม: Yes / No → {ระบุไฟล์ที่ต้องแก้}
