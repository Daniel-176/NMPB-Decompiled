using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

namespace NMPB
{
	public class WebDownload : WebClient
	{
		public int Timeout
		{
			get;
			set;
		}

		public WebDownload() : this(60000)
		{
		}

		public WebDownload(int timeout)
		{
			base.Encoding = System.Text.Encoding.UTF8;
			this.Timeout = timeout;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest webRequest = base.GetWebRequest(address);
			if (webRequest != null)
			{
				webRequest.Timeout = this.Timeout;
			}
			return webRequest;
		}
	}
}