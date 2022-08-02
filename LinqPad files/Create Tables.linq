<Query Kind="SQL">
  <Connection>
    <ID>684f068f-0dfc-4fb6-9089-cad28fcb754e</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Server>(localdb)\MSSQLLocalDB</Server>
    <AllowDateOnlyTimeOnly>true</AllowDateOnlyTimeOnly>
    <DeferDatabasePopulation>true</DeferDatabasePopulation>
    <Database>lcms2_API</Database>
  </Connection>
</Query>

drop table Entries;
drop table Groups;
drop table Types;

create table Groups (
	Id int identity(1,1) primary key,
	Name varchar(255)
)

create table Types (
	Id int identity(1,1) primary key,
	[Group] int foreign key references Groups(Id),
	Name varchar(255)
)

create table Entries (
	Id int identity(1,1) primary key,
	Type int foreign key references Types(Id),
	Original_Title varchar(100),
	Original_Location varchar(100),
	Original_Name text,
	New_Title varchar(100),
	New_Location varchar(100),
	New_Name text
)