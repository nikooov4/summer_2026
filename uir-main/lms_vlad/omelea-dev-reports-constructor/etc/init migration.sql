CREATE TABLE Person.Persons
(
    ID           UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName    NVARCHAR(128),
    MiddleName   NVARCHAR(128) NULL,
    LastName     NVARCHAR(128),
    RegisterDate DATETIME NULL,
    BirthYear    INT NULL,
    BirthMonth   TINYINT NULL CHECK (BirthMonth BETWEEN 1 AND 12),
    BirthDay     TINYINT NULL CHECK (BirthDay BETWEEN 1 AND 31),
    BirthDate    DATETIME NULL
);

CREATE TABLE Person.PersonEmails
(
    Email       NVARCHAR(512) PRIMARY KEY,-- по стандартам RFC 5321 и RFC 5322, не более 320
    PersonID    UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    IsActive    BIT              NOT NULL DEFAULT 1,
    IsPreferred BIT              NOT NULL DEFAULT 1,
    StartDate   DATETIME         NOT NULL,
    EndDate     DATETIME		 NULL,-- CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate)),
	CONSTRAINT PersonEmails_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Person.PersonPhones
(
    Phone          NVARCHAR(20),
    AdditionalCode NVARCHAR(20)		NOT NULL DEFAULT '', -- из-за того NULLable не может быть частью PK/UNIQUE
    PersonID       UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    StartDate      DATETIME         NOT NULL,
    EndDate        DATETIME			NULL, --CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate)),
    PRIMARY KEY (Phone, AdditionalCode, StartDate),
	CONSTRAINT PersonPhones_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Person.PersonEmployments
(
    PersonID   UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    PositionID UNIQUEIDENTIFIER NOT NULL,
    OrgUnitID  UNIQUEIDENTIFIER NOT NULL,
    IsMain     BIT              NOT NULL DEFAULT 1,
    StartDate  DATETIME         NOT NULL,
    EndDate    DATETIME			NULL,-- CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate)),
    PRIMARY KEY (PersonID, PositionID, OrgUnitID, StartDate),
	CONSTRAINT PersonEmployments_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Person.PersonEmploymentsInformal
(
    PersonID  UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    OrgUnitID UNIQUEIDENTIFIER	NOT NULL,
    StartDate DATETIME			NOT NULL,
    EndDate   DATETIME			NULL, --CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate)),
    PRIMARY KEY (PersonID, OrgUnitID, StartDate),
	CONSTRAINT PersonEmploymentsInformal_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Organization.Positions
(
    ID               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PositionName     NVARCHAR(128) NOT NULL,
    ShortName        NVARCHAR(64) NULL,
    ShortestName     NVARCHAR(64) NULL,
    IsTeacher        BIT NOT NULL,
    IsAdministrative BIT NOT NULL                 DEFAULT 0,
    IsTeachingSuppl  BIT NOT NULL                 DEFAULT 0,
    IsTechnical      BIT NOT NULL                 DEFAULT 0,
    IsHousehold      BIT NOT NULL                 DEFAULT 0,
    IsHead           BIT NOT NULL                 DEFAULT 0
);

CREATE TABLE Organization.OrgUnits
(
    ID                   UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UNitName             NVARCHAR(256) NOT NULL,
    UnitCode             NVARCHAR(16) NOT NULL,
    ShortName            NVARCHAR(128) NULL,
    ShortestName         NVARCHAR(64) NULL,
    IsDepartmentGeneral  BIT NOT NULL,
    IsDepartmentGraduate BIT NOT NULL,
    IsDepartment		 AS (IsDepartmentGeneral | IsDepartmentGraduate) PERSISTED,
    IsFaculty            BIT NOT NULL                 DEFAULT 0,
    IsBranch             BIT NOT NULL                 DEFAULT 0,
    IsTechnical          BIT NOT NULL                 DEFAULT 0,
    IsHousehold          BIT NOT NULL                 DEFAULT 0,
    IsNotEducational	 AS ( ~(/*IsDepartment OR IsFaculty*/IsDepartmentGeneral | IsDepartmentGraduate | IsFaculty)) PERSISTED
);

GO
CREATE VIEW Organization.InstructorsView AS
SELECT pe.PersonID,
       pe.PositionID,
       pe.OrgUnitID,
       pe.IsMain,
       pe.StartDate,
       pe.EndDate
FROM Person.PersonEmployments pe
UNION ALL
SELECT pei.PersonID,
       NULL AS PositionID,
       pei.OrgUnitID,
       NULL AS IsMain,
       pei.StartDate,
       pei.EndDate
FROM Person.PersonEmploymentsInformal pei;
GO
CREATE TABLE Organization.Students
(
    PersonID	UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    EdGroupName NVARCHAR(16)		NOT NULL  FOREIGN KEY REFERENCES Education.EducationalGroups(GroupName),
    StartDate	DATETIME			NOT NULL,
    EndDate		DATETIME			NULL,
    PRIMARY KEY (PersonID, EdGroupName, StartDate)
);

CREATE TABLE Education.EducationalGroups
(
    GroupName     NVARCHAR(16) PRIMARY KEY,
    GroupCaption  NVARCHAR(32)		NULL,
    EdProgramID   UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Education.EducationalPrograms(ID),
    AdmissionYear INT				NOT NULL,
    StartDate     DATETIME			NOT NULL,
    EndDate       DATETIME			NULL,
	CONSTRAINT EducationalGroups_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);
GO
CREATE TRIGGER Education.EducationalGroups_AfterInsert
ON Education.EducationalGroups
AFTER INSERT
AS
BEGIN
	UPDATE g
	SET GroupCaption = GroupName
	FROM Education.EducationalGroup g
		INNER JOIN inserted
			on g.GroupName = inserted.GroupName AND i.GroupCaption IS NULL
	;
END
;
GO
CREATE TABLE Education.EducationalPrograms
(
    ID            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProgramName   NVARCHAR(512),
    SpecialtyCode NVARCHAR(16)		NOT NULL FOREIGN KEY REFERENCES Education.Specialties(SpecialtyCode),
    OrgUnitID     UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Organization.OrgUnits(ID),
    CreatedAt     DATETIME			NULL,
    StartDate     DATETIME			NOT NULL,
    EndDate       DATETIME			NULL,
	CONSTRAINT EducationalPrograms_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Education.Specialties
(
    SpecialtyCode			NVARCHAR(16) PRIMARY KEY,
    SpecialtyCaption		NVARCHAR(512),
    EducationLevel			INT CHECK (EducationLevel BETWEEN 1 AND 4),-- 1 - бак, 2 - маг, 3 - спец, 4 - асп
	EducationLevelText		AS (CASE EducationLevel WHEN 1 THEN N'бакалавриат' WHEN 2 THEN N'магистратура' WHEN 3 THEN N'специалитет' WHEN 4 THEN N'аспирантура' ELSE '' END) PERSISTED,
	EducationLevelTextShort	AS (CASE EducationLevel WHEN 1 THEN N'бак' WHEN 2 THEN N'маг' WHEN 3 THEN N'спец' WHEN 4 THEN N'асп' ELSE '' END)  PERSISTED,
    StartDate				DATETIME NOT NULL,
    EndDate					DATETIME NULL,
	CONSTRAINT Specialties_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Education.Courses
(
    ID				UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseName		NVARCHAR(256) NOT NULL
);

CREATE TABLE Education.CoursePrograms
(
    ID                 UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EdCourseID         UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Education.Courses(ID),
    CourseName         NVARCHAR(256),
    EdProgramID        UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Education.EducationalPrograms(ID),
    LectureHours       INT				NOT NULL,
    SeminarHours       INT				NOT NULL,
    LabHours           INT				NOT NULL,
    OtherPresenceHours INT              NULL        DEFAULT 0,
    NonAuditorialHours INT				NULL,
    IsElective         BIT              NOT NULL    DEFAULT 0,
    IsOptional         BIT              NOT NULL    DEFAULT 0
);

CREATE TABLE Education.CourseAuthors
(
    CourseProgramID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Education.CoursePrograms(ID),
    PersonID        UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    PRIMARY KEY (CourseProgramID, PersonID)
);

CREATE TABLE Education.CourseProgramModules
(
    ID              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseProgramID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Education.CoursePrograms(ID),
    ModuleName      NVARCHAR(256)		NOT NULL,
    MaxScore        INT					NOT NULL,
    MinAcceptScore  INT					NOT NULL,
    IsAppraisal     BIT					NOT NULL	DEFAULT 0,
    OrderIndex      INT					NOT NULL
);

CREATE TABLE Education.CourseTasks
(
    ID				UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseModuleID	UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Education.CourseProgramModules(ID),
    TaskName        NVARCHAR(256),
    [Description]	NVARCHAR(MAX)	NOT NULL DEFAULT '',
    TaskTypeID		INT				NOT NULL FOREIGN KEY REFERENCES Education.CourseTaskTypes(ID),
    MaxScore		INT				NOT NULL,
    MinAcceptScore	INT				NOT NULL
);

CREATE TABLE Education.CourseTaskTypes
(
    ID				INT PRIMARY KEY,
    [Name]			NVARCHAR(128)	NOT NULL,
	ShortName		NVARCHAR(20)	NOT NULL,
	AltShortName	NVARCHAR(20)	NOT NULL
);

INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (1, 'Контрольная работа', 'КР', 'к/р');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (2, 'Лабораторная работа', 'ЛР', 'л/р');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (3, 'Коллоквиум', 'КЛК', 'колл');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (4, 'Курсовая работа', 'КРА', 'к/ра');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (5, 'Курсовой проект', 'КП', 'к/п');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (6, 'Домашнее задание', 'ДЗ', 'д/з');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (7, 'Большое домашнее задание', 'БДЗ', 'б.д.з.');
INSERT INTO Education.CourseTaskTypes (ID, [Name], ShortName, AltShortName)
	VALUES (8, 'Реферат', 'РЕФ', 'реф.');


CREATE TABLE Education.CourseCurators
(
    CourseID  UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Education.Courses(ID),
    PersonID  UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    StartDate DATETIME			NOT NULL,
    EndDate   DATETIME			NULL,
    IsLead    BIT				DEFAULT 0,
    PRIMARY KEY (CourseID, PersonID, StartDate),
	CONSTRAINT EdCourseCurators_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Course.Courses
(
    ID				UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseName		NVARCHAR(256) NOT NULL,
	Description		NVARCHAR(MAX) NOT NULL DEFAULT ''
);

CREATE TABLE Course.CourseInstances
(
    CourseID       UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Course.Courses(ID),
    [Year]         INT              NOT NULL,
    ContentScopeID UNIQUEIDENTIFIER NOT NULL,
    CreatedAt      DATETIME			NOT NULL DEFAULT GETDATE(),
    PRIMARY KEY (CourseID, [Year])
);

CREATE TABLE Course.CourseInstanceEdPrograms
(
    CourseID        UNIQUEIDENTIFIER	NOT NULL,
    [Year]          INT					NOT NULL,
    CourseProgramID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Education.CoursePrograms(ID),
    IsMain          BIT					DEFAULT 0,
    PRIMARY KEY (CourseID, Year, CourseProgramID),
    FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances (CourseID, Year)
);

CREATE TABLE Course.CourseInstanceModules
(
    ID             UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CourseID       UNIQUEIDENTIFIER NOT NULL,
    Year           INT              NOT NULL,
    CourseModuleID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Education.CourseProgramModules(ID),
    FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances (CourseID, Year)
);

CREATE TABLE Course.CourseInstanceModuleTasks
(
    ID						UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TaskName				NVARCHAR(256)		NOT NULL,
	TaskDescription			NVARCHAR(4000)		NOT NULL DEFAULT '',
    CourseInstanceModuleID	UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Course.CourseInstanceModules(ID),
    TaskTypeID				INT					NOT NULL FOREIGN KEY REFERENCES Education.CourseTaskTypes(ID),
    TaskID					UNIQUEIDENTIFIER	NULL FOREIGN KEY REFERENCES Education.CourseTasks(ID),
    MaxScore				INT					NOT NULL,
    MinAcceptScore			INT					NOT NULL,
    OrderIndex				INT					NOT NULL,
    StartDate				DATETIME			NOT NULL,
    EndDate					DATETIME,
	CONSTRAINT CourseInstanceModuleTask_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Course.CourseInstanceTeachers
(
    CourseID  UNIQUEIDENTIFIER	NOT NULL,
    Year      INT				NOT NULL,
    PersonID  UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    StartDate DATETIME			NOT NULL DEFAULT GETDATE(),
    EndDate   DATETIME			NULL CHECK (StartDate < EndDate),
    PRIMARY KEY (CourseID, Year, PersonID),
    FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances (CourseID, Year)
);

CREATE TABLE Course.CourseInstanceStudents
(
    CourseID  UNIQUEIDENTIFIER	NOT NULL,
    Year      INT				NOT NULL,
    PersonID  UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    StartDate DATETIME			NOT NULL DEFAULT GETDATE(),
    EndDate   DATETIME			NULL,
    PRIMARY KEY (CourseID, Year, PersonID),
    FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances (CourseID, Year),
	CONSTRAINT CourseInstanceStudents_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE Course.CourseInstanceStudentTasks
(
    CourseInstanceModuleTaskID UNIQUEIDENTIFIER		NOT NULL FOREIGN KEY REFERENCES Course.CourseInstanceModuleTasks(ID),
    PersonID                   UNIQUEIDENTIFIER		NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    Score                      INT					NOT NULL DEFAULT 0,
    AcceptanceDate             DATETIME				NOT NULL DEFAULT GETDATE(),
    Comment                    NVARCHAR(MAX)		NOT NULL DEFAULT '',
    OrderIndex                 INT					NOT NULL,
    StartDate                  DATETIME				NOT NULL DEFAULT GETDATE(),
    EndDate                    DATETIME				NULL,
    PRIMARY KEY (CourseInstanceModuleTaskID, PersonID, OrderIndex),
	CONSTRAINT CourseInstanceStudentTasks_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE CourseReport.AppraisalLists
(
    ID              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name            NVARCHAR(256)		NOT NULL,
    Description     NVARCHAR(2048)		NOT NULL DEFAULT '',
    CourseID        UNIQUEIDENTIFIER	NOT NULL,
    Year            INT					NOT NULL,
    CourseProgramID UNIQUEIDENTIFIER	NULL FOREIGN KEY REFERENCES Education.CoursePrograms(ID),
    DateCreated     DATETIME			NOT NULL DEFAULT GETDATE(),
    DateClosed      DATETIME			NULL,
    Comment         NVARCHAR(MAX)		NOT NULL DEFAULT '',
    FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances (CourseID, Year)
);

CREATE TABLE CourseReport.AppraisalListColumns
(
    ID                     UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name                   NVARCHAR(64),
    Description            NVARCHAR(1024)	NULL DEFAULT '',
    AppraisalListID        UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalLists(ID),
    OrderIndex             INT,
    CourseInstanceModuleID UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Course.CourseInstanceModules(ID),
    ScaleID                UNIQUEIDENTIFIER NULL,
    MaxScore               INT				NOT NULL,
    MinAcceptScore         INT				NOT NULL,
    Comment                NVARCHAR(MAX)	NOT NULL DEFAULT '',
    IsComputed             BIT				NOT NULL DEFAULT 0
);

CREATE TABLE CourseReport.AppraisalScales
(
    ID          UNIQUEIDENTIFIER	PRIMARY KEY DEFAULT NEWID(),
    Name        NVARCHAR(128)		NOT NULL,
    Description NVARCHAR(1024)		NOT NULL DEFAULT '',
    IsEnum      BIT					NOT NULL,
    IsLinear    BIT					NOT NULL,
    CHECK (IsEnum = 1 AND IsLinear = 0 OR IsEnum = 0 AND IsLinear = 1)
);

ALTER TABLE CourseReport.AppraisalListColumns
    ADD CONSTRAINT FK_AppraisalListColumns_ScaleID
        FOREIGN KEY (ScaleID) REFERENCES CourseReport.AppraisalScales (ID);

CREATE TABLE CourseReport.ValueFormats
(
	ID	NVARCHAR(32) NOT NULL PRIMARY KEY
);
INSERT INTO CourseReport.ValueFormats(ID)
VALUES
	('NUMERIC'),
	('ALPHENG'),
	('ALPHRUS')
;

CREATE TABLE CourseReport.AppraisalScalesLinear
(
    ID          UNIQUEIDENTIFIER	PRIMARY KEY FOREIGN KEY REFERENCES CourseReport.AppraisalScales(ID),
    MinValue    INT					NOT NULL,
    MaxValue    INT					NOT NULL,
    ValueFormat NVARCHAR(32)		NOT NULL FOREIGN KEY REFERENCES CourseReport.ValueFormats(ID)
);

CREATE TABLE CourseReport.AppraisalScalesEnumValues
(
    ScaleID     UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalScales(ID),
    Name        NVARCHAR(32)		NOT NULL,
    Description NVARCHAR(1024)		NOT NULL DEFAULT '',
    Code        INT					NOT NULL,
    PRIMARY KEY (ScaleID, Code),
    UNIQUE (ScaleID, Name)
);

CREATE TABLE CourseReport.EnumValuePartialOrdering
(
	Scale1ID	UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalScales(ID),
	Code1		INT					NOT NULL,
	Scale2ID	UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalScales(ID),
	Code2		INT					NOT NULL,
	PRIMARY KEY (Scale1ID, Code1, Scale2ID, Code2),
	FOREIGN KEY (Scale1ID, Code1) REFERENCES CourseReport.AppraisalScalesEnumValues(ScaleID, Code),
	FOREIGN KEY (Scale2ID, Code2) REFERENCES CourseReport.AppraisalScalesEnumValues(ScaleID, Code)
);

CREATE TABLE CourseReport.AppraisalListColumnsCompute
(
    ColumnID    UNIQUEIDENTIFIER	NOT NULL PRIMARY KEY FOREIGN KEY REFERENCES CourseReport.AppraisalListColumns(ID),
    OperationID NVARCHAR(32)		NOT NULL
);

CREATE TABLE CourseReport.AppraisalListColumnsDependencies
(
    ColumnID    UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalListColumnsCompute(ColumnID),
    ColumnDepID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalListColumns(ID),
	PRIMARY KEY (ColumnID, ColumnDepID)
);

CREATE TABLE CourseReport.ColumnOperations
(
    ID          NVARCHAR(32)	NOT NULL PRIMARY KEY,
    Description NVARCHAR(1024)	NOT NULL DEFAULT ''
);

ALTER TABLE CourseReport.AppraisalListColumnsCompute
    ADD CONSTRAINT FK_AppraisalListColumnsCompute_ColumnOperations
        FOREIGN KEY (OperationID) REFERENCES CourseReport.ColumnOperations (ID);

INSERT INTO CourseReport.ColumnOperations (ID, Description)
	VALUES ('SUM', 'Суммирование');
INSERT INTO CourseReport.ColumnOperations (ID, Description)
	VALUES ('AVG', 'Среднее значение');
INSERT INTO CourseReport.ColumnOperations (ID, Description)
	VALUES ('MIN', 'Минимальное значение');
INSERT INTO CourseReport.ColumnOperations (ID, Description)
	VALUES ('MAX', 'Максимальное значение');
INSERT INTO CourseReport.ColumnOperations (ID, Description)
	VALUES ('ENUMRANGE', 'Вычисляемую колонку нужно заполнить енамами согласно диапазонам');


CREATE TABLE CourseReport.AppraisalScalesEnumRanges
(
    ComputeColumnID     UNIQUEIDENTIFIER	NOT NULL REFERENCES CourseReport.AppraisalListColumnsCompute(ColumnID),
    ScaleID				UNIQUEIDENTIFIER	NOT NULL,
	Code				INT					NOT NULL,
    RangeStart			INT					NOT NULL,
    RangeEnd			INT					NOT NULL,	
    PRIMARY KEY (ComputeColumnID, ScaleID, Code),
	FOREIGN KEY (ScaleID, Code) REFERENCES CourseReport.AppraisalScalesEnumValues(ScaleID,Code)
);

CREATE TABLE CourseReport.AppraisalListMarks
(
    AppraisalListColumnID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalListColumns(ID),
    StudentPersonID       UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    TeacherPersonID       UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    Score                 INT				NOT NULL,
    ScoreSourceName       NVARCHAR(2048)	NULL,-- MAX URL length
    UpdatedAt             DATETIME			NOT NULL DEFAULT GETDATE(),
    OldScore              INT				NULL,
    OldTeacherPersonID    UNIQUEIDENTIFIER	NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    OldScoreSourceName    NVARCHAR(2048)	NULL, 
    PRIMARY KEY (AppraisalListColumnID, StudentPersonID)
);

CREATE TABLE CourseReport.AppraisalListMarksHistory
(
    AppraisalListColumnID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseReport.AppraisalListColumns(ID),
    StudentPersonID       UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    TeacherPersonID       UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    Score                 INT				NOT NULL,
    ScoreSourceName       NVARCHAR(2048)	NULL,
    StartDate             DATETIME			NOT NULL DEFAULT GETDATE(),
    EndDate               DATETIME			NULL,
    PRIMARY KEY (AppraisalListColumnID, StudentPersonID, StartDate),
	CONSTRAINT AppraisalListMarksHistory_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE CourseLabs.StudentCourseTasks
(
    ID              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name            NVARCHAR(256),
	TaskDescription	NVARCHAR(4000)		NOT NULL DEFAULT '',
    CourseID        UNIQUEIDENTIFIER	NOT NULL,
    Year            INT					NOT NULL,
    StudentPersonID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
    TaskTypeID      INT					NOT NULL FOREIGN KEY REFERENCES Education.CourseTaskTypes(ID),
    MaxScore        INT					NOT NULL,
    MinAcceptScore  INT					NOT NULL,
    OrderIndex      INT					NOT NULL,
    StartDate       DATETIME			NOT NULL,
    IsCancelled     BIT					NOT NULL DEFAULT 0,
    EndDate         DATETIME			NULL,
	FOREIGN KEY (CourseID, Year) REFERENCES Course.CourseInstances(CourseID,Year),
    UNIQUE (CourseID, Year, StudentPersonID, OrderIndex),
	CONSTRAINT StudentCourseTasks_EndDate CHECK (EndDate IS NULL OR EndDate IS NOT NULL AND (StartDate < EndDate))
);

CREATE TABLE CourseLabs.StudentCourseTaskOptions
(
    StudentCourseTaskID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CourseLabs.StudentCourseTasks(ID),
    TaskOptionID        UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES CourseLabs.CourseTaskOptions(ID),
    IsStudentSelection  BIT              DEFAULT 1,
	PRIMARY KEY (StudentCourseTaskID,TaskOptionID)
);

CREATE TABLE CourseLabs.CourseTaskOptions
(
    ID                         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name                       NVARCHAR(256),
    Description                NVARCHAR(4000),
    TypeCode                   INT DEFAULT 0 CHECK(0 <= TypeCode AND TypeCode <= 3),
	TypeText AS (CASE TypeCode WHEN 0 THEN 'обычный элемент' WHEN 1 THEN 'группа-конъюнкция' WHEN 2 THEN 'группа-дизъюнкция' WHEN 3 THEN 'множество' ELSE '' END) PERSISTED,
    CourseInstanceModuleTaskID UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Course.CourseInstanceModuleTasks(ID)
);

CREATE TABLE CourseLabs.CourseTaskOptionValues
(
    ID                         UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name                       NVARCHAR(128)	NOT NULL,
    Description                NVARCHAR(2048)	NOT NULL DEFAULT '',
    Value                      INT				NOT NULL,
    CourseTaskOptionsID UNIQUEIDENTIFIER FOREIGN KEY REFERENCES CourseLabs.CourseTaskOptions(ID)
);

CREATE TABLE CourseLabs.Relations
(
    ID          INT PRIMARY KEY,
    Name        NVARCHAR(32),
    Description NVARCHAR(1024) NOT NULL DEFAULT ''
);


CREATE TABLE CourseLabs.CourseTaskOptionRelations
(
    TaskOption1ID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseLabs.CourseTaskOptions(ID),
    TaskOption2ID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseLabs.CourseTaskOptions(ID),
    RelationID    INT				NOT NULL FOREIGN KEY REFERENCES CourseLabs.Relations(ID),
	PRIMARY KEY (TaskOption1ID,TaskOption2ID)
);

INSERT INTO CourseLabs.Relations(ID, Name, Description)
VALUES
	(1,'HasExtension','ID1 является расширением для ID2, например, прошитое бинарное дерево для «обычного» бинарного дерева'),
	(2,'IsSubset','ID1 – подмножество для ID2'),
	(3,'Requires','ID1 требует предварительно выполнить ID2'),
	(4,'IsItemOf','ID1 IsItemOf ID2 – принадлежность множеству или группе; можно использовать для реализации конъюнкции или дизъюнкции пререквизитов'),
	(5, 'EqualsTo', 'Объект, обзначенный ID1, равен или идентичен объекту, обозначенному ID2')
;

CREATE TABLE CourseLabs.StudentVisits
(
	ID				UNIQUEIDENTIFIER	NOT NULL PRIMARY KEY,
	CourseID		UNIQUEIDENTIFIER	NOT NULL,
	Year			INT					NOT NULL,
	StudentPersonID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
	VisitDate		DATETIME			NOT NULL,
	RegisterDate	DATETIME			NOT NULL DEFAULT GETDATE(),
	DataSourceName	NVARCHAR(2048)		NULL,
	TeacherPersonID UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES Person.Persons(ID),
	Comment			NVARCHAR(4000)		NOT NULL DEFAULT ''
);

CREATE TABLE CourseLabs.StudentVisitDetails
(
	StudentVisitID		UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseLabs.StudentVisits(ID),
	OrderIndex			INT					NOT NULL,
	StudentCourseTaskID	UNIQUEIDENTIFIER	NULL FOREIGN KEY REFERENCES CourseLabs.StudentCourseTasks(ID),
	CriterionID			UNIQUEIDENTIFIER	NULL FOREIGN KEY REFERENCES CourseLabs.StudentTaskCiteria(ID),
	Score				INT					NULL,
	PRIMARY KEY (StudentVisitID,OrderIndex)
);

CREATE TABLE CourseLabs.StudentVisitDetailComments
(
	ID					UNIQUEIDENTIFIER	NOT NULL PRIMARY KEY,
	StudentVisitID		UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseLabs.StudentVisits(ID),
	--StudentCourseTaskID	UNIQUEIDENTIFIER	NOT NULL FOREIGN KEY REFERENCES CourseLabs.StudentCourseTasks(ID),
	OrderIndex			INT					NOT NULL,
	Comment				VARCHAR(2048)		NOT NULL,
	UNIQUE (StudentVisitID,OrderIndex)
);

CREATE TABLE CourseLabs.TaskCriteria
(
	ID							UNIQUEIDENTIFIER	NOT NULL PRIMARY KEY,
    Name						NVARCHAR(256)		NOT NULL,
	Description					NVARCHAR(4000)		NOT NULL DEFAULT '',
	OrderIndex					INT					NOT NULL,
    MaxScore					INT					NOT NULL,
    MinAcceptScore				INT					NOT NULL,
	IsMoreThanMaxScoreAllowed	BIT					NOT NULL DEFAULT 0
);

CREATE TABLE CourseLabs.StudentCourseTaskCriteria
(
	StudentCourseTaskID	UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES CourseLabs.StudentCourseTasks(ID),
	TaskCriteriaID		UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES CourseLabs.TaskCriteria(ID),
	IsMandatory			BIT					NOT NULL	DEFAULT 1,
	PRIMARY KEY (StudentCourseTaskID,TaskCriteriaID)
);

CREATE TABLE CourseLabs.CourseInstanceModuleTaskCriteria
(
	CourseInstanceModuleTaskID	UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES Course.CourseInstanceModuleTasks(ID),
	TaskCriteriaID				UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES CourseLabs.TaskCriteria(ID),
	IsMandatory					BIT					NOT NULL	DEFAULT 1,
	PRIMARY KEY (CourseInstanceModuleTaskID,TaskCriteriaID)
);

CREATE TABLE CourseLabs.StudentIndCourseInstanceModuleTaskRels
(
	StudentCourseTaskID			UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES CourseLabs.StudentCourseTasks(ID),
	CourseInstanceModuleTaskID	UNIQUEIDENTIFIER	NOT NULL	FOREIGN KEY REFERENCES Course.CourseInstanceModuleTasks(ID),
	RelationID					INT					NOT NULL	FOREIGN KEY REFERENCES CourseLabs.Relations(ID)
);

CREATE TABLE Content.ContentScopes
(
	ID UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
	ParentID UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Content.ContentScopes(ID)
);
