-- Delete all data from tables (in correct order to respect FK constraints)
DELETE FROM UploadDuplicates;
DELETE FROM SentRecords;
DELETE FROM ReceivedRecords;
DELETE FROM UploadBatches;

-- Reset identity seeds
DBCC CHECKIDENT ('UploadBatches', RESEED, 0);
DBCC CHECKIDENT ('SentRecords', RESEED, 0);
DBCC CHECKIDENT ('ReceivedRecords', RESEED, 0);
DBCC CHECKIDENT ('UploadDuplicates', RESEED, 0);

SELECT 'Database emptied successfully!' as Message;
