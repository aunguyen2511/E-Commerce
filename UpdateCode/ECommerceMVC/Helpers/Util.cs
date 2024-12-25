using System.Text;

namespace ECommerceMVC.Helpers
{
    public class Util
    {
        public static string UploadHinh(IFormFile Hinh, string folder)
        {
            try
            {
                // Tạo đường dẫn đầy đủ trên server
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", folder);

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Xử lý trùng tên file
                var fileName = Path.GetFileNameWithoutExtension(Hinh.FileName);
                var extension = Path.GetExtension(Hinh.FileName);
                var uniqueFileName = $"{fileName}_{DateTime.Now.Ticks}{extension}";

                var fullPath = Path.Combine(folderPath, uniqueFileName);

                // Lưu tệp
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    Hinh.CopyTo(fileStream);
                }

                // Trả về đường dẫn web
                return $"/Hinh/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"UploadHinh Error: {ex.Message}");
                return string.Empty;
            }
        }

        public static string GenerateRamdomKey(int length = 5)
        {
            var pattern = @"qazwsxedcrfvtgbyhnujmiklopQAZWSXEDCRFVTGBYHNUJMIKLOP!";
            var sb = new StringBuilder();
            var rd = new Random();
            for (int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }

            return sb.ToString();
        }
    }
}
