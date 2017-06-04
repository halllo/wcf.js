using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
	public class MessageInspector : IDispatchMessageInspector
	{
		public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
		{
			return null;
		}

		public void BeforeSendReply(ref Message reply, object correlationState)
		{
		}
	}

	public class ParameterInspector : IParameterInspector
	{
		public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
		{
		}

		public object BeforeCall(string operationName, object[] inputs)
		{
			return null;
		}
	}

	public class MyBehavior : BehaviorExtensionElement, IEndpointBehavior
	{
		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
		{

		}

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		{

		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
		{
			endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MessageInspector());

			foreach (DispatchOperation dispatchOperation in endpointDispatcher.DispatchRuntime.Operations)
			{
				dispatchOperation.ParameterInspectors.Add(new ParameterInspector());
			}
		}

		public void Validate(ServiceEndpoint endpoint)
		{

		}

		public override Type BehaviorType
		{
			get { return typeof(MyBehavior); }
		}

		protected override object CreateBehavior()
		{
			return new MyBehavior();
		}
	}

}
