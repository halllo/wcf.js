using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using WCFX.Common;
using WCFX.Common.Dtos;
using WCFX.Server.wcf;

namespace WCFX.Server
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, MaxItemsInObjectGraph = int.MaxValue)]
	public class DossierService : IDossierService
	{
		[ExceptionHandledOperation]
		public List<DossierDto> GetAll(bool runWithFullAccessRights)
		{
			return new[]
			{
				new DossierDto {DossierName = "DN1", ReferenceNumber = "RN1", DossierState = DossierState.InBearbeitung },
				new DossierDto {DossierName = "DN2", ReferenceNumber = "RN2", DossierState = DossierState.InBearbeitung },
			}.Concat(runWithFullAccessRights == false ? new DossierDto[] { } :
				new[]
				{
					new DossierDto {DossierName = "DN3", ReferenceNumber = "RN3", DossierState = DossierState.InBearbeitung },
					new DossierDto {DossierName = "DN4", ReferenceNumber = "RN4", DossierState = DossierState.InBearbeitung },
				}
			).ToList();
		}
	}
}