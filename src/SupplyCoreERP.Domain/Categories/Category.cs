using SupplyCoreERP.Products;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Categories
{
	public class Category : FullAuditedAggregateRoot<Guid>
	{
		public string Name { get; private set; }
		public virtual ICollection<Product> Products { get; set; }

		private Category() { }

		public Category(Guid id, string name)
			: base(id)
		{
			SetName(name);
		}

		public void SetName(string name)
		{
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), maxLength: 100).Trim();
		}

	}
}