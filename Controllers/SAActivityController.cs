using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;
using eCHU.Services;
using System.ComponentModel.DataAnnotations;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("")]
    public class SAActivityController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<SAActivityController> _logger;
        private readonly IConfiguration _configuration;

        public SAActivityController(WebAPIContext context, IEmailService emailService,
            ILogger<SAActivityController> logger, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetActivities()
        {
            try
            {
                var (academic_year, semester) = CalculateCurrentAcademicYearAndSemester();

                var activitiesData = await _context.SAActivity
                    .Where(a => a.AcademicYear == academic_year && a.Semester == semester)
                    .ToListAsync();

                var activities = activitiesData
                    .Select(a => new
                    {
                        saa_id = a.ActivityId,
                        aname = a.AName?.Trim() ?? string.Empty,
                        location = a.Location?.Trim() ?? string.Empty,
                        date = a.Date?.ToString("yyyy/MM/dd") ?? string.Empty,
                        start_time = a.StartTime,
                        end_time = a.EndTime,
                        vacancy = a.Vacancy,
                        outline = a.Outline?.Trim() ?? string.Empty,
                        academic_year = a.AcademicYear,
                        semester = a.Semester,
                        remarks = a.Remarks?.Trim() ?? string.Empty,
                        organizer = a.Organizer?.Trim() ?? string.Empty,
                        fee = a.Fee?.Trim() ?? string.Empty,
                        meal = a.Meal?.Trim() ?? string.Empty,
                        status = a.Status?.Trim() ?? string.Empty,
                        number_of_applicants = a.NumberOfApplicants
                    })
                    .ToList();

                if (activities.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        message = $"找到{academic_year}學年第{semester}學期的活動資料",
                        academic_year = academic_year,
                        semester = semester,
                        activities = activities
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = $"找不到{academic_year}學年第{semester}學期的活動資料",
                        academic_year = academic_year,
                        semester = semester
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得活動資料時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        private (int academic_year, int semester) CalculateCurrentAcademicYearAndSemester()
        {
            DateTime now = DateTime.Now;
            int currentYear = now.Year;
            int currentMonth = now.Month;

            int academic_year;
            if (currentMonth >= 8)
            {
                academic_year = currentYear - 1911;
            }
            else
            {
                academic_year = currentYear - 1912;
            }

            int semester;
            if (currentMonth >= 8 || currentMonth <= 1)
            {
                semester = 1;
            }
            else
            {
                semester = 2;
            }

            return (academic_year, semester);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterActivity([FromBody] RegistrationRequest request)
        {
            // 手動驗證請求參數
            if (request == null)
            {
                return BadRequest("請求不能為空");
            }

            if (request.Activity_Id <= 0)
            {
                return BadRequest("活動ID是必填的");
            }

            if (string.IsNullOrWhiteSpace(request.Student_Id))
            {
                return BadRequest("學生ID是必填的");
            }

            if (string.IsNullOrWhiteSpace(request.email))
            {
                return BadRequest("Email是必填的");
            }

            if (string.IsNullOrWhiteSpace(request.phone_number))
            {
                return BadRequest("電話號碼是必填的");
            }

            try
            {
                var activity = await _context.SAActivity.FirstOrDefaultAsync(a => a.ActivityId == request.Activity_Id);
                if (activity == null)
                {
                    return NotFound("找不到該活動");
                }

                if (activity.Status != "可報名")
                {
                    return BadRequest("此活動已額滿，無法報名");
                }

                var studentExists = await _context.Student.AnyAsync(s => s.StudentId == request.Student_Id.Trim());
                if (!studentExists)
                {
                    return BadRequest("學生ID不存在");
                }

                var alreadyRegistered = await _context.SAActivityRecord.AnyAsync(r =>
                    r.ActivityId == request.Activity_Id &&
                    r.StudentId == request.Student_Id.Trim() &&
                    r.Status == "已報名");

                if (alreadyRegistered)
                {
                    return BadRequest("您已經報名過此活動");
                }
                
                // 創建新的報名記錄
                var record = new SAActivityRecord
                {
                    ActivityId = request.Activity_Id,
                    StudentId = request.Student_Id.Trim(),
                    Email = request.email.Trim(),
                    PhoneNumber = request.phone_number.Trim(),
                    RegistrationTime = DateTime.Now,
                    Status = "已報名",
                    EmailStatus = "待發送"
                };

                _context.SAActivityRecord.Add(record);

                // 更新活動報名人數
                if (activity.NumberOfApplicants.HasValue)
                {
                    activity.NumberOfApplicants = activity.NumberOfApplicants.Value + 1;
                }
                else
                {
                    activity.NumberOfApplicants = 1;
                }

                // 檢查是否達到上限
                if (activity.Vacancy.HasValue && activity.NumberOfApplicants >= activity.Vacancy.Value)
                {
                    activity.Status = "已額滿";
                }

                await _context.SaveChangesAsync();

                // 嘗試發送確認郵件
                try
                {
                    _logger.LogInformation($"開始發送郵件給: {request.email}, 學生ID: {request.Student_Id}, 活動ID: {request.Activity_Id}");

                    bool emailSent = await SendConfirmationEmail(request.email, request.Student_Id, activity);

                    record.EmailStatus = emailSent ? "已發送" : "發送失敗";
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"郵件發送結果: {(emailSent ? "成功" : "失敗")}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發送確認郵件時發生錯誤");
                    record.EmailStatus = "發送失敗";
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "報名成功",
                    registration_id = record.RegistrationId,
                    email_status = record.EmailStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "註冊活動時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        private async Task<bool> SendConfirmationEmail(string email, string studentId, SAActivity activity)
        {
            try
            {
                string subject = $"活動報名確認: {activity.AName?.Trim()}";

                string body = $@"
<html>
<body>
    <h2>活動報名確認</h2>
    <p>親愛的學生 {studentId}：</p>
    <p>感謝您報名參加「{activity.AName?.Trim()}」活動。以下是活動詳情：</p>
    <ul>
        <li><strong>活動名稱：</strong>{activity.AName?.Trim()}</li>
        <li><strong>活動日期：</strong>{activity.Date?.ToString("yyyy/MM/dd")}</li>
        <li><strong>活動時間：</strong>{activity.StartTime} - {activity.EndTime}</li>
        <li><strong>活動地點：</strong>{activity.Location?.Trim()}</li>
    </ul>
    <p>若有任何問題，請隨時與我們聯繫。</p>
    <p>此致，<br>活動管理團隊</p>
</body>
</html>";

                // 獲取附件路徑
                var attachmentPaths = GetActivityAttachments(activity, studentId);

                return await _emailService.SendEmailAsync(email, subject, body, attachmentPaths);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送確認郵件時發生錯誤");
                return false;
            }
        }

        private List<string>? GetActivityAttachments(SAActivity activity, string studentId)
        {
            try
            {
                var attachments = new List<string>();
                string? filePath = _configuration["ActivityDocumentsPath"];

                if (string.IsNullOrEmpty(filePath))
                {
                    _logger.LogWarning("ActivityDocumentsPath not configured, skipping attachments");
                    return null;
                }

                string activitySpecificFile = Path.Combine(filePath, $"activity_{activity.ActivityId}.pdf");
                if (System.IO.File.Exists(activitySpecificFile))
                {
                    _logger.LogInformation($"Adding activity-specific attachment: {activitySpecificFile}");
                    attachments.Add(activitySpecificFile);
                }

                string generalDocFile = Path.Combine(filePath, "general_instructions.pdf");
                if (System.IO.File.Exists(generalDocFile))
                {
                    _logger.LogInformation($"Adding general attachment: {generalDocFile}");
                    attachments.Add(generalDocFile);
                }

                return attachments.Any() ? attachments : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加附件時發生錯誤");
                return null;
            }
        }

        [HttpGet("activities/student/{Student_ID}")]
        public async Task<IActionResult> GetActivitiesByStudentId(string Student_ID)
        {
            if (string.IsNullOrEmpty(Student_ID))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var records = await _context.SAActivityRecord
                    .Where(r => r.StudentId == Student_ID && r.Status == "已報名")
                    .ToListAsync();

                var activities = await _context.SAActivity.ToListAsync();

                var studentActivities = (from record in records
                                         join activity in activities on record.ActivityId equals activity.ActivityId
                                         select new
                                         {
                                             Activity_Id = activity.ActivityId,
                                             activity_name = activity.AName?.Trim() ?? string.Empty,
                                             activity_date = activity.Date?.ToString("yyyy/MM/dd") ?? string.Empty,
                                             start_time = activity.StartTime,
                                             end_time = activity.EndTime,
                                             location = activity.Location?.Trim() ?? string.Empty,
                                             status = record.Status
                                         }).ToList();

                if (studentActivities.Any())
                {
                    return Ok(new { success = true, message = "找到學生報名的活動資料", activities = studentActivities });
                }
                else
                {
                    return Ok(new { success = false, message = "該學生沒有報名任何活動" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得學生活動資料時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelRegistration([FromBody] CancelRegistrationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("請求參數不能為空");
                }

                if (string.IsNullOrEmpty(request.Student_Id))
                {
                    return BadRequest("學號不能為空");
                }

                var activity = await _context.SAActivity.FirstOrDefaultAsync(a => a.ActivityId == request.Activity_Id);
                if (activity == null)
                {
                    return BadRequest("找不到該活動");
                }

                var student = await _context.Student.FirstOrDefaultAsync(s => s.StudentId == request.Student_Id.Trim());
                if (student == null)
                {
                    return BadRequest("找不到該學生");
                }

                var registration = await _context.SAActivityRecord.FirstOrDefaultAsync(r =>
                    r.ActivityId == request.Activity_Id &&
                    r.StudentId == request.Student_Id.Trim() &&
                    r.Status == "已報名");

                if (registration == null)
                {
                    return BadRequest("找不到此學生對該活動的有效報名記錄");
                }

                if (activity.NumberOfApplicants.HasValue && activity.NumberOfApplicants.Value > 0)
                {
                    activity.NumberOfApplicants = activity.NumberOfApplicants.Value - 1;
                }

                UpdateActivityStatus(activity);
                registration.Status = "已取消";

                // 發送取消確認郵件
                try
                {
                    await SendCancellationEmail(registration.Email!, registration.StudentId, activity);
                    registration.EmailStatus = "已發送";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "發送取消確認郵件時發生錯誤");
                    registration.EmailStatus = "發送失敗";
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = $"已成功取消「{activity.AName?.Trim()}」的報名"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消報名時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        private void UpdateActivityStatus(SAActivity activity)
        {
            if (activity.Vacancy.HasValue &&
                activity.NumberOfApplicants.HasValue &&
                activity.NumberOfApplicants.Value < activity.Vacancy.Value)
            {
                activity.Status = "可報名";
            }
        }

        private async Task SendCancellationEmail(string email, string studentId, SAActivity activity)
        {
            try
            {
                string subject = $"活動取消確認: {activity.AName?.Trim()}";

                string body = $@"
<html>
<body>
    <h2>活動取消確認</h2>
    <p>親愛的學生 {studentId}：</p>
    <p>您已成功取消報名「{activity.AName?.Trim()}」活動。以下是活動詳情：</p>
    <ul>
        <li><strong>活動名稱：</strong>{activity.AName?.Trim()}</li>
        <li><strong>活動日期：</strong>{activity.Date?.ToString("yyyy/MM/dd")}</li>
        <li><strong>活動時間：</strong>{activity.StartTime} - {activity.EndTime}</li>
        <li><strong>活動地點：</strong>{activity.Location?.Trim()}</li>
    </ul>
    <p>若有任何問題，請隨時與我們聯繫。</p>
    <p>此致，<br>活動管理團隊</p>
</body>
</html>";

                await _emailService.SendEmailAsync(email, subject, body);
                _logger.LogInformation($"已成功發送取消確認郵件至: {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送取消確認郵件時發生錯誤");
                throw;
            }
        }
    }

    // 定義請求模型類別，避免模型驗證問題
    public class RegistrationRequest
    {
        public int Activity_Id { get; set; }
        public string Student_Id { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phone_number { get; set; } = string.Empty;
    }

    public class CancelRegistrationRequest
    {
        public int Activity_Id { get; set; }
        public string Student_Id { get; set; } = string.Empty;
    }
}