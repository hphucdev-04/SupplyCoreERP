using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Products;
using SupplyCoreERP.Suppliers;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Batches
{
	public class ProductBatch : FullAuditedAggregateRoot<Guid>
	{
		public Guid ProductId { get; private set; }
		public virtual Product Product { get; protected set; }
		public string BatchNumber { get; private set; }
		public DateTime ManufacturingDate { get; private set; }
		public DateTime ExpiryDate { get; private set; }
		public Guid? SupplierId { get; private set; }
		public virtual Supplier Supplier { get; protected set; }
		public BatchQAStatus Status { get; private set; }

		protected ProductBatch() { }

		public ProductBatch(Guid id, Guid productId, string batchNumber, DateTime mfg, DateTime exp, Guid? supplierId) : base(id)
		{
			if (exp <= mfg) throw new ArgumentException("Hạn sử dụng phải lớn hơn Ngày sản xuất!");
			ProductId = productId; 
			BatchNumber = batchNumber.ToUpper();
			ManufacturingDate = mfg; 
			ExpiryDate = exp; 
			SupplierId = supplierId;
			Status = BatchQAStatus.PendingQA;
		}

		public void UpdateInfo(DateTime mfg, DateTime exp, Guid? supplierId)
		{
			if (exp <= mfg) throw new ArgumentException("Hạn sử dụng phải lớn hơn Ngày sản xuất!");
			ManufacturingDate = mfg; 
			ExpiryDate = exp; 
			SupplierId = supplierId;
		}

		public void ApproveQA() => Status = BatchQAStatus.Approved;
		public void RejectQA() => Status = BatchQAStatus.Rejected;
		public void Recall() => Status = BatchQAStatus.Recalled;
		public void MarkExpired() => Status = BatchQAStatus.Expired;
	}
}