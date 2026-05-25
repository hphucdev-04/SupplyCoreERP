using System;
using System.Threading.Tasks;
using SupplyCoreERP.Inventory.Balances;
using SupplyCoreERP.Inventory.Tickets;
using SupplyCoreERP.Procurement.PurchaseOrders;
using SupplyCoreERP.Procurement.PurchaseRequisitions;
using SupplyCoreERP.Sales.Orders;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Catalog.Products;

public class ProductManager : DomainService
{
    // Dependencies
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<InventoryBalance, Guid> _balanceRepo;
    private readonly IRepository<InventoryTicketLine, Guid> _ticketLineRepo;
    private readonly IRepository<PurchaseOrderLine, Guid> _poLineRepo;
    private readonly IRepository<SalesOrderLine, Guid> _soLineRepo;
    private readonly IRepository<PurchaseRequisitionLine, Guid> _prLineRepo;

    // Constructor injection
    public ProductManager(
        IRepository<Product, Guid> productRepository,
        IRepository<InventoryBalance, Guid> balanceRepo,
        IRepository<InventoryTicketLine, Guid> ticketLineRepo,
        IRepository<PurchaseOrderLine, Guid> poLineRepo,
        IRepository<SalesOrderLine, Guid> soLineRepo,
        IRepository<PurchaseRequisitionLine, Guid> prLineRepo)
    {
        _productRepository = productRepository;
        _balanceRepo = balanceRepo;
        _ticketLineRepo = ticketLineRepo;
        _poLineRepo = poLineRepo;
        _soLineRepo = soLineRepo;
        _prLineRepo = prLineRepo;
    }

    public virtual async Task CheckCodeAsync(string code, Guid? excludeId = null)
    {
        Check.NotNullOrWhiteSpace(code, nameof(code));
        string normalizedCode = code.Trim().ToUpper();

        // Check Code
        if (await _productRepository.AnyAsync(x => x.Code == normalizedCode && x.Id != excludeId))
        {
            throw new BusinessException("SupplyCoreERP:DuplicateProductCode", $"Mã sản phẩm '{code}' đã tồn tại!");
        }
    }

    public virtual async Task<bool> HasTransactionsAsync(Guid productId)
    {
        // 1. Kiểm tra tồn kho
        if (await _balanceRepo.AnyAsync(x => x.ProductId == productId))
        {
            return true;
        }

        // 2. Kiểm tra dòng phiếu kho
        if (await _ticketLineRepo.AnyAsync(x => x.ProductId == productId))
        {
            return true;
        }

        // 3. Kiểm tra dòng đơn mua hàng
        if (await _poLineRepo.AnyAsync(x => x.ProductId == productId))
        {
            return true;
        }

        // 4. Kiểm tra dòng đơn bán hàng
        if (await _soLineRepo.AnyAsync(x => x.ProductId == productId))
        {
            return true;
        }

        // 5. Kiểm tra dòng yêu cầu mua hàng
        if (await _prLineRepo.AnyAsync(x => x.ProductId == productId))
        {
            return true;
        }

        return false;
    }

    public virtual async Task ValidateBaseUnitChangeAsync(Product product, Guid newBaseUnitId)
    {
        Check.NotNull(product, nameof(product));

        // Check nếu đơn vị gốc không thay đổi thì không cần kiểm tra gì thêm
        if (product.BaseUnitId == newBaseUnitId)
        {
            return;
        }

        if (await _balanceRepo.AnyAsync(x => x.ProductId == product.Id))
        {
            throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh số dư tồn kho.");
        }

        if (await _ticketLineRepo.AnyAsync(x => x.ProductId == product.Id))
        {
            throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh phiếu kho.");
        }

        if (await _poLineRepo.AnyAsync(x => x.ProductId == product.Id))
        {
            throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh dòng đơn mua hàng.");
        }

        if (await _soLineRepo.AnyAsync(x => x.ProductId == product.Id))
        {
            throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh dòng đơn bán hàng.");
        }

        if (await _prLineRepo.AnyAsync(x => x.ProductId == product.Id))
        {
            throw new BusinessException("SupplyCoreERP:CannotChangeBaseUnitWithTransactions", "Không thể thay đổi đơn vị gốc vì sản phẩm đã phát sinh dòng yêu cầu mua hàng.");
        }
    }

    public virtual async Task ValidateUnitChangeAsync(Product product)
    {
        Check.NotNull(product, nameof(product));

        if (await HasTransactionsAsync(product.Id))
        {
            throw new BusinessException(
                "SupplyCoreERP:CannotChangeUnitWithTransactions",
                "Không thể thêm, sửa hệ số hoặc xóa đơn vị tính vì sản phẩm đã phát sinh giao dịch lịch sử."
            );
        }
    }
}







