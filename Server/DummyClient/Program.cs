using DummyClient.Session;
using ServerCore;
using System;
using System.Net;
using System.Threading;

namespace DummyClient
{
	class Program
	{
		static int DummyClientCount { get; } = 30;

		static void Main(string[] args)
		{
			Thread.Sleep(3000);

			// DNS (Domain Name System)
			//string host = Dns.GetHostName();
			//IPHostEntry ipHost = Dns.GetHostEntry(host);
			//IPAddress ipAddr = ipHost.AddressList[1];
			//IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = IPAddress.Parse("218.153.112.108");
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 5001);


            Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); },
				Program.DummyClientCount);

			while (true)
			{
				Thread.Sleep(10000);
			}
		}
	}
}
