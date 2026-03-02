using SupplyCoreERP.Inventories.Batches;
using SupplyCoreERP.Inventories.Warehouses;
using SupplyCoreERP.Products;
using SupplyCoreERP.Warehouses;
using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventories.Tickets
{
	public class InventoryTicketDetail : FullAuditedEntity<Guid>
	{
		public Guid TicketId { get; private set; }
		public virtual InventoryTicket Ticket { get; protected set; }
		public Guid ProductId { get; private set; }
		public virtual Product Product { get; protected set; }
		public Guid ProductBatchId { get; private set; }
		public virtual ProductBatch ProductBatch { get; protected set; }
		public Guid BinId { get; private set; }
		public virtual Bin Bin { get; protected set; }
		public decimal Quantity { get; private set; }

		protected InventoryTicketDetail() { }

		public InventoryTicketDetail(Guid id, Guid ticketId, Guid productId, Guid batchId, Guid binId, decimal qty) : base(id)
		{
			TicketId = ticketId; 
			ProductId = productId; 
			ProductBatchId = batchId; 
			BinId = binId;
			Quantity = qty;
		}

		public void UpdateActualQuantity(decimal qty) => Quantity = qty >= 0 ? qty : throw new ArgumentException("Số lượng không hợp lệ!");
	}
}
