/*
    RaceDay Database Script
    Module: PROG6212
    Database: RaceDay

    Description:
    This script creates the RaceDay database structure,
    relationships, constraints and sample seed data.

    Main roles:
    - Organiser
    - Participant
*/

-- =========================================
-- PART A - DATABASE
-- =========================================

CREATE DATABASE RaceDay; GO USE RaceDay; GO

-- =========================================
-- PART B - TABLES
-- =========================================

USE RaceDay;
GO

CREATE TABLE [USER] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    PhoneNumber NVARCHAR(20),
    DateCreated DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT CK_USER_Role
        CHECK (Role IN ('Organiser', 'Participant'))
);
GO


CREATE TABLE EVENT (
    EventID INT IDENTITY(1,1) PRIMARY KEY,
    OrganiserID INT NOT NULL,
    EventName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    EventDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    Location NVARCHAR(150) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_EVENT_USER
        FOREIGN KEY (OrganiserID)
        REFERENCES [USER](UserID),

    CONSTRAINT CK_EVENT_Status
        CHECK (Status IN ('Upcoming', 'Open', 'Closed', 'Completed', 'Cancelled'))
);
GO


USE RaceDay;
GO


CREATE TABLE CATEGORY (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    DistanceKm DECIMAL(6,2) NOT NULL,
    Description NVARCHAR(500)
);
GO


USE RaceDay;
GO

CREATE TABLE EVENT_CATEGORY (
    EventCategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EntryFee DECIMAL(10,2) NOT NULL,
    MaxParticipants INT NOT NULL,
    AvailableSlots INT NOT NULL,

    CONSTRAINT FK_EVENT_CATEGORY_EVENT
        FOREIGN KEY (EventID)
        REFERENCES EVENT(EventID),

    CONSTRAINT FK_EVENT_CATEGORY_CATEGORY
        FOREIGN KEY (CategoryID)
        REFERENCES CATEGORY(CategoryID),

    CONSTRAINT CK_EVENT_CATEGORY_EntryFee
        CHECK (EntryFee >= 0),

    CONSTRAINT CK_EVENT_CATEGORY_MaxParticipants
        CHECK (MaxParticipants > 0),

    CONSTRAINT CK_EVENT_CATEGORY_AvailableSlots
        CHECK (AvailableSlots >= 0
               AND AvailableSlots <= MaxParticipants)
);
GO


USE RaceDay;
GO

CREATE TABLE ENROLMENT (
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventCategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    EnrolmentStatus NVARCHAR(20) NOT NULL,
    RaceNumber INT NOT NULL,

    CONSTRAINT FK_ENROLMENT_USER
        FOREIGN KEY (ParticipantID)
        REFERENCES [USER](UserID),

    CONSTRAINT FK_ENROLMENT_EVENT_CATEGORY
        FOREIGN KEY (EventCategoryID)
        REFERENCES EVENT_CATEGORY(EventCategoryID),

    CONSTRAINT CK_ENROLMENT_Status
        CHECK (EnrolmentStatus IN ('Pending', 'Confirmed', 'Cancelled', 'Completed')),

    CONSTRAINT UQ_ENROLMENT_RaceNumber
        UNIQUE (EventCategoryID, RaceNumber)
);
GO


USE RaceDay;
GO

CREATE TABLE RESULT (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME,
    Position INT,
    ResultStatus NVARCHAR(20) NOT NULL,

    CONSTRAINT FK_RESULT_ENROLMENT
        FOREIGN KEY (EnrolmentID)
        REFERENCES ENROLMENT(EnrolmentID),

    CONSTRAINT CK_RESULT_Position
        CHECK (Position IS NULL OR Position > 0),

    CONSTRAINT CK_RESULT_Status
        CHECK (ResultStatus IN ('Pending', 'Finished', 'DNS', 'DNF'))
);
GO


-- =========================================
-- PART C - SEED DATA 
-- =========================================

USE RaceDay;
GO

-- =========================
-- USERS
-- =========================

IF NOT EXISTS (SELECT 1 FROM [USER] WHERE Email = 'thabo.mokoena@raceday.co.za')
BEGIN
    INSERT INTO [USER]
        (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
    VALUES
        ('Thabo', 'Mokoena', 'thabo.mokoena@raceday.co.za',
         'DemoHash_Thabo_123', 'Organiser', '0712345678');
END;

IF NOT EXISTS (SELECT 1 FROM [USER] WHERE Email = 'lerato.nkosi@raceday.co.za')
BEGIN
    INSERT INTO [USER]
        (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
    VALUES
        ('Lerato', 'Nkosi', 'lerato.nkosi@raceday.co.za',
         'DemoHash_Lerato_123', 'Organiser', '0723456789');
END;

IF NOT EXISTS (SELECT 1 FROM [USER] WHERE Email = 'sipho.dlamini@example.com')
BEGIN
    INSERT INTO [USER]
        (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
    VALUES
        ('Sipho', 'Dlamini', 'sipho.dlamini@example.com',
         'DemoHash_Sipho_123', 'Participant', '0734567890');
END;

IF NOT EXISTS (SELECT 1 FROM [USER] WHERE Email = 'ayanda.khumalo@example.com')
BEGIN
    INSERT INTO [USER]
        (FirstName, LastName, Email, PasswordHash, Role, PhoneNumber)
    VALUES
        ('Ayanda', 'Khumalo', 'ayanda.khumalo@example.com',
         'DemoHash_Ayanda_123', 'Participant', '0745678901');
END;
GO


USE RaceDay;
GO

-- =========================
-- CATEGORIES
-- =========================

IF NOT EXISTS (SELECT 1 FROM CATEGORY WHERE CategoryName = '5km Fun Run')
BEGIN
    INSERT INTO CATEGORY
        (CategoryName, DistanceKm, Description)
    VALUES
        ('5km Fun Run', 5.00, 'A short recreational running event suitable for beginners.');
END;

IF NOT EXISTS (SELECT 1 FROM CATEGORY WHERE CategoryName = '10km Road Race')
BEGIN
    INSERT INTO CATEGORY
        (CategoryName, DistanceKm, Description)
    VALUES
        ('10km Road Race', 10.00, 'A competitive road race for intermediate runners.');
END;

IF NOT EXISTS (SELECT 1 FROM CATEGORY WHERE CategoryName = '21km Half Marathon')
BEGIN
    INSERT INTO CATEGORY
        (CategoryName, DistanceKm, Description)
    VALUES
        ('21km Half Marathon', 21.10, 'A half marathon for experienced runners.');
END;

IF NOT EXISTS (SELECT 1 FROM CATEGORY WHERE CategoryName = '10km Walk')
BEGIN
    INSERT INTO CATEGORY
        (CategoryName, DistanceKm, Description)
    VALUES
        ('10km Walk', 10.00, 'A walking event suitable for recreational participants.');
END;
GO


USE RaceDay;
GO

-- =========================
-- EVENTS
-- =========================

DECLARE @Organiser1 INT =
(
    SELECT UserID
    FROM [USER]
    WHERE Email = 'thabo.mokoena@raceday.co.za'
);

DECLARE @Organiser2 INT =
(
    SELECT UserID
    FROM [USER]
    WHERE Email = 'lerato.nkosi@raceday.co.za'
);

IF NOT EXISTS (SELECT 1 FROM EVENT WHERE EventName = 'Durban Sunrise Run')
BEGIN
    INSERT INTO EVENT
        (OrganiserID, EventName, Description, EventDate, StartTime,
         Location, Status)
    VALUES
        (@Organiser1,
         'Durban Sunrise Run',
         'A community road running event along the Durban beachfront.',
         '2026-10-10',
         '06:30',
         'Durban Beachfront',
         'Open');
END;

IF NOT EXISTS (SELECT 1 FROM EVENT WHERE EventName = 'KwaZulu-Natal City Challenge')
BEGIN
    INSERT INTO EVENT
        (OrganiserID, EventName, Description, EventDate, StartTime,
         Location, Status)
    VALUES
        (@Organiser2,
         'KwaZulu-Natal City Challenge',
         'A city road race bringing together runners from across KwaZulu-Natal.',
         '2026-11-15',
         '07:00',
         'Moses Mabhida Stadium',
         'Upcoming');
END;

IF NOT EXISTS (SELECT 1 FROM EVENT WHERE EventName = 'Umhlanga Community Walk')
BEGIN
    INSERT INTO EVENT
        (OrganiserID, EventName, Description, EventDate, StartTime,
         Location, Status)
    VALUES
        (@Organiser1,
         'Umhlanga Community Walk',
         'A family-friendly community walking event.',
         '2026-12-05',
         '07:30',
         'Umhlanga Promenade',
         'Upcoming');
END;
GO


USE RaceDay;
GO

-- =========================
-- EVENT CATEGORIES
-- =========================

DECLARE @Event1 INT =
(
    SELECT EventID
    FROM EVENT
    WHERE EventName = 'Durban Sunrise Run'
);

DECLARE @Event2 INT =
(
    SELECT EventID
    FROM EVENT
    WHERE EventName = 'KwaZulu-Natal City Challenge'
);

DECLARE @Event3 INT =
(
    SELECT EventID
    FROM EVENT
    WHERE EventName = 'Umhlanga Community Walk'
);

DECLARE @Cat5K INT =
(
    SELECT CategoryID
    FROM CATEGORY
    WHERE CategoryName = '5km Fun Run'
);

DECLARE @Cat10K INT =
(
    SELECT CategoryID
    FROM CATEGORY
    WHERE CategoryName = '10km Road Race'
);

DECLARE @Cat21K INT =
(
    SELECT CategoryID
    FROM CATEGORY
    WHERE CategoryName = '21km Half Marathon'
);

DECLARE @CatWalk INT =
(
    SELECT CategoryID
    FROM CATEGORY
    WHERE CategoryName = '10km Walk'
);

-- Durban Sunrise Run
IF NOT EXISTS
(
    SELECT 1
    FROM EVENT_CATEGORY
    WHERE EventID = @Event1
      AND CategoryID = @Cat5K
)
BEGIN
    INSERT INTO EVENT_CATEGORY
        (EventID, CategoryID, EntryFee, MaxParticipants, AvailableSlots)
    VALUES
        (@Event1, @Cat5K, 80.00, 100, 100);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM EVENT_CATEGORY
    WHERE EventID = @Event1
      AND CategoryID = @Cat10K
)
BEGIN
    INSERT INTO EVENT_CATEGORY
        (EventID, CategoryID, EntryFee, MaxParticipants, AvailableSlots)
    VALUES
        (@Event1, @Cat10K, 120.00, 150, 150);
END;

-- KwaZulu-Natal City Challenge
IF NOT EXISTS
(
    SELECT 1
    FROM EVENT_CATEGORY
    WHERE EventID = @Event2
      AND CategoryID = @Cat10K
)
BEGIN
    INSERT INTO EVENT_CATEGORY
        (EventID, CategoryID, EntryFee, MaxParticipants, AvailableSlots)
    VALUES
        (@Event2, @Cat10K, 150.00, 200, 200);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM EVENT_CATEGORY
    WHERE EventID = @Event2
      AND CategoryID = @Cat21K
)
BEGIN
    INSERT INTO EVENT_CATEGORY
        (EventID, CategoryID, EntryFee, MaxParticipants, AvailableSlots)
    VALUES
        (@Event2, @Cat21K, 220.00, 250, 250);
END;

-- Umhlanga Community Walk
IF NOT EXISTS
(
    SELECT 1
    FROM EVENT_CATEGORY
    WHERE EventID = @Event3
      AND CategoryID = @CatWalk
)
BEGIN
    INSERT INTO EVENT_CATEGORY
        (EventID, CategoryID, EntryFee, MaxParticipants, AvailableSlots)
    VALUES
        (@Event3, @CatWalk, 60.00, 100, 100);
END;
GO


USE RaceDay;
GO

-- =========================
-- ENROLMENTS
-- =========================

DECLARE @Participant1 INT =
(
    SELECT UserID
    FROM [USER]
    WHERE Email = 'sipho.dlamini@example.com'
);

DECLARE @Participant2 INT =
(
    SELECT UserID
    FROM [USER]
    WHERE Email = 'ayanda.khumalo@example.com'
);

DECLARE @Event1Cat5K INT =
(
    SELECT EC.EventCategoryID
    FROM EVENT_CATEGORY EC
    INNER JOIN EVENT E ON EC.EventID = E.EventID
    INNER JOIN CATEGORY C ON EC.CategoryID = C.CategoryID
    WHERE E.EventName = 'Durban Sunrise Run'
      AND C.CategoryName = '5km Fun Run'
);

DECLARE @Event1Cat10K INT =
(
    SELECT EC.EventCategoryID
    FROM EVENT_CATEGORY EC
    INNER JOIN EVENT E ON EC.EventID = E.EventID
    INNER JOIN CATEGORY C ON EC.CategoryID = C.CategoryID
    WHERE E.EventName = 'Durban Sunrise Run'
      AND C.CategoryName = '10km Road Race'
);

DECLARE @Event2Cat21K INT =
(
    SELECT EC.EventCategoryID
    FROM EVENT_CATEGORY EC
    INNER JOIN EVENT E ON EC.EventID = E.EventID
    INNER JOIN CATEGORY C ON EC.CategoryID = C.CategoryID
    WHERE E.EventName = 'KwaZulu-Natal City Challenge'
      AND C.CategoryName = '21km Half Marathon'
);

IF NOT EXISTS
(
    SELECT 1
    FROM ENROLMENT
    WHERE ParticipantID = @Participant1
      AND EventCategoryID = @Event1Cat5K
)
BEGIN
    INSERT INTO ENROLMENT
        (ParticipantID, EventCategoryID, EnrolmentStatus, RaceNumber)
    VALUES
        (@Participant1, @Event1Cat5K, 'Confirmed', 101);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM ENROLMENT
    WHERE ParticipantID = @Participant2
      AND EventCategoryID = @Event1Cat10K
)
BEGIN
    INSERT INTO ENROLMENT
        (ParticipantID, EventCategoryID, EnrolmentStatus, RaceNumber)
    VALUES
        (@Participant2, @Event1Cat10K, 'Confirmed', 201);
END;

IF NOT EXISTS
(
    SELECT 1
    FROM ENROLMENT
    WHERE ParticipantID = @Participant1
      AND EventCategoryID = @Event2Cat21K
)
BEGIN
    INSERT INTO ENROLMENT
        (ParticipantID, EventCategoryID, EnrolmentStatus, RaceNumber)
    VALUES
        (@Participant1, @Event2Cat21K, 'Pending', 301);
END;
GO


USE RaceDay;
GO

-- =========================
-- RESULTS
-- =========================

DECLARE @Enrolment1 INT =
(
    SELECT E.EnrolmentID
    FROM ENROLMENT E
    INNER JOIN [USER] U ON E.ParticipantID = U.UserID
    INNER JOIN EVENT_CATEGORY EC ON E.EventCategoryID = EC.EventCategoryID
    INNER JOIN EVENT EV ON EC.EventID = EV.EventID
    WHERE U.Email = 'sipho.dlamini@example.com'
      AND EV.EventName = 'Durban Sunrise Run'
);

DECLARE @Enrolment2 INT =
(
    SELECT E.EnrolmentID
    FROM ENROLMENT E
    INNER JOIN [USER] U ON E.ParticipantID = U.UserID
    INNER JOIN EVENT_CATEGORY EC ON E.EventCategoryID = EC.EventCategoryID
    INNER JOIN EVENT EV ON EC.EventID = EV.EventID
    WHERE U.Email = 'ayanda.khumalo@example.com'
      AND EV.EventName = 'Durban Sunrise Run'
);

IF NOT EXISTS
(
    SELECT 1 FROM RESULT WHERE EnrolmentID = @Enrolment1
)
BEGIN
    INSERT INTO RESULT
        (EnrolmentID, FinishTime, Position, ResultStatus)
    VALUES
        (@Enrolment1, '00:28:45', 12, 'Finished');
END;

IF NOT EXISTS
(
    SELECT 1 FROM RESULT WHERE EnrolmentID = @Enrolment2
)
BEGIN
    INSERT INTO RESULT
        (EnrolmentID, FinishTime, Position, ResultStatus)
    VALUES
        (@Enrolment2, '00:55:20', 34, 'Finished');
END;
GO

-- =========================================
-- PART D: VERIFICATION QUERIES
-- =========================================

USE RaceDay;
GO

SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
GO


USE RaceDay;
GO

SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
WHERE OBJECT_NAME(fk.parent_object_id) = 'EVENT_CATEGORY';
GO


USE RaceDay;
GO

SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
WHERE OBJECT_NAME(fk.parent_object_id) = 'ENROLMENT';
GO


USE RaceDay;
GO

SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
WHERE OBJECT_NAME(fk.parent_object_id) = 'RESULT';
GO


USE RaceDay;
GO

SELECT 
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN 
    ('USER', 'EVENT', 'CATEGORY', 'EVENT_CATEGORY', 'ENROLMENT', 'RESULT')
ORDER BY TABLE_NAME, ORDINAL_POSITION;
GO


USE RaceDay;
GO

SELECT
    Role,
    COUNT(*) AS NumberOfUsers
FROM [USER]
GROUP BY Role;
GO

SELECT
    COUNT(*) AS NumberOfEvents
FROM EVENT;
GO

SELECT
    E.EventName,
    C.CategoryName,
    EC.EntryFee,
    EC.MaxParticipants,
    EC.AvailableSlots
FROM EVENT_CATEGORY EC
INNER JOIN EVENT E
    ON EC.EventID = E.EventID
INNER JOIN CATEGORY C
    ON EC.CategoryID = C.CategoryID
ORDER BY E.EventName, C.CategoryName;
GO

SELECT
    U.FirstName + ' ' + U.LastName AS Participant,
    E.EventName,
    C.CategoryName,
    EN.EnrolmentStatus,
    EN.RaceNumber
FROM ENROLMENT EN
INNER JOIN [USER] U
    ON EN.ParticipantID = U.UserID
INNER JOIN EVENT_CATEGORY EC
    ON EN.EventCategoryID = EC.EventCategoryID
INNER JOIN EVENT E
    ON EC.EventID = E.EventID
INNER JOIN CATEGORY C
    ON EC.CategoryID = C.CategoryID
ORDER BY E.EventName, Participant;
GO

SELECT
    R.ResultID,
    U.FirstName + ' ' + U.LastName AS Participant,
    E.EventName,
    R.FinishTime,
    R.Position,
    R.ResultStatus
FROM RESULT R
INNER JOIN ENROLMENT EN
    ON R.EnrolmentID = EN.EnrolmentID
INNER JOIN [USER] U
    ON EN.ParticipantID = U.UserID
INNER JOIN EVENT_CATEGORY EC
    ON EN.EventCategoryID = EC.EventCategoryID
INNER JOIN EVENT E
    ON EC.EventID = E.EventID
ORDER BY R.Position;
GO


USE RaceDay;
GO

SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys fk
ORDER BY TableName, ForeignKeyName;
GO