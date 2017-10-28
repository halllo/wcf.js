using System;
using System.Runtime.Serialization;

namespace WCFX.Common.Dtos
{
	[Serializable]
	[DataContract]
	public class DossierDto : ModelWithChangeInfos
	{
		[DataMember] public string ReferenceNumber { get; set; }
		[DataMember] public string DossierName { get; set; }
		[DataMember] public DossierState DossierState { get; set; }
		[DataMember] public DateTime? FilingDate { get; set; }
		[DataMember] public int Year { get; set; }
	}


	[DataContract]
	public enum DossierState
	{
		[EnumMember] Potenziell = 1,
		[EnumMember] InBearbeitung,
		[EnumMember] Abgeschlossen,
		[EnumMember] Abgelehnt
	}
}