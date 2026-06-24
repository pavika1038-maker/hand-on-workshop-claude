# SIT Execution Report — Leave Request and Approval

**Module:** Leave Request and Approval  
**วันที่รัน:** 2026-06-16  
**Executed by:** leave-request-sit-agent  
**Base URL:** `http://localhost:5173`  
**Status รวม:** 🟡 READY TO EXECUTE (Antigravity MCP ยังไม่ได้ติดตั้ง)

---

## ⚠️ Execution Status

| # | Item | สถานะ | รายละเอียด |
|---|------|--------|-----------|
| B-001 | **Antigravity Browser Integration** | 🔴 BLOCKED | ToolSearch ไม่พบ browser tools — ต้องติดตั้ง MCP server ก่อน |
| ✅ | Base URL | พร้อม | `http://localhost:5173` — verify แล้ว HTTP 200 |
| ✅ | Login credentials | พร้อม | อ่านจาก LoginPage.tsx + AuthContext.tsx — ใช้ email/password |
| ✅ | Form element names | พร้อม | อ่านจาก LeaveRequestListPage.tsx + ApprovalListPage.tsx |
| ✅ | Navigation structure | พร้อม | อ่านจาก MainLayout.tsx — sidebar + logout button |

> **ขั้นตอนต่อไป:** ติดตั้ง Antigravity Browser Integration MCP server → restart Claude Code session → re-run command ด้วย Base URL `http://localhost:5173`

---

## Pre-flight Checklist (ตรวจก่อน execute)

### Environment
- [ ] Antigravity Browser Integration MCP server ติดตั้งแล้วและ active
- [ ] ToolSearch พบ browser tools: `navigate`, `click`, `fill`, `screenshot`, `assert`
- [x] Base URL ระบุถูกต้อง: `http://localhost:5173`
- [x] Application ขึ้นที่ Base URL และ respond ปกติ (HTTP 200 verified)

### Credentials (ยืนยันจาก LoginPage.tsx)
| Role | Email | Password |
|------|-------|----------|
| Employee (EMP001) | `somchai@abc.com` | `1234` |
| Manager (EMP002) | `wipa@abc.com` | `1234` |
| HR (EMP003) | `nanta@abc.com` | `1234` |

### Test Data & Seed
- [ ] SQLite DB seed data พร้อม: EMP001 (สมชาย), EMP002 (วิภา), EMP003 (นันทา) ใน Employees
- [ ] EMP001.ManagerId = EMP002, EMP003.ManagerId = EMP002
- [ ] LeaveBalances: EMP001 Annual → entitled=12, used=2, pending=0, remaining=10
- [ ] LeaveTypes: id=1 ลาพักผ่อนประจำปี, id=2 ลาป่วย, id=3 ลากิจส่วนตัว
- [ ] Holiday calendar: 2026-07-01 ถึง 2026-07-03 ไม่ตรงกับวันหยุด

---

## Result Summary

| Scenario ID | Scenario Name | Status | Evidence |
|-------------|---------------|--------|----------|
| SIT-001 | Employee submits leave request successfully | 🟡 PENDING | — |
| SIT-002 | Manager approves leave request | 🟡 PENDING | — |

**Total:** 0 PASS / 0 FAIL / 2 PENDING (รอ Antigravity MCP)

---

## Automation Steps (Updated — ยืนยันจาก Source Code)

### SIT-001: Employee submits leave request successfully

**Identity:** สมชาย ใจดี (EMP001) — Employee → login ด้วย `somchai@abc.com` / `1234`  
**Test Data:** Annual Leave, start=2026-07-01, end=2026-07-03, reason="ท่องเที่ยวประจำปีกับครอบครัว"

| Step | Action | Target | Input / Value | Expected | Actual | Status |
|------|--------|--------|---------------|----------|--------|--------|
| 1 | navigate | `http://localhost:5173/` | — | LoginPage แสดง (ยังไม่มี session) | — | PENDING |
| 2 | screenshot | EVD-001 | — | หน้า Login ก่อน login | — | PENDING |
| 3 | fill | label "Username (Email)" | `somchai@abc.com` | ค่าถูก set | — | PENDING |
| 4 | fill | label "Password" | `1234` | ค่าถูก set | — | PENDING |
| 5 | click | button "เข้าสู่ระบบ" | — | loading state → redirect | — | PENDING |
| 6 | wait | topbar แสดง "👤 สมชาย ใจดี (Employee)" | max 5s | login สำเร็จ | — | PENDING |
| 7 | screenshot | EVD-002 | — | Dashboard หลัง login | — | PENDING |
| 8 | click | sidebar "📋 ยื่นคำร้องขอลา" | — | navigate → `/leave-requests` | — | PENDING |
| 9 | assert | heading | "ยื่นคำร้องขอลาใหม่" | section แสดง | — | PENDING |
| 10 | select | dropdown "ประเภทการลา" | "ลาพักผ่อนประจำปี" | option เลือกได้ | — | PENDING |
| 11 | fill | input "วันเริ่มลา" (type=date) | `2026-07-01` | ค่าถูก set | — | PENDING |
| 12 | fill | input "วันสิ้นสุด" (type=date) | `2026-07-03` | ค่าถูก set | — | PENDING |
| 13 | assert | input "จำนวนวัน" (readonly) | `3 วัน` | auto-calc ถูกต้อง | — | PENDING |
| 14 | fill | textarea "เหตุผลการลา" | `ท่องเที่ยวประจำปีกับครอบครัว` | ค่าถูก set | — | PENDING |
| 15 | screenshot | EVD-003 | — | Form ครบก่อน submit | — | PENDING |
| 16 | click | button "ยื่นคำร้อง" | — | loading "กำลังยื่น..." → POST `/api/v1/leave-requests` | — | PENDING |
| 17 | wait | success message แสดง | max 5s | — | — | PENDING |
| 18 | assert | success box | pattern "ยื่นคำร้องสำเร็จ (LR-2026-…)" | สีเขียว | — | PENDING |
| 19 | screenshot | EVD-004 | — | Success message + request ref | — | PENDING |
| 20 | assert | table "รายการคำร้องของฉัน" | row ใหม่ปรากฏ | status badge "รอการอนุมัติ" | — | PENDING |
| 21 | screenshot | EVD-005 | — | My Requests list showing Pending request | — | PENDING |

**SIT-001 Expected Final State:**
- DB: LeaveRequest ใหม่ Status='Pending', DurationDays=3
- UI: LeaveBalance PendingDays=3, Remaining=7 (ตรวจที่ HomePage หรือ balance widget)

---

### SIT-002: Manager approves leave request

**Dependency:** SIT-001 ต้องผ่านก่อน (มี Pending request ของ EMP001)

**Phase A — Manager Login & Approve**

| Step | Action | Target | Input / Value | Expected | Actual | Status |
|------|--------|--------|---------------|----------|--------|--------|
| 1 | click | button "ออกจากระบบ" (topbar ขวา) | — | logout → navigate `/` → LoginPage | — | PENDING |
| 2 | fill | label "Username (Email)" | `wipa@abc.com` | ค่าถูก set | — | PENDING |
| 3 | fill | label "Password" | `1234` | ค่าถูก set | — | PENDING |
| 4 | click | button "เข้าสู่ระบบ" | — | redirect → Dashboard | — | PENDING |
| 5 | assert | topbar | "👤 วิภา รักงาน (Manager)" | login สำเร็จ | — | PENDING |
| 6 | click | sidebar "✅ อนุมัติ/ปฏิเสธการลา" | — | navigate → `/approvals` | — | PENDING |
| 7 | assert | tab active | "คำร้องขอลา" | default tab เปิดอยู่ | — | PENDING |
| 8 | assert | table row | "สมชาย ใจดี" | ปรากฏใน inbox (DEF-002 check) | — | PENDING |
| 9 | assert | row detail | "ลาพักผ่อนประจำปี", "1 ก.ค. 2569 – 3 ก.ค. 2569", "3 วัน" | ข้อมูลถูกต้อง | — | PENDING |
| 10 | screenshot | EVD-006 | — | Approval Inbox ก่อน approve | — | PENDING |
| 11 | click | button "อนุมัติ" ใน row ของ EMP001 | — | Modal "✅ ยืนยันการอนุมัติ" แสดง | — | PENDING |
| 12 | assert | modal title | "✅ ยืนยันการอนุมัติ" | modal popup ถูกต้อง | — | PENDING |
| 13 | fill | textarea "หมายเหตุ (ถ้ามี)" | (เว้นว่าง — optional) | — | — | PENDING |
| 14 | click | button "อนุมัติ" ใน modal | — | PATCH `/api/v1/approvals/{id}/approve` | — | PENDING |
| 15 | assert | success banner | "อนุมัติสำเร็จ" (สีเขียว ด้านบน) | banner แสดง 3 วินาที | — | PENDING |
| 16 | assert | inbox list | row ของ EMP001 หายไป | list refresh อัตโนมัติ | — | PENDING |
| 17 | screenshot | EVD-007 | — | Inbox หลัง approve (row หาย) | — | PENDING |

**Phase B — Employee ตรวจ Balance (DEF-001 check)**

| Step | Action | Target | Input / Value | Expected | Actual | Status |
|------|--------|--------|---------------|----------|--------|--------|
| 18 | click | button "ออกจากระบบ" | — | logout → LoginPage | — | PENDING |
| 19 | fill | "Username (Email)" | `somchai@abc.com` | — | — | PENDING |
| 20 | fill | "Password" | `1234` | — | — | PENDING |
| 21 | click | "เข้าสู่ระบบ" | — | Dashboard EMP001 | — | PENDING |
| 22 | assert | balance widget / dashboard | Annual UsedDays=5, PendingDays=0, Remaining=7 | **DEF-001 check** — ถ้า UsedDays ยังเท่าเดิม = DEFECT | — | PENDING |
| 23 | screenshot | EVD-008 | — | Balance EMP001 หลัง Approve (DEF-001) | — | PENDING |
| 24 | click | sidebar "📋 ยื่นคำร้องขอลา" | — | `/leave-requests` | — | PENDING |
| 25 | assert | table row | status badge ของ request = "อนุมัติแล้ว" | Approved | — | PENDING |
| 26 | screenshot | EVD-009 | — | My Requests status=Approved | — | PENDING |

**SIT-002 Expected Final State:**
- DB: LeaveRequest Status='Approved', ApprovedBy='EMP002', ApprovalDate NOT NULL
- DB: LeaveBalance EMP001 → UsedDays=5, PendingDays=0, Remaining=7
- UI: balance แสดงถูกต้องทั้ง 3 ค่า

---

## Evidence List

| Evidence ID | Scenario | Description | File | Status |
|-------------|----------|-------------|------|--------|
| EVD-001 | SIT-001 | หน้า Login ก่อน login (สมชาย) | evidence/SIT-001/evd-001-login-page.png | PENDING |
| EVD-002 | SIT-001 | Dashboard หลัง login Employee | evidence/SIT-001/evd-002-dashboard-after-login.png | PENDING |
| EVD-003 | SIT-001 | Form ครบก่อน submit | evidence/SIT-001/evd-003-form-complete.png | PENDING |
| EVD-004 | SIT-001 | Success message + request ref | evidence/SIT-001/evd-004-submit-success.png | PENDING |
| EVD-005 | SIT-001 | My Requests list (status=Pending) | evidence/SIT-001/evd-005-request-list-pending.png | PENDING |
| EVD-006 | SIT-002 | Approval Inbox ก่อน approve | evidence/SIT-002/evd-006-inbox-before-approve.png | PENDING |
| EVD-007 | SIT-002 | Inbox หลัง approve (row หาย) | evidence/SIT-002/evd-007-inbox-after-approve.png | PENDING |
| EVD-008 | SIT-002 | Balance EMP001 หลัง Approve (DEF-001) | evidence/SIT-002/evd-008-balance-after-approve.png | PENDING |
| EVD-009 | SIT-002 | My Requests status=Approved | evidence/SIT-002/evd-009-request-approved.png | PENDING |

---

## Q&A — คำถามที่ตอบแล้วจาก Source Code

| # | คำถาม | คำตอบ (จาก Source) | ไฟล์ |
|---|-------|-------------------|------|
| Q1 | ปุ่ม Approve อยู่บน row โดยตรง หรือต้องเข้า Detail? | **บน row โดยตรง** แต่คลิกแล้วเปิด Modal popup ยืนยันก่อน | ApprovalListPage.tsx:233 |
| Q2 | หลัง Approve list refresh ทันทีหรือไม่? | **ใช่** — `setLeaveRefresh(v => v+1)` trigger reload อัตโนมัติใน modal close | ApprovalListPage.tsx:163-164 |
| Q3 | Login field ชื่ออะไร? | label = **"Username (Email)"**, type=text, ใส่ email address | LoginPage.tsx |
| Q4 | Balance widget แสดง PendingDays แยกหรือไม่? | ต้องตรวจ HomePage.tsx ระหว่าง execute | — |
| Q5 | Logout/switch identity ทำอย่างไร? | **button "ออกจากระบบ"** ที่ topbar ด้านขวา → navigate('/', replace) | MainLayout.tsx:29 |

---

## Defect / Observation Log

| ID | Scenario | Step | Description | Severity | Screenshot | Status |
|----|----------|------|-------------|----------|------------|--------|
| — | — | — | ยังไม่มีข้อมูล (รอ execute) | — | — | — |

**Risk ที่ต้องระวัง (จาก lesson-learned-sit-defects.md):**

| Risk | Flow | Step ตรวจ | Ref |
|------|------|----------|-----|
| Balance ไม่ deduct หลัง Approve | SIT-002 Phase B | Step 22 (EVD-008) | DEF-001 |
| Manager Queue ว่างถ้า ManagerId=null | SIT-002 Phase A | Step 8 | DEF-002 |
| Reject reason ไม่แสดงให้ Employee | SIT-003 (ถัดไป) | — | DEF-006 |

---

## วิธีติดตั้ง Antigravity Browser Integration

เพื่อให้ execute automation ได้ ต้องทำขั้นตอนต่อไปนี้:

1. **ติดตั้ง MCP server** ตามเอกสาร Antigravity Browser Integration  
2. **เพิ่มในไฟล์ `.claude/settings.json`** หรือ `claude_desktop_config.json`:
   ```json
   {
     "mcpServers": {
       "antigravity": {
         "command": "npx",
         "args": ["-y", "@antigravity/browser-mcp"]
       }
     }
   }
   ```
3. **Restart Claude Code session** เพื่อ load MCP server
4. **ตรวจสอบ** ด้วย ToolSearch → ต้องพบ tools: navigate, click, fill, assert, screenshot
5. **Re-run** command ด้วย Base URL: `http://localhost:5173`

---

## Re-run Command (เมื่อ environment พร้อม)

```
ใช้ test agent leave-request-sit-agent
ให้ test agent ใช้ Antigravity Browser Integration

อ่านไฟล์ต่อไปนี้:
- 10-requirement-definition/a0-business-requirement/req-summary/leave-request-and-approval-requirement-summary.md
- 10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md
- 20-system-design/b0-functional-design/leave-request-and-approval-sequence-diagram.md
- 30-coding/c2-sit/sit-scenario-leave-request.md
- 30-coding/c2-sit/sit-test-data-leave-request.yaml
- 80-knowledge-base/testing/lesson-learned-sit-defects.md

Base URL: http://localhost:5173

Run Scope:
- execute SIT-001 Employee submits leave request successfully
- execute SIT-002 Manager approves leave request
```

---

*Report อัปเดตโดย leave-request-sit-agent — credentials และ element names ยืนยันจาก source code*  
*อัปเดตสถานะจาก BLOCKED → PASS/FAIL หลัง execute จริง*
