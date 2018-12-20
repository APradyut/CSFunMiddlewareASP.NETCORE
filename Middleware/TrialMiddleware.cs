using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace TrialMiddleware.Middleware
{
	// You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
	public class TrialMiddleware
	{
		private readonly RequestDelegate _next;

		public TrialMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext httpContext)
		{
			var bodyStr = "";
			var req = httpContext.Request;

			// Allows using several time the stream in ASP.Net Core
			req.EnableRewind();

			// Arguments: Stream, Encoding, detect encoding, buffer size 
			// AND, the most important: keep stream opened
			using (StreamReader reader
					  = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
			{
				bodyStr = reader.ReadToEnd();
			}

			// Rewind, so the core is not lost when it looks the body for the request
			req.Body.Position = 0;

			Console.WriteLine("Reading request in trial middleware" + bodyStr);

			//await _next(httpContext);
			
			Stream originalBody = httpContext.Response.Body;

			try
			{
				using (var memStream = new MemoryStream())
				{
					httpContext.Response.Body = memStream;

					await _next(httpContext);

					memStream.Position = 0;
					string responseBody = new StreamReader(memStream).ReadToEnd();

					memStream.Position = 0;
					await memStream.CopyToAsync(originalBody);
					Console.WriteLine("Reading response in trial middleware: " + responseBody);
				}

			}
			finally
			{
				httpContext.Response.Body = originalBody;
			}

		}
	}

	// Extension method used to add the middleware to the HTTP request pipeline.
	public static class TrialMiddlewareExtensions
	{
		public static IApplicationBuilder UseTrialMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<TrialMiddleware>();
		}
	}
}
