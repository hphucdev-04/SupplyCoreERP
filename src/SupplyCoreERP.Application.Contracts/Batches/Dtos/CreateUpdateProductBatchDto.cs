using System;
using System.ComponentModel.DataAnnotations;

namespace SupplyCoreERP.Batches.Dtos;

public class CreateUpdateProductBatchDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(100)]
    public string BatchNumber { get; set; }

    [Required]
    public DateTime ManufacturingDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    public Guid? SupplierId { get; set; }
}
