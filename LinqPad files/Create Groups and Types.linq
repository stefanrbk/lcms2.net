<Query Kind="Statements">
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

var pub = new Groups() { Name = "Public" };
var plug = new Groups() { Name = "Plugin" };

Groups.InsertOnSubmit(pub);
Groups.InsertOnSubmit(plug);
Groups.Context.SubmitChanges();

Types.InsertOnSubmit(new Types() { Name = "Structs/Typedefs", GroupEntity = pub });
Types.InsertOnSubmit(new Types() { Name = "Functions", GroupEntity = pub });

Types.InsertOnSubmit(new Types() { Name = "Structs/Typedefs", GroupEntity = plug });
Types.InsertOnSubmit(new Types() { Name = "Functions", GroupEntity = plug });

Types.InsertOnSubmit(new Types() { Name = "Enums", GroupEntity = pub });
Types.InsertOnSubmit(new Types() { Name = "Enums", GroupEntity = plug });

Types.Context.SubmitChanges();