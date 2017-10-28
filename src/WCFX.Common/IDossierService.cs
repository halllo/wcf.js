using System.Collections.Generic;
using System.ServiceModel;
using WCFX.Common.Dtos;

namespace WCFX.Common
{
	[ServiceContract]
	public interface IDossierService : IWcfService
	{
		[OperationContract] List<DossierDto> GetAll(bool runWithFullAccessRights);
	}
}