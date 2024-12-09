using System.Text;
using static NuGet.Packaging.PackagingConstants;

namespace MyEStore.Models.Services
{
    public class MyTool
    {
        // f. Tạo hàm dùng chung upload file
        public static string UploadImageToFolder(IFormFile file, string folderName)
        {
            if (file == null)
            {
                return string.Empty;
            }
            try
            {
                var fileName = $"{DateTime.Now.Ticks}_{file.FileName}";
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", /* Cái Hinh này ở trong đồ án mình là img */ folderName, fileName);
                using (var myFile = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(myFile);
                }
                return fileName;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        // e. Tạo hàm dùng chung để khởi tạo mã ngẫu nhiên
        public static string GetRandom(int length = 5)
        {
            var pattern = @"1234567890qazwsxedcrfvtgbyhn@#$%";
            var rd = new Random();
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }
            return sb.ToString();
        }

    }
}