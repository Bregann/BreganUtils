using BreganUtils.Dtos;
using RestSharp;
using Serilog;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BreganUtils.ProjectMonitor
{
    public class ProjectMonitorCommon
    {
        public static void ReportProjectUp(string projectName)
        {
            if (ProjectMonitorConfig.ApiKey == "" || ProjectMonitorConfig.Mode == "debug")
            {
                return;
            }

            try
            {
                //Ping the api every 5s reporting that the service is up
                var timer = new Timer(10000);
                timer.Elapsed += (sender, e) => SendProjectApiRequestUptime(sender, e, projectName);
                timer.Enabled = true;
                timer.Start();

                //Ping the api every 10s reporting that the server is up
                var timer2 = new Timer(20000);
                timer2.Elapsed += (sender, e) => SendAddAndUpdateSystemData(sender, e);
                timer2.Enabled = true;
                timer2.Start();
            }
            catch (Exception e)
            {
                Log.Error($"[Project Monitor] Error with - ReportProjectUp {e}");
                return;
            }
        }

        private static async void SendProjectApiRequestUptime(object sender, ElapsedEventArgs e, string projectName)
        {
            try
            {
                //Get the process data
                var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                var cpuUsage = await GetCpuUsageOfApplication();
                var ramUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/CommonUpdates/AddAndUpdateProjectHealthStatus", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);

                request.AddBody(new AddAndUpdateProjectHealthStatusDto
                {
                    ProjectName = projectName,
                    ProjectUptime = uptime,
                    CPUUsage = cpuUsage,
                    RAMUsage = ramUsage
                });

                client.Post(request);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Monitor] Error with SendProjectApiRequestUptime - {ex}");
                return;
            }
        }

        private static void SendAddAndUpdateSystemData(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Get the data
                var sysUptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                var sysName = Environment.MachineName;

                var client = new RestClient(ProjectMonitorConfig.ApiUrl);
                var request = new RestRequest("/api/CommonUpdates/AddAndUpdateSystemData", Method.Post);

                request.AddHeader("Authorization", ProjectMonitorConfig.ApiKey);

                request.AddBody(new AddAndUpdateSystemDataDto
                {
                    SystemName = sysName,
                    SystemUptime = sysUptime
                });

                client.Post(request);
            }
            catch (Exception ex)
            {
                Log.Error($"[Project Monitor] Error with SendAddAndUpdateSystemData - {ex}");
                return;
            }
        }

        private static async Task<double> GetCpuUsageOfApplication()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;

            await Task.Delay(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
    }
}