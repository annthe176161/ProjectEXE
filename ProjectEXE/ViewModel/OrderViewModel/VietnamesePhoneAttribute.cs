using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ProjectEXE.ViewModel.OrderViewModel
{
    public class VietnamesePhoneAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success; // Let Required handle this
            }

            string phone = value.ToString().Trim();

            // Remove spaces and special characters
            phone = Regex.Replace(phone, @"[\s\-\.\(\)]", "");

            // Vietnamese phone number patterns
            var patterns = new[]
            {
                @"^(03[2-9]|05[689]|07[06-9]|08[1-9]|09[0-46-9])\d{7}$",  // Mobile 10 digits
                @"^(024|028|0236|0239|0254|0259|0261|0262|0263|0269|0271|0272|0274|0275|0276|0277|0290|0291|0292|0293|0294|0296|0297|0299)\d{7,8}$" // Landline
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(phone, pattern))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult("Số điện thoại không đúng định dạng Việt Nam (VD: 0901234567)");
        }
    }

    public class VietnameseNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string name = value.ToString().Trim();

            // Check for Vietnamese name pattern (letters, spaces, hyphens, dots)
            if (!Regex.IsMatch(name, @"^[\p{L}\s\-\.]+$"))
            {
                return new ValidationResult("Họ tên chỉ được chứa chữ cái, khoảng trắng, dấu gạch ngang và dấu chấm");
            }

            // Check minimum words (at least 2 words)
            var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2)
            {
                return new ValidationResult("Vui lòng nhập họ và tên đầy đủ (ít nhất 2 từ)");
            }

            // Check for reasonable name length per word
            foreach (var word in words)
            {
                if (word.Length > 20)
                {
                    return new ValidationResult("Mỗi từ trong tên không được quá 20 ký tự");
                }
            }

            return ValidationResult.Success;
        }
    }
}
