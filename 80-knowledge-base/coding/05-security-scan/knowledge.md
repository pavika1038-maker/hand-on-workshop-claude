# Security Scan Knowledge — สำหรับใช้กับ AI

## วัตถุประสงค์

ไฟล์นี้ใช้เป็น context สำหรับให้ AI ช่วยวิเคราะห์ผล security scan, สร้าง fix recommendation และ review code ในมุม security

---

## Tool ที่ใช้ในทีม

| Tool | ใช้สำหรับ |
|------|----------|
| OWASP ZAP | Web application pentest — scan automated และ manual |
| Burp Suite | Web application pentest — manual testing เชิงลึก |

---

## แนวทางการ Review ผล ZAP Scan

เมื่อได้รับ ZAP report ให้ AI ทำตามขั้นตอน:

1. **จัดกลุ่ม alert** ตาม Risk Level (High → Medium → Low → Informational)
2. **Map กับ security checklist** ว่า alert นั้นตรงกับข้อไหนใน `security-checklist.md`
3. **ระบุ fix recommendation** ตาม security measure ที่กำหนดใน checklist
4. **ระบุ phase** ที่ควร fix (Development / Before releasing)
5. **สร้าง fix log** ในรูปแบบ:

```markdown
| Alert | Risk | Phase | Fix Required | Status |
|-------|------|-------|-------------|--------|
| SQL Injection | High | Development | ใช้ bind parameter | Open |
```

---

## OWASP ZAP Alert → Security Measure Mapping

| ZAP Alert | Fix |
|-----------|-----|
| SQL Injection | ใช้ bind parameter, whitelist validation, escape special char |
| Cross Site Scripting (XSS) | Escape HTML output, ใช้ CSP header |
| CRLF Injection | Escape %0D%0A ใน HTTP header และ email header |
| Cookie No HttpOnly Flag | เพิ่ม HttpOnly flag ใน cookie config |
| Cookie Without Secure Flag | เพิ่ม Secure flag ใน cookie config |
| Cookie without SameSite Attribute | เพิ่ม SameSite=Strict หรือ Lax |
| X-Content-Type-Options Header Missing | เพิ่ม header: X-Content-Type-Options: nosniff |
| Content Security Policy (CSP) | กำหนด CSP header ใน response |
| Cross-Domain Misconfiguration | ตรวจ CORS ให้ whitelist เฉพาะ domain ที่อนุญาต |
| Session Management Response Identified | ตรวจว่า session ID ไม่ sequential และมีความยาวพอ |
| Parameter Tampering | Validate ทุก parameter ฝั่ง server |
| External Redirect | Whitelist URL ที่อนุญาต redirect |
| PII Disclosure | ไม่ส่ง personal data ใน response ที่ไม่จำเป็น |
| Remote File Inclusion | ปิด allow_url_include, validate file type |
| Server Side Code Injection | ตรวจ file upload ด้วย Magic Bytes ไม่ใช่แค่ extension |

---

## หลักการ Fix ก่อน Go-live

- **High risk ทั้งหมด** ต้อง fix และ retest ก่อน release
- **Medium risk** ต้อง fix หรือมี documented mitigation plan
- **Low risk** บันทึกไว้ใน risk register และ fix ใน sprint ถัดไปได้
- **Informational** บันทึกไว้เป็น reference ไม่บังคับ fix
