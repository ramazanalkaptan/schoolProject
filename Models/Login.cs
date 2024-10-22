using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class Login
    {
        [Key]
        public int Login_Id { get; set; }
        [DisplayName("Kullanıcı Adı")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Login_Name { get; set; }
        [DisplayName("Şifre")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Login_Password { get; set; }
    }
}
