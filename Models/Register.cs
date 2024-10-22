using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class Register
    {
        [Key]
        public int Register_Id { get; set; }
        [DisplayName("Kullanıcı Adı")]
        [Required(ErrorMessage ="Boş Bırakılamaz")]
        public string Register_Name { get; set; }
        [DisplayName("E-mail")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Register_Email { get; set; }
        [DisplayName("Şifre")]
        [StringLength(12, ErrorMessage = "En az 6 karakterli olmalıdır!", MinimumLength = 6)]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Register_Password { get; set; }
        [DisplayName("Tekrar Şifre")]
        [StringLength(12,ErrorMessage ="En az 6 karakterli olmalıdır!",MinimumLength =6)]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        [NotMapped]
        public string Register_Password2 { get; set; }
    }
}
