-- ============================================================
-- REOLMARKED - COMPLETE DATABASE SETUP
-- ============================================================
-- This script does four things:
-- 1. Deletes the old database (so we start completely fresh).
-- 2. Creates three tables: SHELF, SHELFRENTER, RENTAL.
-- 3. Inserts test data (shelves, renters, and one rental).
-- 4. Shows the contents so we can verify everything works.
-- ============================================================

-- ============================================================
-- STEP 1: DROP THE OLD DATABASE (IF IT EXISTS)
-- ============================================================

-- Switch to the "master" system database.
-- We cannot drop a database while we are currently inside it.
USE master;
GO

-- "GO" separates batches. Some commands (like CREATE DATABASE)
-- must be the only statement in their batch.
-- Check whether a database named ReolmarkedDb already exists.
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'ReolmarkedDb')
BEGIN
-- Force all other connections out and roll back any active transactions.
-- Without SINGLE_USER, the DROP DATABASE would fail if the app is running.
ALTER DATABASE ReolmarkedDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

-- Delete the database and everything in it.
DROP DATABASE ReolmarkedDb;
END
GO

-- ============================================================
-- STEP 2: CREATE THE FRESH DATABASE
-- ============================================================

-- Create a brand-new, empty database.
CREATE DATABASE ReolmarkedDb;
GO

-- Switch into the new database so the rest of the script runs inside it.
USE ReolmarkedDb;
GO

-- ============================================================
-- STEP 3: CREATE THE SHELF TABLE (Reoler)
-- ============================================================
-- Each row represents one physical shelf in the store.
CREATE TABLE dbo.SHELF (
-- Primary key, auto-numbered by SQL Server (1, 2, 3, ...).
-- IDENTITY(1,1) means "start at 1, increment by 1".
ShelfNumber INT IDENTITY(1,1) NOT NULL,

-- Status is text: Ledig / Booket / Opsagt / UdeAfDrift.
-- NVARCHAR because C# stores the enum as a string.
-- NOT NULL means the column must always have a value.
[Status]        NVARCHAR(50) NOT NULL,

-- Configuration is text: TreHylderOgBøjle / SeksHylder.
[Configuration] NVARCHAR(50) NOT NULL,

-- PK makes ShelfNumber unique across all rows.
CONSTRAINT PK_Shelf PRIMARY KEY (ShelfNumber)
);
GO

-- ============================================================
-- STEP 4: CREATE THE SHELFRENTER TABLE (Reollejere)
-- ============================================================
-- Each row represents one person who can rent shelves.
CREATE TABLE dbo.SHELFRENTER (
-- Auto-numbered primary key.
RenterId INT IDENTITY(1,1) NOT NULL,

-- Name and phone lengths match the constants in ShelfRenter.cs.
-- That way validation happens identically in C# and in SQL.
FirstName   NVARCHAR(50) NOT NULL,
LastName    NVARCHAR(50) NOT NULL,
PhoneNumber NVARCHAR(20) NOT NULL,

CONSTRAINT PK_Renter PRIMARY KEY (RenterId)
);
GO

-- ============================================================
-- STEP 5: CREATE THE RENTAL TABLE (Udlejninger)
-- ============================================================
-- Links a shelf to a renter, starting on a specific date.
CREATE TABLE dbo.RENTAL (
-- ShelfNumber is BOTH PK and FK here.
-- As PK: only one active rental per shelf (matches 0..1 in the DCD).
-- As FK: it must match an existing shelf in the SHELF table.
ShelfNumber INT NOT NULL,

-- Only the date is needed, so we use DATE instead of DATETIME.
StartDate   DATE NOT NULL,

-- FK to SHELFRENTER: which renter is renting.
RenterId    INT NOT NULL,

-- FK to the employee who created the rental (for auditing).
UserId      INT NOT NULL,

-- Composite PK: ShelfNumber may only appear once in this table.
CONSTRAINT PK_Rental PRIMARY KEY (ShelfNumber),

-- FK: prevents inserting a rental for a shelf that doesn't exist.
CONSTRAINT FK_Rental_Shelf  FOREIGN KEY (ShelfNumber) REFERENCES dbo.SHELF(ShelfNumber),

-- FK: prevents inserting a rental for a renter that doesn't exist.
CONSTRAINT FK_Rental_Renter FOREIGN KEY (RenterId)    REFERENCES dbo.SHELFRENTER(RenterId)
);
GO

-- ============================================================
-- STEP 6: INSERT TEST DATA
-- ============================================================

-- Test renters. IDs are assigned automatically: 1, 2.
INSERT INTO dbo.SHELFRENTER (FirstName, LastName, PhoneNumber)
VALUES
('Mette', 'Frederiksen', '22334455'), -- RenterId = 1
('Søren', 'Pape', '22887766'); -- RenterId = 2
GO

-- Test shelves. Status values MUST match the C# Status enum exactly.
-- Configuration values MUST match the C# Configuration enum exactly.
-- Otherwise Enum.TryParse in MapShelf would fall back to defaults.
INSERT INTO dbo.SHELF ([Status], [Configuration])
VALUES
('Ledig', 'SeksHylder'), -- ShelfNumber = 1
('Ledig', 'TreHylderOgBøjle'), -- ShelfNumber = 2
('Ledig', 'SeksHylder'), -- ShelfNumber = 3
('Booket', 'SeksHylder'), -- ShelfNumber = 4
('Opsagt', 'TreHylderOgBøjle'), -- ShelfNumber = 5
('UdeAfDrift', 'SeksHylder'), -- ShelfNumber = 6
('Ledig', 'TreHylderOgBøjle'), -- ShelfNumber = 7
('Ledig', 'SeksHylder'); -- ShelfNumber = 8
GO

-- One existing rental: shelf 4 is booked by Søren Pape (RenterId = 2).
-- UserId = 1 (the employee who created the rental).
INSERT INTO dbo.RENTAL (ShelfNumber, StartDate, RenterId, UserId)
VALUES (4, '2026-01-15', 2, 1);
GO

-- ============================================================
-- STEP 7: VERIFY THE DATA
-- ============================================================
SELECT * FROM dbo.SHELF;
SELECT * FROM dbo.SHELFRENTER;
SELECT * FROM dbo.RENTAL;
GO