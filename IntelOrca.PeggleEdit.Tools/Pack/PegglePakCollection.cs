using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelOrca.PeggleEdit.Tools.Pack
{
	public class PegglePakCollection : PakCollection
	{
		public PegglePakCollection() : base() { }
		public PegglePakCollection(string path) : base(path) { }

		public PakRecord GetImageRecord(string path)
		{
			string[] extensions = new string[] { "tga", "jpg", "png", "gif", "j2k", "jp2" };
			foreach (string ext in extensions) {
				PakRecord record = GetRecord(Path.ChangeExtension(path, ext));
				if (record != null)
					return record;
			}

			return null;
		}
	}
}
