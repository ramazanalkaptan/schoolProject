using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace School.Models
{
    public class UserForgotPassword
    {
        public int Id { get; set; }
        //New Password
        [DisplayName("E-mail")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string UserForgotEmail { get; set; }
        [DisplayName("Sıfırlama Kodu")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string RegisterResetCode { get; set; }
        [DisplayName("Yeni Şifre")]
        [StringLength(12, ErrorMessage = "En az 6 karakterli olmalıdır!", MinimumLength = 6)]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string? NewRegisterPassword { get; set; }
        [DisplayName("Tekrar Şifre")]
        [StringLength(12, ErrorMessage = "En az 6 karakterli olmalıdır!", MinimumLength = 6)]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        [NotMapped]
        public string? NewRegisterPassword2 { get; set; }
        public DateTime? PasswordResetCodeExpiration { get; set; }
    }
}
