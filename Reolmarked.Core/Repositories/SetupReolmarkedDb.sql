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

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ReolmarkedDb')
BEGIN
    ALTER DATABASE ReolmarkedDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE ReolmarkedDb;
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
-- STEP 3: CREATE THE SHELF TABLE
-- ============================================================
CREATE TABLE dbo.SHELF (
    ShelfNumber        INT IDENTITY(1,1) NOT NULL,
    [Status]           NVARCHAR(30) NOT NULL,
    ShelfConfiguration NVARCHAR(50) NOT NULL,
    CONSTRAINT PK_Shelf PRIMARY KEY (ShelfNumber)
);
GO

-- ============================================================
-- STEP 4: CREATE THE SHELFRENTER TABLE
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
-- STEP 5: CREATE THE RENTAL TABLE
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
-- STEP 6: CREATE THE MONTHLY_SETTLEMENT TABLE
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
-- STEP 7: INSERT TEST DATA
-- ============================================================
INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber, Balance)
VALUES
('Mette', 'Frederiksen', '22334455', 0.00), -- RenterId = 1
('Søren', 'Pape', '22887766', 150.00);    -- RenterId = 2
GO

INSERT INTO dbo.SHELF ([Status], ShelfConfiguration)
VALUES
('Ledig', 'SeksHylder'),         -- ShelfNumber = 1
('Ledig', 'TreHylderOgBøjle'),  -- ShelfNumber = 2
('Ledig', 'SeksHylder'),         -- ShelfNumber = 3
('Booket', 'SeksHylder'),        -- ShelfNumber = 4
('Opsagt', 'TreHylderOgBøjle'),  -- ShelfNumber = 5
('UdeAfDrift', 'SeksHylder'),   -- ShelfNumber = 6
('Ledig', 'TreHylderOgBøjle'),  -- ShelfNumber = 7
('Ledig', 'SeksHylder');        -- ShelfNumber = 8
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
-- STEP 8: VERIFY THE DATA
-- ============================================================
SELECT * FROM dbo.SHELF;
SELECT * FROM dbo.SHELFRENTER;
SELECT * FROM dbo.RENTAL;
SELECT * FROM dbo.MONTHLY_SETTLEMENT;
GO