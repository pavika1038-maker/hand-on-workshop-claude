# Interface Functions — Output Template (API)

> สำหรับ Interface แบบ API (REST, SOAP, GraphQL, gRPC)

---

```markdown
---
function_id: "IFA-[NNN]"
function_name: "[API Name]"
category: "Interface API"
version: "1.0"
status: "Draft"
author: ""
last_updated: ""
---

# IFA-[NNN] — [API Name]

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | IFA-[NNN] |
| API Name | [ชื่อ API] |
| Category | Interface API |
| Direction | [Inbound / Outbound / Bidirectional] |
| Pattern | [REST API / SOAP / GraphQL / gRPC] |
| Description | [อธิบาย API] |
| Source System | [ระบบที่เรียก] |
| Destination System | [ระบบที่ถูกเรียก] |
| Related Requirement IDs | [SIR-xxx, IF-xxx] |

## 2. Business Purpose

[ทำไม API นี้ถึงมีอยู่]

## 3. API Description

| รายการ | รายละเอียด |
| --- | --- |
| Protocol | [HTTPS / HTTP] |
| Method | [GET / POST / PUT / PATCH / DELETE] |
| Base URL | [https://api.example.com/v1] |
| Endpoint | [/resource/{id}] |
| Content-Type | [application/json / application/xml / multipart/form-data] |
| Authentication | [API Key / OAuth 2.0 / Bearer Token / Basic Auth / Certificate] |
| Rate Limit | [requests per second/minute] |
| Timeout | [Connection timeout / Read timeout] |
| Retry Policy | [จำนวนครั้ง, backoff strategy] |
| API Version | [v1 / v2] |

## 4. Request Specification

### 4.1 Headers

| Header | Value | Required | Description |
| --- | --- | --- | --- |
| Content-Type | application/json | Y | Request body format |
| Authorization | Bearer {token} | Y | Authentication token |
| X-Correlation-ID | {uuid} | Y | Tracking ID |
| X-API-Key | {api_key} | Conditional | API Key (ถ้าใช้ API Key auth) |

### 4.2 Path Parameters

| Parameter | Data Type | Required | Description |
| --- | --- | --- | --- |
| | | | |

### 4.3 Query Parameters

| Parameter | Data Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| | | | | |

### 4.4 Request Body

```json
{
  "field_1": "string",
  "field_2": 0,
  "field_3": true,
  "nested_object": {
    "sub_field_1": "string"
  },
  "array_field": [
    {
      "item_field_1": "string"
    }
  ]
}
```

### 4.5 Request Field Mapping

| No | Field | Data Type | Required | Validation | Description |
| :---: | --- | --- | --- | --- | --- |
| 1 | field_1 | String(50) | Y | Not empty | [คำอธิบาย] |
| 2 | field_2 | Integer | N | >= 0 | [คำอธิบาย] |
| 3 | field_3 | Boolean | N | — | [คำอธิบาย] |

## 5. Response Specification

### 5.1 Success Response (HTTP 200)

```json
{
  "status": "success",
  "code": "200",
  "message": "Operation completed successfully",
  "data": {
    "field_1": "string",
    "field_2": 0
  },
  "timestamp": "2026-04-17T10:30:00+07:00"
}
```

### 5.2 Error Response

```json
{
  "status": "error",
  "code": "ERR_CODE",
  "message": "Error description",
  "errors": [
    {
      "field": "field_name",
      "message": "Validation error detail"
    }
  ],
  "timestamp": "2026-04-17T10:30:00+07:00"
}
```

### 5.3 Response Field Mapping

| No | Field | Data Type | Description |
| :---: | --- | --- | --- |
| 1 | status | String | success / error |
| 2 | code | String | HTTP status code หรือ error code |
| 3 | message | String | ข้อความอธิบาย |
| 4 | data | Object | ข้อมูลผลลัพธ์ (เฉพาะ success) |
| 5 | errors | Array | รายละเอียด error (เฉพาะ error) |
| 6 | timestamp | DateTime | เวลาที่ตอบกลับ (ISO 8601) |

## 6. HTTP Status Codes

| Status Code | Meaning | เมื่อใดที่ใช้ |
| --- | --- | --- |
| 200 | OK | สำเร็จ |
| 201 | Created | สร้างข้อมูลสำเร็จ |
| 400 | Bad Request | Request body ไม่ถูกต้อง / validation fail |
| 401 | Unauthorized | Authentication ล้มเหลว |
| 403 | Forbidden | ไม่มีสิทธิ์เข้าถึง |
| 404 | Not Found | ไม่พบข้อมูล |
| 409 | Conflict | ข้อมูลซ้ำ / state conflict |
| 422 | Unprocessable Entity | Business rule validation fail |
| 429 | Too Many Requests | เกิน rate limit |
| 500 | Internal Server Error | ระบบผิดพลาด |
| 503 | Service Unavailable | ระบบไม่พร้อมให้บริการ |

## 7. Trigger / Timing

| Trigger | Description | Timing |
| --- | --- | --- |
| [Realtime / Scheduled / Event-driven] | [คำอธิบาย] | [เวลา/เงื่อนไข] |

## 8. Data Mapping (Source ↔ Destination)

| No | Source Field | Dest Field | Data Type | Required | Transformation |
| :---: | --- | --- | --- | --- | --- |
| 1 | | | | | |

## 9. Error Handling

| Error Case | HTTP Status | System Behavior | Recovery |
| --- | --- | --- | --- |
| Invalid request | 400 | Return validation errors | Client แก้ไข request |
| Auth failure | 401 | Return unauthorized | Client ตรวจสอบ credentials |
| Not found | 404 | Return not found | Client ตรวจสอบ ID |
| Rate limit exceeded | 429 | Return retry-after header | Client รอแล้วลองใหม่ |
| Server error | 500 | Log error + return generic message | Admin ตรวจสอบ |
| Timeout | — | Retry ตาม policy | Client retry |

## 10. Business Rules

| Rule ID | Business Rule | Impact | Source |
| --- | --- | --- | --- |
| BR-IFA[NNN]-001 | [อธิบาย rule] | [ผลกระทบ] | [Reference] |

## 11. Security

| รายการ | รายละเอียด |
| --- | --- |
| Authentication | [วิธี authentication] |
| Authorization | [Role/Permission ที่ต้องมี] |
| Encryption | [TLS 1.2+ / mTLS] |
| Input Validation | [Sanitization rules] |
| Rate Limiting | [Limit per client/IP] |
| Logging | [ข้อมูลที่ log / ข้อมูลที่ mask] |

## 12. Example Request / Response

### Example 1: [Success Case]

**Request:**
```http
POST /v1/resource HTTP/1.1
Host: api.example.com
Content-Type: application/json
Authorization: Bearer eyJhbGci...

{
  "field_1": "value_1",
  "field_2": 100
}
```

**Response:**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "status": "success",
  "code": "200",
  "message": "Operation completed successfully",
  "data": {
    "id": "RES-001",
    "field_1": "value_1"
  },
  "timestamp": "2026-04-17T10:30:00+07:00"
}
```

### Example 2: [Error Case]

**Request:**
```http
POST /v1/resource HTTP/1.1
Host: api.example.com
Content-Type: application/json
Authorization: Bearer eyJhbGci...

{
  "field_1": ""
}
```

**Response:**
```http
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "status": "error",
  "code": "VALIDATION_ERROR",
  "message": "Request validation failed",
  "errors": [
    {
      "field": "field_1",
      "message": "field_1 is required"
    }
  ],
  "timestamp": "2026-04-17T10:30:00+07:00"
}
```

## 13. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| | | |

## Change Log

| Version | Date | Author | Change Type | Description |
|---------|------|--------|-------------|-------------|
| 1.0 | | | Created | สร้างเอกสารครั้งแรก |
```
