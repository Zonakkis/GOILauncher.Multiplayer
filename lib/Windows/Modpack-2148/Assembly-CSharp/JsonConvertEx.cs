using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

// Token: 0x020002E5 RID: 741
public static class JsonConvertEx
{
	// Token: 0x06001927 RID: 6439 RVA: 0x000785A4 File Offset: 0x000767A4
	public static string SerializeObject<T>(T value)
	{
		StringWriter stringWriter = new StringWriter(new StringBuilder(), CultureInfo.InvariantCulture);
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
		using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = Formatting.Indented;
			jsonTextWriter.IndentChar = '\t';
			jsonTextWriter.Indentation = 1;
			jsonSerializer.Serialize(jsonTextWriter, value, typeof(T));
		}
		return stringWriter.ToString();
	}
}
