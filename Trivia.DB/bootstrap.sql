-- bootstrap script for Trivia application
-- Please run before attempting to start the application
------ uncomment next few lines if you'd *really* like to recreate the database
 --use master;
 --go
 --ALTER DATABASE  [Trivia]
 --SET SINGLE_USER
 --WITH ROLLBACK IMMEDIATE
 --drop database [Trivia]
 --go
------ normal creation after here
declare @spid int;
select @spid = min(spid) from master.dbo.sysprocesses where dbid = db_id('Trivia');
while @spid is not null
    begin
        exec('Kill '+@spid);
        select @spid = min(spid) from master.dbo.sysprocesses where dbid = db_id('Trivia');
    end;
go
create database Trivia;
go
use Trivia;
go
if not exists (select name from master..syslogins where name = 'Trivia')
    begin
        create login [Trivia] with password = 'tricky';
    end;
go
create user [Trivia]
	for login [Trivia]
	with default_schema = dbo
GO
grant connect to [Trivia]
go
exec sp_addrolemember N'db_datareader', N'Trivia';
go
exec sp_addrolemember N'db_datawriter', N'Trivia';
go
exec sp_addrolemember N'db_owner', N'Trivia';
go