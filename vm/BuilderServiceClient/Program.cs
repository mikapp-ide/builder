using System;

namespace BuilderServiceClient {
	class Program {
		static void Main(string[] args) {

			var proxy = new Services.BuildServiceClient();
			var result = proxy.Build("b7bbeef0-a42f-cbe5-eb2f-5d7560fec4ed");
			Console.WriteLine(string.Format("Server Returned: {0}", result));
			Console.ReadLine();
		}
	}
}
