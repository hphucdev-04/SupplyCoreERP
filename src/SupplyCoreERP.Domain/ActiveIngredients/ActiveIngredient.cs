using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.ActiveIngredients
{
	public class ActiveIngredient : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; } 
		public string Name { get; private set; } 
		private ActiveIngredient() { }

		public ActiveIngredient(Guid id, string code, string name) : base(id)
		{
			SetCode(code);
			SetName(name);
		}

		public void Update(string name)
		{
			SetName(name);
		}

		private void SetCode(string code)
		{ 
			Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50).Trim().ToUpper(); 
		}
		private void SetName(string name) 
		{ 
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 255).Trim(); 
		}
	}
}
