using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Runtime;

namespace Krofiler.Web.Data
{
	public class DumpCollectingService
	{
		private ConcurrentDictionary<int, List<DumpInfo>> Dumps = new ConcurrentDictionary<int, List<DumpInfo>>();

		public DumpInfo CreateNewDump(int pid)
		{
			var tmpFile = Path.GetTempFileName();
			var dumper = new Dumper();
			dumper.Collect(pid, tmpFile, true, Dumper.DumpTypeOption.Heap);

			var list = Dumps.GetOrAdd(pid, new List<DumpInfo>());
			var parsedDump = ParseDump(tmpFile);
			list.Add(parsedDump);
			return parsedDump;
		}

		public List<DumpInfo> GetDumps(int pid)
		{
			if (Dumps.TryGetValue(pid, out var result))
				return result;
			return new List<DumpInfo>();
		}

		private DumpInfo ParseDump(string dumpFileName)
		{
			using DataTarget dataTarget = DataTarget.LoadDump(dumpFileName);
			using ClrRuntime runtime = dataTarget.ClrVersions.Single().CreateRuntime();
			ClrHeap heap = runtime.Heap;

			return new DumpInfo(dumpFileName, File.GetCreationTime(dumpFileName), runtime.Heap.Segments.Sum(s => (long)s.CommittedMemory.Length));
		}
	}
}
