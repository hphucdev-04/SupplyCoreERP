using System;
using System.Collections.Generic;
using System.Text;

namespace SupplyCoreERP.Customers.Dtos;

public class CustomerSummaryDto
{
    public int TotalCount { get; set; }
    public int TotalActive { get; set; }
    public int TotalInactive { get; set; }
    public int TotalOrganization { get; set; }
    public int TotalIndividual { get; set; }
}

