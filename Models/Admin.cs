using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School.Models
{
    public class Admin
    {
        [Key]
        public int Admin_Id { get; set; }
        [DisplayName("Kullanıcı Adı")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Admin_Name { get; set; }
        [DisplayName("E-mail")]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Admin_SurName { get; set; }
        [DisplayName("Şifre")]
        [StringLength(12, ErrorMessage = "En az 6 karakterli olmalıdır!", MinimumLength = 6)]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string Admin_Password { get; set; }
        [DisplayName("Tekrar Şifre")]
        [NotMapped]
        [Required(ErrorMessage = "Boş Bırakılamaz")]
        public string? Admin_Password2 { get; set; }
    }
}
