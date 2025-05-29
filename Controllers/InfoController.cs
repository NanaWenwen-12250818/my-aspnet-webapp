using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("")]
    public class InfoController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<InfoController> _logger;

        public InfoController(WebAPIContext context, ILogger<InfoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("info")]
        public async Task<IActionResult> InfoGet(string student_id)
        {
            if (string.IsNullOrEmpty(student_id))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var studentEntity = await _context.Student
                    .Include(s => s.Department)
                    .Where(s => s.StudentId.Trim() == student_id)
                    .FirstOrDefaultAsync();

                if (studentEntity != null)
                {
                    string departmentName = string.Empty;
                    if (studentEntity.Department != null)
                    {
                        departmentName = studentEntity.Department.DeptName?.Trim() ?? string.Empty;
                    }

                    var student = new
                    {
                        student_id = studentEntity.StudentId.Trim(),
                        sdepartment = departmentName,
                        cclass = studentEntity.Class?.Trim() ?? string.Empty,
                        semail = studentEntity.SEmail?.Trim() ?? string.Empty,
                        room = studentEntity.Room?.ToString() ?? string.Empty,
                        phone_number = studentEntity.PhoneNumber?.Trim() ?? string.Empty,
                        name = studentEntity.SName?.Trim() ?? string.Empty,
                        gender = studentEntity.Gender?.Trim() ?? string.Empty,
                        date_of_birth = studentEntity.DateOfBirth?.ToString("yyyy-MM-dd") ?? string.Empty
                    };

                    return Ok(new
                    {
                        success = true,
                        message = "找到對應的資訊",
                        department = student.sdepartment,
                        cclass = student.cclass,
                        email = student.semail,
                        sroom = student.room,
                        sstudent_id = student.student_id,
                        sphone_number = student.phone_number,
                        sname = student.name,
                        sgender = student.gender,
                        sdate_of_birth = student.date_of_birth
                    });
                }
                else
                {
                    return Ok(new { success = false, message = "找不到對應的資訊" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得學生資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }
    }
}