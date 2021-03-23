using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StandaloneRocksmith2014CableHack
{
	[Serializable]
	public class AudioDevice
	{
		public string ID { get; set; }
		public string Name { get; set; }
		public string PID { get; set; }
		public string VID { get; set; }
		public byte[] PIDBytes { get; private set; }
		public byte[] VIDBytes { get; private set; }

		public AudioDevice(string name, string id) 
		{
			this.Name = name;
			this.ID = id;
			this.PID = Regex.Match(id, "(?<=PID_)([\\dA-F]{4})")?.ToString() ?? "";
			if(string.IsNullOrEmpty(this.PID))
				this.PID = Regex.Match(id, "(?<=DEV_)([\\dA-F]{4})")?.ToString() ?? "";

			this.VID = Regex.Match(id, "(?<=VID_)([\\dA-F]{4})")?.ToString() ?? "";
			if (string.IsNullOrEmpty(this.VID))
				this.VID = Regex.Match(id, "(?<=VEN_)([\\dA-F]{4})")?.ToString() ?? "";

			if(!string.IsNullOrEmpty(this.PID))
				this.PIDBytes = new[] { byte.Parse(this.PID.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture), byte.Parse(this.PID.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) };
			if(!string.IsNullOrEmpty(this.VID))
				this.VIDBytes = new[] { byte.Parse(this.VID.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture), byte.Parse(this.VID.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) };

			Array.Reverse(this.PIDBytes);
			Array.Reverse(this.VIDBytes);
		}

		public override string ToString()
		{
			return $"{this.Name, -30} VID: {this.VID} PID: {this.PID}";
		}
	}
}
