# Functional Design Knowledge Base

## วัตถุประสงค์

Knowledge Base นี้เป็นมาตรฐานองค์กรสำหรับการจัดทำเอกสาร Functional Design
ออกแบบให้ **1 Function = 1 Document** รองรับการ scale ระดับ Enterprise
ใช้ร่วมกันได้ระหว่าง BA / SA / Dev

## โครงสร้างโฟลเดอร์

```text
functional-design/
│
├── 00-governance/                    ← กฎ มาตรฐาน template
│   ├── naming-convention.md
│   ├── document-template.md
│   └── versioning-guideline.md
│
├── function-index.md                 ← Single Source of Truth (ดัชนี function ทั้งหมด)
│
├── 01-common-functions/              ← Login, Profile, Menu, Dashboard
│   └── com-[NNN]-[name].md
│
├── 02-screen-functions/              ← Form, Worklist, Search, Detail
│   └── scr-[NNN]-[name].md
│
├── 03-report-functions/              ← Report, Dashboard, Summary
│   └── rpt-[NNN]-[name].md
│
├── 04-interface-functions/           ← API, File Transfer, Integration
│   └── int-[NNN]-[name].md
│
├── 05-batch-functions/               ← Scheduled Job, Sync, Calculation
│   └── bat-[NNN]-[name].md
│
└── 06-archive/                       ← เอกสารเวอร์ชันเก่า
```

## Recommended Workflow

```text
1. Define Menu / Navigation (COM-003)
       ↓
2. Create Function Index (ลงทะเบียน function ทั้งหมด)
       ↓
3. Create Function Docs (1 function = 1 file ตาม template)
       ↓
4. Update Index + Cross-link
       ↓
5. Refine Menu / Flow
```

## Best Practices (Enterprise)

| # | กฎ | เหตุผล |
|---|-----|--------|
| 1 | ✅ ทุก function ต้องมี ID | ใช้ track และ trace ได้ |
| 2 | ✅ ห้ามรวมหลาย function ในไฟล์เดียว | ลดความซับซ้อน แก้ไขง่าย |
| 3 | ✅ Menu ต้อง define ก่อนเสมอ | เป็นตัวกำหนดโครงสร้างระบบ |
| 4 | ✅ ใช้ Function Index คุม scope | Single Source of Truth |
| 5 | ✅ ใช้ relative link เชื่อมเอกสาร | ย้ายโฟลเดอร์ได้โดยไม่เสีย link |
| 6 | ✅ ใช้ template มาตรฐาน | ทุกเอกสารมีโครงสร้างเดียวกัน |
| 7 | ✅ อัปเดต Change Log ทุกครั้งที่แก้ไข | ติดตามการเปลี่ยนแปลงได้ |

## Quick Links

| เอกสาร | Path |
|--------|------|
| Naming Convention | [00-governance/naming-convention.md](00-governance/naming-convention.md) |
| Document Template | [00-governance/document-template.md](00-governance/document-template.md) |
| Versioning Guideline | [00-governance/versioning-guideline.md](00-governance/versioning-guideline.md) |
| Function Index | [function-index.md](function-index.md) |
