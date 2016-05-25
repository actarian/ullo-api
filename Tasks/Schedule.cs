using Hangfire;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web;
using System.IO;

namespace Ullo.Tasks
{
	public class Schedule
	{
		public static void Start()
		{
			RecurringJob.AddOrUpdate("AlwaysOn", () => AlwaysOn(), "*/15 * * * *"); //15 minutes
			
			/* -- Hanfire syntax --	            
			 
			1) Fire and forget 
			//BackgroundJob.Enqueue(() => Console.WriteLine("Hello, world!"));            
			 
			2) Calling methods with delay
			//BackgroundJob.Schedule(() => Console.WriteLine("Hello, world"),TimeSpan.FromDays(1));
				 
			3) Performing recurrent tasks
			//RecurringJob.AddOrUpdate("some-id",() => Console.Write("Easy!"), Cron.Daily); //CRON expression: http://en.wikipedia.org/wiki/Cron#CRON_expression
				 
			4) Remove recurring job
			//RecurringJob.RemoveIfExists("some-id");
				 
			5) Trigger recurring job
			//RecurringJob.Trigger("some-id");
				 
			*/
		}

		public static void AlwaysOn()
		{
			Console.Write("AlwaysOn");
			//noop
			using (var client = new HttpClient())
			{
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["ApiUri"]);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				HttpResponseMessage response = client.GetAsync("api/stream/anonymous").Result;
				if (!response.IsSuccessStatusCode)
				{
					var content = response.Content.ReadAsStringAsync().Result;
					throw new Exception(content);
				} else
				{
					var content = response.Content.ReadAsStringAsync().Result;
					Console.Write("AlwaysOn", content);
				}
			}
		}

	}
}
