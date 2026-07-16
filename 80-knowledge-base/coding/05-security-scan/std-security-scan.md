# Security Scan Standard — Web Application

## ภาพรวม

| รายการ | รายละเอียด |
|--------|-----------|
| ประเภทระบบ | Web Application |
| Tool ที่ใช้ | OWASP ZAP (Pentest), Burp Suite (Pentest) |
| มาตรฐานอ้างอิง | OWASP Top 10 |
| Severity Level | High / Medium / Low / Informational |

## Severity และเกณฑ์การ Fix

| Severity | เกณฑ์ | ต้อง Fix ก่อน Go-live |
|----------|--------|----------------------|
| High | ระบบถูก exploit ได้โดยตรง เช่น SQL Injection, XSS, RCE | ✅ บังคับ |
| Medium | มีความเสี่ยง แต่ต้องมีเงื่อนไขเพิ่ม | ✅ บังคับ (หรือมี mitigation plan) |
| Low | ความเสี่ยงต่ำ หรือ best practice | ⚠️ ควร fix |
| Informational | ข้อมูลทั่วไป ไม่ใช่ vulnerability | ❌ ไม่บังคับ |

## Phase ที่ต้อง Scan

| Phase | เป้าหมาย |
|-------|---------|
| Requirements analysis | ตรวจ SSL/TLS, logging, account creation |
| Development | ตรวจ input validation, injection, session management, file upload |
| Before releasing | ตรวจ information leakage, unauthorized access, backup files |

---

## OWASP ZAP — รายการที่ Scan ได้

รายการต่อไปนี้เป็น security risk ที่ OWASP ZAP สามารถตรวจพบได้โดยตรง:

| ZAP Alert | Risk Category | หมายเหตุ |
|-----------|--------------|---------|
| Session ID in URL Rewrite | Session Hijacking | ห้ามส่ง Session ID ผ่าน URL |
| Cookie No HttpOnly Flag | Session Hijacking | Cookie ต้องมี HttpOnly flag |
| Cookie Without Secure Flag | Session Hijacking | Cookie ต้องมี Secure flag |
| Session Management Response Identified | Session Hijacking | ตรวจ session ที่ sequential |
| Java Serialization Object | Session Hijacking | ห้าม serialize object สำคัญใน hidden field |
| Viewstate / Insecure JSF ViewState | Session Hijacking | ตรวจ ViewState ที่ไม่ได้ encrypt |
| Parameter Tampering | Malicious Request | ตรวจ parameter ที่ถูก tamper |
| SQL Injection | SQL Injection | ใช้ bind parameter และ whitelist validation |
| Cross Site Scripting (XSS) | XSS | Escape output ทุก HTML field |
| CRLF Injection | HTTP Header Injection | Escape newline character ใน HTTP header |
| External Redirect | Redirect Attack | ตรวจ redirect URL ที่มาจาก user input |
| Cross-Domain Misconfiguration | API Security | ตรวจ CORS configuration |
| Cookie without SameSite Attribute | API Security | Cookie ต้องมี SameSite attribute |
| PII Disclosure | Information Leakage | ตรวจ personal data ที่ expose ใน response |
| Content Security Policy (CSP) | XSS Prevention | ต้องมี CSP header |
| Remote File Inclusion | File Upload | ตรวจไฟล์ที่อัปโหลดจาก URL ภายนอก |
| Server Side Code Injection | File Upload | ตรวจ script ที่แอบฝังในไฟล์อัปโหลด |
| X-Content-Type-Options Header Missing | File Upload | ต้องมี X-Content-Type-Options: nosniff |
| Remote Code Execution | File Upload | ตรวจ executable ที่อัปโหลด |
