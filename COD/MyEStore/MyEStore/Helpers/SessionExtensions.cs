using System.Text.Json;

// Thầy làm cái này là namespace MyEStore.Models
namespace MyEStore.Helpers
{
    public static class SessionExtensions
    {
        // Cái kiểu <T> là kiểu tuỳ ý, muốn là gì cũng được hết
        // Viết kiểu gì cũng được hết mà ko phải kế thừa
        // Tên hàm phải có từ khoá static
        // method phải có từ khoá "this" để chỉ ra cái hàm Set thuộc về lớp ISession

        // Cái dòng này: session.SetString(key, JsonSerializer.Serialize(value));
        // Là convert từ 1 cái type bất kỳ nào đó về thành cái chuỗi Json 

        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
