# SQL Draft Output Template

> Template สำหรับเอกสาร SQL DDL draft ที่ generate จาก Data Architecture และ Class Diagram  
> Tech Stack: SQLite ผ่าน EF Core Migrations (ห้ามใช้ SQL Server-specific syntax)

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Project | {project_name} |
| Feature / Module | {feature_or_module} |
| Source Design | {class_diagram_file}, {data_architecture_file} |
| Generated Date | {date} |
| Author | {author} |

---

## 2. Assumptions

- {assumption_1}
- {assumption_2}

---

## 3. Table Definitions

### 3.1 {TableName}

**Purpose:** {อธิบายว่า table นี้เก็บข้อมูลอะไร}  
**Related Entity:** `{EntityClassName}` ใน `{Layer}.{Namespace}`  
**Related SRS:** {requirement_id}

```sql
CREATE TABLE IF NOT EXISTS {TableName} (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- {อธิบาย column กลุ่มนี้}
    {ColumnName}    {TYPE}          NOT NULL,
    {ColumnName}    {TYPE}          NULL,

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,   -- UTC ISO 8601
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

**Column Descriptions:**

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | INTEGER | NOT NULL | Auto-increment primary key |
| {ColumnName} | {TYPE} | {Y/N} | {description} |
| CreatedAt | TEXT | NOT NULL | Timestamp UTC (ISO 8601) |
| CreatedBy | TEXT | NOT NULL | User ID หรือ system name |
| UpdatedAt | TEXT | NULL | Timestamp UTC เมื่อแก้ไขล่าสุด |
| UpdatedBy | TEXT | NULL | User ID หรือ system name |

---

### 3.2 {TableName_2}

(ทำซ้ำ pattern ข้างต้นสำหรับทุก table)

---

## 4. Foreign Key Constraints

```sql
-- {TableName} → {ParentTableName}
-- FK: {TableName}.{ForeignKeyColumn} → {ParentTableName}.Id
-- Note: SQLite FK enforcement ต้อง enable ผ่าน PRAGMA foreign_keys = ON
```

---

## 5. Indexes

```sql
-- {อธิบายเหตุผลที่สร้าง index นี้}
CREATE INDEX IF NOT EXISTS IX_{TableName}_{ColumnName}
    ON {TableName} ({ColumnName});

-- Composite index สำหรับ {query pattern}
CREATE INDEX IF NOT EXISTS IX_{TableName}_{Col1}_{Col2}
    ON {TableName} ({Col1}, {Col2});
```

---

## 6. Seed Data

```sql
-- {อธิบาย seed data ที่ต้องใส่ก่อน run ระบบ}
INSERT INTO {TableName} ({Col1}, {Col2}, CreatedAt, CreatedBy)
VALUES
    ({value1}, {value2}, datetime('now'), 'system'),
    ({value1}, {value2}, datetime('now'), 'system');
```

---

## 7. EF Core Migration Note

> SQL Draft นี้ใช้สำหรับ review และ reference เท่านั้น  
> schema จริงให้ generate ผ่าน EF Core Migration ด้วยคำสั่ง:

```bash
dotnet ef migrations add {MigrationName} --project LeaveRequest.Infrastructure --startup-project LeaveRequest.API
dotnet ef database update --project LeaveRequest.Infrastructure --startup-project LeaveRequest.API
```

---

## 8. Open Issues / Deviation

| # | Issue | Status |
|---|-------|--------|
| 1 | {issue description} | Open / Resolved |

---

## SQLite Data Type Reference

| C# Type | SQLite Type | EF Core Config |
|---------|-------------|----------------|
| `int` | `INTEGER` | (default) |
| `string` | `TEXT` | `.HasMaxLength(n)` |
| `bool` | `INTEGER` (0/1) | (default) |
| `DateTime` | `TEXT` | ใช้ UTC ISO 8601 |
| `DateOnly` | `TEXT` | ใช้ `yyyy-MM-dd` |
| `decimal` | `REAL` | `.HasColumnType("REAL")` |
| `Guid` | `TEXT` | (auto-convert) |
| `enum` | `TEXT` | `.HasConversion<string>()` |
