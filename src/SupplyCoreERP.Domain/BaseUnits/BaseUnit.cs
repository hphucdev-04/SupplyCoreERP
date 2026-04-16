using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.BaseUnits
{
	public class BaseUnit : FullAuditedAggregateRoot<Guid>
	{
		public string Code { get; private set; } // VD: HOP, VIEN, CAI
		public string Name { get; private set; } // VD: Hộp, Viên, Cái

		private BaseUnit() { }

		public BaseUnit(Guid id, string code, string name) : base(id)
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
			Name = Check.NotNullOrWhiteSpace(name, nameof(Name), 100).Trim();
		}
	}
}
