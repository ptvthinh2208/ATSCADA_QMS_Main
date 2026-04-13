# =========================================================================================
# SCRIPT ĐÓNG GÓI PHẦN MỀM QMS COUNTER (SINGLE-FILE)
# =========================================================================================

$ProjectName = "ATSCADA_WinForms"
$ProjectFile = "ATSCADA_WinForms.csproj"
$OutputFolder = "EXPORT_RELEASE"

# 1. Quay về thư mục gốc của project WinForms
$CurrentDir = Get-Location
Write-Host "--- Bat dau qua trinh dong goi: $ProjectName ---" -ForegroundColor Cyan

# 2. Xóa thư mục cũ nếu có
if (Test-Path $OutputFolder) {
    Remove-Item -Path $OutputFolder -Recurse -Force
}

# 3. Chạy lệnh Publish của .NET 8
# -c Release: Build bản thương mại
# -r win-x64: Đóng gói cho Windows 64-bit
# --self-contained true: Tích hợp sẵn .NET Runtime (Khách không cần cài .NET)
# -p:PublishSingleFile=true: Gom tất cả thành 1 file .exe
# -p:IncludeNativeLibrariesForSelfExtract=true: Nhúng các thư viện gốc
Write-Host "--- Dang build va nen file (Vui long cho trong giay lat)... ---" -ForegroundColor Yellow

dotnet publish $ProjectFile -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -o tmp_build

# 4. Tạo cấu trúc thư mục phân phối chuyên nghiệp
Write-Host "--- Dang sap xep file vao thu muc $OutputFolder ---" -ForegroundColor Yellow
New-Item -ItemType Directory -Path $OutputFolder -Force | Out-Null

# Copy file chạy chính
Copy-Item "tmp_build\$($ProjectName).exe" -Destination "$OutputFolder\$($ProjectName).exe"

# Copy file cấu hình (để ở ngoài cho khách dễ sửa IP)
if (Test-Path "appsettings.json") {
    Copy-Item "appsettings.json" -Destination "$OutputFolder\appsettings.json"
}

# Copy biểu tượng (để khách có thể dùng làm shortcut)
if (Test-Path "medicine.ico") {
    Copy-Item "medicine.ico" -Destination "$OutputFolder\medicine.ico"
}

# 5. Dọn dẹp
Remove-Item -Path "tmp_build" -Recurse -Force

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "DONG GOI THANH CONG!" -ForegroundColor Green
Write-Host "Thu muc: $CurrentDir\$OutputFolder" -ForegroundColor Green
Write-Host "-> Ban hay nen thu muc nay lai va gui cho khach hang." -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
pause
