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
	public class SecondMiddleware
	{
		private readonly RequestDelegate _next;

		public SecondMiddleware(RequestDelegate next)
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

			Console.WriteLine("Second read" + bodyStr);


			await _next(httpContext);
		}
	}

	// Extension method used to add the middleware to the HTTP request pipeline.
	public static class SecondMiddlewareExtensions
	{
		public static IApplicationBuilder UseSecondMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<SecondMiddleware>();
		}
	}
}
