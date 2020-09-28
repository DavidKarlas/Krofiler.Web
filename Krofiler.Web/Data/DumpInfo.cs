using System;

namespace Krofiler.Web.Data
{
	public class DumpInfo
	{
		public DumpInfo(string dumpFileName, System.DateTime creationTime, long commitedMemory)
		{
			DumpFileName = dumpFileName;
			CreationTime = creationTime;
			CommitedMemory = commitedMemory;
		}

		public string DumpFileName { get; }
		public DateTime CreationTime { get; }
		public long CommitedMemory { get; }
	}
}