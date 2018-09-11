using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFX.Server.wcf
{
	public class SoapLoggerMessageInspector : IDispatchMessageInspector
	{
		public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
		{
			var buffer = request.CreateBufferedCopy(Int32.MaxValue);
			var msgCopy = buffer.CreateMessage();
			request = buffer.CreateMessage();
			var strMessage = buffer.CreateMessage().ToString();
			var xrdr = msgCopy.GetReaderAtBodyContents();
			var bodyData = xrdr.ReadOuterXml();
			var soap = strMessage.Replace("... stream ...", bodyData);

			Logger.Log("Received:\n" + soap, color: ConsoleColor.DarkGray);

			return null;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
			var buffer = reply.CreateBufferedCopy(Int32.MaxValue);
			reply = buffer.CreateMessage();
			var soap = buffer.CreateMessage().ToString();

			Logger.Log("Sending:\n" + soap + "\n", color: ConsoleColor.DarkGray);
		}
	}







	public class SoapLoggerBehavior : IEndpointBehavior
	{
		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new SoapLoggerMessageInspector());
		}

		public void Validate(ServiceEndpoint endpoint)
		{
		}
	}
}