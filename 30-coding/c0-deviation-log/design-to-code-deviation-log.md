# Deviation Log — Leave Request and Approval System

> บันทึกจุดที่ source code เบี่ยงเบนจาก design specification  
> จากการทำ Design-to-Code Alignment Review ครอบคลุมทุก layer

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Module | Leave Request and Approval System (Full System) |
| Function ID | SCR-001 ถึง SCR-010 |
| Design Source | leave-request-and-approval-method-signature.md, leave-request-and-approval-sequence-diagram.md, leave-request-and-approval-application-architecture-design.md, leave-request-and-approval-security-architecture-design.md, leave-request-and-approval-screen-srs.md, leave-request-and-approval-class-diagram.md |
| Code File(s) | LeaveRequestService.cs, LeaveRequestsController.cs, ApprovalsController.cs, LeaveBalancesController.cs, HrController.cs, ILeaveRequestService.cs, LeaveBalanceRepository.cs, LeaveRequestListPage.tsx, ApprovalListPage.tsx |
| Review Date | 2026-06-23 |
| Reviewed By | AI-assisted review (Claude Code) |

---

## 2. Deviation Log

| # | ประเภท | Design Spec | Actual Code | เหตุผล | การตัดสินใจ |
|---|--------|-------------|-------------|--------|------------|
| 1 | Structure | method-signature.md §3: แยก 3 interfaces (`ILeaveRequestService`, `IApprovalService`, `ICancelRequestService`) | `ILeaveRequestService.cs`: รวม method ทุกอย่างไว้ interface เดียว | ลดไฟล์ใน workshop scope | Defer → Sprint 2 (refactor sprint) |
| 2 | Method | method-signature.md §3.1: `GetMyLeaveRequestsAsync(string, LeaveRequestFilterDto, PaginationDto, ct)` | `GetMyRequestsAsync(string, int, int, ct)` — ชื่อต่าง, ไม่มี FilterDto | simplify สำหรับ MVP | Defer → Sprint 2 |
| 3 | Method | method-signature.md §3.1: `GetLeaveRequestDetailAsync(string requestingEmployeeId, Guid, ct)` | `GetDetailAsync(Guid, ct)` — ขาด `requestingEmployeeId` | ไม่มี auth layer จึงยังไม่ต้องใช้ | Defer → Sprint 2 (ทำพร้อม auth) |
| 4 | Method | method-signature.md §3.2: `RejectLeaveRequestAsync(string, Guid, string reason, ct)` — required | `RejectAsync(Guid, string, string? comment, ct)` — nullable, parameter order ต่าง | รองรับ screen SRS SF-004 ที่ reason = Optional | Accept (spec ขัดแย้งกัน — screen SRS ชนะ) |
| 5 | Method | method-signature.md §3.1: `CancelPendingLeaveRequestAsync → Task` (void, ไม่มี reason) | `CancelAsync(Guid, string, string?, ct) → Task<string>` | เพิ่ม reason สำหรับ cancel ที่ผ่านการอนุมัติ | Accept (code ดีกว่า spec) |
| 6 | Method | method-signature.md §3.2: `ApproveLeaveRequestAsync → Task<ApprovalResult>` | `ApproveAsync → Task` (void) | MVP ไม่ต้องการ structured result | Defer → Sprint 2 |
| 7 | Missing | sequence-diagram.md §2 Submit: VR-003 (probation), VR-004 (1-year service), VR-005/006 (advance notice), VR-007 (medical cert) | `SubmitLeaveRequestAsync`: มีแค่ VR-001 (outsource) + VR-002 (balance) | อยู่นอก scope workshop MVP | Defer → Sprint 3 (business rules sprint) |
| 8 | Missing | sequence-diagram.md §2 Step 13: fire-and-forget `PublishLeaveSubmittedAsync()` | ไม่มี notification publish หลัง commit | messaging infrastructure ยังไม่พร้อม | Defer → Sprint 4 (messaging sprint) |
| 9 | Missing | sequence-diagram.md §3: INSERT `ApprovalHistories` ทุก action (approve/reject/cancel-approve/cancel-reject) | `ApproveAsync`, `RejectAsync`, `ApproveCancelAsync`, `RejectCancelAsync`: ไม่มี INSERT | ลืม implement + ยังไม่มี `IApprovalHistoryRepository` | **Fix → Sprint 2 (audit trail จำเป็น)** |
| 10 | Logic | sequence-diagram.md §7 Transaction Rules: ทุก write ต้องมี BeginTx→ops→Commit→Rollback | `RejectCancelAsync`: เรียกแค่ `SaveChangesAsync` ไม่มี transaction wrapper | ตกหล่นตอน implement | **Fix → ทันที** |
| 11 | Structure | architecture-design.md §8.2: `POST /leave-requests/{id}/cancel-requests` แยกจาก cancel ทันที | `PATCH {id}/cancel` route เดียวทำ 2 flow โดย branch ตาม status ใน service | ลด route complexity | Accept (pattern ชัดเจน, service รับผิดชอบ branching) |
| 12 | Violation | architecture-design.md §3 Layer Rules: Controller → Application Service → Repository | `LeaveBalancesController.cs`: inject `ILeaveBalanceRepository` โดยตรง ข้าม Application layer | ตกหล่นตอน implement | **Fix → Sprint 2 (สร้าง ILeaveBalanceService)** |
| 13 | Mismatch | architecture-design.md §8.2: `GET /api/v1/leave-balances?employeeId=` | Route จริง: `GET /api/v1/leave-balances/dashboard` | frontend และ backend ตรงกัน ใช้งานได้ | Accept (อัปเดต spec แทน) |
| 14 | Missing | security-architecture-design.md §3: `[Authorize]` + RBAC policy; §4: JWT Bearer / Entra ID | ทุก Controller: ไม่มี `[Authorize]` เลย; identity อ่านจาก `X-Employee-Id` header | Known L-001/L-007; workshop mock auth | Accept for workshop → **Fix → Sprint 2 (security sprint)** |
| 15 | Missing | security-architecture-design.md §3: data isolation per role (employee เห็นแค่ตัวเอง, manager เห็น team) | `GetDetail` ไม่มี ownership check; ใครก็ดู approval queue ของ Manager อื่นได้ | depends on auth (I-014) | Defer → Sprint 2 (ทำพร้อม auth) |
| 16 | Missing | screen-srs.md SCR-003 §2.3.3: `half_day_period` AM/PM dropdown (conditional เมื่อ `is_half_day = true`) | `LeaveRequestListPage.tsx`: มีแค่ `isHalfDay` checkbox, ไม่มี AM/PM selector | ตกหล่นตอน generate frontend | **Fix → Sprint 2** |
| 17 | Mismatch | screen-srs.md SCR-003 §2.3.4: `reason` = Required Y | `LeaveRequestListPage.tsx`: `reason` textarea ไม่มี `required`, ไม่มี asterisk (*) | ตกหล่นตอน generate frontend | **Fix → ทันที** |
| 18 | Mismatch | screen-srs.md SCR-003 §2.3.5: `total_days` = working days | `calcDays`: `Math.floor(diff / 86_400_000) + 1` รวม weekend | ต้องมี holiday calendar ก่อน | Defer → Sprint 3 (ต้องออกแบบ holiday config) |
| 19 | Missing | screen-srs.md SCR-003 §3: error codes ERR-LR-001–008 แสดงใต้ field โดยตรง | แสดงเฉพาะ generic banner; ไม่มี per-field error mapping | scope เกิน MVP | Defer → Sprint 2 (UX improvement) |
| 20 | Mismatch | screen-srs.md SCR-003: Confirm dialog ควร styled component | `LeaveRequestListPage.tsx`: ใช้ `window.confirm()` (native) | รวดเร็วสำหรับ workshop | Accept for workshop → Fix Sprint 2 |
| 21 | Mismatch | screen-srs.md SCR-007 §2.3: `sla_countdown` ต้องเป็น HH:MM countdown timer | `ApprovalListPage.tsx`: แสดง `formatDate(item.slaDeadline)` เป็น date string | ตกหล่นตอน generate frontend | **Fix → Sprint 2** |
| 22 | Mismatch | method-signature.md §7: SlaDeadline = SubmittedAt + 1 วันทำการ | `SubmitLeaveRequestAsync`: `DateTime.UtcNow.AddHours(48)` hardcode | ต้องมี business calendar ก่อน | Defer → Sprint 3 (ทำพร้อม I-018) |
| 23 | Mismatch | architecture-design.md §2: Frontend = Angular + Angular Material | Implementation: React + TypeScript + Vite | จงใจเปลี่ยน stack สำหรับ workshop | Accept → อัปเดต architecture doc |

---

## 3. Action Required

| # | Action | Owner | Due |
|---|--------|-------|-----|
| 1 | **Fix I-010**: เพิ่ม `BeginTransactionAsync` / `CommitAsync` / `RollbackAsync` ใน `RejectCancelAsync` | Dev | ทันที |
| 2 | **Fix I-017**: เพิ่ม `required` attribute + asterisk (*) บน `reason` textarea ใน `LeaveRequestListPage.tsx` | Dev | ทันที |
| 3 | Fix I-009: สร้าง `IApprovalHistoryRepository` + implement INSERT ApprovalHistory ใน approve/reject methods | Dev | Sprint 2 |
| 4 | Fix I-012: สร้าง `ILeaveBalanceService` ใน Application layer; ย้าย logic จาก `LeaveBalancesController` | Dev | Sprint 2 |
| 5 | Fix I-016: เพิ่ม AM/PM dropdown conditional ใน `LeaveRequestListPage.tsx` | Dev | Sprint 2 |
| 6 | Fix I-021: เพิ่ม countdown timer component ใน `ApprovalListPage.tsx` คำนวณ `slaDeadline - now` | Dev | Sprint 2 |
| 7 | Implement I-014: JWT/Entra ID auth + `[Authorize]` + RBAC policies | Dev + Infra | Sprint 2 (security sprint) |
| 8 | Implement I-015: Data isolation ownership check หลัง auth พร้อม | Dev | Sprint 2 (หลัง I-014) |
| 9 | Implement I-007: Business rules VR-003 ถึง VR-007 | Dev | Sprint 3 |
| 10 | Implement I-018/I-022: Business day calculator + holiday calendar config | Dev + Designer | Sprint 3 |
| 11 | อัปเดต `application-architecture-design.md`: แก้ frontend stack (Angular → React) + route `/leave-balances/dashboard` | Designer | Sprint 2 |
| 12 | ชี้แจง spec ขัดแย้ง: `reject reason` required (method-signature.md) vs Optional (screen-srs.md SF-004) | Product / Designer | Sprint 2 |

---

## 4. Summary

- Deviation ทั้งหมด: **23 รายการ**
- Accept (ยอมรับ): **6** — #4, #5, #11, #13, #20 (workshop), #23
- **Fix ทันที: 2** — #10 (RejectCancelAsync transaction), #17 (reason required)
- Fix Sprint 2: **6** — #3 (layer violation), #6 (I-009 ApprovalHistory), #14 (auth), #16 (AM/PM), #19 (I-019 per-field errors), #21 (SLA countdown)
- Defer Sprint 3+: **9** — #1, #2, #7, #8, #15, #18, #22 + ที่เหลือ
- ต้องอัปเดต Design ไหม: **Yes**
  - `leave-request-and-approval-application-architecture-design.md` (React stack, `/dashboard` route)
  - `leave-request-and-approval-screen-srs.md` SF-004 (ชี้แจง reason optional/required)
