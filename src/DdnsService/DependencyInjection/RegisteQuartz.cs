using DdnsService.Jobs;
using DdnsService.Services;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;


namespace DdnsService
{
    public static class RegisteQuartz
    {
        public static IServiceCollection AddQuartz(this IServiceCollection services)
        {
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = false; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });

            services.AddQuartz(q =>
            {
                q.SchedulerId = $"DdnsScheduler";
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp =>
                {
                    tp.MaxConcurrency = 3;
                });
                // auto-interrupt long-running job
                q.UseJobAutoInterrupt(options =>
                {
                    // this is the default
                    options.DefaultMaxRunTime = TimeSpan.FromMinutes(10);
                });
                // convert time zones using converter that can handle Windows/Linux differences
                q.UseTimeZoneConverter();
            });
            //注册JobScheduler
            services.AddSingleton<IJobSchedulerService, JobSchedulerService>();
            //add base job
            services.AddTransient<DdnsJob>();
            // Quartz.Extensions.Hosting allows you to fire background service that handles scheduler lifecycle
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            });
            return services;
        }

    }
}
