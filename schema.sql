CREATE TABLE Customer(CustomerId int IDENTITY PRIMARY KEY, FullName nvarchar(100) NOT NULL, MeterNumber nvarchar(30) NOT NULL UNIQUE, Address nvarchar(200) NOT NULL);
CREATE TABLE MeterReading(ReadingId int IDENTITY PRIMARY KEY, CustomerId int NOT NULL REFERENCES Customer, ReadingDate date NOT NULL, Units int NOT NULL CHECK(Units>=0));
CREATE UNIQUE INDEX IX_Reading_Customer_Date ON MeterReading(CustomerId,ReadingDate DESC) INCLUDE(Units);
CREATE TABLE Bill(BillId int IDENTITY PRIMARY KEY, ReadingId int NOT NULL UNIQUE REFERENCES MeterReading, UsageKwh int NOT NULL, Rate decimal(9,2) NOT NULL, Amount decimal(14,2) NOT NULL);

GO
CREATE VIEW dbo.BillingDashboard AS
SELECT b.BillId,c.FullName AS Customer,c.MeterNumber,r.ReadingDate,b.UsageKwh,b.Rate,b.Amount
FROM Bill b JOIN MeterReading r ON r.ReadingId=b.ReadingId JOIN Customer c ON c.CustomerId=r.CustomerId;
GO
CREATE PROCEDURE dbo.RecordReading @CustomerId int,@ReadingDate date,@Units int AS
BEGIN
 SET NOCOUNT ON;
 SET XACT_ABORT ON;
 BEGIN TRAN;
 IF NOT EXISTS(SELECT 1 FROM Customer WITH(UPDLOCK,HOLDLOCK) WHERE CustomerId=@CustomerId)
   THROW 50001,'Customer ID does not exist.',1;
 DECLARE @previous int,@previousDate date;
 SELECT TOP(1) @previous=Units,@previousDate=ReadingDate FROM MeterReading WHERE CustomerId=@CustomerId ORDER BY ReadingDate DESC;
 IF @ReadingDate <= @previousDate OR @Units < @previous THROW 50002,'The date must advance and cumulative units cannot decrease.',1;
 INSERT MeterReading(CustomerId,ReadingDate,Units) VALUES(@CustomerId,@ReadingDate,@Units);
 IF @previous IS NOT NULL
   INSERT Bill(ReadingId,UsageKwh,Rate,Amount) VALUES(CONVERT(int,SCOPE_IDENTITY()),@Units-@previous,6.50,(@Units-@previous)*6.50);
 COMMIT;
END;