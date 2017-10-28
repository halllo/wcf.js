using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WCFX.Common.Dtos
{
	public interface IModel : ICloneable
	{
		int Id { get; set; }
		Guid XId { get; set; }
		int Timestamp { get; set; }
		bool IsCreated { get; }
		bool IsUnchanged { get; }
		bool IsDeleted { get; }
		bool IsUpdated { get; }

		void SetUnchanged();
		void SetCreated();
		void SetUpdated();
		bool HasId();
		void SetDeleted();

		void CopyPersistenceInfo(IModel newState);
	}











	public interface IModelWithChangeInfos : IModel
	{
		DateTime CreationDate { get; set; }
		string CreationUser { get; set; }
		DateTime? UpdateDate { get; set; }
		string UpdateUser { get; set; }
		void CopyChangeInfoValuesFrom(IModelWithChangeInfos source);
	}















	[Serializable]
	[DataContract]
	public abstract class ModelBase : IModel
	{
		protected ModelBase()
		{
			XId = Guid.NewGuid();
		}

		[DataMember] public int Id { get; set; }
		[DataMember] public Guid XId { get; set; }
		[DataMember] public virtual int Timestamp { get; set; }

		public bool IsCreated => PersistenceState == PersistenceInfo.Created;
		public bool IsUnchanged => PersistenceState == PersistenceInfo.Unchanged;
		public bool IsDeleted => PersistenceState == PersistenceInfo.Deleted;
		public bool IsUpdated => PersistenceState == PersistenceInfo.Updated;

		public void CopyPersistenceInfo(IModel model)
		{
			if (model.IsUnchanged)
			{
				SetUnchanged();
			}
			else if (model.IsCreated)
			{
				SetCreated();
			}
			else if (model.IsUpdated)
			{
				SetUpdated();
			}
			else if (model.IsDeleted)
			{
				SetDeleted();
			}
		}

		public void SetUnchanged() => PersistenceState = PersistenceInfo.Unchanged;

		public virtual void SetCreated() => PersistenceState = PersistenceInfo.Created;

		public virtual void SetUpdated()
		{
			if (!IsCreated) PersistenceState = PersistenceInfo.Updated;
		}

		public void SetDeleted() => PersistenceState = PersistenceInfo.Deleted;

		public bool HasId() => Id > 0;

		public virtual object Clone()
		{
			using (var memoryStream = new MemoryStream())
			{
				var formatter = new BinaryFormatter();
				formatter.Serialize(memoryStream, this);
				memoryStream.Position = 0;
				var copy = formatter.Deserialize(memoryStream);
				return copy;
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (GetType() != obj.GetType())
			{
				return false;
			}

			var model = (ModelBase)obj;
			return XId == model.XId;
		}

		public override int GetHashCode() => XId.GetHashCode();

		[DataMember] private PersistenceInfo PersistenceState { get; set; }
	}



















	[Serializable]
	[DataContract]
	public class ModelWithChangeInfos : ModelBase, IModelWithChangeInfos
	{
		public ModelWithChangeInfos()
		{
			CreationDate = DateTime.Now;
		}

		[DataMember] public DateTime CreationDate { get; set; }
		[DataMember] public string CreationUser { get; set; }
		[DataMember] public DateTime? UpdateDate { get; set; }

		public string UpdateUser
		{
			get { return mUpdateUser; }
			set { mUpdateUser = value; }
		}

		public void CopyChangeInfoValuesFrom(IModelWithChangeInfos source)
		{
			XId = source.XId;
			Id = source.Id;
			Timestamp = source.Timestamp;
			CreationDate = source.CreationDate;
			CreationUser = source.CreationUser;
			UpdateDate = source.UpdateDate;
			UpdateUser = source.UpdateUser;

			CopyPersistenceInfo(source);
		}

		[DataMember] private string mUpdateUser;
	}












	[DataContract]
	internal enum PersistenceInfo
	{
		[EnumMember] Unchanged,
		[EnumMember] Created,
		[EnumMember] Updated,
		[EnumMember] Deleted
	}
}
