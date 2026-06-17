![1776402879033](image/knowledge/1776402879033.png)![1776402890977](image/knowledge/1776402890977.png)# Application Architecture

## 1. ภาพรวม

Application Architecture คือการออกแบบโครงสร้างภายในของแอปพลิเคชัน ครอบคลุมตั้งแต่ pattern การจัดวาง code, การแบ่ง layer, การสื่อสารระหว่าง component ไปจนถึงการออกแบบ UX/UI ที่เป็นส่วนหนึ่งของ Presentation Layer

## 2. Architecture Pattern ที่แนะนำ

## 2. Software Architecture Pattern ที่ต้องรู้

> การเลือก architecture pattern ที่เหมาะสมเป็นสิ่งสำคัญในการแก้ปัญหาอย่างมีประสิทธิภาพ
> ส่วนนี้อธิบาย 6 pattern หลักที่ใช้กันแพร่หลาย พร้อม best practice สำหรับ Microsoft stack
>
> อ้างอิง: ByteByteGo — 6 Software Architectural Patterns You Must Know

### 2.1 Layered Architecture (N-Tier)

แต่ละ layer มีบทบาทที่ชัดเจนและแยกจากกัน เหมาะกับระบบที่ต้องสร้างเร็ว แต่ถ้าไม่มีกฎที่ดี source code อาจไม่เป็นระเบียบ

**เหมาะกับ:** ระบบ CRUD ทั่วไป, ระบบ ERP ภายในองค์กร, ระบบที่ทีมพัฒนาคุ้นเคย

```text
┌─────────────────────────────────────────────┐
│          Presentation Layer                 │
│   (Angular / Vue / Blazor)                  │
├─────────────────────────────────────────────┤
│          Business Layer                     │
│   (ASP.NET Core Web API / Service Layer)    │
├─────────────────────────────────────────────┤
│          Persistence Layer                  │
│   (Entity Framework Core / Dapper)          │
├─────────────────────────────────────────────┤
│          Database Layer                     │
│   (SQL Server / Azure SQL)                  │
└─────────────────────────────────────────────┘
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| เข้าใจง่าย ทีมส่วนใหญ่คุ้นเคย | Source code อาจไม่เป็นระเบียบถ้าไม่มีกฎชัดเจน |
| สร้างได้เร็ว เหมาะกับ MVP | Business logic อาจรั่วไหลไปอยู่ผิด layer |
| แยกความรับผิดชอบชัดเจน | ยากต่อการ scale แยก component |
| ทดสอบแต่ละ layer ได้ | Performance อาจลดลงเมื่อมีหลาย layer |

**Best Practice (Microsoft Stack):**
- แต่ละ layer ต้องสื่อสารผ่าน interface เท่านั้น ห้ามข้าม layer
- ใช้ Dependency Injection (DI) ผ่าน `Microsoft.Extensions.DependencyInjection`
- แยก project ใน Visual Studio Solution ตาม layer เช่น `MyApp.Web`, `MyApp.Application`, `MyApp.Domain`, `MyApp.Infrastructure`
- Business Logic ต้องอยู่ใน Business Layer เท่านั้น ห้ามกระจายไปอยู่ใน Controller หรือ Data Access

### 2.2 Microservices Architecture

แตกระบบขนาดใหญ่ออกเป็น component ย่อย ๆ ที่จัดการได้ง่ายขึ้น แต่ละ service ทำงานอิสระ มี database เป็นของตัวเอง สามารถ deploy และ scale แยกกันได้

**เหมาะกับ:** ระบบขนาดใหญ่ที่มีหลายทีมพัฒนาพร้อมกัน, ระบบที่ต้อง scale แยก component

```text
         ┌──────────────────────────────────┐
         │        API Gateway               │
         │   (Azure API Management)         │
         └──────┬───────┬───────┬───────────┘
                │       │       │
    ┌───────────▼┐ ┌────▼─────┐ ┌▼──────────┐ ┌──────────┐
    │  Sales     │ │ Service  │ │  Parts    │ │ Notifi-  │
    │  Service   │ │ /After-  │ │  Service  │ │ cation   │
    │  (.NET)    │ │ sales    │ │  (.NET)   │ │ Service  │
    │  ┌──────┐  │ │  (.NET)  │ │  ┌──────┐ │ │  (.NET)  │
    │  │ DB   │  │ │  ┌────┐  │ │  │ DB   │ │ └──────────┘
    │  └──────┘  │ │  │ DB │  │ │  └──────┘ │
    └────────────┘ │  └────┘  │ └───────────┘
                   └──────────┘
    ─────────────────────────────────────────────
              Azure Service Bus / Event Grid
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| Fault tolerant — service หนึ่งล่มไม่กระทบทั้งระบบ | เพิ่มความซับซ้อนของระบบ (network, deployment, monitoring) |
| Scale แยก component ได้ | ต้องจัดการ distributed transaction |
| แต่ละทีมพัฒนาอิสระ | Debugging ข้าม service ยากขึ้น |
| Deploy แยกได้ ไม่ต้องรอกัน | ต้องมีทีม DevOps ที่แข็งแกร่ง |

**Best Practice (Microsoft Stack):**
- แต่ละ service มี database เป็นของตัวเอง (Database per Service)
- สื่อสารแบบ asynchronous ผ่าน Azure Service Bus สำหรับ event
- สื่อสารแบบ synchronous ผ่าน REST API เฉพาะกรณี request-response
- ใช้ Azure API Management เป็น API Gateway
- ใช้ Azure Container Apps หรือ Azure Kubernetes Service (AKS) สำหรับ hosting
- ใช้ Dapr (Distributed Application Runtime) เพื่อลดความซับซ้อนของ service-to-service communication

### 2.3 Event-Driven Architecture

Service สื่อสารกันโดยการส่ง event ที่ service อื่นอาจรับหรือไม่รับก็ได้ ส่งเสริม loose coupling ระหว่าง component

**เหมาะกับ:** ระบบที่ต้องการ real-time processing, ระบบ notification, ระบบที่มี workflow หลายขั้นตอน

```text
┌──────────┐         ┌─────────────────────────┐
│ Producer │────────▶│   Event Broker           │
│ (Leave   │  Event  │   (Azure Service Bus)    │
│  Service)│         │                          │
└──────────┘         │   Topic: "leave-events"  │
                     │   ┌───────────────────┐  │
┌──────────┐         │   │ Subscription:     │  │───▶ Email Service
│ Producer │────────▶│   │ "email-notify"    │  │
│ (Approval│  Event  │   ├───────────────────┤  │
│  Service)│         │   │ Subscription:     │  │───▶ Audit Service
└──────────┘         │   │ "audit-log"       │  │
                     │   ├───────────────────┤  │
                     │   │ Subscription:     │  │───▶ HRIS Adapter
                     │   │ "hris-sync"       │  │
                     │   └───────────────────┘  │
                     └─────────────────────────┘
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| Loose coupling — producer ไม่ต้องรู้จัก consumer | ทดสอบ individual component ยากขึ้น |
| เพิ่ม consumer ใหม่ได้โดยไม่กระทบ producer | Debugging event flow ซับซ้อน |
| รองรับ real-time processing | ต้องจัดการ event ordering และ idempotency |
| Scale consumer แยกได้ | Eventual consistency (ไม่ใช่ immediate) |

**Best Practice (Microsoft Stack):**
- ใช้ Azure Service Bus (Topic + Subscription) สำหรับ pub/sub pattern
- ใช้ Azure Event Grid สำหรับ event routing ระดับ infrastructure
- ใช้ CloudEvents specification เป็นมาตรฐาน event format
- Consumer ต้อง idempotent (process ซ้ำได้โดยไม่เกิด side effect)
- เปิด Dead Letter Queue สำหรับ message ที่ process ไม่สำเร็จ
- ใช้ `CorrelationId` ในทุก event สำหรับ distributed tracing

### 2.4 Client-Server Architecture

ประกอบด้วย 2 component หลัก คือ client และ server ที่สื่อสารกันผ่าน network เหมาะกับ real-time services

**เหมาะกับ:** Web application ทั่วไป, ระบบที่ client เป็น browser หรือ mobile app เชื่อมต่อกับ server ส่วนกลาง

```text
┌──────────┐                              ┌──────────────┐
│  Client  │                              │   Server     │
│  (Angular│         Network              │   (ASP.NET   │
│   / Vue) │◄────── HTTPS/REST ──────────▶│    Core API) │
└──────────┘                              │              │
                                          │   ┌────────┐ │
┌──────────┐                              │   │  SQL   │ │
│  Client  │◄────── HTTPS/REST ──────────▶│   │ Server │ │
│  (Mobile │                              │   └────────┘ │
│   Browser│                              └──────────────┘
└──────────┘
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| เหมาะกับ real-time services | Server อาจเป็น single point of failure |
| จัดการ centralized ง่าย | ต้อง scale server เมื่อ client เพิ่ม |
| Security ควบคุมที่ server ได้ | ขึ้นอยู่กับ network availability |
| Client ไม่ต้องมี business logic | Server down = ทุก client ใช้งานไม่ได้ |

**Best Practice (Microsoft Stack):**
- ใช้ Load Balancer (Azure Application Gateway / On-Premise LB) ป้องกัน single point of failure
- ใช้ Health Check endpoint สำหรับ monitoring server status
- Client สื่อสารกับ server ผ่าน REST API (HTTPS) เท่านั้น
- ใช้ SignalR สำหรับ real-time bidirectional communication (เช่น notification, live update)
- ใช้ Azure Front Door หรือ CDN สำหรับ static content delivery

### 2.5 Plugin-Based Architecture

ประกอบด้วย 2 ส่วนหลัก คือ Core System และ Plugin modules ที่เป็น independent component ให้ functionality เฉพาะทาง เหมาะกับระบบที่ต้องขยายได้ตลอดเวลา

**เหมาะกับ:** ระบบที่ต้องเพิ่ม feature ใหม่บ่อย, ระบบ ERP ที่มี module เสริม, ระบบ reporting ที่มี custom report

```text
                    ┌──────────────┐
                    │  Plugin:     │
                    │  Sales Module│
                    └──────┬───────┘
┌──────────────┐           │           ┌──────────────┐
│  Plugin:     │    ┌──────▼───────┐   │  Plugin:     │
│  HR Module   │◄──▶│  Core System │◄─▶│  Parts Module│
└──────────────┘    │  (DMS Engine) │   └──────────────┘
                    └──────▲───────┘
┌──────────────┐           │           ┌──────────────┐
│  Plugin:     │           │           │  Plugin:     │
│  Accounting  │───────────┘           │  Custom      │
│  Module      │                       │  Report      │
└──────────────┘                       └──────────────┘
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| ขยาย feature ได้ง่ายโดยไม่กระทบ core | เปลี่ยนแปลง core ทำได้ยาก |
| แต่ละ plugin พัฒนาและ deploy แยกได้ | ต้องออกแบบ plugin interface ให้ดีตั้งแต่แรก |
| เหมาะกับ third-party extension | Plugin อาจมี compatibility issue กับ core version |
| ลด coupling ระหว่าง module | ต้องจัดการ plugin lifecycle (install, update, remove) |

**Best Practice (Microsoft Stack):**
- ออกแบบ Plugin Interface (contract) ให้ stable ตั้งแต่แรก ใช้ versioning
- ใช้ MEF (Managed Extensibility Framework) หรือ custom plugin loader ใน .NET
- Plugin ต้อง self-contained (มี dependency เป็นของตัวเอง)
- ใช้ NuGet package สำหรับ distribute plugin ภายในองค์กร
- Core system ต้องมี plugin registry สำหรับจัดการ enable/disable plugin

### 2.6 Hexagonal Architecture (Ports and Adapters)

สร้าง abstraction layer ที่ปกป้อง core ของแอปพลิเคชันและแยกออกจาก external integration เพื่อ modularity ที่ดีขึ้น หรือเรียกอีกชื่อว่า Clean Architecture ในแนวคิดเดียวกัน

**เหมาะกับ:** ระบบที่ต้องการ testability สูง, ระบบที่มีแผนเปลี่ยน database หรือ UI framework ในอนาคต

```text
                    ┌──────────────┐
                    │   Adapter:   │
                    │   Web API    │
                    │   (ASP.NET)  │
                    └──────┬───────┘
                           │ Port: HTTP
┌──────────────┐    ┌──────▼───────────────┐    ┌──────────────┐
│   Adapter:   │    │                      │    │   Adapter:   │
│   Database   │◄──▶│    Core Domain       │◄──▶│   Message    │
│   (EF Core / │    │                      │    │   Queue      │
│    SQL Server│    │  Entities            │    │   (Service   │
│   )          │    │  Value Objects       │    │    Bus)      │
└──────────────┘    │  Domain Services     │    └──────────────┘
                    │  Use Cases           │
┌──────────────┐    │  Business Rules      │    ┌──────────────┐
│   Adapter:   │    │                      │    │   Adapter:   │
│   File       │◄──▶│  Ports (Interfaces)  │◄──▶│   External   │
│   Storage    │    │                      │    │   API        │
│   (Blob)     │    └──────────────────────┘    │   (HRIS)     │
└──────────────┘                                └──────────────┘
```

| ข้อดี | ข้อจำกัด |
|-------|---------|
| Core domain ไม่ขึ้นกับ framework ใด ๆ | เพิ่มเวลาพัฒนาและ learning curve |
| เปลี่ยน database, UI, external system ได้โดยไม่กระทบ core | มี boilerplate code มากขึ้น (interfaces, adapters) |
| Testability สูงมาก (mock adapter ได้) | ทีมต้องเข้าใจ concept ดีพอ |
| Modularity สูง | อาจ over-engineering สำหรับระบบเล็ก |

**Best Practice (Microsoft Stack):**
- Domain Layer ต้องไม่มี dependency กับ framework ใด ๆ (ไม่ reference Entity Framework, ASP.NET)
- Port = Interface ประกาศใน Domain Layer, Adapter = Implementation อยู่ใน Infrastructure Layer
- ใช้ MediatR สำหรับ CQRS pattern (Command/Query separation)
- ใช้ AutoMapper หรือ Mapster สำหรับ mapping ระหว่าง layer
- Repository Interface (Port) ประกาศใน Domain, Repository Implementation (Adapter) อยู่ใน Infrastructure

### 2.7 สรุปเปรียบเทียบ 6 Architecture Patterns

| เกณฑ์ | Layered | Microservices | Event-Driven | Client-Server | Plugin-Based | Hexagonal |
|-------|---------|---------------|-------------|---------------|-------------|-----------|
| ความซับซ้อน | ต่ำ | สูง | กลาง-สูง | ต่ำ | กลาง | กลาง-สูง |
| Coupling | ปานกลาง | ต่ำ | ต่ำมาก | ปานกลาง | ต่ำ | ต่ำมาก |
| Scalability | ทั้งระบบ | แยก component | แยก consumer | ต้อง scale server | แยก plugin | ทั้งระบบ |
| Testability | ปานกลาง | สูง | ยาก (event flow) | ปานกลาง | ปานกลาง | สูงมาก |
| เวลาพัฒนา | เร็ว | ช้า | ปานกลาง | เร็ว | ปานกลาง | ปานกลาง |
| Learning Curve | ต่ำ | สูง | กลาง | ต่ำ | กลาง | สูง |
| Fault Tolerance | ต่ำ | สูง | สูง | ต่ำ (SPOF) | ปานกลาง | ปานกลาง |

### 2.8 แนวทางเลือก Pattern สำหรับองค์กร

```text
┌─────────────────────────────────────────────────────────────────┐
│  คำถามช่วยตัดสินใจ Architecture Pattern                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ระบบเป็น Web App ทั่วไป + ทีมเล็ก (1-2 ทีม)?                │
│  ├── ใช่ → Layered Architecture ✅ (เริ่มที่นี่)               │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ต้องการเปลี่ยน DB/UI ในอนาคต + ต้องการ testability สูง?      │
│  ├── ใช่ → Hexagonal Architecture                              │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ระบบใหญ่ + หลายทีม + ต้อง scale แยก component?               │
│  ├── ใช่ → Microservices Architecture                          │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ต้องการ real-time event processing + loose coupling?          │
│  ├── ใช่ → Event-Driven Architecture                           │
│  └── ไม่ → ต่อข้อถัดไป                                         │
│                                                                 │
│  ระบบต้องขยาย module/feature บ่อย + third-party plugin?       │
│  ├── ใช่ → Plugin-Based Architecture                           │
│  └── ไม่ → Client-Server Architecture (default)               │
│                                                                 │
│  💡 สามารถผสม pattern ได้ เช่น:                                │
│     Layered + Event-Driven (ใช้ event สำหรับ async task)       │
│     Microservices + Event-Driven (service สื่อสารผ่าน event)   │
│     Hexagonal + Plugin-Based (core + extensible modules)       │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

## 3. UX/UI Architecture (ภายใน Presentation Layer)

### 3.1 ภาพรวม UX/UI Architecture

UX/UI Architecture เป็นส่วนหนึ่งของ Presentation Layer ที่ออกแบบโครงสร้างของ frontend application

### 3.2 Frontend Architecture Pattern

#### 3.2.1 Angular (องค์กรแนะนำ)

**Angular** — เหมาะกับระบบ enterprise ที่ต้องการ structure ชัดเจน, TypeScript built-in, รองรับ HTML5 standard

```text
┌────────────────────────────────────────────┐
│  Angular SPA (TypeScript + HTML5)          │
│  ├── modules/        (Feature Modules)     │
│  ├── components/     (Reusable UI)         │
│  ├── services/       (API Calls + Logic)   │
│  ├── guards/         (Route Guards)        │
│  ├── interceptors/   (HTTP Interceptors)   │
│  ├── models/         (TypeScript Interfaces)│
│  └── store/          (State Mgmt - NgRx)   │
└────────────────────┬───────────────────────┘
                     │ REST API (HTTPS)
┌────────────────────┴───────────────────────┐
│  ASP.NET Core Web API (C# .NET Core)      │
└────────────────────────────────────────────┘
```

**รองรับทุกอุปกรณ์:** PC, Laptop, Tablet, Mobile ผ่าน Responsive Web Design (HTML5 standard)

**Best Practice Angular + .NET:**
- ใช้ TypeScript strict mode เสมอ
- ใช้ Angular Material หรือ PrimeNG เป็น UI component library
- จัดโครงสร้างเป็น Feature Module (Lazy Loading) เพื่อ performance
- State management ใช้ NgRx (Redux pattern) สำหรับ complex state หรือ RxJS BehaviorSubject สำหรับ simple state
- API client ใช้ Angular HttpClient พร้อม Interceptor สำหรับ auth token, error handling, loading state
- Authentication ใช้ MSAL Angular (@azure/msal-angular) สำหรับ Microsoft Entra ID
- ใช้ Angular CLI สำหรับ scaffolding, build, test
- Responsive layout ใช้ Angular Flex-Layout หรือ CSS Grid/Flexbox

#### 3.2.2 Vue.js (ทางเลือกเสริม)

**Vue.js** — เหมาะกับระบบที่ต้องการ learning curve ต่ำ, flexible, compatible กับ HTML5 standard

```text
┌────────────────────────────────────────────┐
│  Vue.js SPA (TypeScript + HTML5)           │
│  ├── views/          (Route Pages)         │
│  ├── components/     (Reusable UI)         │
│  ├── composables/    (Composition API)     │
│  ├── services/       (API Calls)           │
│  ├── stores/         (State Mgmt - Pinia)  │
│  └── types/          (TypeScript Types)    │
└────────────────────┬───────────────────────┘
                     │ REST API (HTTPS)
┌────────────────────┴───────────────────────┐
│  ASP.NET Core Web API (C# .NET Core)      │
└────────────────────────────────────────────┘
```

**Best Practice Vue.js + .NET:**
- ใช้ Vue 3 + Composition API + TypeScript เสมอ
- ใช้ Vuetify หรือ PrimeVue เป็น UI component library
- State management ใช้ Pinia (official Vue store)
- API client ใช้ Axios พร้อม interceptor สำหรับ auth token
- Authentication ใช้ MSAL Browser (@azure/msal-browser) สำหรับ Microsoft Entra ID
- ใช้ Vite เป็น build tool (เร็วกว่า Webpack)

#### 3.2.3 Blazor (ทางเลือกสำหรับทีม .NET)

**Blazor Server** — เหมาะกับระบบ intranet ที่ network latency ต่ำ และทีมถนัด C# มากกว่า JavaScript

```text
┌────────────────────────────────┐
│  Browser                       │
│  ┌──────────────────────────┐  │
│  │  Blazor UI (Razor)       │  │
│  │  ← SignalR Connection →  │  │
│  └──────────────────────────┘  │
└────────────────────┬───────────┘
                     │ SignalR (WebSocket)
┌────────────────────┴───────────┐
│  Server                        │
│  ┌──────────────────────────┐  │
│  │  Blazor Component Tree   │  │
│  │  + Application State     │  │
│  │  + ASP.NET Core Pipeline │  │
│  └──────────────────────────┘  │
└────────────────────────────────┘
```

**Best Practice Blazor:**
- ใช้ Component-based architecture: แยก component เป็น reusable ย่อย ๆ
- ใช้ Fluent UI Blazor หรือ MudBlazor เป็น UI component library
- จัดโครงสร้าง folder ตาม feature ไม่ใช่ตาม type
- State management ใช้ Fluxor (Redux pattern) หรือ Cascading Parameters

#### 3.2.4 แนวทางเลือก Frontend Framework

| เกณฑ์ | Angular (แนะนำ) | Vue.js | Blazor |
|-------|-----------------|--------|--------|
| HTML5 Compatible | ✅ เต็มรูปแบบ | ✅ เต็มรูปแบบ | ✅ (ผ่าน Razor) |
| Multi-device (PC/Mobile/Laptop) | ✅ Responsive | ✅ Responsive | ✅ Responsive |
| TypeScript Support | ✅ Built-in | ✅ รองรับ | ❌ ใช้ C# |
| Enterprise Structure | ✅ สูง (Module-based) | ปานกลาง | ปานกลาง |
| Learning Curve | ปานกลาง | ต่ำ | ต่ำ (สำหรับ .NET dev) |
| Microsoft Entra ID (MSAL) | ✅ @azure/msal-angular | ✅ @azure/msal-browser | ✅ Built-in |
| Low-code Compatible | ✅ ใช้ร่วมกับ Power Apps ได้ | ✅ | ✅ |
| Vendor Ecosystem | กว้าง | กว้าง | Microsoft-centric |

### 3.3 UX/UI Design System

#### 3.3.1 Microsoft Fluent Design System

องค์กรที่ใช้ Microsoft stack ควรยึด Fluent Design System เป็นมาตรฐาน

**หลักการออกแบบ 5 ข้อของ Fluent:**

| หลักการ | คำอธิบาย | ตัวอย่างการนำไปใช้ |
|---------|----------|-------------------|
| Light | ใช้แสงและเงาสร้าง depth | Elevation (shadow) แยก layer ของ UI element |
| Depth | สร้างความลึกเพื่อจัดลำดับความสำคัญ | Dialog overlay, flyout menu |
| Motion | ใช้ animation อย่างมีจุดประสงค์ | Transition เมื่อเปิด/ปิด panel |
| Material | วัสดุดิจิทัลที่สะท้อนแสง | Acrylic background, Mica material |
| Scale | รองรับทุกขนาดหน้าจอ | Responsive breakpoints |

#### 3.3.2 Responsive Breakpoints มาตรฐาน

| Breakpoint | ขนาดหน้าจอ | Layout |
|------------|-----------|--------|
| Small | < 640px | Single column, stacked |
| Medium | 640px - 1023px | Two column, compact navigation |
| Large | 1024px - 1365px | Full navigation, side panel |
| X-Large | ≥ 1366px | Full layout, expanded content |

#### 3.3.3 Accessibility มาตรฐาน

| เกณฑ์ | มาตรฐาน | รายละเอียด |
|-------|---------|-----------|
| WCAG Level | AA (ขั้นต่ำ) | ตาม WCAG 2.1 Level AA |
| Color Contrast | ≥ 4.5:1 (normal text) | ใช้เครื่องมือ Accessibility Insights for Web ตรวจสอบ |
| Keyboard Navigation | Tab order ครบทุก interactive element | ทดสอบด้วย keyboard เท่านั้น |
| Screen Reader | รองรับ Narrator (Windows built-in) | ใช้ ARIA labels ครบทุก element |
| Focus Indicator | มองเห็นได้ชัดเจน | ใช้ Fluent UI built-in focus style |

### 3.4 Component Architecture

```text
┌─────────────────────────────────────────────────┐
│  App Shell                                      │
│  ├── Navigation (Top Bar / Side Nav)            │
│  ├── Layout Container                           │
│  │   ├── Page Header (Breadcrumb, Title)        │
│  │   ├── Content Area                           │
│  │   │   ├── Form Components                    │
│  │   │   │   ├── Input Fields                   │
│  │   │   │   ├── Select / Dropdown              │
│  │   │   │   ├── Date Picker                    │
│  │   │   │   └── File Upload                    │
│  │   │   ├── Data Display Components            │
│  │   │   │   ├── Data Table / Grid              │
│  │   │   │   ├── Card                           │
│  │   │   │   └── Chart                          │
│  │   │   └── Action Components                  │
│  │   │       ├── Button (Primary/Secondary)     │
│  │   │       ├── Command Bar                    │
│  │   │       └── Dialog / Confirmation          │
│  │   └── Footer                                 │
│  └── Toast / Notification Area                  │
└─────────────────────────────────────────────────┘
```

**Best Practice Component Architecture:**
- ใช้ Atomic Design: Atoms → Molecules → Organisms → Templates → Pages
- แต่ละ component ต้อง self-contained (มี style, logic, test เป็นของตัวเอง)
- ใช้ Design Token จาก Fluent UI สำหรับ color, spacing, typography (ห้าม hardcode ค่า)
- Form validation ใช้ FluentValidation (Backend) + client-side validation (Frontend)
- Error state ของ form ต้องแสดง inline ใต้ field ที่ผิด ไม่ใช่รวมด้านบน

## 4. API Architecture

### 4.1 RESTful API Design Standard

| เกณฑ์ | มาตรฐาน |
|-------|---------|
| Specification | OpenAPI 3.0 (Swagger) |
| Versioning | URL path versioning: `/api/v1/`, `/api/v2/` |
| Naming | ใช้ kebab-case สำหรับ URL: `/api/v1/leave-requests` |
| HTTP Methods | GET (อ่าน), POST (สร้าง), PUT (แก้ไขทั้งหมด), PATCH (แก้ไขบางส่วน), DELETE (ลบ) |
| Status Codes | 200 OK, 201 Created, 204 No Content, 400 Bad Request, 401 Unauthorized, 403 Forbidden, 404 Not Found, 409 Conflict, 500 Internal Server Error |
| Response Format | JSON with camelCase property names |
| Pagination | `?page=1&pageSize=20` พร้อม response header `X-Total-Count` |
| Filtering | `?status=pending&leaveType=annual` |
| Sorting | `?sortBy=createdDate&sortOrder=desc` |

### 4.2 API Response Standard

```json
{
  "success": true,
  "data": { },
  "error": null,
  "metadata": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

**Error Response:**

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "ข้อมูลไม่ถูกต้อง",
    "details": [
      { "field": "startDate", "message": "วันที่เริ่มลาต้องไม่เป็นอดีต" }
    ]
  }
}
```

## 5. State Management

| สถานการณ์ | แนวทาง | เครื่องมือ |
|-----------|--------|-----------|
| State เฉพาะ component | Local state | Blazor: `@bind` / React: `useState` |
| State ข้าม component (parent-child) | Props / Parameters | Blazor: `[Parameter]` / React: props |
| State ข้าม component (ไม่ใช่ parent-child) | Global state | Blazor: Fluxor / React: Zustand |
| State จาก server (API data) | Server state | Blazor: Service injection / React: TanStack Query |
| State ถาวร (persist ข้ามหน้า) | Persistent state | Browser localStorage / sessionStorage |

## 6. Error Handling Strategy

### 6.1 Frontend Error Handling

| ประเภท | การจัดการ | ตัวอย่าง |
|--------|----------|---------|
| Validation Error | แสดง inline ใต้ field | "กรุณากรอกวันที่เริ่มลา" |
| API Error (4xx) | แสดง toast / message bar | "ไม่สามารถบันทึกข้อมูลได้" |
| Network Error | แสดง retry dialog | "ไม่สามารถเชื่อมต่อ server ได้ กรุณาลองใหม่" |
| Unexpected Error | แสดง error boundary page | "เกิดข้อผิดพลาด กรุณาติดต่อผู้ดูแลระบบ" |

### 6.2 Backend Error Handling

- ใช้ Global Exception Handler middleware ของ ASP.NET Core
- ห้าม expose stack trace หรือ internal error detail ให้ client
- Log ทุก exception ด้วย Serilog → Azure Application Insights
- ใช้ Problem Details (RFC 7807) เป็น error response format

## 7. Checklist สำหรับ Application Architecture Review

- [ ] เลือก architecture pattern ที่เหมาะกับขนาดและความซับซ้อนของระบบ
- [ ] แยก layer/project ใน solution ชัดเจน
- [ ] ใช้ Dependency Injection ทุก layer
- [ ] Business Logic อยู่ใน Domain/Application Layer เท่านั้น
- [ ] API ออกแบบตาม RESTful standard พร้อม OpenAPI spec
- [ ] Frontend ใช้ Component-based architecture
- [ ] ใช้ Fluent Design System เป็นมาตรฐาน UI
- [ ] Responsive design ครอบคลุม breakpoints ที่กำหนด
- [ ] Accessibility ผ่าน WCAG 2.1 Level AA ขั้นต่ำ
- [ ] Error handling ครบทั้ง frontend และ backend
- [ ] State management pattern ชัดเจน

---

## 8. ตัวอย่างการออกแบบ Application Architecture: HASTH DMS

> ส่วนนี้เป็นตัวอย่างจริงจากโปรเจค HASTH DMS (Dealer Management System) เพื่อแสดงให้เห็นว่า best practice ที่อธิบายไว้ข้างต้นถูกนำไปใช้จริงอย่างไร

### 8.1 Architecture Overview

HASTH DMS ออกแบบโดยใช้ **Layered + Service-Oriented Architecture** โดยแยกความรับผิดชอบระหว่าง User Interface, Business Logic, Data Management, และ System Integration อย่างชัดเจน

```text
┌─────────────────────────────────────────────────────────────────────┐
│                    HASTH DMS — Application Architecture              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Layer 1: Presentation Layer (Frontend)                      │  │
│  │  ┌─────────────────┐  ┌─────────────────┐                   │  │
│  │  │  Dealer Portal  │  │  HQ Portal      │                   │  │
│  │  │  (Web App)      │  │  (Web App)      │                   │  │
│  │  └────────┬────────┘  └────────┬────────┘                   │  │
│  │           │  REST API (HTTPS)  │                             │  │
│  └───────────┴────────────────────┴─────────────────────────────┘  │
│                          │                                         │
│  ┌───────────────────────┴───────────────────────────────────────┐  │
│  │  Layer 2: Business Logic Layer (Backend Services)            │  │
│  │  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐   │  │
│  │  │  Sales   │ │ Service/ │ │  Parts   │ │  Accounting  │   │  │
│  │  │  Service │ │ After-   │ │  Service │ │  Service     │   │  │
│  │  │          │ │ sales    │ │          │ │  (AR/AP/GL)  │   │  │
│  │  └────┬─────┘ └────┬─────┘ └────┬─────┘ └──────┬───────┘   │  │
│  │       │             │            │              │            │  │
│  │  ┌────┴─────────────┴────────────┴──────────────┴────────┐  │  │
│  │  │  Cross-Cutting Services                               │  │  │
│  │  │  (Logging, Error Handling, Audit Trail, API Version,  │  │  │
│  │  │   Monitoring, Auth/AuthZ)                              │  │  │
│  │  └───────────────────────────────────────────────────────┘  │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                          │                                         │
│  ┌───────────────────────┴───────────────────────────────────────┐  │
│  │  Layer 3: Integration Layer                                  │  │
│  │  ┌─────┐ ┌─────┐ ┌─────┐ ┌──────┐ ┌─────┐ ┌──────────┐    │  │
│  │  │ CRM │ │ LMS │ │ PMS │ │ AFS  │ │ CMS │ │ VHC      │    │  │
│  │  └─────┘ └─────┘ └─────┘ └──────┘ └─────┘ │ Survey   │    │  │
│  │                                             └──────────┘    │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                          │                                         │
│  ┌───────────────────────┴───────────────────────────────────────┐  │
│  │  Layer 4: Data Layer                                         │  │
│  │  ┌──────────────────────────────────────────────────────┐    │  │
│  │  │  Central DMS Database                                │    │  │
│  │  │  (Transactional + Historical Data)                   │    │  │
│  │  └──────────────────────────────────────────────────────┘    │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                          │                                         │
│  ┌───────────────────────┴───────────────────────────────────────┐  │
│  │  Layer 5: Reporting & Dashboard Layer                        │  │
│  │  (Read-only, Dedicated Views / Reporting APIs)               │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │  │
│  │  │ Dealer KPI   │  │ HQ Mgmt     │  │ Operational  │       │  │
│  │  │ Dashboard    │  │ Reports     │  │ Summaries    │       │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘       │  │
│  └─────────────────────────────────────────────────────────────┘  │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

**เป้าหมายของ Architecture นี้:**

| เป้าหมาย | วิธีที่ Architecture ตอบโจทย์ |
|----------|------------------------------|
| Modernization จาก Legacy DMS | แยก layer ชัดเจน สามารถ migrate ทีละส่วนได้ |
| Maintainability | Business Logic รวมศูนย์ใน Backend Services ไม่กระจายไปหลายที่ |
| Reduced Coupling | แต่ละ Business Domain เป็น independent service มี API เป็นของตัวเอง |
| Flexibility | เพิ่ม module ใหม่ได้โดยไม่กระทบ module เดิม |
| Long-term Evolution | รองรับการเปลี่ยน technology stack ในอนาคตเพราะแยก layer ชัดเจน |

### 8.2 Presentation Layer (Frontend)

#### วัตถุประสงค์
ให้บริการ User Interface สำหรับ Dealer users และ HQ internal users

#### หลักการออกแบบ

| หลักการ | รายละเอียด |
|---------|-----------|
| Web-based | ทำงานผ่าน browser รองรับ desktop, laptop, tablet, mobile |
| UI-only responsibility | รับผิดชอบเฉพาะการแสดงผลและรับ input จากผู้ใช้ |
| ไม่มี business logic | ห้ามใส่ business rule ใน frontend |
| ไม่เข้าถึง database โดยตรง | ทุกการทำงานต้องผ่าน Backend API เท่านั้น |

#### ความรับผิดชอบ

```text
Presentation Layer
├── แสดงหน้าจอและฟอร์ม
├── รับ input จากผู้ใช้
├── แสดงผลลัพธ์ รายงาน และ dashboard
├── เรียก Backend API สำหรับทุก operation
└── จัดการ UI state (loading, error, success)
```

#### การ map กับ Best Practice ขององค์กร

| Best Practice (Section 3) | การนำไปใช้ใน HASTH DMS |
|---------------------------|----------------------|
| Component-based architecture | แยก component เป็น reusable ย่อย ๆ ตาม Atomic Design |
| HTML5 Compatible Frontend | ใช้ Angular หรือ Vue.js ที่ compatible กับ HTML5 standard |
| Responsive breakpoints | รองรับทุกอุปกรณ์: PC, Laptop, Tablet, Mobile |
| Accessibility | WCAG 2.1 Level AA, keyboard navigation, contrast ratio ≥ 4.5:1 |
| State management | Local state สำหรับ component, Global state (NgRx/Pinia) สำหรับ cross-component |

### 8.3 Business Logic Layer (Backend Services)

#### วัตถุประสงค์
รวมศูนย์ business rules และ processing logic ทั้งหมดไว้ที่เดียว

#### หลักการออกแบบ

| หลักการ | รายละเอียด |
|---------|-----------|
| Centralized business logic | ทุก business rule อยู่ใน layer นี้เท่านั้น |
| Consistent behavior | ทุก channel (web, API, reporting) ได้ผลลัพธ์เดียวกัน |
| Reusable | หลาย frontend หรือ integration สามารถเรียกใช้ service เดียวกันได้ |
| Testable | แยก business logic ออกจาก infrastructure ทำให้ unit test ได้ง่าย |

#### Core Business Domains

แต่ละ domain ถูกออกแบบเป็น **independent service** ที่มี API เป็นของตัวเอง:

```text
Business Logic Layer
├── Sales Service
│   ├── Prospect Management
│   ├── Vehicle Sales
│   ├── Retail Sale
│   └── Delivery & Registration
│
├── Service / Aftersales Service
│   ├── Service Appointment
│   ├── Repair Order
│   ├── Warranty & Claim Processing
│   └── Service History Management
│
├── Parts Service
│   ├── Parts Ordering
│   ├── Parts Stock Management
│   ├── Counter Sales
│   └── Emergency / VOR Orders
│
└── Accounting Service
    ├── Accounts Receivable (AR)
    ├── Accounts Payable (AP)
    ├── General Ledger (GL)
    └── Financial Postings from DMS Transactions
```

#### การ map กับ Architecture Pattern ขององค์กร

| Pattern (Section 2) | การนำไปใช้ใน HASTH DMS |
|---------------------|----------------------|
| Layered Architecture | แยก Presentation → Business Logic → Data Access → Database ชัดเจน |
| Service-Oriented | แต่ละ Business Domain เป็น independent service มี API แยก |
| Dependency Injection | ใช้ DI สำหรับทุก service dependency |
| Interface-based communication | แต่ละ layer สื่อสารผ่าน interface ไม่ข้าม layer |

### 8.4 Integration Layer

#### วัตถุประสงค์
จัดการการสื่อสารระหว่าง DMS กับระบบภายนอกอย่างมีมาตรฐานและควบคุมได้

#### วิธีการเชื่อมต่อ

| วิธี | เมื่อใดใช้ | ตัวอย่าง |
|------|----------|---------|
| API-based integration | Realtime data exchange | CRM, LMS, Aftersales System |
| File-based integration | Batch data transfer ตามรอบเวลา | PMS, Accounting export |

#### ระบบที่เชื่อมต่อ

```text
Integration Layer
├── CRM (Customer Relationship Management)
├── LMS — Sales (Lead Management System)
├── LMS — Aftersales
├── PMS (Parts Management System)
├── AFS (Aftersales System)
├── CMS (Content Management System)
├── VHC (Vehicle Health Check)
├── Survey (Customer Satisfaction Survey)
└── Reporting & Dashboard Tools
```

#### กฎการออกแบบ Integration

| กฎ | เหตุผล |
|----|--------|
| DMS เป็น core system สำหรับ dealer operations | ป้องกันการกระจาย business logic ไปหลายระบบ |
| ห้ามฝัง external system logic ใน DMS business logic | ลด coupling ระหว่างระบบ |
| การเปลี่ยนแปลงระบบภายนอกต้องไม่กระทบ DMS มาก | ใช้ adapter pattern แยก integration logic ออกจาก core |

#### การ map กับ Integration Architecture ขององค์กร

| Best Practice (03-integration-architecture.md) | การนำไปใช้ใน HASTH DMS |
|-----------------------------------------------|----------------------|
| API Gateway (Azure API Management) | ทุก external API ผ่าน gateway |
| Resilience pattern (Polly) | Retry, Circuit Breaker, Timeout สำหรับ external call |
| CloudEvents format | Event schema มาตรฐานสำหรับ async messaging |
| Batch file transfer (SFTP) | File-based integration ใช้ SFTP + checksum |

### 8.5 Data Layer

#### วัตถุประสงค์
จัดเก็บข้อมูลทั้งหมดของ DMS สำหรับ operations และ history

#### หลักการออกแบบ

| หลักการ | รายละเอียด |
|---------|-----------|
| Central DMS database | ฐานข้อมูลกลางเดียวสำหรับทุก business domain |
| Transactional + Historical | รองรับทั้งข้อมูลธุรกรรมปัจจุบันและข้อมูลย้อนหลัง |
| Logical ownership per domain | แต่ละ business domain มี schema หรือ table group เป็นของตัวเอง |
| Access ผ่าน Backend Services เท่านั้น | ห้าม frontend หรือ external system เข้าถึง database โดยตรง |

```text
Data Layer — Access Control
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Frontend    │     │  External    │     │  Reporting   │
│  (Blazor/    │     │  System      │     │  Tool        │
│   React)     │     │  (CRM, LMS)  │     │  (Power BI)  │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                    │                    │
       │  ✅ ผ่าน API      │  ✅ ผ่าน API      │  ✅ ผ่าน View/API
       │                    │                    │
┌──────▼────────────────────▼────────────────────▼───────┐
│              Backend Services (API Layer)               │
└──────────────────────────┬─────────────────────────────┘
                           │  ✅ เข้าถึงได้
┌──────────────────────────▼─────────────────────────────┐
│              Central DMS Database                       │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │  Sales   │ │ Service  │ │  Parts   │ │ Account  │  │
│  │  Schema  │ │ Schema   │ │  Schema  │ │ Schema   │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
└────────────────────────────────────────────────────────┘
```

**กฎสำคัญ:** ทุกการเข้าถึงข้อมูลต้องผ่าน Backend Services เท่านั้น เพื่อรับประกัน data consistency และ security

### 8.6 Reporting & Dashboard Layer

#### วัตถุประสงค์
ให้ข้อมูลเชิงวิเคราะห์และภาพรวมการดำเนินงาน โดยไม่กระทบ transaction processing

#### หลักการออกแบบ

| หลักการ | รายละเอียด |
|---------|-----------|
| Read-only access | ไม่มีการเขียนข้อมูลจาก reporting layer |
| Dedicated views / APIs | ใช้ view หรือ reporting API แยกจาก transaction API |
| แยกจาก transaction processing | ป้องกัน report query กระทบ performance ของ transaction |

#### ประเภทรายงาน

| ประเภท | ผู้ใช้ | ตัวอย่าง |
|--------|-------|---------|
| Dealer KPI Dashboard | Dealer Manager | ยอดขาย, จำนวน service, parts turnover |
| HQ Management Reports | HQ Management | ภาพรวมเครือข่าย dealer, performance comparison |
| Operational Summaries | Dealer Staff, HQ Staff | รายงานประจำวัน, สรุปงานค้าง |

### 8.7 Authentication & Authorization

#### Authentication (การยืนยันตัวตน)

| เกณฑ์ | รายละเอียด |
|-------|-----------|
| วิธีการ | Central authentication mechanism (Microsoft Entra ID) |
| จุดเดียว | จัดการที่ส่วนกลาง ไม่กระจายไปแต่ละ service |
| Token | JWT (Access Token + Refresh Token) |

#### Authorization (การกำหนดสิทธิ์)

| เกณฑ์ | รายละเอียด |
|-------|-----------|
| Model | Role-Based Access Control (RBAC) |
| มิติที่ 1 | User Role (เช่น Sales Staff, Service Advisor, Parts Manager, Accountant) |
| มิติที่ 2 | Dealer / Outlet Scope (เห็นข้อมูลเฉพาะ dealer ของตนเอง) |
| มิติที่ 3 | HQ Roles (เห็นข้อมูลข้าม dealer ตามสิทธิ์) |
| จุดบังคับ | Authorization ถูก enforce ภายใน Backend Business Services ไม่ใช่ที่ frontend |

```text
Authentication & Authorization Flow
┌──────────┐    ┌──────────────┐    ┌──────────────┐    ┌──────────┐
│  User    │    │  Frontend    │    │  Auth        │    │  Backend │
│          │    │  (Web App)   │    │  Service     │    │  Service │
└────┬─────┘    └──────┬───────┘    └──────┬───────┘    └────┬─────┘
     │                 │                   │                  │
     │  1. Login       │                   │                  │
     │────────────────▶│                   │                  │
     │                 │  2. Authenticate  │                  │
     │                 │──────────────────▶│                  │
     │                 │  3. Token (JWT)   │                  │
     │                 │◀──────────────────│                  │
     │                 │                   │                  │
     │                 │  4. API call      │                  │
     │                 │  + Bearer token   │                  │
     │                 │──────────────────────────────────────▶
     │                 │                   │  5. Validate     │
     │                 │                   │  token           │
     │                 │                   │  6. Check role   │
     │                 │                   │  + dealer scope  │
     │                 │  7. Response      │                  │
     │                 │◀──────────────────────────────────────
     │  8. แสดงผล     │                   │                  │
     │◀────────────────│                   │                  │
```

### 8.8 Cross-Cutting Application Services

บริการที่ใช้ร่วมกันทุก module ไม่ได้เป็นของ business domain ใด domain หนึ่ง:

| Service | หน้าที่ | เครื่องมือ Microsoft |
|---------|--------|---------------------|
| Application Logging | บันทึก log ทุก operation | Serilog → Azure Application Insights |
| Error Handling | จัดการ exception แบบรวมศูนย์ | ASP.NET Core Global Exception Handler |
| Audit Trail | บันทึกประวัติการเปลี่ยนแปลงข้อมูล | Custom audit middleware + SQL Server |
| API Version Management | จัดการ version ของ API | ASP.NET Core API Versioning + Azure APIM |
| Monitoring Hooks | ติดตามสถานะระบบ | Azure Monitor + Application Insights |
| Authentication | ยืนยันตัวตนผู้ใช้ | Microsoft Entra ID + MSAL |
| Authorization | ตรวจสอบสิทธิ์การเข้าถึง | ASP.NET Core Authorization + Claims-based |

### 8.9 สรุปประโยชน์ของ Architecture

| ประโยชน์ | คำอธิบาย | Best Practice ที่เกี่ยวข้อง |
|----------|---------|---------------------------|
| รองรับ Legacy Modernization | แยก layer ชัดเจน migrate ทีละส่วนได้ | Section 2.1 Layered Architecture |
| Modular Development | แต่ละ business domain พัฒนาแยกกันได้ | Section 2.2 Clean Architecture |
| Reduced Complexity & Coupling | แต่ละ service มี API เป็นของตัวเอง ไม่พึ่งพากัน | Section 2.4 แนวทางเลือก Pattern |
| Improved Stability | Cross-cutting services จัดการ error, logging, monitoring รวมศูนย์ | Section 6 Error Handling Strategy |
| Long-term Evolution | เปลี่ยน technology stack ได้โดยไม่กระทบ business logic | Section 2.2 Clean Architecture |
| Consistent UX | ใช้ Fluent Design System เป็นมาตรฐาน UI ทุก portal | Section 3.3 UX/UI Design System |
| Security by Design | Authentication/Authorization enforce ที่ backend ทุก service | Section 3.2 Frontend Architecture |

### 8.10 Mapping กับ Best Practice ขององค์กร (สรุป)

| Layer ใน HASTH DMS | Best Practice ขององค์กร (Section) | เครื่องมือ |
|-------------------|----------------------------------|-----------|
| Presentation Layer | Section 3: UX/UI Architecture | Angular / Vue.js (HTML5 compatible) — รองรับ PC, Laptop, Mobile |
| Business Logic Layer | Section 2: Architecture Pattern (Layered + Service-Oriented) | C# .NET Core / ASP.NET Web API |
| Integration Layer | 03-integration-architecture.md | Azure API Management + Azure Service Bus |
| Data Layer | 02-data-architecture.md | MS SQL Server (Latest Version) + Entity Framework Core |
| Reporting Layer | 02-data-architecture.md Section 7 | Power BI + SQL Server Views |
| Auth/AuthZ | 05-security-architecture.md | Microsoft Entra ID (AD Login) + MSAL + MFA (ไม่ใช่ SMS) |
| Infrastructure | 04-infrastructure-cloud-architecture.md | Azure Cloud (HQ owned) + Windows Server (Latest) |
| Backup & Security | 04-infrastructure-cloud-architecture.md | Azure Backup Tool (HQ provided) + HQ Antivirus Tool |
| Cross-Cutting Services | Section 6: Error Handling | Serilog + Azure Monitor + Application Insights |
