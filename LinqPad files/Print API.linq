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
  <RuntimeVersion>6.0</RuntimeVersion>
</Query>

var sbMain = new StringBuilder();
var sbContents = new StringBuilder();

var gList = Groups.OrderBy(g => g.Name);
foreach (var group in gList)
{
	if (Types.Where(t => t.GroupEntity == group).Count() == 0)
		continue;
	var gTitle = $"{group.Name} API";
	var gLink = gTitle.ToLowerInvariant().Replace(' ', '-');
	sbContents.AppendLine($"- [{gTitle}](#{gLink})");
	sbMain.AppendLine($"## {gTitle}\n");

	var tList = Types.Where(t => t.GroupEntity == group).OrderBy(t => t.Name);
	foreach (var type in tList)
	{
		if (Entries.Where(e => e.TypeEntity == type).Count() == 0)
			continue;
		var tTitle = type.Name;
		var tLink = tTitle.ToLowerInvariant().Replace(' ', '-');
		sbContents.AppendLine($"  - [{tTitle}](#{tLink})");
		sbMain.AppendLine($"### {type.Name}\n");
		
		var eList = Entries.Where(e => e.TypeEntity == type).OrderBy(e => e.Original_Title).ToList();
		foreach (var entry in eList)
		{
			if (eList[0] != entry)
				sbMain.AppendLine($"---\n");
				
			var eTitle = entry.Original_Title;
			if (!String.IsNullOrEmpty(entry.New_Title))
			{
				eTitle += $" ⇆ {entry.New_Title}";
			}
			var eLink = eTitle.ToLowerInvariant().Replace(' ', '-');
			sbContents.AppendLine($"    - [{eTitle}](#{eLink})");
			sbMain.AppendLine($"#### {eTitle}\n");

			if (entry.Original_Location is not null && entry.Original_Location != "")
				sbMain.AppendLine($"`{entry.Original_Location}`");
			sbMain.AppendLine($"```c\n{entry.Original_Name}\n```");

			if (!String.IsNullOrEmpty(entry.New_Location))
				sbMain.AppendLine($"`{entry.New_Location}`");
			if (!String.IsNullOrEmpty(entry.New_Name))
				sbMain.AppendLine($"```csharp\n{entry.New_Name}\n```");
		}
	}
}

@$"# Lcms2 API

## Table of Contents

{sbContents.ToString()}
{sbMain.ToString()}".Dump();