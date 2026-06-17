# Integration Architecture

## 1. ภาพรวม

Integration Architecture คือการออกแบบรูปแบบการเชื่อมต่อและแลกเปลี่ยนข้อมูลระหว่างระบบ ทั้งภายในและภายนอกองค์กร โดยยึด Azure Integration Services เป็นแกนหลัก

## 2. Integration Pattern

### 2.1 ภาพรวม Pattern ที่รองรับ

```text
                    ┌───────────────────────────────┐
                    │   Azure API Management        │
                    │   (API Gateway / Facade)       │
                    └──────────┬────────────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         │                     │                     │
    ┌────▼────┐          ┌────▼────┐          ┌────▼────┐
    │ Sync    │          │ Async   │          │ Batch   │
    │ Request │          │ Event   │          │ File    │
    │ Response│          │ Driven  │          │ Transfer│
    │ (REST)  │          │ (Msg)   │          │ (SFTP)  │
    └─────────┘          └─────────┘          └─────────┘
    Production           Internal              HRIS
    Planning             Events                Sync
```

### 2.2 Synchronous (Request-Response)

**เมื่อใดควรใช้:** ต้องการผลลัพธ์ทันที เช่น เช็คตารางงาน, ค้นหาข้อมูล

| เกณฑ์ | รายละเอียด |
|-------|-----------|
| Protocol | REST API over HTTPS (TLS 1.2+) |
| Format | JSON (UTF-8) |
| Authentication | API Key, OAuth 2.0 (Microsoft Entra ID), Certificate |
| Timeout | Connection 5 วินาที, Read 10 วินาที |
| Retry | 2 ครั้ง (exponential backoff เริ่มที่ 2 วินาที) |
| Rate Limit | กำหนดตาม SLA ของ external system |
| Circuit Breaker | ใช้ Polly library (.NET) กำหนด threshold 5 failures → open 30 วินาที |

**Best Practice:**
- ใช้ Azure API Management เป็น gateway สำหรับทุก external API call
- กำหนด retry policy, caching policy, rate limit policy ที่ APIM level
- ใช้ Polly (.NET) สำหรับ resilience pattern: Retry, Circuit Breaker, Timeout, Bulkhead
- Log ทุก request/response ไปยัง Azure Application Insights
- ห้ามเรียก external API จาก database layer (Stored Procedure)

### 2.3 Asynchronous (Event-Driven)

**เมื่อใดควรใช้:** ไม่ต้องการผลลัพธ์ทันที เช่น ส่ง email, อัปเดต audit log, notify ระบบอื่น

| เกณฑ์ | รายละเอียด |
|-------|-----------|
| Message Broker | Azure Service Bus (Queue / Topic) |
| Protocol | AMQP 1.0 over TLS |
| Message Format | JSON (UTF-8) พร้อม message header ระบุ event type |
| Delivery | At-least-once delivery (consumer ต้อง idempotent) |
| Dead Letter | เปิด Dead Letter Queue สำหรับ message ที่ process ไม่สำเร็จ |
| Max Retry | 10 ครั้ง ก่อนส่ง dead letter |
| Message TTL | 24 ชั่วโมง |

**Azure Service Bus Pattern:**

```text
┌──────────┐    ┌─────────────────────┐    ┌──────────┐
│ Producer │───▶│ Service Bus Topic   │───▶│ Consumer │
│ (Leave   │    │ "leave-events"      │    │ (Email   │
│  Service)│    │  ├─ Subscription:   │    │  Service)│
└──────────┘    │  │  "email-notify"  │    └──────────┘
                │  ├─ Subscription:   │    ┌──────────┐
                │  │  "audit-log"     │───▶│ Audit    │
                │  └─ Subscription:   │    │ Service  │
                │     "hris-sync"     │    └──────────┘
                └─────────────────────┘    ┌──────────┐
                                      ───▶│ HRIS     │
                                           │ Adapter  │
                                           └──────────┘
```

**Best Practice:**
- ใช้ Topic + Subscription สำหรับ 1 event → หลาย consumers
- ใช้ Queue สำหรับ 1 event → 1 consumer (competing consumers pattern)
- Message body ต้องมี `correlationId` สำหรับ traceability
- Consumer ต้อง idempotent (process ซ้ำได้โดยไม่เกิด side effect)
- Monitor dead letter queue ด้วย Azure Monitor Alert

### 2.4 Batch (File Transfer)

**เมื่อใดควรใช้:** ส่งข้อมูลจำนวนมากตามรอบเวลา เช่น sync master data, ส่งรายงาน

| เกณฑ์ | รายละเอียด |
|-------|-----------|
| Protocol | SFTP (SSH File Transfer Protocol) |
| Format | CSV (UTF-8 with BOM) หรือ JSON Lines |
| Authentication | SSH Key Pair (RSA 4096-bit) |
| Integrity | SHA-256 checksum file แนบคู่กับ data file |
| Acknowledgment | Receiver สร้าง ACK file ภายใน 1 ชั่วโมง |
| File Naming | `{SYSTEM}_{ENTITY}_{YYYYMMDD}_{HHMMSS}.csv` |
| Encryption | ไฟล์ encrypt ด้วย PGP หรือ AES-256 ก่อนส่ง (สำหรับ sensitive data) |

**Best Practice:**
- ใช้ Azure Data Factory สำหรับ orchestrate batch pipeline
- มี monitoring dashboard สำหรับ batch job status
- ทุก batch job ต้อง log: start time, end time, record count, success/failure
- ใช้ staging table ใน database สำหรับรับข้อมูล batch ก่อน merge เข้า production table
- มี reconciliation report เปรียบเทียบ record count ระหว่าง source และ destination

## 3. API Management

### 3.1 Azure API Management (APIM)

```text
┌─────────────┐    ┌─────────────────────────┐    ┌─────────────┐
│  External   │    │  Azure API Management   │    │  Backend    │
│  Consumer   │───▶│  ├── Authentication     │───▶│  API        │
│  (Mobile,   │    │  ├── Rate Limiting      │    │  (ASP.NET   │
│   Partner)  │    │  ├── Caching            │    │   Core)     │
└─────────────┘    │  ├── Transformation     │    └─────────────┘
                   │  ├── Logging            │
┌─────────────┐    │  └── Circuit Breaker    │    ┌─────────────┐
│  Internal   │───▶│                         │───▶│  External   │
│  Frontend   │    │  Products:              │    │  API        │
│  (Blazor/   │    │  ├── Internal (no key)  │    │  (HRIS,     │
│   React)    │    │  ├── Partner (API key)  │    │   Prod Plan)│
└─────────────┘    │  └── Public (OAuth)     │    └─────────────┘
                   └─────────────────────────┘
```

**Best Practice APIM:**
- ทุก API ต้องผ่าน APIM ไม่เปิดให้เรียก backend ตรง
- แยก Product: Internal (ไม่ต้อง key), Partner (API key), Public (OAuth 2.0)
- กำหนด rate limit: Internal 1000 req/min, Partner 100 req/min, Public 50 req/min
- เปิด response caching สำหรับ GET endpoint ที่ข้อมูลไม่เปลี่ยนบ่อย
- ใช้ Named Values สำหรับ configuration (ไม่ hardcode ใน policy)
- ใช้ OpenAPI spec เป็น single source of truth สำหรับ API definition

### 3.2 API Versioning Strategy

| วิธี | รูปแบบ | เมื่อใดควรใช้ |
|------|--------|-------------|
| URL Path | `/api/v1/leave-requests` | แนะนำเป็นค่า default — ชัดเจน เข้าใจง่าย |
| Header | `api-version: 2.0` | เมื่อไม่ต้องการเปลี่ยน URL |
| Query String | `/api/leave-requests?api-version=1.0` | ใช้ร่วมกับ APIM |

**Best Practice:**
- ใช้ URL Path versioning เป็นค่า default ขององค์กร
- Version เก่าต้อง support อย่างน้อย 12 เดือนหลัง deprecate
- ทุก breaking change ต้องขึ้น major version ใหม่
- Non-breaking change (เพิ่ม field, เพิ่ม endpoint) ไม่ต้องขึ้น version

## 4. Event Schema Standard

### 4.1 CloudEvents Format (แนะนำ)

```json
{
  "specversion": "1.0",
  "type": "com.company.leave.request.submitted",
  "source": "/leave-service/api/v1/leave-requests",
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "time": "2026-04-16T09:15:00Z",
  "datacontenttype": "application/json",
  "data": {
    "leaveRequestId": "LR-2026-00123",
    "employeeId": "EMP000245",
    "leaveType": "ANNUAL",
    "startDate": "2026-04-21",
    "endDate": "2026-04-22",
    "status": "PENDING_MANAGER"
  }
}
```

**Best Practice:**
- ใช้ CloudEvents specification เป็นมาตรฐาน event format ขององค์กร
- `type` ใช้ reverse domain notation: `com.company.{domain}.{entity}.{action}`
- `id` ใช้ UUID v4 ไม่ซ้ำกัน
- `time` ใช้ ISO 8601 UTC เสมอ
- `data` เก็บเฉพาะข้อมูลที่จำเป็น ไม่ส่ง entire entity

## 5. Error Handling & Resilience

### 5.1 Resilience Pattern (ใช้ Polly .NET)

| Pattern | เมื่อใดควรใช้ | การตั้งค่า |
|---------|-------------|-----------|
| Retry | Transient failure (network timeout, 503) | 3 ครั้ง, exponential backoff 2s/4s/8s |
| Circuit Breaker | ป้องกัน cascading failure | Open หลัง fail 5 ครั้ง, half-open หลัง 30 วินาที |
| Timeout | ป้องกัน request ค้าง | Optimistic timeout 10 วินาที |
| Bulkhead | จำกัด concurrent requests | Max 25 concurrent calls ต่อ external service |
| Fallback | ให้ค่า default เมื่อ external service ไม่พร้อม | Return cached data หรือ default response |

### 5.2 Error Classification

| HTTP Status | ประเภท | Retry ได้หรือไม่ | การจัดการ |
|-------------|--------|----------------|----------|
| 400 | Client Error | ไม่ได้ | แจ้ง caller ให้แก้ไข request |
| 401/403 | Auth Error | ไม่ได้ | Refresh token แล้วลองใหม่ 1 ครั้ง |
| 404 | Not Found | ไม่ได้ | แจ้ง caller ว่าไม่พบข้อมูล |
| 408/429 | Timeout/Rate Limit | ได้ | Retry ด้วย backoff |
| 500 | Server Error | ได้ | Retry ด้วย backoff |
| 502/503/504 | Infrastructure Error | ได้ | Retry ด้วย backoff + circuit breaker |

## 6. Monitoring & Observability

### 6.1 Integration Monitoring Stack

| เครื่องมือ | หน้าที่ |
|-----------|--------|
| Azure Application Insights | Distributed tracing, request logging, dependency tracking |
| Azure Monitor | Metrics, alerts, dashboards |
| Azure Log Analytics | Centralized log query (KQL) |
| Azure Service Bus Metrics | Queue depth, dead letter count, message throughput |
| Azure Data Factory Monitor | Batch pipeline status, run history |

### 6.2 Key Metrics ที่ต้อง Monitor

| Metric | Threshold | Alert |
|--------|-----------|-------|
| API response time (P95) | > 5 วินาที | Warning |
| API error rate | > 5% | Critical |
| Service Bus dead letter count | > 0 | Warning |
| Service Bus queue depth | > 1000 | Warning |
| Batch job failure | > 0 | Critical |
| Circuit breaker open | > 0 | Critical |

## 7. Checklist สำหรับ Integration Architecture Review

- [ ] เลือก integration pattern ที่เหมาะสม (sync/async/batch)
- [ ] ทุก external API ผ่าน Azure API Management
- [ ] กำหนด timeout, retry, circuit breaker policy
- [ ] ใช้ CloudEvents format สำหรับ event
- [ ] Message consumer เป็น idempotent
- [ ] เปิด Dead Letter Queue สำหรับ async messaging
- [ ] มี monitoring และ alerting ครบ
- [ ] API versioning strategy ชัดเจน
- [ ] Error handling classification ครบทุก HTTP status
- [ ] Batch job มี logging, reconciliation, และ retry mechanism
