using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Common.DocumentSequences;

public interface IDocumentSequenceManager : IDomainService
{
    Task<string> GenerateAsync(string prefix);
}
