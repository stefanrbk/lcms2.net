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

void Clean(ref string value)
{
	var sb = new StringBuilder(value);

	sb.Replace("\t", "    ");
	sb.Replace("\r", "");

	while (sb[0] == '\n')
		sb.Remove(0, 1);

	while (sb[^1] == '\n')
		sb.Remove(sb.Length - 1, 1);

	var enu = false;
	var func = false;
	var control = false;
	for (var i = 0; i < sb.Length; i++)
	{
		try
		{
			if (sb[i] == 'e' && sb[i + 1] == 'n' && sb[i + 2] == 'u' && sb[i + 3] == 'm')
				enu = true;
			if (sb[i] == '(')
				func = true;

			if (sb[i] is '{' or '(' or '[')
				control = true;
			if (sb[i] == '{')
			{
				if (sb[i + 1] == '\n' && sb[i + 2] == '\n')
				{
					sb.Remove(i-- + 1, 1);
					continue;
				}
				if (sb[i + 1] != '\n')
				{
					sb.Insert(i-- + 1, '\n');
					continue;
				}
			}
			if (sb[i] is ' ')
			{
				if (sb[i + 1] is '\n')
				{
					sb.Remove(i--, 1);
					i--;
					continue;
				}
				if (sb[i + 1] is '}')
				{
					sb[i--] = '\n';
					continue;
				}
			}
			if (sb[i] is ',')
			{

				if (func && (sb[i + 1] is '\n'))
				{
					sb.Remove(i-- + 1, 1);
					continue;
				}
				if (enu && (sb[i + 1] is not '\n'))
				{
					sb.Insert(i-- + 1, '\n');
					continue;
				}
			}
			if (sb[i] == '\n')
			{
				if (!control)
				{
					sb.Remove(i--, 1);
					continue;
				}
				if (sb[i + 1] == ' ' && sb[i + 2] == ' ' && sb[i + 3] == ' ' && sb[i + 4] == ' ')
				{
					if (sb[i + 5] == '\n')
					{
						//sb.Remove(i-- +1, 4);
						//continue;
					}
					i += 4;
					continue;
				}
				if (sb[i + 1] != '}')
				{
					if (sb[i + 1] is not ' ' || sb[i + 2] is not ' ' || sb[i + 3] is not ' ' || sb[i + 4] is not ' ')
					{
						sb.Insert(i-- + 1, ' ');
						continue;
					}
				}
				if (sb[i + 1] == '\n' && sb[i + 2] == '}')
				{
					sb.Remove(i--, 1);
					continue;
				}
			}
			if (sb[i] == ' ' && sb[i+ 1] == ' ')
			{
				sb.Remove(i--, 1);
				continue;
			}
			if (sb[i] == '}' && sb[i - 1] == '\n' && (sb[i - 2] == ' ' || sb[i - 2] == '\n'))
			{
				sb.Remove(i-- - 2, 1);
				i--;
				continue;
			}
			if (sb[i] == '/' && sb[i + 1] == '/')
			{
				var j = 0;
				while (sb[i + j++] != '\n') { }
				sb.Remove(i--, --j);
				i--;
				continue;
			}
		}
		catch { }
	}
	var asString = sb.ToString();
	if (value != asString)
	{
		Console.WriteLine("Cleaning data...");
		Console.WriteLine(value);
		Console.WriteLine("\t\t\t\t\t\t⇓");
		Console.WriteLine(asString);
		Console.WriteLine("-------------------------");
	}

	value = asString;
}



foreach (var entry in Entries)
{
	if (!String.IsNullOrEmpty(entry.Original_Name))
		Clean(ref entry.Original_Name);

	if (!String.IsNullOrEmpty(entry.New_Name))
		Clean(ref entry.New_Name);
}

//Entries.Context.SubmitChanges();