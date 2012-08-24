using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using Amazon.Runtime;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GlacierTool
{
	public static class GlacierTool
	{
		public static void Main(string[] args)
		{
			Console.Write("GlacierTool\n(c) 2012 Justin Gottula\n\n");
			
			if (args.Length < 1)
				Usage();
			
			/* commands */
			switch (args[0].ToLower())
			{
			case "listvaults":
			case "inventory":
			case "download":
				Console.Error.Write("Command not implemented yet.\n");
				Environment.Exit(1);
				break;
			case "upload":
				CheckArgs(args.Length, 8);
				Upload(args.Skip(1).ToArray());
				break;
			case "hints":
				Console.Write("No hints yet.\n");
				break;
			default:
				Console.Error.Write(String.Format("Can't understand the " +
					"command '{0}'.\n", args[0]));
				Usage();
				break;
			}
		}
		
		public static void Upload(string[] args)
		{
			string id = args[0];
			string access = args[1];
			string secret = args[2];
			RegionEndpoint region = ParseRegion(args[3]);
			string vault = args[4];
			string path = args[5];
			string desc = args[6];
			
			var info = new FileInfo(path);
			long size = 0;
			
			if (info.Exists)
				size = info.Length;
			else
			{
				Console.Error.Write("The given path does not exist.\n");
				Usage();
			}
			
			Console.Write("About to perform the following upload:\n\n" +
				String.Format("{0,-16}{1}\n", "Path:", path) + 
				String.Format("{0,-16}{1:f6} GiB\n", "Size:",
					(decimal)size / (1024m * 1024m * 1024m)) +
				String.Format("{0,-16}\"{1}\"\n", "Description:", desc) +
				String.Format("\n{0,-16}{1}\n", "Region:", region.DisplayName) +
				String.Format("{0,-16}{1}\n", "Vault:", vault) +
				String.Format("\n{0,-16}${1:f2}/month\n", "Storage cost:",
					((decimal)size / (1024m * 1024m * 1024m) * 0.01m)) +
				String.Format("{0,-16}${1:f2} (< 90 days)\n", "Deletion fee:",
					((decimal)size / (1024m * 1024m * 1024m) * 0.03m)) +
				String.Format("{0,-16}${1:f2}\n", "Retrieval cost:",
					((decimal)size / (1024m * 1024m * 1024m) * 0.12m)) +
				String.Format("\n{0,-16}{1}\n", "Account ID:", id) +
				String.Format("{0,-16}{1}\n", "Access key:", access) +
				String.Format("{0,-16}{1}\n", "Secret key:", secret) +
				"\nARE YOU SURE YOU WANT TO PROCEED? [y/N] ");
			
			bool proceed = false;
			do
			{
				ConsoleKeyInfo key = Console.ReadKey(true);
				
				switch (Char.ToLower(key.KeyChar))
				{
				case 'y':
					Console.WriteLine(key.KeyChar);
					proceed = true;
					break;
				case 'n':
					Console.Write(key.KeyChar);
					goto case '\n';
				case '\n':
					Console.WriteLine();
					Console.Write("Upload aborted.\n");
					Environment.Exit(0);
					break;
				}
			}
			while (!proceed);
			
			Console.Write("\nUpload started at {0:G}.\n", DateTime.Now);
			
			Console.CancelKeyPress += CtrlC;
			
			var progress = new Progress();
			
			var options = new UploadOptions();
			options.AccountId = id;
			options.StreamTransferProgress += progress.Update;
			
			var creds = new BasicAWSCredentials(access, secret);
			var manager = new ArchiveTransferManager(creds, region);
			
			progress.Start();
			
			/* not asynchronous */
			UploadResult result = manager.Upload(vault, desc, path, options);
			
			Console.Write("\n\nUpload complete.\n" +
				String.Format("Archive ID: {0}\n", result.ArchiveId));
		}
		
		public static RegionEndpoint ParseRegion(string str)
		{
			RegionEndpoint region = null;
			
			foreach (var field in typeof(RegionEndpoint).GetFields())
			{
				object obj = field.GetValue(null);
				
				if (obj is RegionEndpoint && field.Name == str)
				{
					region = (RegionEndpoint)obj;
					break;
				}
			}
			
			if (region == null)
			{
				Console.Error.Write("Can't parse the region given.\n");
				Usage();
			}
			
			return region;
		}
		
		public static void CheckArgs(int actual, int correct)
		{
			if (actual != correct)
			{
				if (actual < correct)
					Console.Error.Write("Too few arguments were given for " +
						"that command.\n");
				else
					Console.Error.Write("Too many arguments were given for " +
						"that command.\n");
				
				Usage();
			}
		}
		
		public static void Usage()
		{
			Console.Error.Write("\nUsage: [mono] GlacierTool.exe " +
				"<command> <parameters>\n\n" +
				String.Format("{0,-12}{1}\n", "Command:",
					"Parameters & Description:") +
				String.Format("{0,-12}{1}\n", "listvaults",
					"account_id acc_key sec_key region") +
				String.Format("{0,-12}  {1}\n", "",
					"list the vaults located in <region>") +
				String.Format("{0,-12}{1}\n", "inventory",
					"account_id acc_key sec_key region vault") +
				String.Format("{0,-12}  {1}\n", "",
					"get the latest inventory for <vault>") +
				String.Format("{0,-12}{1}\n", "upload",
					"account_id acc_key sec_key region vault path " +
					"description") +
				String.Format("{0,-12}  {1}\n", "",
					"upload the file located at <path> as an archive to " +
					"<vault>") +
				String.Format("{0,-12}{1}\n", "download",
					"account_id acc_key sec_key region vault arch_id path") +
				String.Format("{0,-12}  {1}\n", "",
					"download the archive specified by <arch_id> from " +
					"<vault> to the") +
				String.Format("{0,-12}  {1}\n", "", "file located at <path>") +
				String.Format("{0,-12}{1}\n", "hints", "") +
				String.Format("{0,-12}  {1}\n", "",
					"display some helpful hints about potential problems"));
			
			Console.Error.Write("\nAvailable regions are listed below.\n" +
				String.Format("\n{0,-16}{1}\n", "Region:", "Description:"));
			foreach (var field in typeof(RegionEndpoint).GetFields())
			{
				object obj = field.GetValue(null);
				
				if (obj is RegionEndpoint)
				{
					RegionEndpoint reg = (RegionEndpoint)obj;
					
					Console.Error.WriteLine(String.Format("{0,-16}{1}",
						field.Name, reg.DisplayName));
				}
			}
			
			Environment.Exit(1);
		}
		
		public static void Hints()
		{
			/* hint: mono ssl problems need root certs installed
			 * (use the mozilla thing) */
			/* hint: where to find acc id, access key, secret key */
		}
		
		public static void CtrlC(object sender, ConsoleCancelEventArgs e)
		{
			Console.Write("\n\nUpload canceled.\n");
			Environment.Exit(1);
		}
	}
	
	public class Progress
	{
		Stopwatch watch;
		
		public Progress()
		{
			watch = new Stopwatch();
			
			Console.Write("\n{0}{1,11}{2,13}{3,11}{4,12}\n", "Percent",
				"Uploaded", "Total Size", "Elapsed", "Remaining");
		}
		
		public void Start()
		{
			watch.Start();
		}
		
		public void Update(object sender, StreamTransferProgressArgs e)
		{
			/* TODO: add bitrate indicator (use timer with interval of 1-10
			 * seconds, which compares bytes transferred before and after)
			 * also, use this for the ETA calculation (with longer interval) */
			
			float percent;
			if (e.TotalBytes != 0)
				percent = 100f * ((float)e.TransferredBytes /
					(float)e.TotalBytes);
			else
				percent = 0f;
			
			TimeSpan eta;
			if (e.TransferredBytes != 0)
			{
				long bytesLeft = e.TotalBytes - e.TransferredBytes;
				float rate = (float)watch.Elapsed.Ticks /
					(float)e.TransferredBytes;
				eta = new TimeSpan((long)((float)bytesLeft * rate));
			}
			else
				eta = new TimeSpan(0);
			
			/* return to first column */
			Console.CursorLeft = 0;
			
			/* BUG: the hack used below will roll over if either time is over
			 * 24 hours, which is likely for large archives
			 * 
			 * in the future: use __d __:__:__ (right-aligned space-padded
			 * two-digit days, only displayed if >24 hrs)
			 */
			
			/* Mono can't handle custom TimeSpan formatting, so we convert the
			 * TimeSpans to DateTimes before printing them */
			Console.Write(String.Format("{0,6:f1}%{1,7:d} MiB{2,9:d} MiB" +
				"{3,11:HH:mm:ss}{4,12:HH:mm:ss}", percent,
				(e.TransferredBytes / 1024 / 1024),
				(e.TotalBytes / 1024 / 1024), new DateTime(watch.Elapsed.Ticks),
				new DateTime(eta.Ticks)));
		}
	}
}
