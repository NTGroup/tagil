using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace po.fwdr.api.AppInfra.ContentNegotiators
{
	public class JsonOnlyNegotiator : IContentNegotiator
	{
		private readonly JsonMediaTypeFormatter _jsonFormatter;

		public JsonOnlyNegotiator(JsonMediaTypeFormatter formatter)
		{
			_jsonFormatter = formatter;
		}

		public ContentNegotiationResult Negotiate(Type type, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
		{
			var result = new ContentNegotiationResult(_jsonFormatter, new MediaTypeHeaderValue("application/json"));
			return result;
		}
	}
}