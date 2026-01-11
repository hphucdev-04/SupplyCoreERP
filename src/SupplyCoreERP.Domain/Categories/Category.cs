using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Categories
{
	public class Category : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }

		private Category() { }

		public Category(Guid id, string code, string name, string description = null)
			: base(id)
		{
			SetCode(code);
			SetName(name);
			Description = description;
		}

		public void SetCode(string code)
		{
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), maxLength: 50).Trim().ToUpper();
		}

		public void SetName(string name)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), maxLength: 255).Trim();
		}

		public void SetDescription(string description)
		{
			Description = description;
		}
	}
}