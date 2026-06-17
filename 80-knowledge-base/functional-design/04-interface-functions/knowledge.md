# Interface Functions — องค์ความรู้หลัก

## 1. คำจำกัดความ

Interface Functions คือ function ที่จัดการการเชื่อมต่อและแลกเปลี่ยนข้อมูลกับระบบภายนอก แบ่งเป็น 2 กลุ่มหลัก:

- **Interface File** — แลกเปลี่ยนข้อมูลผ่านไฟล์ (SFTP, FTP, Blob Storage)
- **Interface API** — แลกเปลี่ยนข้อมูลผ่าน API (REST, SOAP, GraphQL)

## 2. ประเภทของ Interface

### 2.1 Interface File (IFF)

| ประเภท | Direction | Pattern | ตัวอย่าง |
|--------|-----------|---------|---------|
| File Inbound | รับไฟล์เข้า | SFTP / FTP / Blob / Local Folder | รับไฟล์ข้อมูลจากระบบภายนอก → staging → process |
| File Outbound | ส่งไฟล์ออก | SFTP / FTP / Blob | ดึงข้อมูลจาก DB → สร้างไฟล์ → ส่งไประบบปลายทาง |

### 2.2 Interface API (IFA)

| ประเภท | Direction | Pattern | ตัวอย่าง |
|--------|-----------|---------|---------|
| API Inbound | รับข้อมูลเข้า (Provide API) | REST / SOAP / GraphQL | ให้ระบบภายนอกเรียกเข้ามา |
| API Outbound | ส่งข้อมูลออก (Consume API) | REST / SOAP / GraphQL | เรียก API ระบบภายนอก |
| Event Publish | ส่ง event | Service Bus / Message Queue | แจ้ง event เมื่อเกิดการเปลี่ยนแปลง |
| Event Subscribe | รับ event | Service Bus / Message Queue | รับ event จากระบบอื่น |

## 3. ความแตกต่างระหว่าง Interface File กับ API

| หัวข้อ | Interface File (IFF) | Interface API (IFA) |
|--------|---------------------|---------------------|
| การสื่อสาร | Asynchronous (batch) | Synchronous / Asynchronous |
| Timing | Scheduled (daily, hourly) | Realtime / Near-realtime |
| Data Volume | สูง (bulk data) | ต่ำ-กลาง (per transaction) |
| Protocol | SFTP, FTP, Blob Storage, Local Folder | HTTPS, gRPC |
| Format | CSV, TSV, XML, JSON, Excel, DAT (text) | JSON, XML |
| Error Handling | File-level (reject/retry file) + Record-level (skip/reject) | Request-level (HTTP status) |
| Monitoring | File received/processed | Request/Response latency |
| ID Prefix | IFF-[NNN] | IFA-[NNN] |
| Template | output-template-file.md | output-template-api.md |
| Sample | output-sample-file-{ib/ob}-NNN.md | output-sample-api-NNN.md |

## 4. โครงสร้างเอกสาร

### 4.1 Interface File (IFF)

เอกสาร Interface File มี 2 รูปแบบ ขึ้นกับความซับซ้อน:

#### รูปแบบย่อ (Simple — SFTP sync, single entity)

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| 1. Overview | ข้อมูลทั่วไป (ID, Name, Direction, Pattern, Systems) | Y |
| 2. Business Purpose | เหตุผลทางธุรกิจ | Y |
| 3. Interface Description | Protocol, Auth, Frequency, Schedule, Timeout, Retry | Y |
| 4. File Specification | Format, Encoding, Delimiter, Naming, Path | Y |
| 5. Data Mapping | Source → Dest field mapping + sample data | Y |
| 6. Trigger / Timing | เวลาและเงื่อนไขที่ trigger | Y |
| 7. Processing Logic | Pre/Data/Post processing steps | Y |
| 8. Expected Result | Success, Partial, Failure scenarios | Y |
| 9. Error Handling | Error cases + recovery | Y |
| 10. Business Rules | กฎเกณฑ์ทางธุรกิจ | Y |
| 11. Monitoring & Alerting | Alert channels + recipients | Y |
| 12. Notes / Assumptions | สมมติฐาน | Y |
| Change Log | ประวัติการเปลี่ยนแปลง | Y |

#### รูปแบบเต็ม (Complex — multi-entity, API integration, branching logic)

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| Document Header | Doc No, Project/System/Team metadata | Y |
| 1. Outline | Function Name, Solution/Package, Process Flow Diagrams (System + Outline) | Y |
| 2. Item Description | Interface file layout summary (field list) | Y |
| 3. Process Description | Step-by-step processing with entity CRUD, API calls, branching logic | Y |
| 4. Outline Process | Flow summary (text-based decision tree) | Y |
| 5. Screen Display | Screen items showing processed data (ถ้ามี) | เมื่อมี UI |
| 6. Database Tables | Staging + Reject table definitions with column types | Y |
| 7. Revision History | ประวัติการเปลี่ยนแปลง + Key Updates summary | Y |

### 4.2 Interface API (IFA)

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| 1. Overview | ข้อมูลทั่วไป (ID, Name, Direction, Pattern, Systems) | Y |
| 2. Business Purpose | เหตุผลทางธุรกิจ | Y |
| 3. API Description | Protocol, Method, URL, Endpoint, Auth, Rate Limit, Timeout | Y |
| 4. Request Specification | Headers, Path/Query Params, Body, Field Mapping | Y |
| 5. Response Specification | Success/Error response + Field Mapping | Y |
| 6. HTTP Status Codes | Status codes ที่ใช้ | Y |
| 7. Trigger / Timing | เวลาและเงื่อนไขที่ trigger | Y |
| 8. Data Mapping | Source ↔ Dest field mapping | Y |
| 9. Error Handling | Error cases + HTTP status + recovery | Y |
| 10. Business Rules | กฎเกณฑ์ทางธุรกิจ | Y |
| 11. Security | Auth, Authorization, Encryption, Rate Limit, Logging | Y |
| 12. Example Request/Response | ตัวอย่าง request/response จริง | Y |
| 13. Notes / Assumptions | สมมติฐาน | Y |
| Change Log | ประวัติการเปลี่ยนแปลง | Y |

## 5. หลักการออกแบบ (ทั้ง File และ API)

| หลักการ | คำอธิบาย |
|---------|---------|
| Contract-first | ออกแบบ interface contract (request/response หรือ file spec) ก่อน implement |
| Idempotent | การเรียกซ้ำหรือ process ซ้ำต้องได้ผลลัพธ์เดียวกัน |
| Error handling | ต้องจัดการ timeout, retry, circuit breaker |
| Logging | ทุก request/response หรือ file processing ต้อง log พร้อม correlation ID |
| Security | ใช้ authentication ทุก interface (API Key, OAuth, SSH Key, Certificate) |
| Versioning | API ต้องมี version, File ต้องมี naming convention ที่ชัดเจน |

## 6. หลักการเฉพาะ Interface File

| หลักการ | คำอธิบาย |
|---------|---------|
| Checksum | ทุกไฟล์ต้องมี checksum file แนบคู่ |
| Archive | ไฟล์ที่ process แล้วต้องย้ายไป archive folder |
| Error Path | ไฟล์ที่ผิดพลาดต้องย้ายไป error folder |
| Encoding | ระบุ encoding ชัดเจน (UTF-8 สำหรับภาษาไทย) |
| Naming Convention | ระบุ pattern ชัดเจน (PREFIX_YYYYMMDD_HHMMSS.ext) |
| Processing Log | สร้าง summary log หลัง process (records processed, skipped, errors) |
| Staging Table | Inbound file ต้อง insert เข้า staging table ก่อน process |
| Reject Table | Record ที่ไม่ผ่าน validation ต้องเก็บใน reject table พร้อมเหตุผล |

## 7. Interface File Processing Pattern

### 7.1 Inbound Pattern (รับไฟล์เข้า)

```text
┌─────────────────────────────────────┐
│  1. Check inbound folder            │
│     - File exists? → Read file      │
│     - Not exists? → Do nothing      │
├─────────────────────────────────────┤
│  2. Insert into staging table       │
│     - Parse file → Insert records   │
│     - Failed? → End                 │
├─────────────────────────────────────┤
│  3. Read from staging table         │
│     - Filter by file name + date    │
│     - No data? → End               │
├─────────────────────────────────────┤
│  4. Process each record             │
│     - Branch by Case ID / Type      │
│     - Lookup existing data          │
│     - Create / Update / Skip        │
│     - Call external API (ถ้ามี)     │
│     - Log result (history/reject)   │
├─────────────────────────────────────┤
│  5. Next record → Loop back to 4    │
│     - All done? → End               │
└─────────────────────────────────────┘
```

### 7.2 Outbound Pattern (ส่งไฟล์ออก)

```text
┌─────────────────────────────────────┐
│  1. Query source data               │
│     - Apply filters + conditions    │
│     - No data? → End                │
├─────────────────────────────────────┤
│  2. Loop each record                │
│     - Call external API (ถ้ามี)     │
│     - Get additional data           │
│     - Handle API success/failure    │
├─────────────────────────────────────┤
│  3. Create interface file           │
│     - Map fields to output format   │
│     - With/without enriched data    │
├─────────────────────────────────────┤
│  4. Transfer file                   │
│     - Send to destination           │
│     - Archive source file           │
├─────────────────────────────────────┤
│  5. Exception handling              │
│     - Failed? → Do not write file   │
│     - Log error → End               │
└─────────────────────────────────────┘
```

## 8. Staging & Reject Table Pattern

### 8.1 Staging Table

| Component | คำอธิบาย |
|-----------|---------|
| Primary Key | Auto-generated running number (Record_No) |
| File Metadata | File_Name — ชื่อไฟล์ที่ import |
| Business Fields | ข้อมูลจากไฟล์ทั้งหมด (Case ID, VIN, Name, Phone, etc.) |
| Reject Flag | Is_Reject — Y/N flag สำหรับ record ที่ถูก reject |
| Naming | tmp_[interface_name] (เช่น tmp_connected_car_interface) |

### 8.2 Reject Table

| Component | คำอธิบาย |
|-----------|---------|
| Primary Key | Auto-generated running number (Record_No) |
| Business Fields | Mirror staging table fields |
| Reject Metadata | STG_RECORD_NO (link กลับ staging), STG_SQL_STAGE (reject reason), STG_INSERT_BY_PGM (reject step) |
| Naming | rej_[interface_name] (เช่น rej_connected_car_interface) |

## 9. Case ID / Branching Pattern

Interface ที่มีหลาย business scenario ใช้ Case ID branching:

| Pattern | คำอธิบาย | ตัวอย่าง |
|---------|---------|---------|
| Activation (Case 1) | สร้างข้อมูลใหม่ | Lookup → Not found → Create customer + entity |
| Key Info Change (Case 2) | แก้ไขข้อมูลที่มีอยู่ | Lookup → Found → Compare → Update fields |
| Cancellation (Case 3) | ยกเลิก/ปิดข้อมูล | Lookup → Found → Update status = Inactive |

แต่ละ case ต้องมี:
- Lookup logic (ค้นหาข้อมูลที่มีอยู่)
- Found/Not found branching
- Entity CRUD operations (Create/Update per entity)
- API integration (ถ้าต้อง sync กับระบบภายนอก)
- Error/Reject handling per record

## 10. API Integration within File Process

File interface ที่ต้องเรียก API ระหว่าง process:

| Step | คำอธิบาย |
|------|---------|
| 1. Register Session | เรียก API เพื่อขอ session ID (API0010 pattern) |
| 2. Send/Receive Data | เรียก API เพื่อส่ง/รับข้อมูล (API0020/API0080/API0120 pattern) |
| 3. Handle Response | ตรวจสอบ success/Error_Flag → log result |
| 4. Error Logging | เก็บ request/response parameters ใน history table |

### API Response Handling Pattern

| Response | Action |
|----------|--------|
| success = True | Log success → Continue to next record |
| success = False, Error_Flag = 1 (Validation) | Log error in history + reject table → Next record |
| success = False, Error_Flag = 2 (System) | Log error in history + reject table → Next record (or End) |

## 11. Conditional Output Pattern (Outbound)

File outbound ที่มีข้อมูลจาก API อาจสร้างไฟล์ 2 รูปแบบ:

| Format | เมื่อใด | ตัวอย่าง |
|--------|---------|---------|
| Without enriched data | API call ล้มเหลว หรือไม่พบข้อมูล | Consent fields = blank |
| With enriched data | API call สำเร็จ | Consent fields = Y/N mapped from API response |

### Value Mapping Pattern

```text
If [API_Value] = 1 Then Set 'Y'
Else If [API_Value] = '-1' Then Set 'N'
Else Set blank
```

## 12. Process Flow Diagram Pattern

Interface File ควรมี flowchart อย่างน้อย 1 รูปแบบ (แนะนำ 2):

| รูปแบบ | Layout | เมื่อใช้ |
|--------|--------|---------|
| System Overview | `flowchart LR` | แสดงระบบที่เกี่ยวข้อง + ทิศทางข้อมูล |
| Outline Process | `flowchart TD` | แสดง decision tree + branching logic ทั้งหมด |

### Mermaid Styling Convention

- ระบบปัจจุบัน (process): `fill:#EAD7A4,stroke:#333`
- ระบบภายนอก (source/dest): `fill:#E0E0E0,stroke:#333`
- Scheduler/trigger: `stroke-dasharray: 5 5`
- Data store: `[( )]` cylinder shape
- Decision: `{ }` diamond shape
- Start/End: `([ ])` stadium shape
- Merge point: `(( ))` circle shape

## 13. หลักการเฉพาะ Interface API

| หลักการ | คำอธิบาย |
|---------|---------|
| RESTful | ใช้ HTTP methods ตาม REST convention (GET/POST/PUT/DELETE) |
| Status Codes | ใช้ HTTP status codes ที่เหมาะสม (200, 400, 401, 404, 500) |
| Response Format | ใช้ standard response format (status, code, message, data, errors, timestamp) |
| Rate Limiting | กำหนด rate limit ต่อ client |
| Correlation ID | ทุก request ต้องมี X-Correlation-ID สำหรับ tracing |
| Non-blocking | API ที่เป็น advisory ไม่ควร block business process |
| Auto-refresh Token | Implement auto-refresh สำหรับ OAuth token |

## 14. Data Mapping Standard

ทุก interface ต้องมี data mapping table ที่ระบุ:

| Column | คำอธิบาย |
|--------|---------|
| Source Field | ชื่อ field ต้นทาง |
| Dest Field | ชื่อ field ปลายทาง |
| Data Type | ชนิดข้อมูล + ความยาว |
| Required | Y / N / Conditional |
| Default | ค่า default เมื่อ source ว่าง |
| Transformation | กฎการแปลงข้อมูล (Map, Format, Calculate) |

## 15. Error Handling Pattern

### 15.1 Interface File

| Error Level | คำอธิบาย | ตัวอย่าง |
|-------------|---------|---------|
| Connection | เชื่อมต่อ SFTP / folder ไม่ได้ | Retry + alert admin |
| File | ไฟล์ไม่พบ / format ผิด / checksum ไม่ตรง | Reject file + alert |
| Record | ข้อมูลบาง record ไม่ผ่าน validation | Skip record + log to reject table |
| API | API call ล้มเหลวระหว่าง process | Log to history + reject → next record |
| Exception | Unhandled error | Do not write file → End process |

### 15.2 Interface API

| Error Level | คำอธิบาย | ตัวอย่าง |
|-------------|---------|---------|
| Network | Connection timeout / DNS failure | Retry with backoff |
| Authentication | Token expired / invalid credentials | Auto-refresh token |
| Validation | Request body ไม่ถูกต้อง | Return 400 + error details |
| Business | Business rule violation | Return 422 + error details |
| Server | Internal server error | Retry + alert admin |

## 16. Best Practice

### ทั่วไป
- ทุก interface ต้องมี monitoring และ alerting
- ห้ามฝัง external system logic ใน core business logic
- ทุก interface ต้อง log request/response หรือ file processing
- ใช้ mermaid flowchart อธิบาย process flow

### Interface File
- ใช้ checksum file แนบคู่ทุกครั้ง
- Archive ไฟล์ที่ process แล้ว ไม่ลบทิ้ง
- ระบุ encoding ชัดเจน (UTF-8 สำหรับภาษาไทย)
- สร้าง processing summary log ทุกครั้ง
- Inbound: ต้องมี staging table + reject table
- Outbound: ต้องระบุ SQL conditions สำหรับดึงข้อมูล
- Complex interface: ใช้ Case ID branching + entity-level CRUD
- API integration: log ทุก API request/response parameter ใน history table
- ระบุ database table definitions (staging, reject) พร้อม column types

### Interface API
- ใช้ API Gateway เป็น single entry point
- ใช้ retry with exponential backoff สำหรับ transient errors
- API ที่เป็น advisory ไม่ควร block business process เมื่อ unavailable
- ใช้ standard response format ทุก API
- ระบุ example request/response ในเอกสาร
