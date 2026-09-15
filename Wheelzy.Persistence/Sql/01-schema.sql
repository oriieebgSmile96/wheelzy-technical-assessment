-- Wheelzy technical assessment — Question 1
-- Normalized SQL Server schema for a car-sale case, buyer quotes, and status history.
-- Goal: store each make / model / submodel / zip / buyer once, then relate them.

IF OBJECT_ID(N'dbo.CaseStatusHistory', N'U') IS NOT NULL DROP TABLE dbo.CaseStatusHistory;
IF OBJECT_ID(N'dbo.CaseQuote', N'U') IS NOT NULL DROP TABLE dbo.CaseQuote;
IF OBJECT_ID(N'dbo.SaleCase', N'U') IS NOT NULL DROP TABLE dbo.SaleCase;
IF OBJECT_ID(N'dbo.BuyerZipCode', N'U') IS NOT NULL DROP TABLE dbo.BuyerZipCode;
IF OBJECT_ID(N'dbo.Buyer', N'U') IS NOT NULL DROP TABLE dbo.Buyer;
IF OBJECT_ID(N'dbo.ZipCode', N'U') IS NOT NULL DROP TABLE dbo.ZipCode;
IF OBJECT_ID(N'dbo.CarSubmodel', N'U') IS NOT NULL DROP TABLE dbo.CarSubmodel;
IF OBJECT_ID(N'dbo.CarModel', N'U') IS NOT NULL DROP TABLE dbo.CarModel;
IF OBJECT_ID(N'dbo.CarMake', N'U') IS NOT NULL DROP TABLE dbo.CarMake;
IF OBJECT_ID(N'dbo.CaseStatusType', N'U') IS NOT NULL DROP TABLE dbo.CaseStatusType;
GO

CREATE TABLE dbo.CarMake
(
    MakeId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarMake PRIMARY KEY,
    Name     NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_CarMake_Name UNIQUE (Name)
);

CREATE TABLE dbo.CarModel
(
    ModelId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarModel PRIMARY KEY,
    MakeId   INT NOT NULL,
    Name     NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_CarModel_Make_Name UNIQUE (MakeId, Name),
    CONSTRAINT FK_CarModel_CarMake FOREIGN KEY (MakeId) REFERENCES dbo.CarMake (MakeId)
);

CREATE TABLE dbo.CarSubmodel
(
    SubmodelId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CarSubmodel PRIMARY KEY,
    ModelId    INT NOT NULL,
    Name       NVARCHAR(100) NOT NULL,
    CONSTRAINT UQ_CarSubmodel_Model_Name UNIQUE (ModelId, Name),
    CONSTRAINT FK_CarSubmodel_CarModel FOREIGN KEY (ModelId) REFERENCES dbo.CarModel (ModelId)
);

CREATE TABLE dbo.ZipCode
(
    ZipCode CHAR(5) NOT NULL CONSTRAINT PK_ZipCode PRIMARY KEY,
    CONSTRAINT CK_ZipCode_Format CHECK (ZipCode LIKE '[0-9][0-9][0-9][0-9][0-9]')
);

-- QuoteAmount is the buyer's standard offer for any car in the zips they cover.
-- Example: Buyer ABC covers 10 zip codes and pays $500 for each car.
CREATE TABLE dbo.Buyer
(
    BuyerId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Buyer PRIMARY KEY,
    Name         NVARCHAR(200) NOT NULL,
    QuoteAmount  DECIMAL(12,2) NOT NULL,
    CONSTRAINT CK_Buyer_QuoteAmount CHECK (QuoteAmount >= 0),
    CONSTRAINT UQ_Buyer_Name UNIQUE (Name)
);

CREATE TABLE dbo.BuyerZipCode
(
    BuyerId INT NOT NULL,
    ZipCode CHAR(5) NOT NULL,
    CONSTRAINT PK_BuyerZipCode PRIMARY KEY (BuyerId, ZipCode),
    CONSTRAINT FK_BuyerZipCode_Buyer FOREIGN KEY (BuyerId) REFERENCES dbo.Buyer (BuyerId),
    CONSTRAINT FK_BuyerZipCode_ZipCode FOREIGN KEY (ZipCode) REFERENCES dbo.ZipCode (ZipCode)
);

CREATE TABLE dbo.CaseStatusType
(
    StatusTypeId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CaseStatusType PRIMARY KEY,
    Name                NVARCHAR(100) NOT NULL,
    RequiresStatusDate  BIT NOT NULL CONSTRAINT DF_CaseStatusType_RequiresStatusDate DEFAULT (0),
    CONSTRAINT UQ_CaseStatusType_Name UNIQUE (Name)
);

-- One row per customer request to sell a car.
-- Year stays on the case because it belongs to this specific vehicle, not the catalog.
CREATE TABLE dbo.SaleCase
(
    CaseId      INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SaleCase PRIMARY KEY,
    Year        SMALLINT NOT NULL,
    SubmodelId  INT NOT NULL,
    ZipCode     CHAR(5) NOT NULL,
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_SaleCase_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_SaleCase_Year CHECK (Year BETWEEN 1900 AND 2100),
    CONSTRAINT FK_SaleCase_CarSubmodel FOREIGN KEY (SubmodelId) REFERENCES dbo.CarSubmodel (SubmodelId),
    CONSTRAINT FK_SaleCase_ZipCode FOREIGN KEY (ZipCode) REFERENCES dbo.ZipCode (ZipCode)
);

-- Quotes are snapshotted onto the case so a later change to Buyer.QuoteAmount
-- does not rewrite history. Exactly one quote can be current per case.
CREATE TABLE dbo.CaseQuote
(
    CaseQuoteId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CaseQuote PRIMARY KEY,
    CaseId      INT NOT NULL,
    BuyerId     INT NOT NULL,
    Amount      DECIMAL(12,2) NOT NULL,
    IsCurrent   BIT NOT NULL CONSTRAINT DF_CaseQuote_IsCurrent DEFAULT (0),
    CreatedAt   DATETIME2(0) NOT NULL CONSTRAINT DF_CaseQuote_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_CaseQuote_Amount CHECK (Amount >= 0),
    CONSTRAINT UQ_CaseQuote_Case_Buyer UNIQUE (CaseId, BuyerId),
    CONSTRAINT FK_CaseQuote_SaleCase FOREIGN KEY (CaseId) REFERENCES dbo.SaleCase (CaseId),
    CONSTRAINT FK_CaseQuote_Buyer FOREIGN KEY (BuyerId) REFERENCES dbo.Buyer (BuyerId)
);

CREATE UNIQUE INDEX UX_CaseQuote_Current
    ON dbo.CaseQuote (CaseId)
    WHERE IsCurrent = 1;

CREATE TABLE dbo.CaseStatusHistory
(
    CaseStatusId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CaseStatusHistory PRIMARY KEY,
    CaseId        INT NOT NULL,
    StatusTypeId  INT NOT NULL,
    IsCurrent     BIT NOT NULL CONSTRAINT DF_CaseStatusHistory_IsCurrent DEFAULT (0),
    ChangedBy     NVARCHAR(256) NOT NULL,
    ChangedAt     DATETIME2(0) NOT NULL CONSTRAINT DF_CaseStatusHistory_ChangedAt DEFAULT (SYSUTCDATETIME()),
    -- Mandatory only when the status type requires it (Picked Up). Enforced below.
    StatusDate    DATETIME2(0) NULL,
    CONSTRAINT FK_CaseStatusHistory_SaleCase FOREIGN KEY (CaseId) REFERENCES dbo.SaleCase (CaseId),
    CONSTRAINT FK_CaseStatusHistory_CaseStatusType FOREIGN KEY (StatusTypeId) REFERENCES dbo.CaseStatusType (StatusTypeId)
);

CREATE UNIQUE INDEX UX_CaseStatusHistory_Current
    ON dbo.CaseStatusHistory (CaseId)
    WHERE IsCurrent = 1;

GO

CREATE OR ALTER TRIGGER dbo.TR_CaseStatusHistory_RequireStatusDate
ON dbo.CaseStatusHistory
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.CaseStatusType t ON t.StatusTypeId = i.StatusTypeId
        WHERE t.RequiresStatusDate = 1
          AND i.StatusDate IS NULL
    )
    BEGIN
        THROW 50001, 'StatusDate is required when the status type requires a status date (for example Picked Up).', 1;
    END
END;
GO

INSERT INTO dbo.CaseStatusType (Name, RequiresStatusDate)
VALUES
    (N'Pending Acceptance', 0),
    (N'Accepted', 0),
    (N'Picked Up', 1);
GO
