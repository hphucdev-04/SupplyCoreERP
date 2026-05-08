
using System;
using Volo.Abp.Domain.Entities;

namespace SupplyCoreERP.DocumentSequences;

public class DocumentSequence : AggregateRoot<Guid>
{
    public string DocumentType { get; set; }
    public string PrefixDate { get; set; }
    public int LastValue { get; set; }

    protected DocumentSequence() { }

    public DocumentSequence(Guid id, string documentType, string prefixDate) : base(id)
    {
        DocumentType = documentType;
        PrefixDate = prefixDate;
        LastValue = 1;
    }

    public void Increment(string today)
    {
        if (PrefixDate != today)
        {
            PrefixDate = today;
            LastValue = 1;
        }
        else
        {
            LastValue += 1;
        }
    }

}
