# Functional Design — Versioning Guideline

## 1. Version Format

```
MAJOR.MINOR
```

| ส่วน | เมื่อใดเปลี่ยน | ตัวอย่าง |
|------|--------------|---------|
| MAJOR | เปลี่ยน scope, flow, หรือ business rule หลักของ function | 1.0 → 2.0 |
| MINOR | เพิ่มรายละเอียด, แก้ไขเล็กน้อย, เพิ่ม field | 1.0 → 1.1 |

## 2. Status Lifecycle

```text
Draft → In Review → Approved → Revised
                                  ↓
                              (กลับไป Draft เมื่อแก้ไข major)
```

| Status | คำอธิบาย |
|--------|---------|
| Draft | กำลังเขียน ยังไม่พร้อม review |
| In Review | ส่ง review แล้ว รอ feedback |
| Approved | ผ่านการ review พร้อมใช้งาน |
| Revised | มีการแก้ไขหลัง approve (ต้อง review ใหม่) |

## 3. Change Log ในเอกสาร

ทุกเอกสาร functional design ควรมี Change Log ใน front-matter หรือท้ายเอกสาร:

```markdown
## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | 2026-04-16 | BA Team | Created | สร้างเอกสารครั้งแรก |
| 1.1 | 2026-04-17 | BA Team | Updated | เพิ่ม field delegate_to |
```

## 4. กฎสำคัญ

- ทุกการเปลี่ยนแปลงต้องบันทึกใน Change Log
- ห้ามแก้ไขเอกสารที่ status = Approved โดยไม่เปลี่ยน status เป็น Revised
- เมื่อ Revised ต้อง bump version number
- Function Index ต้องอัปเดต status ให้ตรงกับเอกสารเสมอ
