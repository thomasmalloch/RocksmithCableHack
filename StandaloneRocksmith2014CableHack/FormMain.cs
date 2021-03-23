using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace StandaloneRocksmith2014CableHack
{
	public partial class FormMain : Form
	{
		[DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
		public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out uint lpNumberOfBytesRead);

		[DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
		public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, out uint lpNumberOfBytesWritten);

		private const string RocksmithStartQuery = "SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'Rocksmith2014.exe'";
		private const string RocksmithStopQuery = "SELECT * FROM __InstanceDeletionEvent WITHIN 10 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'Rocksmith2014.exe'";
		private const string RocksmithRunningQuery = "SELECT * FROM Win32_Process WHERE Name = 'Rocksmith2014.exe'";
		private const string AudioDeviceQuery = "SELECT * FROM Win32_SoundDevice WHERE DeviceID LIKE '%USB%'";
		private const string AudioDeviceChangeQuery = "SELECT * FROM __InstanceOperationEvent  WITHIN 5 WHERE TargetInstance ISA 'Win32_SoundDevice' AND TargetInstance.DeviceID LIKE '%USB%'";

		private readonly ManagementEventWatcher RocksmithStartWatcher;
		private readonly ManagementEventWatcher RocksmithExitWatcher;
		private readonly ManagementEventWatcher USBAudioWatcher;

		private long PIDLocation = 0;
		private long VIDLocation = 0;
		private volatile int RocksmithPID = 0;

		private bool _IsRocksmithRunning = false;

		private bool IsRocksmithRunning
		{
			get => this._IsRocksmithRunning;
			set
			{
				if (this._IsRocksmithRunning == value)
					return;

				this._IsRocksmithRunning = value;
				this.btnHack.Enabled = value;
				if (this._IsRocksmithRunning)
				{
					this.LogLine($"Rocksmith found. PID: {this.RocksmithPID}", Color.Green);
					if ((this.chkAuto.Checked) &&
					    (this.cboAudioDevices.Items.Count > 0) &&
					    (this.cboAudioDevices.SelectedItem is AudioDevice device))
					{
						new Thread(() => this.HackTHREAD(this.RocksmithPID, device)).Start();
					}
				}
				else
				{
					this.LogLine("Rocksmith exited.", Color.Orange);
					this.PIDLocation = 0;
					this.VIDLocation = 0;
				}
			}
		}

		public FormMain()
		{
			this.InitializeComponent();

			this.chkAuto.CheckedChanged -= this.chkAuto_CheckedChanged;
			this.chkWatchRocksmith.CheckedChanged -= this.chkWatchRocksmith_CheckedChanged;

			this.chkAuto.Checked = Properties.Settings.Default.IsAutoHacking;
			this.chkWatchRocksmith.Checked = Properties.Settings.Default.IsListening;

			this.chkAuto.CheckedChanged += this.chkAuto_CheckedChanged;
			this.chkWatchRocksmith.CheckedChanged += this.chkWatchRocksmith_CheckedChanged;

			this.btnRefreshDevices_Click(null, null);

			// Watchers
			this.RocksmithStartWatcher = new ManagementEventWatcher(new WqlEventQuery(RocksmithStartQuery));
			this.RocksmithStartWatcher.EventArrived += this.RocksmithStartWatcher_EventArrived;

			this.RocksmithExitWatcher = new ManagementEventWatcher(new WqlEventQuery(RocksmithStopQuery));
			this.RocksmithExitWatcher.EventArrived += this.RocksmithExitWatcher_EventArrived;

			this.USBAudioWatcher = new ManagementEventWatcher(new WqlEventQuery(AudioDeviceChangeQuery));
			this.USBAudioWatcher.EventArrived += (o, e) =>
			{
				try
				{
					this.BeginInvoke(new Action(() => { this.btnRefreshDevices_Click(null, null); }));
				}
				catch (Exception ex)
				{
					this.LogLineTHREADSAFE(ex.Message, Color.Red);
				}
			};

			this.USBAudioWatcher.Start();

			if (this.chkWatchRocksmith.Checked)
			{
				this.RocksmithStartWatcher.Start();
			}
			else
			{
				this.btnHack.Enabled = true;
				return;
			}

			this.LogLine("Listening for Rocksmith 2014 to start ...");
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				this.components?.Dispose();

			this.RocksmithStartWatcher?.Dispose();
			this.RocksmithExitWatcher?.Dispose();
			this.USBAudioWatcher?.Dispose();
			base.Dispose(disposing);
		}

		private List<AudioDevice> GetAudioDevices()
		{
			try
			{
				List<AudioDevice> devices = new List<AudioDevice>();
				using (ManagementObjectSearcher objSearcher = new ManagementObjectSearcher(AudioDeviceQuery))
				{
					foreach (ManagementBaseObject m in objSearcher.Get())
					{
						ManagementObject device = m as ManagementObject;
						if (device == null)
							continue;

						string id = device.GetPropertyValue("DeviceID").ToString();
						string name = device.GetPropertyValue("Name").ToString();
						AudioDevice audioDevice = new AudioDevice(name, id);
						if (string.IsNullOrEmpty(audioDevice.PID) || string.IsNullOrEmpty(audioDevice.VID))
							continue;

						devices.Add(audioDevice);
					}
				}

				return devices;
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
				return null;
			}
		}

		private void btnRefreshDevices_Click(object sender, EventArgs e)
		{
			try
			{
				this.LogLine("Refreshing devices ...");
				this.cboAudioDevices.Items.Clear();
				bool selected = false;
				List<AudioDevice> devices = this.GetAudioDevices();
				if ((devices?.Count ?? 0) == 0)
					throw new Exception("No audio devices detected!");

				foreach (AudioDevice device in devices)
				{
					int index = this.cboAudioDevices.Items.Add(device);
					if (!device.ID.Equals(Properties.Settings.Default.SelectedDevice))
						continue;

					this.cboAudioDevices.SelectedIndex = index;
					selected = true;
					break;
				}

				this.LogLine($"{ devices.Count } device{ ((devices.Count > 1) ? "s" : "") } found.");
				if (!selected)
					this.cboAudioDevices.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
			}
		}

		private void LogLine(string message)
		{
			this.LogLine(message, SystemColors.Window);
		}

		private void LogLineTHREADSAFE(string message, Color colour)
		{
			this.BeginInvoke(new Action(() =>
			{
				this.LogLine(message, colour);
			}));
		}

		private void LogLine(string message, Color colour)
		{
			this.txtStatus.SelectionLength = 0;
			this.txtStatus.SelectionStart = this.txtStatus.TextLength;
			this.txtStatus.SelectionColor = colour;
			this.txtStatus.AppendText(message + Environment.NewLine);
			this.txtStatus.SelectionStart = this.txtStatus.TextLength;
			this.txtStatus.ScrollToCaret();
			Application.DoEvents();
		}

		private void btnHack_Click(object sender, EventArgs e)
		{
			try
			{
				if (this.RocksmithPID != 0)
				{
					if ((this.cboAudioDevices.Items.Count > 0) && (this.cboAudioDevices.SelectedItem is AudioDevice device))
						new Thread(() => this.HackTHREAD(this.RocksmithPID, device)).Start();

					return;
				}

				this.LogLine("Looking for Rocksmith ...");
				string handle = null;
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(RocksmithRunningQuery))
				{
					foreach (ManagementBaseObject o in searcher.Get())
						handle = o["Handle"].ToString();
				}

				if (!int.TryParse(handle, out int pid))
					pid = 0;

				if (pid == 0)
				{
					this.LogLine("Rocksmith not detected.", Color.Orange);
					return;
				}

				this.LogLine($"Rocksmith found. PID: {this.RocksmithPID}", Color.Green);
				if ((this.cboAudioDevices.Items.Count > 0) && (this.cboAudioDevices.SelectedItem is AudioDevice audioDevice))
					new Thread(() => this.HackTHREAD(pid, audioDevice)).Start();
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
			}
		}

		private void cboAudioDevices_SelectionChangeCommitted(object sender, EventArgs e)
		{
			try
			{
				if (!(this.cboAudioDevices.SelectedItem is AudioDevice device))
					return;

				Properties.Settings.Default.SelectedDevice = device.ID;
				Properties.Settings.Default.Save();
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
			}
		}

		private void chkWatchRocksmith_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				this.RocksmithPID = 0;
				this.btnHack.Enabled = (!this.chkWatchRocksmith.Checked);
				this._IsRocksmithRunning = false;
				Properties.Settings.Default.IsListening = this.chkWatchRocksmith.Checked;
				Properties.Settings.Default.Save();
				if (this.chkWatchRocksmith.Checked)
				{
					// check to see if rocksmith is already running
					string handle = null;
					using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(RocksmithRunningQuery))
					{
						foreach (ManagementBaseObject o in searcher.Get())
							handle = o["Handle"].ToString();
					}

					if (!int.TryParse(handle, out int pid))
						pid = 0;

					this.RocksmithPID = pid;
					this.IsRocksmithRunning = (this.RocksmithPID != 0);
					if (this.IsRocksmithRunning)
						this.RocksmithExitWatcher.Start();
					else
						this.RocksmithStartWatcher.Start();

					this.LogLine($"Listening for Rocksmith 2014 to {((this.IsRocksmithRunning) ? "stop" : "start")} ...");
				}
				else
				{
					this.RocksmithStartWatcher.Stop();
					this.RocksmithExitWatcher.Stop();
					this.LogLine("Not listening for Rocksmith.");
				}
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
			}
		}

		private void chkAuto_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				Properties.Settings.Default.IsAutoHacking = this.chkAuto.Checked;
				Properties.Settings.Default.Save();
			}
			catch (Exception ex)
			{
				this.LogLine(ex.Message, Color.Red);
			}
		}

		private void RocksmithStartWatcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			try
			{
				string handle = null;
				using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(RocksmithRunningQuery))
				{
					foreach (ManagementBaseObject o in searcher.Get())
						handle = o["Handle"].ToString();
				}

				this.BeginInvoke(new Action(() =>
				{
					this.RocksmithStartWatcher.Stop();

					if (!int.TryParse(handle, out int pid))
						pid = 0;

					this.RocksmithPID = pid;
					this.IsRocksmithRunning = (this.RocksmithPID != 0);
					if ((this.chkWatchRocksmith.Checked) && (this.IsRocksmithRunning))
						this.RocksmithExitWatcher.Start();

					if ((this.chkWatchRocksmith.Checked) && (!this.IsRocksmithRunning))
						this.RocksmithStartWatcher.Start();
				}));
			}
			catch(Exception ex)
			{
				this.LogLineTHREADSAFE(ex.Message, Color.Red);
			}
		}

		private void RocksmithExitWatcher_EventArrived(object sender, EventArrivedEventArgs e)
		{
			try
			{
				this.BeginInvoke(new Action(() =>
				{
					this.RocksmithExitWatcher.Stop();
					this.IsRocksmithRunning = false;
					if (this.chkWatchRocksmith.Checked)
						this.RocksmithStartWatcher.Start();
				}));
			}
			catch(Exception ex)
			{
				this.LogLineTHREADSAFE(ex.Message, Color.Red);
			}
		}

		private void HackTHREAD(int processID, AudioDevice device, int waitTimeMS = 5000)
		{
			try
			{
				this.BeginInvoke(new Action(() => this.btnHack.Enabled = false));

				// Get the rocksmith process
				if (this.RocksmithPID == 0)
				{
					this.IsRocksmithRunning = false;
					throw new Exception($"Rocksmith not found.");
				}

				Process rocksmith = Process.GetProcessById(processID);
				if (rocksmith?.MainModule == null)
				{
					this.RocksmithPID = 0;
					this.IsRocksmithRunning = false;
					throw new Exception("Unable to locate Rocksmith.");
				}

				this.LogLineTHREADSAFE("Hacking Rocksmith 2014 ...", Color.Green);

				// wait if we need to
				if (waitTimeMS > 0)
					Thread.Sleep(waitTimeMS);

				if ((this.PIDLocation != 0) && (this.VIDLocation != 0))
				{
					if (!WriteProcessMemory(rocksmith.Handle, new IntPtr(this.VIDLocation), device.VIDBytes, 2, out _))
						throw new Exception("Unable to write process memory.");

					if (!WriteProcessMemory(rocksmith.Handle, new IntPtr(this.PIDLocation), device.PIDBytes, 2, out _))
						throw new Exception("Unable to write process memory.");

					this.LogLineTHREADSAFE("Rocksmith 2014 hacked.", Color.Green);
					return;
				}

				// get the first address that we want to search. my version of rocksmith 2014 i find the value around 17000.
				// offset the first search address to speed things up
				IntPtr current = new IntPtr(rocksmith.MainModule.BaseAddress.ToInt64() + 10000);
				long end = rocksmith.MainModule.ModuleMemorySize + rocksmith.MainModule.BaseAddress.ToInt64();

				//Search for the value F806BA12
				byte[] desiredAddress = new byte[] {0xF8, 0x06, 0xBA, 0x12};
				byte[] buffer = new byte[4];
				while (current.ToInt64() < end)
				{
					uint numberOfBytesRead;

					//read some memory
					if (!ReadProcessMemory(rocksmith.Handle, current, buffer, (uint)buffer.Length, out numberOfBytesRead))
						throw new Exception("Unable to read process memory.");

					//couldnt read enough data
					if (numberOfBytesRead != (uint)buffer.Length)
						throw new Exception("Unable to locate memory address to hack.");

					//check to see if we were lucky enough to get the address
					if (BitConverter.ToInt32(buffer, 0) == BitConverter.ToInt32(desiredAddress, 0))
						break;

					//check to see if the array contains the first element in the desired result
					bool found = false;
					for (int i = 1; i < buffer.Length; i++)
					{
						if (buffer[i] != desiredAddress[0])
							continue;

						//found it, grab the 4 bytes
						current = new IntPtr(current.ToInt64() + i);
						if (!ReadProcessMemory(rocksmith.Handle, current, buffer, (uint)buffer.Length, out numberOfBytesRead))
							throw new Exception("Unable to read process memory.");

						//check to see if its what we want
						if (BitConverter.ToInt32(buffer, 0) == BitConverter.ToInt32(desiredAddress, 0))
							found = true;

						break;
					}

					if (found)
						break;

					//increment the pointer
					current = new IntPtr(current.ToInt64() + (uint)buffer.Length);
				}

				// write the memory
				// Susbtitute the BA 12 with YOUR cable's vendor ID
				// BA 12 is offset by 2 in F806BA12
				current = new IntPtr(current.ToInt64() + 2);
				this.VIDLocation = current.ToInt64();
				uint numberOfByteWritten;
				if (!WriteProcessMemory(rocksmith.Handle, current, device.VIDBytes, 2, out numberOfByteWritten))
					throw new Exception("Unable to write process memory.");

				//Locate FF 00 in the line below
				desiredAddress = new byte[] {0xFF, 0x00};
				buffer = new byte[2];
				while (current.ToInt64() < end)
				{
					uint numberOfBytesRead;

					//read some memory
					if (!ReadProcessMemory(rocksmith.Handle, current, buffer, (uint)desiredAddress.Length, out numberOfBytesRead))
						throw new Exception("Unable to read process memory.");

					//couldnt read enough data
					if (numberOfBytesRead != desiredAddress.Length)
						throw new Exception("Unable to locate memory address to hack.");

					//check to see if we were lucky enough to get the address
					if (BitConverter.ToInt16(desiredAddress, 0) == BitConverter.ToInt16(buffer, 0))
						break;

					//check to see if the array contains the first element in the desired result
					bool found = false;
					for (int i = 1; i < buffer.Length; i++)
					{
						if (buffer[i] != desiredAddress[0])
							continue;

						//found it, grab the 4 bytes
						current = new IntPtr(current.ToInt64() + i);
						if (!ReadProcessMemory(rocksmith.Handle, current, buffer, (uint)buffer.Length, out numberOfBytesRead))
							throw new Exception("Unable to read process memory.");

						//check to see if its what we want
						if (BitConverter.ToInt16(buffer, 0) == BitConverter.ToInt16(desiredAddress, 0))
							found = true;

						break;
					}

					if (found)
						break;

					//increment the pointer
					current = new IntPtr(current.ToInt64() + desiredAddress.Length);
				}

				if (current.ToInt64() >= end)
					return;

				//substitute 0xff00 with YOUR cable's product ID
				if (!WriteProcessMemory(rocksmith.Handle, current, device.PIDBytes, (uint)desiredAddress.Length, out numberOfByteWritten))
					throw new Exception("Unable to write process memory.");

				this.PIDLocation = current.ToInt64();
				this.LogLineTHREADSAFE("Rocksmith 2014 hacked.", Color.Green);
			}
			catch (Exception ex)
			{
				this.LogLineTHREADSAFE(ex.Message, Color.Red);
			}
			finally
			{
				this.BeginInvoke(new Action(() => this.btnHack.Enabled = true));
			}
		}
	}
}
