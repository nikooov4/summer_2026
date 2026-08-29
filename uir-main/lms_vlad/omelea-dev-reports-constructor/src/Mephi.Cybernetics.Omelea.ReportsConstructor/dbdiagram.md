## Mephi.Cybernetics.Omelea.ReportsConstructor.DbScripts

```mermaid
erDiagram
  AppraisalListColumns {
    ID uniqueidentifier PK
    SourceIdentifier nvarchar(64) 
    AppraisalListID uniqueidentifier FK
    IsComputed bit 
  }
  AppraisalListColumns }o--|| AppraisalLists : 
  AppraisalListColumnsCompute {
    ColumnID uniqueidentifier PK,FK
    Operation nvarchar(256) 
  }
  AppraisalListColumnsCompute }o--|| AppraisalListColumns : 
  AppraisalListColumnsDependencies {
    ColumnID uniqueidentifier PK,FK
    ColumnDepID uniqueidentifier PK,FK
  }
  AppraisalListColumnsDependencies }o--|| AppraisalListColumnsCompute : 
  AppraisalListColumnsDependencies }o--|| AppraisalListColumns : 
  AppraisalListMarks {
    AppraisalListColumnID uniqueidentifier PK,FK
    StudentPersonID uniqueidentifier PK,FK
    Score int(NULL) 
    UpdatedAt datetime 
  }
  AppraisalListMarks }o--|| Persons : 
  AppraisalListMarks }o--|| AppraisalListColumns : 
  AppraisalLists {
    ID uniqueidentifier PK
    SourceIdentifier nvarchar(64) 
    DateCreated datetime 
    DateClosed datetime(NULL) 
  }
  Persons {
    ID uniqueidentifier PK
    FirstName nvarchar(128)(NULL) 
    MiddleName nvarchar(128)(NULL) 
    LastName nvarchar(128)(NULL) 
    RegisterDate datetime(NULL) 
    BirthYear int(NULL) 
    BirthMonth tinyint(NULL) 
    BirthDay tinyint(NULL) 
    BirthDate datetime(NULL) 
  }
```
