-- ============================================================
-- REOLMARKED - COMPLETE DATABASE SETUP
-- ============================================================
-- This script:
-- 1. Drops the old database if it exists.
-- 2. Creates four tables: SHELF, SHELFRENTER, RENTAL, MONTHLY_SETTLEMENT.
-- 3. Inserts test data according to the updated schema.
-- 4. Displays database content for verification.
-- ============================================================

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ReolmarkedTestDb')
BEGIN
    ALTER DATABASE ReolmarkedTestDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ReolmarkedTestDb;
END
GO


-- ============================================================
-- STEP 2: CREATE THE FRESH DATABASE
-- ============================================================
CREATE DATABASE ReolmarkedDb;
GO

USE ReolmarkedDb;
GO

-- ============================================================
-- STEP 3: CREATE THE ENUM TABLES
-- ============================================================
CREATE TABLE dbo.SHELFCONFIGURATION (
    ShelfConfigurationId    INT IDENTITY(1,1) NOT NULL,
    ShelfConfigurationText      NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_ShelfConfiguration    PRIMARY KEY (ShelfConfigurationId)
);
GO

INSERT INTO dbo.SHELFCONFIGURATION(ShelfConfigurationText)
VALUES
('TreHylderOgBøjle'), -- ShelfConfigurationId = 1
('SeksHylder');    -- ShelfConfigurationId = 2
GO

CREATE TABLE dbo.[STATUS] (
    StatusId    INT IDENTITY(1,1) NOT NULL,
    StatusText      NVARCHAR(30)      NOT NULL,
    CONSTRAINT PK_Status    PRIMARY KEY (StatusId)
);
GO

INSERT INTO dbo.[STATUS] (StatusText)
VALUES
('Ledig'), -- StatusId = 1
('Booket'), -- StatusId = 2
('Opsagt'), -- StatusId = 3
('UdeAfDrift') -- StatusId = 4
GO

-- ============================================================
-- STEP 4: CREATE THE SHELF TABLE
-- ============================================================
CREATE TABLE dbo.SHELF (
    ShelfNumber        INT IDENTITY(1,1) NOT NULL,
    StatusId           INT NOT NULL,
    ShelfConfigurationId INT NOT NULL,
    CONSTRAINT PK_Shelf PRIMARY KEY (ShelfNumber),
    CONSTRAINT FK_Shelf_Status  FOREIGN KEY (StatusId) REFERENCES dbo.[STATUS](StatusId),
    CONSTRAINT FK_Shelf_ShelfConfiguration  FOREIGN KEY (ShelfConfigurationId) REFERENCES dbo.SHELFCONFIGURATION(ShelfConfigurationId)
);
GO

-- ============================================================
-- STEP 5: CREATE THE SHELFRENTER TABLE
-- ============================================================
CREATE TABLE dbo.SHELFRENTER (
    RenterId    INT IDENTITY(1,1) NOT NULL,
    FirstName   NVARCHAR(50) NOT NULL,
    LastName    NVARCHAR(50) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Balance     DECIMAL(18,2) NOT NULL CONSTRAINT DF_ShelfRenter_Balance DEFAULT 0.00,
    CONSTRAINT PK_Renter PRIMARY KEY (RenterId)
);
GO

-- ============================================================
-- STEP 6: CREATE THE RENTAL TABLE
-- ============================================================
CREATE TABLE dbo.RENTAL (
    RentalId    INT IDENTITY(1,1) NOT NULL,
    ShelfNumber INT NOT NULL,
    RenterId    INT NOT NULL,
    StartDate   DATE NOT NULL,
    EndDate     DATE NULL,
    CONSTRAINT PK_Rental PRIMARY KEY (RentalId),
    CONSTRAINT FK_Rental_Shelf  FOREIGN KEY (ShelfNumber) REFERENCES dbo.SHELF(ShelfNumber),
    CONSTRAINT FK_Rental_Renter FOREIGN KEY (RenterId)    REFERENCES dbo.SHELFRENTER(RenterId)
);
GO

-- ============================================================
-- STEP 7: CREATE THE MONTHLY_SETTLEMENT TABLE
-- ============================================================
CREATE TABLE dbo.MONTHLY_SETTLEMENT (
    SettlementId   INT IDENTITY(1,1) NOT NULL,
    RenterId       INT NOT NULL,
    [Month]        INT NOT NULL,
    TotalSales     DECIMAL(18,2) NOT NULL,
    Commission     DECIMAL(18,2) NOT NULL,
    TotalShelfRent DECIMAL(18,2) NOT NULL,
    ShelfCount     INT NOT NULL,
    ExtraDiscount  DECIMAL(18,2) NOT NULL,
    FinalAmount    DECIMAL(18,2) NOT NULL,
    IsProcessed    BIT NOT NULL,
    CONSTRAINT PK_MonthlySettlement PRIMARY KEY (SettlementId),
    CONSTRAINT FK_Settlement_Renter  FOREIGN KEY (RenterId) REFERENCES dbo.SHELFRENTER(RenterId),
    CONSTRAINT CK_Settlement_Month   CHECK ([Month] >= 1 AND [Month] <= 12)
);
GO

-- ============================================================
-- STEP 8: INSERT TEST DATA
-- ============================================================
INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber, Balance)
VALUES
('Mette', 'Frederiksen', '22334455', 0.00), -- RenterId = 1
('Søren', 'Pape', '22887766', 150.00);    -- RenterId = 2
GO

INSERT INTO dbo.SHELF (StatusId, ShelfConfigurationId)
VALUES
(1, 2),  -- ShelfNumber = 1
(1, 1),  -- ShelfNumber = 2
(1, 2),  -- ShelfNumber = 3
(2, 2),  -- ShelfNumber = 4
(3, 1),  -- ShelfNumber = 5
(4, 2),  -- ShelfNumber = 6
(1, 1),  -- ShelfNumber = 7
(1, 2);  -- ShelfNumber = 8
GO

INSERT INTO dbo.RENTAL (ShelfNumber, RenterId, StartDate, EndDate)
VALUES 
(4, 2, '2026-02-01', NULL),
(5, 1, '2026-01-01', '2026-02-28');
GO

INSERT INTO dbo.MONTHLY_SETTLEMENT 
(RenterId, [Month], TotalSales, Commission, TotalShelfRent, ShelfCount, ExtraDiscount, FinalAmount, IsProcessed)
VALUES 
(2, 2, 1000.00, 150.00, 400.00, 1, 0.00, 450.00, 1);
GO

-- ============================================================
-- STEP 9: VERIFY THE DATA
-- ============================================================
SELECT * FROM dbo.SHELFCONFIGURATION;
SELECT * FROM dbo.[STATUS];
SELECT * FROM dbo.SHELF;
SELECT * FROM dbo.SHELFRENTER;
SELECT * FROM dbo.RENTAL;
SELECT * FROM dbo.MONTHLY_SETTLEMENT;

-- Using LEFT JOIN to SELECT SHELF with enum values
SELECT dbo.SHELF.ShelfNumber, dbo.SHELFCONFIGURATION.ShelfConfigurationText, dbo.[STATUS].StatusText 
FROM dbo.SHELF 
LEFT JOIN dbo.SHELFCONFIGURATION ON dbo.SHELF.ShelfConfigurationId=dbo.SHELFCONFIGURATION.ShelfConfigurationId
LEFT JOIN dbo.[STATUS] ON dbo.SHELF.StatusId=dbo.[STATUS].StatusId;
GO