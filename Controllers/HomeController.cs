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

            // E�er �erezlerde kullan�c� ad� ve �ifre varsa bunlar� doldur
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

                if (user != null) // Burada ger�ek bir do�rulama kullanmal�s�n�z.
                {
                    // Beni hat�rla se�ene�i i�aretli ise �erez olu�turuyoruz.
                    if (RememberMe == true) // null de�ilse true kontrol�
                    {
                        CookieOptions options = new CookieOptions();
                        options.Expires = DateTime.Now.AddDays(7); // 7 g�n boyunca hat�rla

                        Response.Cookies.Append("Login_Name", login.Login_Name, options);
                        Response.Cookies.Append("Login_Password", login.Login_Password, options);
                    }
                    else
                    {
                        // Beni hat�rla i�aretli de�ilse �erezleri siliyoruz.
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
                        // Cookie �mr�n� buradan ayarlayabilirsiniz
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(3)).AddMinutes(10)
                    };
                    // Kullan�c�y� oturum a�m�� olarak kabul et
                    HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        authProperties);
                    // Ba�ar�l� giri�
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("Login_Password", "Ge�ersiz kullan�c� ad� veya �ifre.");
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
                TempData["Terms"] = "Devam edebilmek i�in onaylay�n�z.";
                return View(register);
            }

            if (register.Register_Password != register.Register_Password2)
            {
                // �ifreler e�le�miyor
                ModelState.AddModelError("Register_Password2", "�ifreler ayn� de�il!");
            }

            var RegisterName = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Name == register.Register_Name);
            var RegisterEmail = await _context.Registers
                    .FirstOrDefaultAsync(a => a.Register_Email == register.Register_Email);

            if (RegisterName != null)
            {
                ModelState.AddModelError("Register_Name", "B�yle bir kullan�c� ad� zaten var!");
            }
            if (RegisterEmail != null)
            {
                ModelState.AddModelError("Register_Email", "B�yle bir e-posta adresi zaten var!");
            }
            
            // ModelState'in ge�erli olup olmad���n� kontrol edin ve sadece ge�erli oldu�unda kaydet ve y�nlendir.
            if (ModelState.IsValid)
            {
                _context.Add(register);
                await _context.SaveChangesAsync();
                // Ba�ar�l� kay�ttan sonra TempData ile mesaj� ayarla ve y�nlendir
                TempData["SuccessMessage"] = "Kay�t olu�turuldu, giri� sayfas�na y�nlendirilmektesiniz.";
                return View(register);  
            }

            // ModelState ge�ersizse (hatalar varsa) ayn� sayfay� geri d�nd�r.
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
                    var body = $"Ad�n�z: {model.FirstName}\nSoyad: {model.LastName}\nTelefon Numaras�: {model.Phone}\nMesaj�n�z: {model.Message}";

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

                    ViewBag.MessageSuccess = "Mesaj�n�z ba�ar�l� �ekilde iletilmi�tir.";
                    ModelState.Clear(); // Formu temizler
                    return View("Index", new ContactFormModel()); // Yeni model ile view'i d�ner
                }
                catch (Exception)
                {
                    ViewBag.MessageError = "Mesaj g�nderilirken bir hata olu�tu.";
                    return View("Index");
                }
            }

            ViewBag.MessageError = "Mesaj�n�zda hata olu�tu, l�tfen tekrar deneyiniz.";
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
                    var sub = "�ifre yenileme kodu";
                    var body = $"�ifre yenileme kodunuz: {resetCode}";

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

                    // Kullan�c�n�n kay�tl� oldu�u veriye eri�im ve reset kodunu ayarlama
                    userForgotPassword.RegisterResetCode = resetCode;
                    userForgotPassword.PasswordResetCodeExpiration = DateTime.Now.AddMinutes(5); // 5 dk ge�erli
                    userForgotPassword.NewRegisterPassword = "TempPass";
                    _context.Add(userForgotPassword);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "E-postan�za �ifre yenileme kodu g�nderilmi�tir.<br/>�ifre yenileme i�in bilgileri giriniz.<br/>Kod s�resi 120 saniyedir.";
                    return RedirectToAction("UserResetPassword", "Home");
                }
                catch (Exception)
                {
                    ViewBag.MessageError = "Mesaj g�nderilirken bir hata olu�tu.";
                    return View(userForgotPassword);
                }
            }
            else
            {
                ModelState.AddModelError("UserForgotEmail", "Kay�tl� bir email adresi girin!");
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
            // S�f�rlama kodunu kontrol et
            var user = await _context.UserForgotPasswords
                .FirstOrDefaultAsync(a=> a.UserForgotEmail == model.UserForgotEmail && a.RegisterResetCode == model.RegisterResetCode);
            var userNewPassword = await _context.Registers
                .FirstOrDefaultAsync(a => a.Register_Email == model.UserForgotEmail);
            if (model.NewRegisterPassword != model.NewRegisterPassword2)
            {
                ModelState.AddModelError("NewRegisterPassword2", "�ifreler ayn� de�il");
            }
            if (user != null && userNewPassword != null)
            {
                // Kodun s�resinin ge�ip ge�medi�ini kontrol et
                if (user.PasswordResetCodeExpiration > DateTime.Now)
                {
                    // Yeni �ifreyi hashleyerek g�ncelle
                    userNewPassword.Register_Password = model.NewRegisterPassword; // �ifreyi uygun �ekilde hashleyerek kaydetmelisiniz
                    user.RegisterResetCode = null; // Kod kullan�ld�ktan sonra temizlenebilir
                    user.PasswordResetCodeExpiration = null; // Ge�erlilik s�resini temizle

                    _context.UserForgotPasswords.Remove(user);
                    await _context.SaveChangesAsync();

                    TempData["MessageSuccess"] = "�ifreniz ba�ar�yla g�ncellenmi�tir.";
                    return RedirectToAction("Login"); // Giri� sayfas�na y�nlendirin
                }
                else
                {
                    ModelState.AddModelError("RegisterResetCode", "�ifre yenileme kodu s�resi dolmu�. L�tfen yeni bir kod talep edin.");
                }
            }
            else
            {
                ModelState.AddModelError("RegisterResetCode", "Ge�ersiz e-posta adresi veya �ifre yenileme kodu.");
            }

            return View("UserResetPassword", model); // Hata durumunda tekrar �ifre s�f�rlama sayfas�na d�nd�r
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
