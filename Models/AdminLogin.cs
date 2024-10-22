using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace School.Models
{
    public class AdminLogin
    {
        [Key]
        public int AdminLogin_Id { get; set; }
        [DisplayName("Kullanıcı Adı")]
        public string AdminLogin_Name { get; set; }
        [DisplayName("Şifre")]
        public string AdminLogin_Password { get; set; }
    }
}
