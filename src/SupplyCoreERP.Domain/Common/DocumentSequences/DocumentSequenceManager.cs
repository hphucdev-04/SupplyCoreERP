using System;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace SupplyCoreERP.Common.DocumentSequences;

public class DocumentSequenceManager : DomainService, IDocumentSequenceManager
{
    // Dependencies
    private readonly IRepository<DocumentSequence, Guid> _sequenceRepo;

    // Constructor injection
    public DocumentSequenceManager(IRepository<DocumentSequence, Guid> sequenceRepo)
    {
        _sequenceRepo = sequenceRepo;
    }

    public async Task<string> GenerateAsync(string prefix)
    {
        string todayStr = DateTime.Now.ToString("yyMMdd");

        // Tìm kiếm sequence hiện tại cho loại tài liệu (prefix) và ngày hôm nay
        DocumentSequence? sequence = await _sequenceRepo.FirstOrDefaultAsync(x => x.DocumentType == prefix);

        if (sequence == null)
        {
            sequence = new DocumentSequence(
                GuidGenerator.Create(),
                prefix.ToUpper(),
                todayStr
            );

            // Lần đầu tiên tạo sequence cho prefix này, nên khởi tạo LastValue = 1
            await _sequenceRepo.InsertAsync(sequence, autoSave: true);
        }
        else
        {
            sequence.Increment(todayStr);

            // Sử dụng concurrency stamp để đảm bảo tính nhất quán khi có nhiều request cùng cập nhật sequence
            await _sequenceRepo.UpdateAsync(sequence, autoSave: true);
        }

        // PadLeft(4, '0') để đảm bảo số thứ tự luôn có 4 chữ số, ví dụ: 0001, 0002, ..., 9999
        return $"{prefix.ToUpper()}{sequence.PrefixDate}{sequence.LastValue.ToString().PadLeft(4, '0')}";
    }
}






