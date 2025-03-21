-- SQL Script to fix ParaBirimiIliski foreign key constraints
-- This script will drop existing constraints and recreate them with ON DELETE NO ACTION

-- First, check if the constraints exist and drop them if they do
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID')
BEGIN
    ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID]
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID')
BEGIN
    ALTER TABLE [ParaBirimiIliski] DROP CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID]
END

-- Now recreate the constraints with ON DELETE NO ACTION
ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID] 
FOREIGN KEY ([KaynakParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION;

ALTER TABLE [ParaBirimiIliski] ADD CONSTRAINT [FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID] 
FOREIGN KEY ([HedefParaBirimiID]) REFERENCES [ParaBirimi] ([ParaBirimiID]) ON DELETE NO ACTION;

-- Add an explanation comment
-- This fixes the error: "Introducing FOREIGN KEY constraint 'FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID' 
-- on table 'ParaBirimiIliski' may cause cycles or multiple cascade paths. 
-- Specify ON DELETE NO ACTION or ON UPDATE NO ACTION, or modify other FOREIGN KEY constraints." 