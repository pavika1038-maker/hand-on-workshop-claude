# Security Checklist — Web Application

**Tool:** OWASP ZAP / Burp Suite
**ประเภท:** Web Application Pentest Checklist

---

## Phase 1: Requirements Analysis

| No | Security Risk | Details | Security Measure | Tool/Framework |
|----|--------------|---------|-----------------|----------------|
| 1 | Sniffing and data tampering | ผู้โจมตีดักจับข้อมูลบน network หรือแก้ไขข้อมูลระหว่างส่ง | เข้ารหัสด้วย SSL/TLS | AWS / SSL Labs scan |
| 2 | Incident investigation | ไม่มี log ทำให้ไม่สามารถสืบสวน incident ได้ | เก็บ log ที่จำเป็นสำหรับตรวจจับ security breach | AWS CloudTrail, GuardDuty, CloudWatch |
| 3 | Incident investigation | (ต่อ) | ขอให้ infrastructure เก็บ log เพิ่มเติมด้วย | — |
| 4 | Creation of unauthorized account | Spoof From address เพื่อสร้าง account จำนวนมาก | ไม่ใช้ blank email อย่างเดียว ต้อง verify email | Framework built-in |
| 5 | Session hijacking | เดา session ID ถัดไปจาก ID ก่อนหน้า | ใช้ session function ของ middleware | Framework built-in |
| 6–8 | Session hijacking | Session ID sequential, digits, uniqueness | ตรวจ ZAP alert: Session Management Response | Framework built-in |
| 9–10 | Session hijacking via serialization | Serialize object ใน hidden field ถูก tamper | ไม่ serialize object สำคัญใน hidden field | Framework built-in |
| 11–12 | Session hijacking | Session ID ไม่ random | ใช้ Random seed ที่เปลี่ยนทุกครั้ง | Framework built-in |
| 13 | Weak password | Password เริ่มต้นอ่อนเกินไป | Validate password strength | Custom validation |
| 14–17 | Attack on older product version | ช่องโหว่ใน framework เวอร์ชันเก่า | ใช้ version ล่าสุดที่ patch แล้ว | Framework update |
| 18–19 | Information leakage via ASP | ช่องโหว่ใน ASP service | ตรวจสอบกับ ASP provider ว่า assess แล้ว | ASP provider |

---

## Phase 2: Development and Implementation

| No | Security Risk | Details | Security Measure | OWASP ZAP Alert |
|----|--------------|---------|-----------------|-----------------|
| 20–22 | Malicious request / Parameter tampering | Tamper parameter, URL, cookie, XML | Validate format, length, value ทุก input | Parameter Tampering |
| 23 | Cross-site scripting (XSS) | ขโมย cookie ผ่าน malicious script | Escape special characters ใน HTML output | Cross Site Scripting (XSS) |
| 24 | Execution of unauthorized command | ใส่ command delimiter ใน argument | ใช้ relative path จาก root ห้าม parent path | — |
| 25 | Unauthorized file access | Tamper filename ใน parameter เพื่อเข้าถึงไฟล์อื่น | ห้ามใช้ user input เป็นชื่อไฟล์โดยตรง | — |
| 26–28 | SQL Injection | ใส่ malicious SQL ใน input | ใช้ bind parameter, whitelist, escape special char | SQL Injection |
| 29–33 | Execution of unauthorized command via external call | ใส่ command delimiter ใน external command argument | หลีกเลี่ยงการ call external command โดยตรง | — |
| 34–35 | Unauthorized file access | Tamper filename → เข้าถึง system file | ตรวจสิทธิ์ก่อน access file | — |
| 36 | Unauthorized file upload | Upload executable แฝงเป็น image หรือ upload ไฟล์ขนาดใหญ่ | ตรวจ format, size, virus scan | Remote File Inclusion, Server Side Code Injection, X-Content-Type-Options Header Missing, XSS, Remote Code Execution |
| 37 | Session hijacking via HTTP header | ใส่ newline code ใน HTTP response header | Escape newline (%0D%0A หรือ %0A) | CRLF Injection, XSS, External Redirect |
| 38 | Fraudulent email transmission | ใส่ newline ใน email header เพื่อ spam หรือ phishing | Escape newline ใน email header | CRLF Injection, Parameter Tampering, XSS |
| 39 | API-related security breach / JSON hijacking | Cross-domain API call ด้วย user credential | ป้องกัน outside API call | Cross-Domain Misconfiguration, Cookie without SameSite, PII Disclosure, CSP |

---

## Phase 3: Before Releasing

| No | Security Risk | Details | Security Measure |
|----|--------------|---------|-----------------|
| 40–41 | Information leakage via backup file | ผู้โจมตีเดา backup filename แล้วดึงไฟล์ | ทำ deployment checklist ลบ source/backup จาก production |
| 42–43 | Unauthorized access via test artifacts | Debug option, test account ใน production | ลบ test environment ออกจาก production ทั้งหมด |
| 44 | Unauthorized login / spoofing | ดึง proper noun จาก HTML comment หรือ metadata | ลบ comment และ file metadata ออก |
| 45 | Information leakage via robots.txt | ผู้โจมตีอ่าน directory structure จาก robots.txt | ไม่ระบุ directory ที่ไม่จำเป็นใน robots.txt |
| 46 | Information leakage via sample programs | Sample program เปิด system info | ลบ sample program ออกจาก production |
| 47 | Security measure omission | ช่องโหว่ใน web application | ทำ web application assessment ก่อน release |
| 48 | Security measure omission | ช่องโหว่ใน platform/web server config | ทำ platform assessment ก่อน release |
