using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School.DataContext;
using School.Models;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;

namespace School.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContextDb _context;
        private readonly SmtpSettings _smtpSettings;

        public HomeController(DataContextDb context, IConfiguration configuration)
        {
            _context = context;
            _smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            var login = new Login();

            // Eðer çerezlerde kullanýcý adý ve þifre varsa bunlarý doldur
            if (Request.Cookies.ContainsKey("Login_Name"))
            {
                login.Login_Name = Request.Cookies["Login_Name"];
            }

            if (Request.Cookies.ContainsKey("Login_Password"))
            {
                login.Login_Password = Request.Cookies["Login_Password"];
            }

            return View(login);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(Login login, bool? RememberMe)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Registers
                    .FirstOrDefault(a => a.Register_Password == login.Login_Password && a.Register_Name == login.Login_Name);

                if (user != null) // Burada gerçek bir doðrulama kullanmalýsýnýz.
                {
                    // Beni hatýrla seçeneði iþaretli ise çerez oluþturuyoruz.
                    if (RememberMe == true) // null deðilse true kontrolü
                    {
                        CookieOptions options = new CookieOptions();
                        options.Expires = DateTime.Now.AddDays(7); // 7 gün boyunca hatýrla

                        Response.Cookies.Append("Login_Name", login.Login_Name, options);
                        Response.Cookies.Append("Login_Password", login.Login_Password, options);
                    }
                    else
                    {
                        // Beni hatýrla iþaretli deðilse çerezleri siliyoruz.
                        Response.Cookies.Delete("Login_Name");
                        Response.Cookies.Delete("Login_Password");
                    }
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name,user.Register_Name),
                        new Claim(ClaimTypes.NameIdentifier, user.Register_Id.ToString()),
                        new Claim(ClaimTypes.Role,"User")
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        // Cookie ömrünü buradan ayarlayabilirsiniz
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddMinutes(10)
                    };
                    // Kullanýcýyý oturum açmýþ olarak kabul et
                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        authProperties);
                    // Baþarýlý giriþ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login_Password", "Geçersiz kullanýcý adý veya þifre.");
                }
            }

            return View(login);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Register_Id,Register_Name,Register_Email,Register_Password,Register_Password2")] Register register, bool? Terms)
        {
            if (Terms == null)
            {
                TempData["Terms"] = "Devam edebilmek için onaylayýnýz.";
                return View(register);
            }

            if (register.Register_Password != register.Register_Password2)
            {
                // Þifreler eþleþmiyor
                ModelState.AddModelError("Register_Password2", "Þifreler ayný deðil!");
            }

            var RegisterName = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Name == register.Register_Name);
            var RegisterEmail = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Email == register.Register_Email);

            if (RegisterName != null)
            {
                ModelState.AddModelError("Register_Name", "Böyle bir kullanýcý adý zaten var!");
            }
            if (RegisterEmail != null)
            {
                ModelState.AddModelError("Register_Email", "Böyle bir e-posta adresi zaten var!");
            }
            
            // ModelState'in geçerli olup olmadýðýný kontrol edin ve sadece geçerli olduðunda kaydet ve yönlendir.
            if (ModelState.IsValid)
            {
                _context.Add(register);
                await _context.SaveChangesAsync();
                // Baþarýlý kayýttan sonra TempData ile mesajý ayarla ve yönlendir
                TempData["SuccessMessage"] = "Kayýt oluþturuldu, giriþ sayfasýna yönlendirilmektesiniz.";
                return View(register);  
            }

            // ModelState geçersizse (hatalar varsa) ayný sayfayý geri döndür.
            return View(register);
        }
        [Authorize(Roles ="User")]
        public async Task<IActionResult> Logout()
        {
            if (User.IsInRole("User"))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendEmail(ContactFormModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var senderEmail = new MailAddress(_smtpSettings.Username, "Ramazan Alkaptan");
                    var receiverEmail = new MailAddress(_smtpSettings.Username, "Gelen Mesaj");
                    var password = _smtpSettings.Password;
                    var sub = "Web Sitesi Gelen Mesaj";
                    var body = $"Adýnýz: {model.FirstName}\nSoyad: {model.LastName}\nTelefon Numarasý: {model.Phone}\nMesajýnýz: {model.Message}";

                    var smtp = new SmtpClient
                    {
                        Host = _smtpSettings.Host,
                        Port = _smtpSettings.Port,
                        EnableSsl = _smtpSettings.EnableSsl,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };

                    using (var message = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = sub,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }

                    ViewBag.MessageSuccess = "Mesajýnýz baþarýlý þekilde iletilmiþtir.";
                    ModelState.Clear(); // Formu temizler
                    return View("Index", new ContactFormModel()); // Yeni model ile view'i döner
                }
                catch (Exception)
                {
                    ViewBag.MessageError = "Mesaj gönderilirken bir hata oluþtu.";
                    return View("Index");
                }
            }

            ViewBag.MessageError = "Mesajýnýzda hata oluþtu, lütfen tekrar deneyiniz.";
            return View("Index");
        }
        //User Forgot Password
        
        public IActionResult UserForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserForgotPassword(UserForgotPassword userForgotPassword)
        {
            var UserForgotEmail = await _context.Registers
                .FirstOrDefaultAsync(a => a.Register_Email == userForgotPassword.UserForgotEmail);

            if (UserForgotEmail != null)
            {
                var resetCode = new Random().Next(100000, 999999).ToString();

                try
                {
                    var senderEmail = new MailAddress(_smtpSettings.Username, "Ramazan Alkaptan");
                    var receiverEmail = new MailAddress(_smtpSettings.Username, "Gelen Mesaj");
                    var password = _smtpSettings.Password;
                    var sub = "Þifre yenileme kodu";
                    var body = $"Þifre yenileme kodunuz: {resetCode}";

                    var smtp = new SmtpClient
                    {
                        Host = _smtpSettings.Host,
                        Port = _smtpSettings.Port,
                        EnableSsl = _smtpSettings.EnableSsl,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };

                    using (var message = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = sub,
                        Body = body
                    })
                    {
                        smtp.Send(message);
                    }

                    // Kullanýcýnýn kayýtlý olduðu veriye eriþim ve reset kodunu ayarlama
                    userForgotPassword.RegisterResetCode = resetCode;
                    userForgotPassword.PasswordResetCodeExpiration = DateTime.Now.AddMinutes(5); // 5 dk geçerli
                    userForgotPassword.NewRegisterPassword = "TempPass";
                    _context.Add(userForgotPassword);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "E-postanýza þifre yenileme kodu gönderilmiþtir.<br/>Þifre yenileme için bilgileri giriniz.<br/>Kod süresi 120 saniyedir.";
                    return RedirectToAction("UserResetPassword", "Home");
                }
                catch (Exception)
                {
                    ViewBag.MessageError = "Mesaj gönderilirken bir hata oluþtu.";
                    return View(userForgotPassword);
                }
            }
            else
            {
                ModelState.AddModelError("UserForgotEmail", "Kayýtlý bir email adresi girin!");
                return View(userForgotPassword);
            }
        }

        public IActionResult UserResetPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserResetPassword(UserForgotPassword model)
        {
            // Sýfýrlama kodunu kontrol et
            var user = await _context.UserForgotPasswords
                .FirstOrDefaultAsync(a=> a.UserForgotEmail == model.UserForgotEmail && a.RegisterResetCode == model.RegisterResetCode);
            var userNewPassword = await _context.Registers
                .FirstOrDefaultAsync(a => a.Register_Email == model.UserForgotEmail);
            if (model.NewRegisterPassword != model.NewRegisterPassword2)
            {
                ModelState.AddModelError("NewRegisterPassword2", "Þifreler ayný deðil");
            }
            if (user != null && userNewPassword != null)
            {
                // Kodun süresinin geçip geçmediðini kontrol et
                if (user.PasswordResetCodeExpiration > DateTime.Now)
                {
                    // Yeni þifreyi hashleyerek güncelle
                    userNewPassword.Register_Password = model.NewRegisterPassword; // Þifreyi uygun þekilde hashleyerek kaydetmelisiniz
                    user.RegisterResetCode = null; // Kod kullanýldýktan sonra temizlenebilir
                    user.PasswordResetCodeExpiration = null; // Geçerlilik süresini temizle

                    _context.UserForgotPasswords.Remove(user);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "Þifreniz baþarýyla güncellenmiþtir.";
                    return RedirectToAction("Login"); // Giriþ sayfasýna yönlendirin
                }
                else
                {
                    ModelState.AddModelError("RegisterResetCode", "Þifre yenileme kodu süresi dolmuþ. Lütfen yeni bir kod talep edin.");
                }
            }
            else
            {
                ModelState.AddModelError("RegisterResetCode", "Geçersiz e-posta adresi veya þifre yenileme kodu.");
            }

            return View("UserResetPassword", model); // Hata durumunda tekrar þifre sýfýrlama sayfasýna döndür
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
