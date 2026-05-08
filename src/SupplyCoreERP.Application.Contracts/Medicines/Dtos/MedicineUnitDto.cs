using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineUnitDto
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; }
    public int ConversionFactor { get; set; }
    public int Level { get; set; }
}
