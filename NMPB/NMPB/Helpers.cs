using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NMPB
{
	public static class Helpers
	{
		public static void SafeRelease(this Mutex mutex)
		{
			try
			{
				mutex.ReleaseMutex();
			}
			catch (Exception exception)
			{
			}
		}
	}
}