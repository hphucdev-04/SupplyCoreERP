const fs = require('fs');
const path = require('path');

const scanFilesPath = path.join(__dirname, 'ua-scan-files.json');
const importMapPath = path.join(__dirname, 'ua-import-map-output.json');
const outputPath = path.resolve(__dirname, '..', 'intermediate', 'scan-result.json');

try {
  const scanData = JSON.parse(fs.readFileSync(scanFilesPath, 'utf8'));
  const importMapData = JSON.parse(fs.readFileSync(importMapPath, 'utf8'));

  const result = {
    name: "SupplyCoreERP",
    description: "Đây là một giải pháp ERP phân lớp được xây dựng trên ABP Framework (.NET 10) và Angular theo các thực tiễn Thiết kế Hướng Tên miền (DDD). Lưu ý: dự án này có hơn 100 tệp nguồn; cân nhắc việc giới hạn phạm vi phân tích trong một thư mục con để có kết quả nhanh hơn.",
    languages: [
      "csharp",
      "cshtml",
      "css",
      "html",
      "javascript",
      "json",
      "markdown",
      "powershell",
      "typescript",
      "yaml"
    ],
    frameworks: [
      "ABP Framework",
      "Angular",
      "Docker",
      "GitHub Actions"
    ],
    files: scanData.files,
    totalFiles: scanData.totalFiles,
    filteredByIgnore: scanData.filteredByIgnore,
    estimatedComplexity: scanData.estimatedComplexity,
    importMap: importMapData.importMap
  };

  // Ensure output directory exists
  const outputDir = path.dirname(outputPath);
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
  }

  fs.writeFileSync(outputPath, JSON.stringify(result, null, 2), 'utf8');
  console.log('Successfully generated scan-result.json');
} catch (error) {
  console.error('Error generating final scan result:', error);
  process.exit(1);
}
