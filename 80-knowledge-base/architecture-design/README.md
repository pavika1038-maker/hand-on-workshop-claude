# Architecture Design Knowledge Base

โฟลเดอร์นี้เป็นศูนย์รวมองค์ความรู้ มาตรฐาน template และ sample output สำหรับงาน Architecture Design ขององค์กร

## โครงสร้าง

| Folder | Purpose | Files |
|---|---|---|
| `01-application-architecture/` | Application Architecture | `knowledge.md`, `output-template.md`, `output-sample.md` |
| `02-data-architecture/` | Data Architecture | `knowledge.md`, `output-template.md`, `output-sample.md` |
| `03-integration-architecture/` | Integration Architecture | `knowledge.md`, `output-template.md`, `output-sample.md` |
| `04-infrastructure-architecture/` | Infrastructure Architecture | `knowledge.md`, `output-template.md`, `output-sample.md` |
| `05-security-architecture/` | Security Architecture | `knowledge.md`, `output-template.md`, `output-sample.md` |

## วิธีใช้

1. อ่าน [00-architecture-design-index.md](./00-architecture-design-index.md) เพื่อดูภาพรวม
2. เข้า folder ตามมิติ architecture ที่ต้องการ
3. ใช้ `knowledge.md` เป็น baseline ขององค์กร
4. ใช้ `output-template.md` เป็นโครงสร้างมาตรฐานของผลลัพธ์
5. ใช้ `output-sample.md` เป็นตัวอย่างรูปแบบเอกสารที่คาดหวัง

## หมายเหตุ

- โครงสร้างนี้ออกแบบให้ skill และ agent สามารถอ้างอิงองค์ความรู้, template, และ sample ของแต่ละมิติได้จากโฟลเดอร์เดียวกัน
- output จริงของงานออกแบบยังคงถูกสร้างไว้ใต้ `20-system-design/a0-architecture-design/`
