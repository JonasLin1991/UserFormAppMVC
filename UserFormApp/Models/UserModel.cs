using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class UserModel
{
    [Required(ErrorMessage = "姓名為必填")]
    public string Name { get; set; }

    [Range(1, 120, ErrorMessage = "年齡必須介於1到120歲")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Email 為必填")]
    [EmailAddress(ErrorMessage = "Email 格式不正確")]
    public string Email { get; set; }


    [Required(ErrorMessage = "請輸入密碼")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "密碼為6～20字")]
    public string Password { get; set; }

    [NotMapped]
    [Required(ErrorMessage = "確認密碼為必填")]
    [Compare("Password", ErrorMessage ="確認密碼與密碼不一致")]
    [Display(Name = "確認密碼")]
    public string ConfirmPassword { get; set; }

    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public static class FakeUserDB
    {
        public static List<UserModel> RegisterUsers = new List<UserModel>();
    }
}
