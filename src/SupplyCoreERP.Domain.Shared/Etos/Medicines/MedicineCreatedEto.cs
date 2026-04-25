using System;
using Volo.Abp.EventBus;

namespace SupplyCoreERP.Medicines
{
    [EventName("SupplyCoreERP.Medicine.Approved")]
    public class MedicineApprovedEto
    {
        public Guid MedicineId { get; set; }
        public string MedicineCode { get; set; }
        public string MedicineName { get; set; }
        public string CreatorEmail { get; set; }
    }
}