﻿namespace PiControlPanel.Infrastructure.OnDemand.Services
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using NLog;
    using PiControlPanel.Domain.Contracts.Constants;
    using PiControlPanel.Domain.Contracts.Infrastructure.OnDemand;
    using PiControlPanel.Domain.Contracts.Util;
    using PiControlPanel.Domain.Models;
    using PiControlPanel.Domain.Models.Hardware.Cpu;

    public class CpuService : ICpuService
    {
        private readonly ISubject<CpuTemperature> cpuTemperatureSubject;
        private readonly ILogger logger;

        public CpuService(ISubject<CpuTemperature> cpuTemperatureSubject, ILogger logger)
        {
            this.cpuTemperatureSubject = cpuTemperatureSubject;
            this.logger = logger;
        }

        public Task<Cpu> GetAsync()
        {
            logger.Info("Infra layer -> CpuService -> GetAsync");
            var cpu = this.GetCpu();
            return Task.FromResult(cpu);
        }

        public Task<CpuTemperature> GetTemperatureAsync()
        {
            logger.Info("Infra layer -> CpuService -> GetTemperatureAsync");
            var temperature = this.GetTemperature();
            return Task.FromResult(temperature);
        }

        public IObservable<CpuTemperature> GetTemperatureObservable()
        {
            logger.Info("Infra layer -> CpuService -> GetTemperatureObservable");
            return this.cpuTemperatureSubject.AsObservable();
        }

        public Task<CpuAverageLoad> GetAverageLoadAsync(BusinessContext context, int cores)
        {
            logger.Info("Infra layer -> CpuService -> GetAverageLoadAsync");
            var averageLoad = this.GetAverageLoad(cores);
            return Task.FromResult(averageLoad);
        }

        public Task<CpuRealTimeLoad> GetRealTimeLoadAsync(BusinessContext context)
        {
            logger.Info("Infra layer -> CpuService -> GetRealTimeLoadAsync");
            var realTimeLoad = this.GetRealTimeLoad();
            return Task.FromResult(realTimeLoad);
        }

        public void PublishTemperature(CpuTemperature temperature)
        {
            logger.Info("Infra layer -> CpuService -> PublishTemperature");
            this.cpuTemperatureSubject.OnNext(temperature);
        }

        private Cpu GetCpu()
        {
            var result = BashCommands.CatProcCpuInfo.Bash();
            logger.Debug($"Result of '{BashCommands.CatProcCpuInfo}' command: '{result}'");
            string[] lines = result.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            var cores = lines.Count(line => line.StartsWith("processor"));
            logger.Debug($"Number of cores: '{cores}'");
            var model = lines.Last(line => line.StartsWith("model name"))
                .Split(':')[1].Trim();
            logger.Debug($"Cpu model: '{model}'");

            return new Cpu()
            {
                Cores = cores,
                Model = model
            };
        }

        private CpuTemperature GetTemperature()
        {
            var result = BashCommands.MeasureTemp.Bash();
            logger.Debug($"Result of '{BashCommands.MeasureTemp}' command: '{result}'");

            var temperatureResult = result.Substring(result.IndexOf('=') + 1, result.IndexOf("'") - (result.IndexOf('=') + 1));
            logger.Debug($"Temperature substring: '{temperatureResult}'");

            double temperature;
            if (double.TryParse(temperatureResult, out temperature))
            {
                return new CpuTemperature()
                {
                    Temperature = temperature,
                    DateTime = DateTime.Now
                };
            }
            logger.Warn($"Could not parse temperature: '{temperatureResult}'");
            return null;
        }

        private CpuAverageLoad GetAverageLoad(int cores)
        {
            var result = BashCommands.Top.Bash();
            logger.Debug($"Result of '{BashCommands.Top}' command: '{result}'");
            string[] lines = result.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            var averageLoadInfo = lines.First(l => l.Contains("load average:"));
            var regex = new Regex(@"load average: (?<lastMinute>\d+.\d{2}), (?<last5Minutes>\d+.\d{2}), (?<last15Minutes>\d+.\d{2})$");
            var groups = regex.Match(averageLoadInfo).Groups;

            return new CpuAverageLoad()
            {
                LastMinute = (100 * double.Parse(groups["lastMinute"].Value)) / cores,
                Last5Minutes = (100 * double.Parse(groups["last5Minutes"].Value)) / cores,
                Last15Minutes = (100 * double.Parse(groups["last15Minutes"].Value)) / cores,
                DateTime = DateTime.Now
            };
        }

        private CpuRealTimeLoad GetRealTimeLoad()
        {
            var result = BashCommands.Top.Bash();
            logger.Debug($"Result of '{BashCommands.Top}' command: '{result}'");
            string[] lines = result.Split(new[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);

            var realTimeLoadInfo = lines.First(l => l.StartsWith("%Cpu(s):"));
            var regex = new Regex(@"^%Cpu\(s\):\s*(?<user>\d{1,3}.\d{1}) us,\s*(?<kernel>\d{1,3}.\d{1}) sy, .*$");
            var groups = regex.Match(realTimeLoadInfo).Groups;

            return new CpuRealTimeLoad()
            {
                User = double.Parse(groups["user"].Value),
                Kernel = double.Parse(groups["kernel"].Value),
                DateTime = DateTime.Now
            };
        }
    }
}
