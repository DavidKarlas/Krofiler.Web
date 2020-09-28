using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;

namespace Krofiler.Web.Data
{
	public class ProcessListingService
	{
		[DllImport("libc", SetLastError = true)]
		public static extern int getpgid(int pid);

		private static Process GetProcessById(int processId)
		{
			var sw = Stopwatch.StartNew();
			try {
				if (getpgid(processId) < 0)
					return null;
				return Process.GetProcessById(processId);
			}
			catch (ArgumentException) {
				return null;
			}
			finally {
				Console.WriteLine("Time:" + sw.ElapsedMilliseconds);
			}
		}

		public Task<ProcessInfo[]> GetAllProcessesInfo()
		{
			return Task.Run(() =>
			{
				var processes = DiagnosticsClient.GetPublishedProcesses()
						.Select(GetProcessById)
						.Where(process => process != null)
						.OrderBy(process => process.ProcessName)
						.ThenBy(process => process.Id);
				var procInfos = new List<ProcessInfo>();
				foreach (var proc in processes) {
					var procInfo = new ProcessInfo() {
						PID = proc.Id,
						Name = proc.ProcessName,
						Arguments = GetProcessStartArguments(proc.Id),
						StartTime = proc.StartTime
					};
					procInfos.Add(procInfo);
				}
				return procInfos.ToArray();
			});
		}

		private string GetProcessStartArguments(int id)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
				var proc = new Process {
					StartInfo = new ProcessStartInfo {
						FileName = "ps",
						Arguments = $"-ww -o command= -p {id}",
						UseShellExecute = false,
						RedirectStandardOutput = true,
						CreateNoWindow = true
					}
				};
				proc.Start();
				return proc.StandardOutput.ReadToEnd();
			} else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + id))
				using (var objects = searcher.Get()) {
					return objects.Cast<ManagementBaseObject>().SingleOrDefault()?["CommandLine"]?.ToString();
				}
			} else {
				return File.ReadAllText($"/proc/{id}/cmdline");
			}
		}
	}
}
