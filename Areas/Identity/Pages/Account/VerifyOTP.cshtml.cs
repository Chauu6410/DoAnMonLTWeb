using DoAnMonLTWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace DoAnMonLTWeb.Areas.Identity.Pages.Account
{
    public class VerifyOTPModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public VerifyOTPModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string ReturnUrl { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP phải gồm 6 chữ số")]
            public string OtpCode { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản với email '{email}'.");
            }

            Email = email;
            ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return NotFound($"Không tìm thấy user đăng nhập.");
            }

            if (user.OtpCode == Input.OtpCode)
            {
                if (user.OtpExpiryTime.HasValue && user.OtpExpiryTime.Value >= DateTime.Now)
                {
                    // OTP Hợp lệ
                    // Đánh dấu email đã xác thực
                    user.EmailConfirmed = true;
                    // Xóa mã OTP để tránh tái sử dụng
                    user.OtpCode = null;
                    user.OtpExpiryTime = null;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        TempData["SuccessMessage"] = "Xác nhận tài khoản thành công!";
                        return LocalRedirect(ReturnUrl);
                    }
                    
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Mã OTP đã hết hạn. Vui lòng yêu cầu gửi lại mã mới.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không chính xác.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
            {
                return NotFound();
            }

            // Tạo mã OTP mới
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();

            user.OtpCode = otp;
            user.OtpExpiryTime = DateTime.Now.AddMinutes(5);
            await _userManager.UpdateAsync(user);

            var subject = "Mã xác thực tài khoản (Gửi lại) - DoAnMonLTWeb";
            var htmlMessage = $@"
                <h3>Xin chào {user.FullName},</h3>
                <p>Bạn vừa yêu cầu gửi lại mã OTP. Mã OTP xác thực mới của bạn là:</p>
                <h2 style='color:blue; letter-spacing: 2px;'>{otp}</h2>
                <p>Mã này có hiệu lực trong vòng <strong>5 phút</strong>.</p>";

            await _emailSender.SendEmailAsync(Email, subject, htmlMessage);

            ModelState.Clear();
            ModelState.AddModelError(string.Empty, "Mã OTP mới đã được gửi đến email của bạn.");
            return Page();
        }
    }
}
