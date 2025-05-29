using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("")]
    public class LoginController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<LoginController> _logger;

        public LoginController(WebAPIContext context, ILogger<LoginController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("login")]
        public async Task<IActionResult> LoginGet(string student_ID)
        {
            if (string.IsNullOrEmpty(student_ID))
            {
                return BadRequest("帳號不能為空");
            }

            try
            {
                var student = await _context.Student
                    .Where(s => s.StudentId.Trim() == student_ID)
                    .Select(s => new
                    {
                        student_ID = s.StudentId.Trim(),
                        password1 = s.Password1!.Trim(),
                        sname = s.SName!.Trim()
                    })
                    .FirstOrDefaultAsync();

                if (student != null)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "找到對應的密碼和名字",
                        password = student.password1,
                        name = student.sname
                    });
                }
                else
                {
                    return Ok(new { success = false, message = "找不到對應的密碼和名字" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登入驗證時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }
    }
}