using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Uow;

namespace SupplyCoreERP.DocumentSequences;

public class DocumentSequenceManager : DomainService
{
    private readonly IRepository<DocumentSequence, Guid> _sequenceRepo;

    public DocumentSequenceManager(IRepository<DocumentSequence, Guid> sequenceRepo)
    {
        _sequenceRepo = sequenceRepo;
    }

    [UnitOfWork]
    public async Task<string> GenerateAsync(string prefix)
    {
        string todayStr = DateTime.Now.ToString("yyMMdd");

        // Tìm bản ghi sequence cho loại chứng từ này
        DocumentSequence? sequence = await _sequenceRepo.FirstOrDefaultAsync(x => x.DocumentType == prefix);

        if (sequence == null)
        {
            sequence = new DocumentSequence(
                GuidGenerator.Create(),
                prefix.ToUpper(),
                todayStr
            );
            await _sequenceRepo.InsertAsync(sequence, autoSave: true);
        }
        else
        {
            sequence.Increment(todayStr);

            // ABP sẽ tự check ConcurrencyStamp 
            // Nếu 2 người cùng nhấn Lưu, người thứ 2 sẽ báo lỗi thay vì trùng mã.
            await _sequenceRepo.UpdateAsync(sequence, autoSave: true);
        }

        // PadLeft(4, '0') để đảm bảo số luôn có 4 chữ số (0001, 0010, 0100...)
        return $"{prefix.ToUpper()}{sequence.PrefixDate}{sequence.LastValue.ToString().PadLeft(4, '0')}";
    }
}
