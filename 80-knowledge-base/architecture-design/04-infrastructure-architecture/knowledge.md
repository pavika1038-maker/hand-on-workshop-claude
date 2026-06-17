# Infrastructure & Cloud Architecture

## 1. ภาพรวม

Infrastructure Architecture คือการออกแบบโครงสร้างพื้นฐานที่รองรับการทำงานของแอปพลิเคชันทั้งหมด ครอบคลุม compute, network, storage, server design, และ DevOps pipeline โดยยึด Microsoft Technology Stack เป็นแกนหลัก รองรับทั้ง Azure Cloud และ On-Premise

> **หมายเหตุ:**
> - Azure Cloud Service เป็นของ HQ โดย HQ จัดเตรียม Azure Subscription, Azure Backup Tool, และ Antivirus Tool ให้
> - ในบางกรณี เช่น ระบบ Production ที่มีข้อจำกัดด้าน compliance, data sovereignty, หรือ latency ไม่สามารถใช้ Cloud ได้ ต้องออกแบบเป็น On-Premise

## 2. Deployment Model

### 2.1 ทางเลือก Deployment Model

| Model | คำอธิบาย | เมื่อใดควรใช้ | เทคโนโลยี |
|-------|----------|-------------|-----------|
| On-Premise | ติดตั้งบน server ขององค์กร | ข้อจำกัดด้าน compliance, data sovereignty, ระบบ production ที่ห้ามอยู่บน cloud, ต้องการ low latency กับระบบ factory/plant | Windows Server (Latest), IIS, SQL Server |
| Cloud (Azure) | ใช้ Azure เต็มรูปแบบ (HQ owned) | ต้องการ scalability, global reach, ลด infrastructure management | Azure App Service, AKS, Azure SQL |
| Hybrid | On-Premise + Cloud | ย้ายระบบเป็นขั้นตอน, บาง workload อยู่ on-premise บางส่วนอยู่ cloud | Azure Arc, Azure VPN, ExpressRoute |

### 2.2 แนวทางเลือก Deployment Model

```text
┌─────────────────────────────────────────────────────────────────┐
│  คำถามช่วยตัดสินใจ Deployment Model                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ข้อมูลมีข้อจำกัดด้าน compliance/data sovereignty?             │
│  ├── ใช่ → On-Premise                                          │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ระบบต้องเชื่อมต่อกับเครื่องจักร/อุปกรณ์ในโรงงาน (OT)?       │
│  ├── ใช่ → On-Premise หรือ Hybrid (Azure Arc)                  │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ต้องการ low latency < 10ms กับระบบภายใน?                      │
│  ├── ใช่ → On-Premise                                          │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ต้องการ scalability / global access?                           │
│  ├── ใช่ → Cloud (Azure)                                       │
│  └── ไม่ → On-Premise หรือ Hybrid                              │
│                                                                 │
│  มีทีม infrastructure ดูแล server ได้เอง?                       │
│  ├── ใช่ → On-Premise หรือ Hybrid                              │
│  └── ไม่ → Cloud (Azure) — managed service                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 2.3 Operating System & Middleware Standard

| รายการ | มาตรฐาน | หมายเหตุ |
|--------|---------|---------|
| Operating System | Windows Server (Latest Version) | ใช้เวอร์ชันล่าสุดที่ Microsoft support ทั้ง on-premise และ cloud VM |
| Database Server | Microsoft SQL Server (Latest Version) | ใช้เวอร์ชันล่าสุดที่ Microsoft support |
| Web Server | IIS (Internet Information Services) | มาพร้อม Windows Server |
| Runtime | .NET Core / ASP.NET (Latest LTS) | ใช้ Long-Term Support version |
| Backup Tool | Azure Backup (Cloud) / Windows Server Backup (On-Premise) | HQ จัดเตรียม Azure Backup ให้ |
| Antivirus | HQ provided Antivirus Tool | HQ จัดเตรียมและจัดการ license ให้ ทั้ง on-premise และ cloud |

## 3. On-Premise Architecture

### 3.1 ภาพรวม On-Premise

On-Premise Architecture ใช้สำหรับระบบที่ไม่สามารถหรือไม่เหมาะสมที่จะอยู่บน Cloud เช่น:
- ระบบ Production / Manufacturing ที่ต้องเชื่อมต่อกับเครื่องจักรหรืออุปกรณ์ในโรงงาน
- ระบบที่มีข้อจำกัดด้าน data sovereignty (ข้อมูลต้องอยู่ในประเทศ)
- ระบบที่ต้องการ low latency กับระบบภายในองค์กร
- ระบบที่ compliance กำหนดว่าห้ามเก็บข้อมูลบน public cloud

### 3.2 On-Premise Architecture Diagram

```text
┌─────────────────────────────────────────────────────────────────────┐
│  On-Premise Data Center                                             │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  DMZ (Demilitarized Zone)                                    │  │
│  │  ┌─────────────────────┐  ┌─────────────────────┐           │  │
│  │  │  Reverse Proxy /    │  │  Web Application    │           │  │
│  │  │  Load Balancer      │  │  Firewall (WAF)     │           │  │
│  │  └──────────┬──────────┘  └──────────┬──────────┘           │  │
│  └─────────────┼────────────────────────┼───────────────────────┘  │
│                │ Firewall Rule: 443     │                          │
│  ┌─────────────┼────────────────────────┼───────────────────────┐  │
│  │  Application Zone                                            │  │
│  │  ┌─────────────────────┐  ┌─────────────────────┐           │  │
│  │  │  Web Server 1       │  │  Web Server 2       │           │  │
│  │  │  (IIS + .NET Core)  │  │  (IIS + .NET Core)  │           │  │
│  │  │  Windows Server     │  │  Windows Server     │           │  │
│  │  └──────────┬──────────┘  └──────────┬──────────┘           │  │
│  │             │                        │                       │  │
│  │  ┌──────────┴────────────────────────┴──────────┐           │  │
│  │  │  Application Server (Background Services)    │           │  │
│  │  │  - Batch Job / Scheduler                     │           │  │
│  │  │  - Message Queue Consumer                    │           │  │
│  │  │  - Integration Adapter                       │           │  │
│  │  │  Windows Server + .NET Core                  │           │  │
│  │  └──────────────────────┬───────────────────────┘           │  │
│  └─────────────────────────┼────────────────────────────────────┘  │
│                            │ Firewall Rule: 1433 (SQL)            │
│  ┌─────────────────────────┼────────────────────────────────────┐  │
│  │  Database Zone                                               │  │
│  │  ┌─────────────────────┐  ┌─────────────────────┐           │  │
│  │  │  SQL Server         │  │  SQL Server         │           │  │
│  │  │  (Primary)          │  │  (Secondary /       │           │  │
│  │  │  Windows Server     │  │   Always On)        │           │  │
│  │  └─────────────────────┘  └─────────────────────┘           │  │
│  │                                                              │  │
│  │  ┌─────────────────────┐  ┌─────────────────────┐           │  │
│  │  │  File Server        │  │  Backup Server      │           │  │
│  │  │  (Shared Storage)   │  │  (Windows Server    │           │  │
│  │  │                     │  │   Backup / Agent)   │           │  │
│  │  └─────────────────────┘  └─────────────────────┘           │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Management Zone                                             │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │  AD Domain   │  │  Monitoring  │  │  Antivirus   │       │  │
│  │  │  Controller  │  │  Server      │  │  Management  │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 3.3 On-Premise Network Zoning

| Zone | วัตถุประสงค์ | Server ที่อยู่ใน Zone | Firewall Rule |
|------|------------|---------------------|---------------|
| DMZ | รับ traffic จากภายนอก | Reverse Proxy, Load Balancer, WAF | Allow inbound 443 (HTTPS) เท่านั้น |
| Application Zone | ประมวลผล business logic | Web Server (IIS), App Server, Background Services | Allow from DMZ (443), Allow to DB Zone (1433) |
| Database Zone | จัดเก็บข้อมูล | SQL Server (Primary + Secondary), File Server, Backup Server | Allow from App Zone (1433) เท่านั้น, ห้าม inbound จาก DMZ หรือ Internet |
| Management Zone | บริหารจัดการระบบ | AD Domain Controller, Monitoring Server, Antivirus Management | Allow from internal admin network เท่านั้น |

### 3.4 On-Premise High Availability

| Component | วิธี HA | RPO | RTO |
|-----------|--------|-----|-----|
| Web Server | Load Balancer + 2 nodes (Active-Active) | 0 | < 1 นาที (automatic failover) |
| App Server | Windows Failover Cluster หรือ 2 nodes | 0 | < 5 นาที |
| SQL Server | Always On Availability Group (Synchronous) | 0 | < 1 นาที (automatic failover) |
| File Server | DFS Replication หรือ Storage Spaces Direct | < 15 นาที | < 30 นาที |
| Backup | Windows Server Backup + offsite copy | ตาม backup schedule | ตาม restore procedure |

### 3.5 On-Premise Backup Strategy

| ประเภท | ความถี่ | Retention | เครื่องมือ |
|--------|---------|-----------|-----------|
| Full Backup (Database) | ทุกวัน 01:00 | 30 วัน | SQL Server Agent + Windows Server Backup |
| Differential Backup | ทุก 6 ชั่วโมง | 7 วัน | SQL Server Agent |
| Transaction Log Backup | ทุก 15 นาที | 3 วัน | SQL Server Agent |
| System State Backup | ทุกสัปดาห์ | 4 สัปดาห์ | Windows Server Backup |
| Offsite Copy | ทุกวัน (หลัง full backup) | 90 วัน | Copy ไปยัง offsite storage หรือ Azure Blob (Hybrid) |

### 3.6 On-Premise vs Cloud เปรียบเทียบ

| เกณฑ์ | On-Premise | Cloud (Azure) | Hybrid |
|-------|-----------|---------------|--------|
| ค่าใช้จ่ายเริ่มต้น | สูง (ซื้อ hardware) | ต่ำ (pay-as-you-go) | ปานกลาง |
| ค่าใช้จ่ายระยะยาว | ต่ำ-ปานกลาง (depreciation) | ปานกลาง-สูง (monthly) | ปานกลาง |
| Scalability | จำกัด (ต้องซื้อ hardware เพิ่ม) | สูง (auto-scale) | ปานกลาง |
| Data Sovereignty | ✅ ควบคุมเต็มที่ | ขึ้นอยู่กับ region | ✅ เลือกได้ |
| Latency กับระบบภายใน | ✅ ต่ำมาก (LAN) | สูงกว่า (Internet/VPN) | ปานกลาง |
| Maintenance | ต้องดูแลเอง (OS patch, hardware) | Microsoft ดูแล (managed) | ผสม |
| Disaster Recovery | ต้องจัดเตรียมเอง | Built-in (geo-replication) | ผสม |
| Compliance | ✅ ควบคุมเต็มที่ | ต้องตรวจสอบ Azure compliance | ✅ เลือกได้ |

## 4. Server Design

### 4.1 ภาพรวม Server Design

Server Design คือการออกแบบ specification ของ server ที่ใช้ในระบบ ครอบคลุมทั้ง Physical Server และ Virtual Machine (VM) เพื่อให้มั่นใจว่า server มี resource เพียงพอสำหรับ workload ที่ต้องรองรับ

### 4.2 Physical vs Virtual Server

| เกณฑ์ | Physical Server | Virtual Server (VM) | แนะนำ |
|-------|----------------|--------------------|----|
| Performance | สูงสุด (dedicated hardware) | ดี (shared hardware) | Physical สำหรับ DB ขนาดใหญ่ |
| Cost | สูง (ซื้อ hardware) | ต่ำกว่า (share hardware) | VM สำหรับ App/Web server |
| Scalability | ต้องซื้อเพิ่ม | เพิ่ม VM ได้เร็ว | VM สำหรับ scale out |
| Isolation | ✅ สูงสุด | ปานกลาง (hypervisor) | Physical สำหรับ security-critical |
| Management | ดูแลทีละเครื่อง | จัดการผ่าน Hyper-V / VMware | VM สำหรับ management ง่าย |
| DR / Backup | ซับซ้อนกว่า | ง่ายกว่า (VM snapshot) | VM สำหรับ DR ง่าย |

**แนวทางเลือก:**
- **Database Server (Production):** Physical Server หรือ VM ที่ dedicated resource (ไม่ share กับ workload อื่น)
- **Web / App Server:** Virtual Machine (Hyper-V) — scale out ได้ง่าย
- **Background Services:** Virtual Machine — แยก VM ตาม workload
- **DEV / SIT / UAT:** Virtual Machine เสมอ — ประหยัดและจัดการง่าย

### 4.3 Server Sizing Guideline

#### 4.3.1 Web Server (IIS + .NET Core)

| ขนาดระบบ | CPU | RAM | Disk (OS + App) | Network | จำนวน Server |
|----------|-----|-----|-----------------|---------|-------------|
| เล็ก (< 100 concurrent users) | 4 vCPU | 8 GB | 100 GB SSD | 1 Gbps | 1 (+ 1 standby) |
| กลาง (100-500 concurrent users) | 8 vCPU | 16 GB | 200 GB SSD | 1 Gbps | 2 (Active-Active + LB) |
| ใหญ่ (500-2000 concurrent users) | 16 vCPU | 32 GB | 200 GB SSD | 10 Gbps | 3-4 (Active-Active + LB) |

#### 4.3.2 Application Server (Background Services / Batch)

| ขนาดระบบ | CPU | RAM | Disk (OS + App) | Network | จำนวน Server |
|----------|-----|-----|-----------------|---------|-------------|
| เล็ก | 4 vCPU | 8 GB | 100 GB SSD | 1 Gbps | 1 |
| กลาง | 8 vCPU | 16 GB | 200 GB SSD | 1 Gbps | 1-2 |
| ใหญ่ | 16 vCPU | 32 GB | 300 GB SSD | 1 Gbps | 2-3 |

#### 4.3.3 Database Server (SQL Server)

| ขนาดระบบ | CPU | RAM | Disk (OS) | Disk (Data) | Disk (Log) | Disk (TempDB) | Network | จำนวน Server |
|----------|-----|-----|-----------|-------------|------------|---------------|---------|-------------|
| เล็ก (< 50 GB data) | 8 vCPU | 32 GB | 100 GB SSD | 200 GB SSD | 100 GB SSD | 50 GB SSD | 1 Gbps | 1 (+ 1 Always On) |
| กลาง (50-500 GB data) | 16 vCPU | 64 GB | 100 GB SSD | 1 TB SSD | 500 GB SSD | 100 GB SSD | 10 Gbps | 1 (+ 1 Always On) |
| ใหญ่ (500 GB - 5 TB data) | 32 vCPU | 128 GB | 100 GB SSD | 5 TB SSD/NVMe | 1 TB SSD | 200 GB SSD | 10 Gbps | 1 (+ 1 Always On) |

**Best Practice SQL Server Disk Layout:**
- **แยก disk** สำหรับ OS, Data, Log, TempDB เสมอ (ห้ามรวม disk เดียว)
- Data disk: ใช้ SSD หรือ NVMe สำหรับ production
- Log disk: ใช้ SSD แยกจาก Data disk เพื่อ write performance
- TempDB: ใช้ SSD แยก, จำนวน TempDB files = จำนวน CPU cores (สูงสุด 8 files)
- RAM: กำหนด SQL Server Max Memory = Total RAM - 4 GB (เหลือให้ OS)

#### 4.3.4 File Server / Shared Storage

| ขนาดระบบ | CPU | RAM | Disk | Network | หมายเหตุ |
|----------|-----|-----|------|---------|---------|
| เล็ก | 2 vCPU | 4 GB | 500 GB - 1 TB | 1 Gbps | สำหรับไฟล์แนบ, เอกสาร |
| กลาง | 4 vCPU | 8 GB | 1 - 5 TB | 1 Gbps | สำหรับ shared storage หลาย application |
| ใหญ่ | 8 vCPU | 16 GB | 5 - 20 TB | 10 Gbps | สำหรับ enterprise file sharing |

### 4.4 Server Architecture Diagram (On-Premise Production)

```text
┌─────────────────────────────────────────────────────────────────────┐
│  Production Server Layout                                           │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Physical Host 1 (Hyper-V)                                   │  │
│  │  CPU: 32 cores | RAM: 128 GB | Disk: 2 TB SSD               │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │  VM: Web01   │  │  VM: App01   │  │  VM: Mgmt01  │       │  │
│  │  │  8 vCPU      │  │  8 vCPU      │  │  4 vCPU      │       │  │
│  │  │  16 GB RAM   │  │  16 GB RAM   │  │  8 GB RAM    │       │  │
│  │  │  IIS + .NET  │  │  .NET Core   │  │  AD + Mon    │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Physical Host 2 (Hyper-V)                                   │  │
│  │  CPU: 32 cores | RAM: 128 GB | Disk: 2 TB SSD               │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │  VM: Web02   │  │  VM: App02   │  │  VM: File01  │       │  │
│  │  │  8 vCPU      │  │  8 vCPU      │  │  4 vCPU      │       │  │
│  │  │  16 GB RAM   │  │  16 GB RAM   │  │  8 GB RAM    │       │  │
│  │  │  IIS + .NET  │  │  .NET Core   │  │  File Share  │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Physical Server: DB01 (Dedicated — ไม่ใช้ VM)               │  │
│  │  CPU: 16 cores | RAM: 64 GB | OS: 100 GB SSD                │  │
│  │  Data: 1 TB SSD | Log: 500 GB SSD | TempDB: 100 GB SSD      │  │
│  │  SQL Server (Primary) — Always On Availability Group         │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Physical Server: DB02 (Dedicated — ไม่ใช้ VM)               │  │
│  │  CPU: 16 cores | RAM: 64 GB | OS: 100 GB SSD                │  │
│  │  Data: 1 TB SSD | Log: 500 GB SSD | TempDB: 100 GB SSD      │  │
│  │  SQL Server (Secondary) — Always On Availability Group       │  │
│  └───────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Network Infrastructure                                      │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │  Load        │  │  Firewall    │  │  Core Switch │       │  │
│  │  │  Balancer    │  │  (L3/L4)     │  │  (10 Gbps)   │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

### 4.5 Server Naming Convention

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Web Server | `{ENV}-WEB{NN}` | PROD-WEB01, PROD-WEB02, UAT-WEB01 |
| App Server | `{ENV}-APP{NN}` | PROD-APP01, SIT-APP01 |
| Database Server | `{ENV}-DB{NN}` | PROD-DB01, PROD-DB02 |
| File Server | `{ENV}-FILE{NN}` | PROD-FILE01 |
| Management Server | `{ENV}-MGMT{NN}` | PROD-MGMT01 |
| Backup Server | `{ENV}-BKP{NN}` | PROD-BKP01 |

### 4.6 Server Hardening Checklist (On-Premise)

| # | รายการ | รายละเอียด |
|---|--------|-----------|
| 1 | OS Patching | ติดตั้ง Windows Update ทุกเดือน (Patch Tuesday) ภายใน 30 วัน |
| 2 | Disable Unused Services | ปิด service ที่ไม่ใช้ เช่น Print Spooler, Remote Registry |
| 3 | Firewall | เปิด Windows Firewall, allow เฉพาะ port ที่จำเป็น |
| 4 | Antivirus | ติดตั้ง HQ provided Antivirus Tool, update signature ทุกวัน |
| 5 | Local Admin | เปลี่ยน password local admin, ห้ามใช้ default password |
| 6 | Remote Access | ใช้ RDP ผ่าน VPN เท่านั้น, ห้ามเปิด RDP ตรงจาก Internet |
| 7 | Audit Policy | เปิด Windows Security Audit: logon events, object access, policy change |
| 8 | TLS | ปิด TLS 1.0/1.1, เปิดเฉพาะ TLS 1.2+ |
| 9 | SMB | ปิด SMBv1, ใช้ SMBv3 เท่านั้น |
| 10 | Disk Encryption | เปิด BitLocker สำหรับ disk ที่เก็บ sensitive data |

### 4.7 Azure VM Sizing (สำหรับ Cloud Deployment)

เมื่อ deploy บน Azure ให้ map server sizing กับ Azure VM SKU:

| Server Role | Azure VM SKU (แนะนำ) | vCPU | RAM | Disk |
|-------------|---------------------|------|-----|------|
| Web Server (เล็ก) | Standard_D4s_v5 | 4 | 16 GB | Premium SSD |
| Web Server (กลาง) | Standard_D8s_v5 | 8 | 32 GB | Premium SSD |
| App Server (เล็ก) | Standard_D4s_v5 | 4 | 16 GB | Premium SSD |
| App Server (กลาง) | Standard_D8s_v5 | 8 | 32 GB | Premium SSD |
| DB Server (เล็ก) | Standard_E8s_v5 | 8 | 64 GB | Premium SSD (แยก disk) |
| DB Server (กลาง) | Standard_E16s_v5 | 16 | 128 GB | Premium SSD / Ultra Disk |
| DB Server (ใหญ่) | Standard_E32s_v5 | 32 | 256 GB | Ultra Disk |

> **หมายเหตุ:** ใช้ Dsv5 series สำหรับ general purpose, Esv5 series สำหรับ memory-optimized (Database)

## 5. Azure Cloud Architecture

```text
┌─────────────────────────────────────────────────────────────┐
│  Azure Subscription                                         │
│                                                             │
│  ┌─────────────────────────────────────────────────────┐    │
│  │  Resource Group: rg-leave-app-prod                  │    │
│  │                                                     │    │
│  │  ┌──────────────┐  ┌──────────────┐                │    │
│  │  │ Azure App    │  │ Azure SQL    │                │    │
│  │  │ Service      │  │ Database     │                │    │
│  │  │ (Web API +   │  │ (S3 tier)    │                │    │
│  │  │  Blazor)     │  │              │                │    │
│  │  └──────┬───────┘  └──────┬───────┘                │    │
│  │         │                 │                         │    │
│  │  ┌──────┴─────────────────┴───────┐                │    │
│  │  │  Azure Virtual Network         │                │    │
│  │  │  (vnet-leave-app)              │                │    │
│  │  └────────────────────────────────┘                │    │
│  │                                                     │    │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────┐ │    │
│  │  │ Azure Key    │  │ Azure Blob   │  │ Azure    │ │    │
│  │  │ Vault        │  │ Storage      │  │ Redis    │ │    │
│  │  │ (secrets)    │  │ (files)      │  │ Cache    │ │    │
│  │  └──────────────┘  └──────────────┘  └──────────┘ │    │
│  │                                                     │    │
│  │  ┌──────────────┐  ┌──────────────┐                │    │
│  │  │ Azure Service│  │ Azure App    │                │    │
│  │  │ Bus          │  │ Insights     │                │    │
│  │  │ (messaging)  │  │ (monitoring) │                │    │
│  │  └──────────────┘  └──────────────┘                │    │
│  └─────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

## 6. Environment Strategy

### 6.1 Environment Landscape

| Environment | วัตถุประสงค์ | Deployment | Database | ใครใช้ |
|-------------|------------|-----------|----------|-------|
| DEV | พัฒนาและทดสอบ feature ใหม่ | Azure (rg-app-dev) หรือ On-Premise VM | SQL Server Dev (Basic) | Developer |
| SIT | ทดสอบ integration กับระบบภายนอก | Azure (rg-app-sit) หรือ On-Premise VM | SQL Server SIT (S1) | Developer, QA |
| UAT | ทดสอบโดย business user | Azure (rg-app-uat) หรือ On-Premise VM | SQL Server UAT (S2) | Business user, QA |
| PROD | ระบบจริง | Azure (rg-app-prod) หรือ **On-Premise** (ตามข้อจำกัด) | SQL Server PROD (S3 / Dedicated) | End user |

> **หมายเหตุ:** หาก PROD ต้องอยู่ On-Premise ให้ใช้ server sizing ตาม Section 4.3 และ architecture ตาม Section 3.2

### 6.2 Environment Configuration

**Best Practice:**
- แยก Azure Subscription สำหรับ Non-Prod และ Prod
- ใช้ Azure Resource Group แยกตาม environment
- Configuration ทุก environment เก็บใน Azure App Configuration
- Secret (connection string, API key) เก็บใน Azure Key Vault ห้ามเก็บใน code หรือ config file
- ใช้ Managed Identity สำหรับ service-to-service authentication ภายใน Azure (ไม่ใช้ connection string)
- Infrastructure as Code (IaC) ใช้ Bicep หรือ Terraform เก็บใน Azure DevOps Git

## 7. Compute Architecture

### 7.1 ทางเลือก Compute Service

| Service | เมื่อใดควรใช้ | ข้อดี | ข้อจำกัด |
|---------|-------------|------|---------|
| Azure App Service | Web application ทั่วไป | ง่าย, managed, auto-scale | จำกัด customization ระดับ OS |
| Azure Container Apps | Container-based, serverless | Dapr built-in, scale to zero | ยังใหม่ อาจมี limitation |
| Azure Kubernetes Service (AKS) | Microservices ขนาดใหญ่ | full control, multi-container | ซับซ้อน ต้องมีทีม DevOps |
| Azure Functions | Event-driven, short-lived tasks | pay-per-execution, auto-scale | cold start, execution time limit |

### 7.2 แนวทางเลือก Compute

```text
Simple Web App           → Azure App Service (แนะนำเริ่มที่นี่)
Container + Dapr          → Azure Container Apps
Complex Microservices     → Azure Kubernetes Service (AKS)
Background Job / Trigger  → Azure Functions
Batch Processing          → Azure Batch หรือ Azure Functions (Durable)
```

## 8. CI/CD Pipeline

### 8.1 Azure DevOps Pipeline Architecture

```text
┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
│  Code    │    │  Build   │    │  Test    │    │  Deploy  │
│  Commit  │───▶│  (CI)    │───▶│  (Auto)  │───▶│  (CD)    │
│  (Git)   │    │          │    │          │    │          │
└──────────┘    └──────────┘    └──────────┘    └──────────┘
     │               │               │               │
  Azure DevOps   dotnet build    dotnet test     Azure App
  Repos / GitHub  + npm build    + UI test       Service
                  + Docker build                 Deployment
```

### 8.2 Pipeline Stages

| Stage | ขั้นตอน | เครื่องมือ |
|-------|---------|-----------|
| Source | Code commit, branch policy, PR review | Azure DevOps Repos / GitHub |
| Build | Compile, unit test, code analysis | `dotnet build`, `dotnet test`, SonarQube |
| Package | Docker image, NuGet package | Docker, Azure Container Registry |
| Deploy DEV | Auto-deploy เมื่อ merge เข้า develop branch | Azure DevOps Release Pipeline |
| Deploy SIT | Auto-deploy เมื่อ merge เข้า release branch | Azure DevOps Release Pipeline |
| Deploy UAT | Manual approval ก่อน deploy | Azure DevOps Release Pipeline + Approval Gate |
| Deploy PROD | Manual approval + change ticket | Azure DevOps Release Pipeline + Approval Gate |

### 8.3 Branch Strategy

ใช้ **GitFlow** หรือ **Trunk-Based Development** ขึ้นอยู่กับขนาดทีม:

| Strategy | เมื่อใดควรใช้ | Branch หลัก |
|----------|-------------|-------------|
| GitFlow | ทีมขนาดกลาง, release cycle ยาว | main, develop, feature/*, release/*, hotfix/* |
| Trunk-Based | ทีมที่ CI/CD mature, release บ่อย | main, feature/* (short-lived) |

**Best Practice:**
- Branch policy: ต้องมี PR review อย่างน้อย 1 คนก่อน merge
- ต้องผ่าน CI build + unit test ก่อน merge
- ใช้ semantic versioning: `MAJOR.MINOR.PATCH`
- Tag ทุก release ที่ deploy ขึ้น PROD

## 9. Monitoring & Observability

### 9.1 Monitoring Stack

```text
┌─────────────────────────────────────────────────────┐
│  Azure Monitor                                       │
│  ├── Application Insights (APM)                     │
│  │   ├── Request tracing                            │
│  │   ├── Dependency tracking                        │
│  │   ├── Exception logging                          │
│  │   ├── Custom metrics                             │
│  │   └── Live metrics stream                        │
│  ├── Log Analytics Workspace                        │
│  │   ├── Application logs                           │
│  │   ├── Infrastructure logs                        │
│  │   └── Security logs                              │
│  ├── Azure Monitor Alerts                           │
│  │   ├── Metric alerts                              │
│  │   ├── Log query alerts                           │
│  │   └── Action groups (email, Teams, webhook)      │
│  └── Azure Dashboard                                │
│      ├── Application health                         │
│      ├── Infrastructure metrics                     │
│      └── Business KPIs                              │
└─────────────────────────────────────────────────────┘
```

### 9.2 Logging Standard

| Level | เมื่อใดควรใช้ | ตัวอย่าง |
|-------|-------------|---------|
| Trace | Detailed diagnostic (DEV/SIT เท่านั้น) | Method entry/exit, variable values |
| Debug | Debug information | SQL query generated, cache hit/miss |
| Information | Normal operation | "Leave request LR-001 submitted by EMP-001" |
| Warning | Potential issue แต่ไม่กระทบ operation | "Retry attempt 2 for Production Planning API" |
| Error | Error ที่กระทบ operation แต่ระบบยังทำงานต่อได้ | "Failed to send notification email for LR-001" |
| Critical | Error ที่กระทบระบบทั้งหมด | "Database connection pool exhausted" |

**Best Practice:**
- ใช้ Serilog เป็น logging library สำหรับ .NET
- Log format: structured logging (JSON) ไม่ใช่ plain text
- ทุก log ต้องมี `CorrelationId` สำหรับ distributed tracing
- ห้าม log sensitive data (password, PII, medical certificate content)
- PROD ตั้ง minimum level = Information (ไม่เปิด Debug/Trace)

## 10. Checklist สำหรับ Infrastructure Review

- [ ] เลือก deployment model ที่เหมาะสม (cloud/hybrid/on-premise) พร้อมเหตุผล
- [ ] Server sizing เหมาะสมกับ workload (CPU, RAM, Disk, Network)
- [ ] Database server แยก disk สำหรับ OS, Data, Log, TempDB
- [ ] High Availability ครบ: Web (LB), App (Cluster/Multi-node), DB (Always On)
- [ ] Network zoning ชัดเจน: DMZ, Application Zone, Database Zone, Management Zone
- [ ] แยก environment ชัดเจน (DEV/SIT/UAT/PROD)
- [ ] Secret เก็บใน Azure Key Vault (Cloud) หรือ Windows Credential Manager (On-Premise)
- [ ] Infrastructure as Code (Bicep/Terraform) เก็บใน source control
- [ ] CI/CD pipeline ครบทุก stage พร้อม approval gate สำหรับ PROD
- [ ] Branch policy บังคับ PR review + CI pass
- [ ] Monitoring ครบ: APM, logs, alerts, dashboards
- [ ] Logging standard สอดคล้องกับระดับที่กำหนด
- [ ] Backup strategy ครบ: Full + Differential + Transaction Log + Offsite
- [ ] DR plan ทดสอบแล้ว (ทุกไตรมาส)
- [ ] Server hardening checklist ผ่านครบทุกข้อ
- [ ] OS patching schedule กำหนดและปฏิบัติตาม
- [ ] Antivirus ติดตั้งและ update signature ทุก server
- [ ] Server naming convention สอดคล้องกับมาตรฐาน
