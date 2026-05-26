# ĐẶC TẢ & HƯỚNG DẪN ĐỒNG BỘ C# TESTCASE SANG EXCEL QA SPEC
*Tài liệu hướng dẫn dành cho Lập trình viên trong đội dự án SupplyCoreERP*

---

## 1. Giới thiệu Tổng quan

Hệ thống đồng bộ kiểm thử tự động của **SupplyCoreERP** giúp tự động hóa 100% quy trình cập nhật tài liệu kiểm thử đặc tả (QA Excel Specification) trực tiếp từ mã nguồn C#. 

### Hệ thống giải quyết các bài toán:
* **Giải phóng Lập trình viên**: Không cần viết và cập nhật file Excel thủ công. Chỉ cần khai báo nghiệp vụ trực tiếp trong code khi viết test.
* **Tự động đóng băng ID**: Hệ thống tự động cấp phát ID kiểm thử (ví dụ: `TC-CAT-MEDMGR-UT-001`) và khóa vĩnh viễn qua file JSON, chống trùng lặp tuyệt đối.
* **Đồng bộ live kết quả**: Kết quả chạy kiểm thử thực tế (`Passed`/`Failed` từ tệp `.trx` của dotnet test) được tự động nạp live vào cột trạng thái trong Excel Spec.

---

## 2. Cấu trúc Custom Attribute `[QATest]`

Lập trình viên khi viết bất kỳ phương thức kiểm thử nào (`[Fact]` hoặc `[Theory]`) cần gắn Custom Attribute `[QATest]` ngay phía trên.

### A. Cú pháp khai báo chuẩn (Single-line)

```csharp
    [QATest(scenario: "Tạo mới Medicine thành công qua Domain Service.", feature: "Medicine", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_Create_Medicine_Successfully()
    {
        // Code test...
    }
```

### B. Giải thích các tham số (Named Parameters)
Attribute sử dụng 4 tham số đặt tên bắt buộc:
1. `scenario` (string): Mô tả kịch bản kiểm thử bằng **Tiếng Việt nghiệp vụ chuẩn chỉnh** (không chứa tiền tố rác như `[Tự động]`, không viết câu Tiếng Anh lửng lơ). Ví dụ: `"Ném ngoại lệ khi thêm trùng số đăng ký cho cùng một thuốc."`.
2. `feature` (string): Tên thành phần/chức năng được kiểm thử. Ví dụ: `"Medicine"`, `"Supplier"`, `"Customer"`, `"SupplierProduct"`.
3. `layer` (string): Tầng kiến trúc của ca kiểm thử. Ví dụ: `"Domain"`, `"Application"`, `"Infrastructure"`.
4. `priority` (string): Mức độ ưu tiên của ca kiểm thử. Gồm các giá trị: `"High"`, `"Medium"`, `"Low"`.

---

## 3. Cơ chế hoạt động của Giải pháp

Hệ thống đồng bộ vận hành thông qua sự kết hợp của 3 thành phần chính:

```
[Mã nguồn C# Test]                  [ dotnet test ]
 (Có gắn [QATest])                         │
         │                                 ▼
         │                         [Tệp kết quả .trx]
         ▼                                 │
 [Quét mã tĩnh] ─────────► [ generate_excel.py ] ◄──────── [ testcase_mapping.json ]
                                   │
                                   ▼
                   [ Partner_and_Catalog_Tests_Spec.xlsx ]
```

1. **Quét mã tĩnh (Static Analysis)**: Script Python quét đệ quy các tệp `.cs` trong 3 dự án test (`Domain.Tests`, `EntityFrameworkCore.Tests`, `Application.Tests`) để trích xuất nội dung attribute `[QATest]` và tên phương thức kiểm thử.
2. **Đóng băng ID (`testcase_mapping.json`)**:
   * Khi phát hiện ca test mới, script tự động sinh ID dựa trên nhóm tiền tố (ví dụ: `TC-PART-SUP-UT-001`) và ghi vào file `testcase_mapping.json`.
   * Lần chạy tiếp theo, ID của ca test cũ được đọc từ file JSON này để **giữ nguyên cố định**, đảm bảo ID tài liệu không bao giờ bị xê dịch khi bạn thêm/bớt các ca test khác.
3. **Tổng hợp kết quả live từ TRX**:
   * dotnet test xuất log chi tiết ra các tệp `.trx` tập trung trong thư mục `docs/testing/test_result/`.
   * Script Python phân tích tệp `.trx`, đối chiếu tên phương thức để tự động điền trạng thái `Passed`, `Failed` hoặc `Pending` kèm ngày chạy test thực tế vào Excel.

---

## 4. Hướng dẫn vận hành 3 bước hàng ngày (Developer Flow)

Mỗi khi bạn viết xong các ca kiểm thử mới hoặc trước khi bàn giao bàn giao sản phẩm, hãy thực hiện quy trình sau:

### Bước 1: Khai báo Attribute trên test case mới
Viết test case và gắn attribute mô tả nghiệp vụ (không cần tự điền ID):
```csharp
    [QATest(scenario: "Thanh toán nợ thành công cho khách hàng.", feature: "Customer", layer: "Domain", priority: "High")]
    [Fact]
    public async Task Should_PayDebt_Successfully()
    {
        // ...
    }
```

### Bước 2: Chạy kiểm thử xuất log tập trung
Chạy các bộ kiểm thử tương ứng để sinh tệp `.trx` trong thư mục tập trung (tránh nghẽn ghi đè):

```powershell
# 1. Chạy Domain Tests
dotnet test test/SupplyCoreERP.Domain.Tests/SupplyCoreERP.Domain.Tests.csproj --logger "trx;LogFileName=D:\ProjectOwner\SupplyCoreERP\docs\testing\test_result\test_results_domain.trx"

# 2. Chạy EF Core Integration Tests (chạy live 52 ca kiểm thử)
dotnet test test/SupplyCoreERP.EntityFrameworkCore.Tests/SupplyCoreERP.EntityFrameworkCore.Tests.csproj --logger "trx;LogFileName=D:\ProjectOwner\SupplyCoreERP\docs\testing\test_result\test_results_efcore.trx"
```

### 💡 Ý nghĩa thiết kế của Kiến trúc Test ABP (Vì sao chỉ cần 2 file TRX là đủ?)
Trong cấu trúc của tài liệu và thư mục kiểm thử, bạn sẽ thấy chỉ có duy nhất **2 file `.trx` thực tế** (`test_results_domain.trx` và `test_results_efcore.trx`) được sinh ra. Cơ chế này hoạt động dựa trên triết lý thiết kế kiểm thử cực kỳ thông minh của ABP Framework:

#### 1. Cơ chế Abstract (định nghĩa ở `Application.Tests`)
* **Nhiệm vụ**: Định nghĩa 100% **logic kiểm thử nghiệp vụ** (Business Test Logic) độc lập hoàn toàn với hạ tầng cơ sở dữ liệu. Nó chứa các bước `Arrange`, `Act`, `Assert` và kịch bản API. Các class ở đây được khai báo dạng `public abstract class`.
* **Ý nghĩa**: Giúp lập trình viên chỉ cần viết kịch bản nghiệp vụ **một lần duy nhất** (Write Once), không bị ràng buộc bởi loại Database cụ thể. Vì là abstract class nên Xunit runner không chạy trực tiếp và không sinh file TRX riêng cho tầng này.

#### 2. Cơ chế Implementation (định nghĩa ở `EntityFrameworkCore.Tests`)
* **Nhiệm vụ**: Kế thừa trực tiếp lớp abstract nghiệp vụ trên, đồng thời chỉ định Startup Module chạy thật (ví dụ truyền vào `SupplyCoreERPEntityFrameworkCoreTestModule` để chạy thực tế trên SQLite in-memory hoặc PostgreSQL).
* **Ý nghĩa**: Giúp dự án dễ dàng **chuyển đổi hoặc chạy song song kịch bản kiểm thử nghiệp vụ trên nhiều loại Database hạ tầng khác nhau mà không phải viết lại code test nghiệp vụ!**
  * *Ví dụ*: Hôm nay bạn dùng EF Core PostgreSQL (chỉ cần tạo lớp con ở `EntityFrameworkCore.Tests`). Ngày mai bạn muốn test trên MongoDB, bạn chỉ cần tạo lớp con kế thừa tương tự tại `MongoDB.Tests`. Toàn bộ 19 ca test API của Application layer sẽ tự động chạy trên môi trường Database mới 100% không tốn công viết lại!

#### 3. Kết luận về 2 file TRX
Khi bạn chạy lệnh `dotnet test` ở dự án EF Core, Xunit sẽ tự động phát hiện và kích hoạt chạy live toàn bộ các ca kiểm thử thuộc Application Layer này. Do đó, **2 tệp TRX thực tế** (`test_results_domain.trx` và `test_results_efcore.trx`) là **hoàn toàn đầy đủ và trọn vẹn 100%** để phản ánh chính xác kết quả của cả 3 tầng (Domain, EF Core, Application) với tổng cộng 171 ca kiểm thử!

### Bước 3: Đồng bộ và sinh tài liệu Excel QA Spec
Thực thi script Python ở thư mục gốc để cập nhật tài liệu Excel Spec:
```powershell
python docs/testing/generate_excel.py
```

Hệ thống sẽ cập nhật tự động `testcase_mapping.json`, khóa ID và kết xuất ra tài liệu đặc tả [Partner_and_Catalog_Tests_Spec.xlsx](file:///D:/ProjectOwner/SupplyCoreERP/docs/testing/Partner_and_Catalog_Tests_Spec.xlsx) bóng bẩy với phong cách Sleek Navy & Mint cao cấp, sẵn sàng để gửi cho QA hoặc khách hàng nghiệm thu!


