using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFX.Server.WCF
{
	public class ExceptionHandledOperationAttribute : Attribute, IOperationBehavior
	{
		public void Validate(OperationDescription operationDescription)
		{
		}

		public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{
			var invoker = dispatchOperation.Invoker;
			invoker = new WindowsIdentityOperationInvoker(invoker);
			invoker = new ThreadDataResettingOperationInvoker(invoker);
			invoker = new ExceptionHandledOperationInvoker(invoker);

			dispatchOperation.Invoker = invoker;
		}

		public void ApplyClientBehavior(OperationDescription operationDescription,
			ClientOperation clientOperation)
		{
			throw new NotSupportedException();
		}

		public void AddBindingParameters(OperationDescription operationDescription,
			BindingParameterCollection bindingParameters)
		{
		}
	}

















	public abstract class OperationInvoker : IOperationInvoker
	{
		protected OperationInvoker(IOperationInvoker operationInvoker)
		{
			mOperationInvoker = operationInvoker;
		}

		public bool IsSynchronous
		{
			get { return mOperationInvoker.IsSynchronous; }
		}

		public object[] AllocateInputs()
		{
			return mOperationInvoker.AllocateInputs();
		}

		public virtual object Invoke(object instance, object[] inputs, out object[] outputs)
		{
			return mOperationInvoker.Invoke(instance, inputs, out outputs);
		}

		public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback,
			object state)
		{
			return mOperationInvoker.InvokeBegin(instance, inputs, callback, state);
		}

		public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
		{
			return mOperationInvoker.InvokeEnd(instance, out outputs, result);
		}

		protected IOperationInvoker DecoratedOperationInvoker
		{
			get { return mOperationInvoker; }
		}

		private readonly IOperationInvoker mOperationInvoker;
	}


















	public class WindowsIdentityOperationInvoker : OperationInvoker
	{
		public WindowsIdentityOperationInvoker(IOperationInvoker operationInvoker)
			: base(operationInvoker)
		{
		}

		public override object Invoke(object instance, object[] inputs, out object[] outputs)
		{
			var username = GetWindowsUserName();
			Program.Log($"Request von {username}", ConsoleColor.Cyan);
			Program.CurrentUser = username;

			var result = DecoratedOperationInvoker.Invoke(instance, inputs, out outputs);
			return result;
		}

		private string GetWindowsUserName()
		{
			if (OperationContext.Current != null && OperationContext.Current.ServiceSecurityContext != null && OperationContext.Current.ServiceSecurityContext.WindowsIdentity != null)
			{
				return OperationContext.Current.ServiceSecurityContext.WindowsIdentity.Name;
			}

			if (OperationContext.Current != null && OperationContext.Current.ServiceSecurityContext == null)
			{
				var usernameHeaderIndex = OperationContext.Current.IncomingMessageHeaders.FindHeader("Username", "WCFX");

				if (usernameHeaderIndex > -1)
				{
					var username = OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(usernameHeaderIndex);
					return username;
				}
				else
				{
					throw new Exception("Es konnte kein Benutzername-Message-Header gefunden werden.");
				}
			}

			return null;
		}
	}


	public class ThreadDataResettingOperationInvoker : OperationInvoker
	{
		public ThreadDataResettingOperationInvoker(IOperationInvoker operationInvoker)
			: base(operationInvoker)
		{
		}

		public override object Invoke(object instance, object[] inputs, out object[] outputs)
		{
			Program.CurrentUser = null;

			var result = DecoratedOperationInvoker.Invoke(instance, inputs, out outputs);
			return result;
		}
	}


	public class ExceptionHandledOperationInvoker : OperationInvoker
	{
		public ExceptionHandledOperationInvoker(IOperationInvoker operationInvoker)
			: base(operationInvoker)
		{
		}

		public override object Invoke(object instance, object[] inputs, out object[] outputs)
		{
			try
			{
				var result = DecoratedOperationInvoker.Invoke(instance, inputs, out outputs);
				return result;
			}
			catch (Exception ex)
			{
				Program.Log(ex.Message, ConsoleColor.Red);
				throw;
			}
		}
	}
}
