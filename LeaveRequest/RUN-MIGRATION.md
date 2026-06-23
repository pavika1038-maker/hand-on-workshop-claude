# วิธี Run Migration และ Start ระบบ

## ขั้นที่ 1: สร้าง migration ใหม่ (เพิ่ม seed data สำหรับ Employee + LeaveBalance)

เปิด Terminal ใน folder `LeaveRequest/`:

```bash
dotnet ef migrations add SeedEmployeesAndBalances \
  --project src/LeaveRequest.Infrastructure \
  --startup-project src/LeaveRequest.API
```

## ขั้นที่ 2: Apply migration ไปยัง database

```bash
dotnet ef database update \
  --project src/LeaveRequest.Infrastructure \
  --startup-project src/LeaveRequest.API
```

> ถ้า database มีอยู่แล้วจากครั้งก่อน ให้ลบไฟล์ `leave-request.db` ก่อน แล้ว run `database update` ใหม่

## ขั้นที่ 3: Start Backend API

```bash
cd src/LeaveRequest.API
dotnet run
```
> API จะ run บน https://localhost:7082

## ขั้นที่ 4: Start Frontend (terminal ใหม่)

```bash
cd src/LeaveRequest.API/ClientApp
npm run dev
```
> Frontend จะ run บน http://localhost:5173

## Login

| Email | Password | Role |
|-------|----------|------|
| somchai@abc.com | 1234 | Employee |
| wipa@abc.com | 1234 | Manager |
| nanta@abc.com | 1234 | HR |
