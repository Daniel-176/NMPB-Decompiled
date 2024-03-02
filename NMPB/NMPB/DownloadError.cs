using System;

namespace NMPB
{
	public enum DownloadError
	{
		WrongURL,
		UnableToGetFileSize,
		RequestTimeout,
		DownloadTimeout,
		UnableToParseMidi,
		FileTooBig,
		UnableToDownload
	}
}