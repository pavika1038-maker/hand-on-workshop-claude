# Lesson Learned — SIT Defects & Common Pitfalls

> บันทึก defect และ edge case ที่มักพลาดใน SIT ของระบบ Leave Request  
> ใช้เป็น context ให้ AI สร้าง SIT scenario ครอบคลุม risk จากประสบการณ์จริง  
> อัปเดตล่าสุด: 2026-06-16

---

## DEF-001: Leave Balance ไม่ถูก Deduct หลัง Approve

**พบเมื่อ:** Unit Test (BUG-002) / ยังต้องตรวจที่ SIT ด้วย  
**อาการ:** Manager approve คำขอลา แต่ยอดวันลาคงเหลือของ Employee ไม่เปลี่ยนแปลง  
**Flow ที่เสี่ยง:** SIT-002 (Approve) → ตรวจยอด balance ใน profile หน้า Employee  
**วิธีตรวจ:** หลัง Approve ให้ query `SELECT UsedDays, PendingDays FROM LeaveBalances WHERE EmployeeId = 'EMP001'` และดูหน้า Leave Balance ของ Employee  

**SIT Scenario ที่ต้องมี:**
```
SIT-002: Manager approves leave request
  → Expected: UsedDays เพิ่มขึ้น, PendingDays ลดลง
  → Evidence: screenshot หน้า balance ก่อน + หลัง approve
```

---

## DEF-002: Manager Queue ไม่แสดงคำขอของพนักงานที่ไม่มี ManagerId

**พบเมื่อ:** Session 3 Smoke Test (BUG-001)  
**อาการ:** HR (EMP003) ยื่นคำขอลา แต่ไม่ขึ้นใน approval queue ของ Manager ใดเลย  
**สาเหตุ:** ถ้า Employee.ManagerId = null query จะ return 0 rows  
**แก้ไขแล้ว:** ตั้ง ManagerId = "EMP002" ใน seed data  

**SIT Scenario ที่ต้องมี:**
```
SIT-xxx: HR submits leave request and it appears in Manager queue
  → Pre-condition: EMP003 มี ManagerId = EMP002
  → Expected: คำขอของ EMP003 ขึ้นใน queue ของ EMP002
```

---

## DEF-003: ApiResponse Structure ไม่ตรง → Frontend แสดง Error ไม่ได้

**พบเมื่อ:** Session 3 Smoke Test (BUG-006)  
**อาการ:** เมื่อ validation fail (เช่น balance ไม่พอ) หน้าจอไม่แสดง error message — แสดงแค่ spinner หมุนค้าง  
**สาเหตุ:** Backend ส่ง `error: { code, message }` (nested) แต่ Frontend expect `errorCode`, `message` (flat)  
**แก้ไขแล้ว:** Flatten ApiResponse.cs  

**SIT Scenario ที่ต้องมี:**
```
SIT-xxx: System displays validation error when balance is insufficient
  → Expected: หน้าจอแสดง "วันลาคงเหลือไม่เพียงพอ" ชัดเจน
  → ไม่ใช่แค่ check status code — ต้อง check UI message ด้วย
```

---

## DEF-004: Cancel Request ไม่สร้าง record ใน CancelRequests table

**พบเมื่อ:** Code Review (potential, ยังไม่พบใน smoke test)  
**อาการ (potential):** Employee cancel leave ที่ Approved แล้ว Status เปลี่ยนเป็น CancelRequested แต่ Manager ไม่เห็นใน cancel queue  
**สาเหตุ:** ถ้า service update LeaveRequest status แต่ไม่ INSERT CancelRequest row → Manager queue ว่างเปล่า  

**SIT Scenario ที่ต้องมี:**
```
SIT-007: Employee requests cancel on approved leave
  → Expected: สถานะ CancelRequested AND cancel queue ของ Manager มี entry นี้
  → Evidence: screenshot cancel queue ของ Manager
```

---

## DEF-005: Status Transition ไม่ถูก Guard → Cancel ซ้ำได้

**พบเมื่อ:** Unit Test (TC-KB-014) — ยังต้องตรวจที่ SIT ด้วย  
**อาการ:** Employee กด Cancel คำขอที่ Cancelled แล้วอีกครั้ง — ระบบควร error แต่อาจไม่ error  

**SIT Scenario ที่ต้องมี:**
```
SIT-xxx: System prevents double-cancel
  → Pre-condition: มีคำขอสถานะ Cancelled แล้ว
  → Expected: ระบบแสดง error "ไม่สามารถยกเลิกคำขอที่ยกเลิกแล้วได้"
```

---

## DEF-006: Rejection Reason ไม่แสดงให้ Employee เห็น

**พบเมื่อ:** ยังไม่พบ — ต้องตรวจใน SIT  
**อาการ (potential):** Manager reject และระบุเหตุผล แต่หน้าจอ Employee ไม่แสดง rejection reason  
**Risk:** API อาจ return reason แต่ Frontend ไม่ render  

**SIT Scenario ที่ต้องมี:**
```
SIT-003: Manager rejects leave request with reason
  → Expected: Employee เข้า history แล้วเห็น rejection reason ชัดเจน
  → Evidence: screenshot หน้า history ของ Employee
```

---

## DEF-007: Leave Balance ไม่คืนหลัง Approve Cancel

**พบเมื่อ:** Unit Test gap (ISS-001, GAP-18~22) — ยังไม่ได้ test ที่ SIT  
**อาการ (potential):** Manager approve cancel request แต่ UsedDays ไม่ลด / Remaining ไม่คืน  

**SIT Scenario ที่ต้องมี:**
```
SIT-008: Manager approves cancel request
  → Expected: สถานะ Cancelled, UsedDays ลดลง, Remaining เพิ่มขึ้น
  → Evidence: screenshot balance ก่อน + หลัง approve cancel
```

---

## DEF-008: Leave Balance ไม่กลับเป็น Approved หลัง Reject Cancel

**พบเมื่อ:** Unit Test gap (ISS-002, GAP-23~25) — ยังไม่ได้ test ที่ SIT  
**อาการ (potential):** Manager reject cancel request แต่สถานะไม่กลับเป็น Approved  

**SIT Scenario ที่ต้องมี:**
```
SIT-009: Manager rejects cancel request
  → Expected: LeaveRequest กลับเป็น Approved, balance ไม่เปลี่ยน
```

---

## สรุป Edge Cases ที่ต้องระวังใน SIT

| # | Edge Case | Flow | Risk |
|---|----------|------|------|
| EC-SIT-001 | ยื่นลาวันแรกที่ balance พอดีเป๊ะ (boundary) | Submit | Medium |
| EC-SIT-002 | Manager approve แล้ว Employee ดู balance ทันที (race condition) | Approve | Medium |
| EC-SIT-003 | Cancel Pending → balance คืนทันที (verify ทั้ง UI + DB) | Cancel | High |
| EC-SIT-004 | Cancel Approved → CancelRequested → Approve Cancel → ดู balance | Cancel Flow | High |
| EC-SIT-005 | Cancel Approved → CancelRequested → Reject Cancel → ดู status | Cancel Flow | High |
| EC-SIT-006 | HR ยื่นลา → ขึ้นใน Manager queue (ต้องมี ManagerId ถูกต้อง) | Submit | High |
| EC-SIT-007 | Reject โดยไม่กรอก reason → ระบบต้อง block | Reject | High |
| EC-SIT-008 | Employee approve คำขอของตัวเอง (เป็นทั้ง Employee + Manager) | Approve | Medium |
