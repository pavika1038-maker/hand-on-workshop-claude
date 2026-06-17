# Interface Functions

โฟลเดอร์นี้เก็บเอกสาร Functional Design สำหรับ **Interface Functions** (การเชื่อมต่อกับระบบภายนอก)

แบ่งเป็น 2 กลุ่ม:

## Interface File (IFF)

สำหรับ interface แบบ file-based (SFTP, FTP, Blob Storage)

| ไฟล์ | คำอธิบาย |
|------|---------|
| output-template-file.md | Template สำหรับสร้างเอกสาร Interface File |
| output-sample-file-NNN.md | ตัวอย่างเอกสาร Interface File |

### Naming Convention
- Prefix: `IFF`
- Format: `iff-[NNN]-[short-name].md`
- ตัวอย่าง: `iff-001-employee-master-sync.md`, `iff-002-leave-balance-export.md`

## Interface API (IFA)

สำหรับ interface แบบ API (REST, SOAP, GraphQL)

| ไฟล์ | คำอธิบาย |
|------|---------|
| output-template-api.md | Template สำหรับสร้างเอกสาร Interface API |
| output-sample-api-NNN.md | ตัวอย่างเอกสาร Interface API |

### Naming Convention
- Prefix: `IFA`
- Format: `ifa-[NNN]-[short-name].md`
- ตัวอย่าง: `ifa-001-schedule-check.md`, `ifa-002-notification-send.md`

## ไฟล์อื่น

| ไฟล์ | คำอธิบาย |
|------|---------|
| knowledge.md | องค์ความรู้หลักสำหรับ Interface Functions |

## สำคัญ
- ทุก function ต้องลงทะเบียนใน Function Index ก่อนสร้างเอกสาร
- เลือก template ให้ตรงกับประเภท interface (File หรือ API)
