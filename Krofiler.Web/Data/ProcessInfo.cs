using System;

namespace Krofiler.Web.Data
{
	public class ProcessInfo
	{
		public int PID { get; internal set; }
		public string Name { get; internal set; }
		public string Arguments { get; internal set; }
		public DateTime StartTime { get; internal set; }
	}
}