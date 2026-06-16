# SRS Change Log Management Skill

This skill ensures that every update to Software Requirement Specification (SRS) documents is properly tracked and recorded in both the local document and the central change log index.

## Trigger Rule
This skill should be applied whenever any of the following files are edited:
- `10-requirement-definition/b0-system-requirement/*-srs*.md`
- `10-requirement-definition/b0-system-requirement/*-system-requirement-specification-summary.md`

## Automation Steps

### 1. Update Document Change Log
In the document being edited, locate the `## Change Log` section and add a new entry at the top of the table. If the section does not exist, create it.
- **Version**: Increment the minor version (e.g., v2.1 -> v2.2).
- **Date**: Use the current date in `YYYY-MM-DD` format.
- **Section**: Specify the section number or name that was changed.
- **Change Type**: Use `Added`, `Updated`, or `Deleted`.
- **Description**: Provide a concise summary of the specific change.
- **Source**: Note the origin or rationale (e.g., `Stakeholder Request`, `Gap Analysis`, `Project Rule`).

### 2. Update Central SRS Change Log
Open `10-requirement-definition/b0-system-requirement/srs-change-log.md` and add a new row to the `## Change Log` table:
- **#**: Increment the index number.
- **Date**: Current date.
- **Document**: Name of the document (e.g., `Interface SRS (v002)`).
- **Version**: The new version number.
- **Section, Change Type, Description, Source**: As defined in step 1.
- **Post-BRD**: 
    - `Y`: If the change introduces information not present in the original BRD baseline.
    - `N`: If the change is a correction or refinement of content already in the BRD baseline.

### 3. Update File Metadata (Front-matter)
Update the `last_updated` field in the document's YAML front-matter to the current date.

## Example Change Log Row (Markdown)
| # | Date | Document | Version | Section | Change Type | Description | Source | Post-BRD |
|---|------|----------|---------|---------|-------------|-------------|--------|----------|
| 16 | 2026-04-17 | Screen SRS (v002) | 2.2 | 2.3 | Updated | Update file upload limits to 10MB | Security review | Y |
