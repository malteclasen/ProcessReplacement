using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ProcessReplacement
{
	class Program
	{
		private const string EventSource = "Process Replacement";
		private const string EventLog = "Application";

		static Process GetParentProcess(int childProcessId)
		{
			var query = string.Format("SELECT ParentProcessId FROM Win32_Process WHERE ProcessId = {0}", childProcessId);
			var search = new ManagementObjectSearcher("root\\CIMV2", query);
			var results = search.Get().GetEnumerator();
			if (!results.MoveNext()) return null;
			var queryObj = results.Current;
			uint parentId = (uint)queryObj["ParentProcessId"];
			try
			{
				return Process.GetProcessById((int) parentId);
			}
			catch (ArgumentException)
			{
				return null;
			}			
		}

		static void RecordCall(string[] args)
		{
			var message = new StringBuilder();
			message.AppendLine("Process Replacement started");
			message.AppendLine("Arguments: " + string.Join(" ", args));

			AppendParentProcessInfo(message, Process.GetCurrentProcess().Id);

			System.Diagnostics.EventLog.WriteEntry(EventSource, message.ToString());			
		}

		private static void AppendParentProcessInfo(StringBuilder message, int childProcessId)
		{
			var parent = GetParentProcess(childProcessId);
			if (parent != null)
			{
				try
				{
					message.AppendLine(
						string.Format("Parent:\n- Name={0}\n- FileName={1}\n- FileDescription={2}\n- CompanyName={3}\n",
									  parent.ProcessName,
									  parent.MainModule.FileName,
									  parent.MainModule.FileVersionInfo.FileDescription,
									  parent.MainModule.FileVersionInfo.CompanyName
							));
				}
				catch (Win32Exception)
				{
					message.AppendLine(
						string.Format("Parent:\n- Name={0}\n",
									  parent.ProcessName
							));
				}

				AppendParentProcessInfo(message, parent.Id);
			}
			else
			{
				message.AppendLine("Could not determine parent process.");
			}
		}

		static void Main(string[] args)
		{
			try
			{
				if (!System.Diagnostics.EventLog.SourceExists(EventSource))
					System.Diagnostics.EventLog.CreateEventSource(EventSource, EventLog);
			}
			catch (SecurityException)
			{
				Console.WriteLine("Please run this program once with administrator rights to enable event logging.");
			}
			RecordCall(args);
			Console.WriteLine(@"This process has been redirected, the call has been recorded in the windows event log {0} as {1}. Please check the registry keys in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\ to run whatever you intended to run.", EventLog, EventSource);
		}
	}
}
