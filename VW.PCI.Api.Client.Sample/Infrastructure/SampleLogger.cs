using System;
using VW.Api.RestClient;

namespace VW.PCI.Api.Client.Sample.Infrastructure
{
    /// <summary>
    /// Logger mẫu ghi ra Console.
    /// Thực tế thay bằng NLog, Serilog, log4net, hoặc ILogger của framework bạn đang dùng.
    /// </summary>
    public class SampleLogger : ILogger
    {
        public void Debug(string message) => Log("DEBUG", message);
        public void Info(string message)  => Log("INFO ", message);
        public void Warn(string message)  => Log("WARN ", message);
        public void Error(string message) => Log("ERROR", message);
        public void Fatal(string message) => Log("FATAL", message);

        public void Debug(Exception ex) => Log("DEBUG", ex.ToString());
        public void Info(Exception ex)  => Log("INFO ", ex.ToString());
        public void Warn(Exception ex)  => Log("WARN ", ex.ToString());
        public void Error(Exception ex) => Log("ERROR", ex.ToString());
        public void Fatal(Exception ex) => Log("FATAL", ex.ToString());

        private void Log(string level, string message) =>
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
    }
}
