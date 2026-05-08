using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Medicines.Dtos;

public class MedicineSummaryDto
{
    public int TotalCount { get; set; }
    public int TotalActive { get; set; }
    public int TotalInactive { get; set; }
    public int TotalApproved { get; set; }
    public int TotalPending { get; set; }
    public int TotalRejected { get; set; }
}

