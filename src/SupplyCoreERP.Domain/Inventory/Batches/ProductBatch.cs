using System;
using SupplyCoreERP.Catalog.Medicines;
using SupplyCoreERP.Catalog.Products;
using SupplyCoreERP.Enums.Warehouses;
using SupplyCoreERP.Partner.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SupplyCoreERP.Inventory.Batches;

public class ProductBatch : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }
    public Guid ProductId { get; private set; }
    public virtual Product Product { get; protected set; }
    public string BatchNumber { get; private set; }
    public DateTime ManufacturingDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public Guid? SupplierId { get; private set; }
    public virtual Supplier Supplier { get; protected set; }
    public BatchQAStatus Status { get; private set; }

    /// <summary>
    /// Liên kết tới Số đăng ký cụ thể tại thời điểm nhập lô hàng này (chỉ dành cho Thuốc).
    /// </summary>
    public Guid? MedicineRegistrationId { get; private set; }
    public virtual MedicineRegistration MedicineRegistration { get; protected set; }

    protected ProductBatch() { }

    public ProductBatch(
        Guid id,
        string code,
        Guid productId,
        string batchNumber,
        DateTime mfg,
        DateTime exp,
        Guid? supplierId,
        Guid? medicineRegistrationId = null) : base(id)
    {
        if (exp <= mfg)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBatchDates", "Hạn sử dụng phải lớn hơn Ngày sản xuất!");
        }

        Code = Check.NotNullOrWhiteSpace(code, nameof(Code), 50);
        ProductId = productId;
        BatchNumber = Check.NotNullOrWhiteSpace(batchNumber, nameof(BatchNumber), 50).ToUpper();
        ManufacturingDate = mfg;
        ExpiryDate = exp;
        SupplierId = supplierId;
        MedicineRegistrationId = medicineRegistrationId;
        Status = BatchQAStatus.PendingQA;
    }

    public void UpdateInfo(DateTime mfg, DateTime exp, Guid? supplierId, Guid? medicineRegistrationId = null)
    {
        if (exp <= mfg)
        {
            throw new BusinessException("SupplyCoreERP:InvalidBatchDates", "Hạn sử dụng phải lớn hơn Ngày sản xuất!");
        }

        ManufacturingDate = mfg;
        ExpiryDate = exp;
        SupplierId = supplierId;
        MedicineRegistrationId = medicineRegistrationId;
    }

    public void ApproveQA() => Status = BatchQAStatus.Approved;
    public void RejectQA() => Status = BatchQAStatus.Rejected;
    public void Recall() => Status = BatchQAStatus.Recalled;
    public void MarkExpired() => Status = BatchQAStatus.Expired;
}






