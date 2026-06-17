# Organization Knowledge Base: Architecture Design

## วัตถุประสงค์

เอกสารชุดนี้เป็น Knowledge Base ขององค์กร สำหรับแนวทางการออกแบบ Architecture ในทุกมิติ
ใช้เป็นมาตรฐานอ้างอิงในการออกแบบระบบ โดยยึด Microsoft Technology Stack เป็นแกนหลัก
รองรับทั้ง Azure Cloud และ On-Premise

## สารบัญ

| # | เอกสาร | ขอบเขต | ไฟล์ |
|---|--------|--------|------|
| 1 | Application Architecture | สถาปัตยกรรมแอปพลิเคชัน, 6 Architecture Patterns, UX/UI, API, ตัวอย่าง HASTH DMS | [01-application-architecture/knowledge.md](01-application-architecture/knowledge.md) |
| 2 | Data Architecture | สถาปัตยกรรมข้อมูล, SQL Server Design, Data Access, Migration, Backup, Retention | [02-data-architecture/knowledge.md](02-data-architecture/knowledge.md) |
| 3 | Integration Architecture | สถาปัตยกรรมการเชื่อมต่อ, Sync/Async/Batch Pattern, API Management, Resilience | [03-integration-architecture/knowledge.md](03-integration-architecture/knowledge.md) |
| 4 | Infrastructure Architecture | โครงสร้างพื้นฐาน, On-Premise, Server Design, Azure Cloud, CI/CD, Monitoring | [04-infrastructure-architecture/knowledge.md](04-infrastructure-architecture/knowledge.md) |
| 5 | Security Architecture | ความปลอดภัย, Identity (AD/MFA), Network Zoning, OWASP, Encryption, Compliance | [05-security-architecture/knowledge.md](05-security-architecture/knowledge.md) |

## โครงสร้างการจัดเก็บ

แต่ละมิติของ architecture ถูกจัดเก็บไว้ในโฟลเดอร์ของตัวเอง โดยมีไฟล์หลัก 3 ประเภท:

- `knowledge.md` = องค์ความรู้และมาตรฐานองค์กร
- `output-template.md` = template สำหรับสร้าง output document
- `output-sample.md` = ตัวอย่างผลลัพธ์สำหรับใช้อ้างอิงรูปแบบ

## สารบัญละเอียดแยกตามเอกสาร

### 01 — Application Architecture

| Section | หัวข้อ | เนื้อหาหลัก |
|---------|--------|------------|
| 1 | ภาพรวม | ขอบเขตของ Application Architecture |
| 2 | 6 Software Architecture Patterns | Layered, Microservices, Event-Driven, Client-Server, Plugin-Based, Hexagonal + ตารางเปรียบเทียบ + decision tree |
| 3 | UX/UI Architecture | Angular (แนะนำ), Vue.js, Blazor, Fluent Design System, Responsive, Accessibility, Component Architecture |
| 4 | API Architecture | RESTful API Design Standard, Response Format |
| 5 | State Management | Local / Global / Server / Persistent state |
| 6 | Error Handling Strategy | Frontend + Backend error handling |
| 7 | Checklist | Application Architecture Review checklist |
| 8 | ตัวอย่าง HASTH DMS | 5 layers, 4 business domains, Auth/AuthZ, Cross-Cutting Services, mapping กับ best practice |

### 02 — Data Architecture

| Section | หัวข้อ | เนื้อหาหลัก |
|---------|--------|------------|
| 1 | ภาพรวม | ขอบเขตของ Data Architecture |
| 2 | Database Architecture | Technology Stack, Naming Convention, Data Type Standard, Audit Columns, Schema Design, Indexing Strategy |
| 3 | Data Access Pattern | Entity Framework Core (ORM), Dapper (Micro ORM) |
| 4 | Data Migration & Versioning | EF Core Migrations, DbUp, SSDT, Data Seeding |
| 5 | Data Quality & Governance | Quality Rules, Data Classification (ISO 27001) |
| 6 | Backup & Recovery | Backup Strategy, RPO/RTO, Disaster Recovery |
| 7 | Data Retention & Archival | Retention period ตามประเภทข้อมูล |
| 8 | Checklist | Data Architecture Review checklist |

### 03 — Integration Architecture

| Section | หัวข้อ | เนื้อหาหลัก |
|---------|--------|------------|
| 1 | ภาพรวม | ขอบเขตของ Integration Architecture |
| 2 | Integration Pattern | Synchronous (REST), Asynchronous (Azure Service Bus), Batch (SFTP) |
| 3 | API Management | Azure API Management, API Versioning Strategy |
| 4 | Event Schema Standard | CloudEvents Format |
| 5 | Error Handling & Resilience | Polly .NET (Retry, Circuit Breaker, Timeout, Bulkhead, Fallback), Error Classification |
| 6 | Monitoring & Observability | Integration Monitoring Stack, Key Metrics |
| 7 | Checklist | Integration Architecture Review checklist |

### 04 — Infrastructure Architecture

| Section | หัวข้อ | เนื้อหาหลัก |
|---------|--------|------------|
| 1 | ภาพรวม | ขอบเขต รองรับทั้ง Azure Cloud และ On-Premise |
| 2 | Deployment Model | On-Premise / Cloud / Hybrid + decision tree, OS & Middleware Standard |
| 3 | **On-Premise Architecture** | Architecture Diagram (4 zones), Network Zoning, High Availability, Backup Strategy, เปรียบเทียบ On-Premise vs Cloud |
| 4 | **Server Design** | Physical vs Virtual, Server Sizing (Web/App/DB/File), Production Server Layout, Naming Convention, Hardening Checklist, Azure VM SKU Mapping |
| 5 | Azure Cloud Architecture | Azure resource diagram |
| 6 | Environment Strategy | DEV/SIT/UAT/PROD (Cloud + On-Premise), Configuration best practice |
| 7 | Compute Architecture | Azure App Service, Container Apps, AKS, Functions |
| 8 | CI/CD Pipeline | Azure DevOps Pipeline, Stages, Branch Strategy |
| 9 | Monitoring & Observability | Azure Monitor Stack, Logging Standard |
| 10 | Checklist | Infrastructure Review checklist (18 ข้อ) |

### 05 — Security Architecture

| Section | หัวข้อ | เนื้อหาหลัก |
|---------|--------|------------|
| 1 | ภาพรวม | ขอบเขต อ้างอิง ISO 27001:2022 |
| 2 | Security Layers | Defense in Depth (5 layers) |
| 3 | Identity & Access Management | Microsoft Entra ID (AD Login), MFA (ไม่ใช่ SMS), Application & Database Zoning, RBAC, Auth Flow |
| 4 | Network Security | Network Segmentation, NSG, Private Endpoint, WAF |
| 5 | Application Security | OWASP Top 10, Input Validation, HTTP Security Headers |
| 6 | Data Security | Encryption (TDE, AES-256, TLS 1.2+), Secret Management (Key Vault) |
| 7 | Security Audit & Compliance | Audit Logging (ISO 27001 Annex A 8.15), Security Review Checklist |
| 8 | Compliance Mapping | ISO 27001, PDPA, OWASP ASVS |

## Technology Stack หลักขององค์กร

> อ้างอิงจาก Technology Standard ที่ HQ กำหนด โดยใช้ Microsoft Azure เป็น Cloud Platform หลัก

| Layer | เทคโนโลยีหลัก | ทางเลือกเสริม | หมายเหตุ |
|-------|--------------|---------------|---------|
| Cloud Platform | Microsoft Azure (HQ owned) | — | Azure Cloud Service ที่ HQ เป็นเจ้าของ |
| Frontend | Angular, Vue.js | Framework ที่ compatible กับ HTML5 standard | รองรับทุกอุปกรณ์: PC, Laptop, Mobile |
| Backend | C# .NET Core, ASP.NET | — | รองรับ Low-code Development ร่วมกับ vendor |
| Database | Microsoft SQL Server (Latest Version) | Azure SQL | — |
| Development Approach | Low-code Development / JavaScript Framework / .NET Framework | ตาม vendor recommend | — |
| Identity & Security | Microsoft Entra ID (Azure AD), MFA (ไม่ใช่ SMS) | Native service ของ Azure | AD Login, Application/Database Zoning |
| Operating System | Windows Server (Latest Version) | — | ทั้ง On-Premise และ Cloud VM |
| DevOps | Azure DevOps | — | — |
| Backup | Azure Backup Tool / Windows Server Backup | — | HQ จัดเตรียมให้ |
| Antivirus | HQ provided Antivirus Tool | — | HQ จัดเตรียมให้ |
| Monitoring | Azure Monitor, Application Insights | — | — |
| Reporting | Power BI, SQL Server Reporting Services | — | — |

## แนวทางการใช้เอกสารชุดนี้

1. อ่าน index นี้ก่อนเพื่อเข้าใจภาพรวม
2. เลือกอ่านเอกสารตามหัวข้อที่เกี่ยวข้องกับงานออกแบบ
3. ใช้เป็น checklist ว่าการออกแบบครอบคลุมทุกมิติหรือไม่
4. อ้างอิง best practice และ pattern ที่แนะนำในแต่ละเอกสาร
5. ดูตัวอย่างจริงจาก HASTH DMS ใน Application Architecture (Section 8)
6. สำหรับระบบ Production ที่ต้องอยู่ On-Premise ดู Infrastructure Architecture (Section 3-4)
