using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpClient
{
	class Program
	{
		static void Main(string[] args)
		{
			var c = new ServiceReference1.HelloWorldServiceClient();
			var result = c.SayHello();
			Console.WriteLine(result);
			Console.ReadLine();
		}
	}
}
