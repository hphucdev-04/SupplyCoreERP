using SupplyCoreERP.Medicines;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

namespace SupplyCoreERP.DosageForms
{
	public class DosageFormManager : DomainService
	{
		private readonly IRepository<DosageForm, Guid> _repository;
		private readonly IRepository<Medicine, Guid> _medicineRepository;

		public DosageFormManager(
			IRepository<DosageForm, Guid> repository,
			IRepository<Medicine, Guid> medicineRepository)
		{
			_repository = repository;
			_medicineRepository = medicineRepository;
		}

		public async Task<DosageForm> CreateAsync(string code, string name)
		{
			Check.NotNullOrWhiteSpace(code, nameof(code));
			Check.NotNullOrWhiteSpace(name, nameof(name));

			var normalizedCode = code.Trim().ToUpper();
			var normalizedName = name.Trim();

			//Check trùng mã
			if (await _repository.AnyAsync(x => x.Code == normalizedCode))
			{
				throw new UserFriendlyException($"Mã dạng bào chế '{code}' đã tồn tại!");
			}

			//Check trùng tên
			if (await _repository.AnyAsync(x => x.Name == normalizedName))
			{
				throw new UserFriendlyException($"Tên dạng bào chế '{name}' đã tồn tại!");
			}

			return new DosageForm(GuidGenerator.Create(), normalizedCode, normalizedName);
		}

		public async Task UpdateAsync(DosageForm entity, string newCode, string newName)
		{
			Check.NotNull(entity, nameof(entity));
			Check.NotNullOrWhiteSpace(newCode, nameof(newCode));
			Check.NotNullOrWhiteSpace(newName, nameof(newName));

			var normalizedCode = newCode.Trim().ToUpper();
			var normalizedName = newName.Trim();

			//Check trùng mã
			if (await _repository.AnyAsync(x => x.Code == normalizedCode && x.Id != entity.Id))
			{
				throw new UserFriendlyException($"Mã dạng bào chế '{newCode}' đã được sử dụng!");
			}

			//Check trùng tên
			if (await _repository.AnyAsync(x => x.Name == normalizedName && x.Id != entity.Id))
			{
				throw new UserFriendlyException($"Tên dạng bào chế '{newName}' đã được sử dụng!");
			}

			entity.Update(normalizedCode, normalizedName);
		}

		public async Task DeleteAsync(DosageForm entity)
		{
			Check.NotNull(entity, nameof(entity));

			//Không xóa nếu đang có thuốc dùng dạng này
			var isInUse = await _medicineRepository.AnyAsync(x => x.DosageFormId == entity.Id);

			if (isInUse)
			{
				throw new UserFriendlyException($"Không thể xóa '{entity.Name}' vì đang có thuốc sử dụng dạng bào chế này!");
			}

			await _repository.DeleteAsync(entity);
		}
	}
}
