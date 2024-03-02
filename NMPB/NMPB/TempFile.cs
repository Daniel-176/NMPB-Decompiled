using System;
using System.IO;
using System.Reflection;

namespace NMPB
{
	internal sealed class TempFile : IDisposable
	{
		private string _path;

		public string Path
		{
			get
			{
				if (this._path == null)
				{
					throw new ObjectDisposedException(this.GetType().Name);
				}
				return this._path;
			}
		}

		public TempFile() : this(System.IO.Path.GetTempFileName())
		{
		}

		public TempFile(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException("path");
			}
			this._path = path;
		}

		public void Dispose()
		{
			this.Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
			}
			if (this._path != null)
			{
				try
				{
					File.Delete(this._path);
				}
				catch
				{
				}
				this._path = null;
			}
		}

		~TempFile()
		{
			this.Dispose(false);
		}

		public static implicit operator String(TempFile x)
		{
			return x._path;
		}
	}
}