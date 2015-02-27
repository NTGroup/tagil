using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using po.fwdr.api.AppInfra.ContentNegotiators;

namespace po.fwdr.api.AppConfigurators
{
	public class JsonContentConfig
	{
		public static JsonSerializerSettings GeneralSettings { get { return _generalSettings; } }

		public static void Register(HttpConfiguration config)
		{
			var jsonFormatter = new JsonMediaTypeFormatter();
			//optional: set serializer settings here
			jsonFormatter.SerializerSettings = _generalSettings;

			config.Formatters.Clear();
			config.Formatters.Add(jsonFormatter);

			config.Services.Replace(typeof(IContentNegotiator), new JsonOnlyNegotiator(jsonFormatter));

		}

		private static readonly JsonSerializerSettings _generalSettings =
			new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				NullValueHandling = NullValueHandling.Ignore,
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = new List<JsonConverter>
				{
					new StringEnumConverter()
					//,
					//new IsoDateTimeConverter()
				},
				DateTimeZoneHandling = DateTimeZoneHandling.Utc
			};
	}
}