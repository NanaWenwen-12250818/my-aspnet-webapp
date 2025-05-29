using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;
using System.Text.RegularExpressions;
using System.Net;
using System.Text.Json.Serialization;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("leave")]
    public class LeaveRecordController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<LeaveRecordController> _logger;

        public LeaveRecordController(WebAPIContext context, ILogger<LeaveRecordController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitLeaveRequest()
        {
            try
            {
                _logger.LogInformation("開始處理請假申請");

                LeaveRecord? leaveRequest = null;

                // 判斷請求內容類型
                if (Request.HasFormContentType)
                {
                    _logger.LogInformation("處理 form-data 格式請求");

                    // 處理 multipart/form-data 格式的請求
                    var form = await Request.ReadFormAsync();

                    // 記錄接收到的欄位
                    foreach (var field in form)
                    {
                        _logger.LogInformation($"接收欄位: {field.Key} = {field.Value}");
                    }

                    // 驗證必要欄位
                    if (!form.ContainsKey("Student_Id") || string.IsNullOrEmpty(form["Student_Id"]))
                    {
                        return BadRequest(new { success = false, message = "Student_Id 欄位為必填" });
                    }

                    if (!form.ContainsKey("Start_Date") || string.IsNullOrEmpty(form["Start_Date"]))
                    {
                        return BadRequest(new { success = false, message = "Start_Date 欄位為必填" });
                    }

                    if (!form.ContainsKey("End_Date") || string.IsNullOrEmpty(form["End_Date"]))
                    {
                        return BadRequest(new { success = false, message = "End_Date 欄位為必填" });
                    }

                    if (!form.ContainsKey("Start_Period") || string.IsNullOrEmpty(form["Start_Period"]))
                    {
                        return BadRequest(new { success = false, message = "Start_Period 欄位為必填" });
                    }

                    if (!form.ContainsKey("LType") || string.IsNullOrEmpty(form["LType"]))
                    {
                        return BadRequest(new { success = false, message = "LType 欄位為必填" });
                    }

                    // 解析日期和數字欄位
                    DateTime startDate, endDate;
                    int startPeriod;
                    int? endPeriod = null;

                    if (!DateTime.TryParse(form["Start_Date"], out startDate))
                    {
                        return BadRequest(new { success = false, message = "Start_Date 格式不正確" });
                    }

                    if (!DateTime.TryParse(form["End_Date"], out endDate))
                    {
                        return BadRequest(new { success = false, message = "End_Date 格式不正確" });
                    }

                    if (!int.TryParse(form["Start_Period"], out startPeriod))
                    {
                        return BadRequest(new { success = false, message = "Start_Period 必須為數字" });
                    }

                    if (form.ContainsKey("End_Period") && !string.IsNullOrEmpty(form["End_Period"]))
                    {
                        if (int.TryParse(form["End_Period"], out int endPeriodValue))
                        {
                            endPeriod = endPeriodValue;
                        }
                    }

                    leaveRequest = new LeaveRecord
                    {
                        StudentId = form["Student_Id"].ToString().Trim(),
                        StartDate = startDate,
                        EndDate = endDate,
                        StartPeriod = startPeriod,
                        EndPeriod = endPeriod,
                        LType = form["LType"].ToString().Trim(),
                        Reason = form.ContainsKey("Reason") ? form["Reason"].ToString() ?? string.Empty : string.Empty,
                        ApplyDate = DateTime.Now,
                        Progress = "學生送件",
                        PhoneNumber = form.ContainsKey("Phone_Number") ? form["Phone_Number"].ToString() ?? string.Empty : string.Empty
                    };

                    // 處理上傳的檔案
                    if (form.Files.Count > 0)
                    {
                        var file = form.Files.FirstOrDefault(f => f.Name == "Certificate") ?? form.Files[0];
                        if (file != null && file.Length > 0)
                        {
                            _logger.LogInformation($"處理上傳檔案: {file.FileName}, 大小: {file.Length} bytes");
                            using var memoryStream = new MemoryStream();
                            await file.CopyToAsync(memoryStream);
                            leaveRequest.Certificate = memoryStream.ToArray();
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("處理 JSON 格式請求");
                    // 處理 JSON 格式的請求
                    var requestDto = await Request.ReadFromJsonAsync<LeaveRequestDto>();
                    if (requestDto != null)
                    {
                        // 驗證必要欄位
                        if (string.IsNullOrEmpty(requestDto.Student_Id))
                        {
                            return BadRequest(new { success = false, message = "Student_Id 欄位為必填" });
                        }

                        if (string.IsNullOrEmpty(requestDto.Start_Date))
                        {
                            return BadRequest(new { success = false, message = "Start_Date 欄位為必填" });
                        }

                        if (string.IsNullOrEmpty(requestDto.End_Date))
                        {
                            return BadRequest(new { success = false, message = "End_Date 欄位為必填" });
                        }

                        if (requestDto.Start_Period <= 0)
                        {
                            return BadRequest(new { success = false, message = "Start_Period 欄位為必填且必須大於0" });
                        }

                        if (string.IsNullOrEmpty(requestDto.LType))
                        {
                            return BadRequest(new { success = false, message = "LType 欄位為必填" });
                        }

                        // 解析日期
                        DateTime startDate, endDate;

                        if (!DateTime.TryParse(requestDto.Start_Date, out startDate))
                        {
                            return BadRequest(new { success = false, message = "Start_Date 格式不正確" });
                        }

                        if (!DateTime.TryParse(requestDto.End_Date, out endDate))
                        {
                            return BadRequest(new { success = false, message = "End_Date 格式不正確" });
                        }

                        leaveRequest = new LeaveRecord
                        {
                            StudentId = requestDto.Student_Id.Trim(),
                            StartDate = startDate,
                            EndDate = endDate,
                            StartPeriod = requestDto.Start_Period,
                            EndPeriod = requestDto.End_Period,
                            LType = requestDto.LType.Trim(),
                            Reason = requestDto.Reason ?? string.Empty,
                            ApplyDate = DateTime.Now,
                            Progress = "學生送件",
                            PhoneNumber = requestDto.Phone_Number ?? string.Empty,
                            Certificate = null // JSON 請求不支持文件上傳
                        };

                        _logger.LogInformation($"JSON 解析成功: StudentId={leaveRequest.StudentId}, StartDate={leaveRequest.StartDate:yyyy-MM-dd}, EndDate={leaveRequest.EndDate:yyyy-MM-dd}");
                    }
                }

                // 最終驗證
                if (leaveRequest == null)
                {
                    _logger.LogError("無法解析請假資料");
                    return BadRequest(new { success = false, message = "無法解析請假資料，請確認請求格式正確" });
                }



                _logger.LogInformation($"解析後的請假資料: StudentId={leaveRequest.StudentId}, StartDate={leaveRequest.StartDate:yyyy-MM-dd}, EndDate={leaveRequest.EndDate:yyyy-MM-dd}");

                // 計算學年和學期
                var academicInfo = CalculateAcademicYearAndSemester(leaveRequest.StartDate);
                leaveRequest.AcademicYear = academicInfo.Item1;
                leaveRequest.Semester = academicInfo.Item2;

                _logger.LogInformation($"計算學年學期: {leaveRequest.AcademicYear}學年第{leaveRequest.Semester}學期");

                // 驗證學生存在
                var studentExists = await _context.Student.AnyAsync(s => s.StudentId == leaveRequest.StudentId);
                if (!studentExists)
                {
                    _logger.LogWarning($"學生ID不存在: {leaveRequest.StudentId}");
                    return BadRequest(new { success = false, message = "學生ID不存在" });
                }

                _logger.LogInformation("開始計算請假時數");
                // 計算實際的請假時數
                int actualHours = 0;
                try
                {
                    actualHours = await CalculateLeaveHours(leaveRequest);
                    _logger.LogInformation($"計算得出請假時數: {actualHours}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "計算請假時數時發生錯誤");
                    // 如果計算時數失敗，仍然繼續處理，但設為 0
                    actualHours = 0;
                }

                _logger.LogInformation("開始取得受影響的課程");
                // 取得所有受到影響的課程（在保存主記錄之前）
                List<LeaveRecord> affectedCourses = new List<LeaveRecord>();
                try
                {
                    affectedCourses = await GetAffectedCourses(leaveRequest);
                    _logger.LogInformation($"找到 {affectedCourses.Count} 個受影響的課程記錄");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "取得受影響課程時發生錯誤");
                    // 如果取得課程失敗，仍然繼續處理，但沒有課程記錄
                    affectedCourses = new List<LeaveRecord>();
                }

                // 使用交易確保資料一致性
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("保存主請假記錄");
                    // 保存主請假記錄（確保 CourseId 為 null）
                    leaveRequest.CourseId = null;
                    _context.LeaveRecord.Add(leaveRequest);
                    await _context.SaveChangesAsync(); // 先儲存以取得 LeaveId

                    var mainLeaveId = leaveRequest.LeaveId;
                    _logger.LogInformation($"主請假記錄已保存，LeaveId: {mainLeaveId}");

                    if (affectedCourses.Count > 0)
                    {
                        _logger.LogInformation("開始保存課程請假記錄");

                        // 為每個課程記錄創建新的 LeaveRecord，讓每個都有自己的 LeaveId
                        foreach (var course in affectedCourses)
                        {
                            var newCourseRecord = new LeaveRecord
                            {
                                CourseId = course.CourseId,
                                StudentId = course.StudentId,
                                StartDate = course.StartDate,
                                EndDate = course.EndDate,
                                StartPeriod = course.StartPeriod,
                                EndPeriod = course.EndPeriod,
                                AcademicYear = course.AcademicYear,
                                Semester = course.Semester,
                                LType = course.LType,
                                Reason = course.Reason,
                                ApplyDate = course.ApplyDate,
                                Progress = course.Progress,
                                PhoneNumber = course.PhoneNumber,
                                Certificate = null
                            };

                            _context.LeaveRecord.Add(newCourseRecord);
                        }

                        // 一次性保存所有課程記錄，提高效率
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"已保存 {affectedCourses.Count} 個課程請假記錄");
                    }

                    // 提交交易
                    await transaction.CommitAsync();
                    _logger.LogInformation("請假申請處理成功");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "保存記錄時發生錯誤，已回滾交易");
                    throw;
                }

                _logger.LogInformation("請假申請處理成功");
                // 在 API 響應中使用計算的實際時數
                return Ok(new
                {
                    success = true,
                    message = "請假申請提交成功",
                    leave_id = leaveRequest.LeaveId,
                    academic_year = leaveRequest.AcademicYear,
                    semester = leaveRequest.Semester,
                    hours = actualHours,
                    affected_courses_count = affectedCourses.Count
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "資料庫更新錯誤");
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;

                if (innerMessage.Contains("CHECK") || innerMessage.Contains("條件約束") || innerMessage.Contains("Progress"))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Progress 值無效。有效的狀態值為：學生送件/導師已送件/系教官已送件/流程結束"
                    });
                }

                return StatusCode(500, new
                {
                    success = false,
                    message = "數據庫錯誤",
                    detail = innerMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "處理請假申請時發生未預期的錯誤");
                return StatusCode(500, new
                {
                    success = false,
                    message = "內部服務器錯誤",
                    detail = ex.Message
                });
            }
        }

        // 修改後的查詢方法 - 統一處理假資料和真實請假資料
        [HttpGet("records")]
        public async Task<IActionResult> GetLeaveRecords(string student_id, int? academic_year = null, int? semester = null)
        {
            if (string.IsNullOrEmpty(student_id))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                // 先查詢學生資料以獲取入學日期
                var student = await _context.Student.FirstOrDefaultAsync(s => s.StudentId == student_id.Trim());
                if (student == null)
                {
                    return BadRequest("找不到該學生");
                }

                // 如果沒有提供學年學期，則獲取該學生入學後的所有學年學期
                if (!academic_year.HasValue && !semester.HasValue)
                {
                    return await GetAllSemesterRecords(student);
                }

                // 如果有指定學年學期，返回該學期的記錄
                if (academic_year.HasValue && semester.HasValue)
                {
                    return await GetSpecificSemesterRecords(student_id, academic_year.Value, semester.Value);
                }

                return BadRequest("必須同時提供學年和學期，或都不提供");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SQL錯誤或一般錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpGet("affectedcourses")]
        public async Task<IActionResult> GetAffectedCourses(string student_id, DateTime start_date, DateTime end_date, int start_period, int? end_period = null)
        {
            try
            {
                // 驗證輸入參數
                if (string.IsNullOrEmpty(student_id))
                {
                    return BadRequest("學號不能為空");
                }

                if (start_date == default(DateTime) || end_date == default(DateTime))
                {
                    return BadRequest("請假日期不能為空");
                }

                if (start_period <= 0)
                {
                    return BadRequest("起始節次必須大於0");
                }

                // 確認學生存在
                if (!await _context.Student.AnyAsync(s => s.StudentId == student_id.Trim()))
                {
                    return BadRequest("學生ID不存在");
                }

                // 建立模擬請假記錄
                var academicInfo = CalculateAcademicYearAndSemester(start_date);
                var leaveRecord = new LeaveRecord
                {
                    StudentId = student_id,
                    StartDate = start_date,
                    EndDate = end_date,
                    StartPeriod = start_period,
                    EndPeriod = end_period,
                    AcademicYear = academicInfo.Item1,
                    Semester = academicInfo.Item2
                };

                // 取得所有受到影響的課程
                var affectedCoursesDetails = await GetAffectedCoursesDetails(leaveRecord);

                // 整理返回的數據
                var coursesByDate = affectedCoursesDetails
                    .GroupBy(c => new { Date = c.Date, Weekday = c.Weekday })
                    .OrderBy(g => g.Key.Date)
                    .Select(g => new
                    {
                        date = g.Key.Date.ToString("yyyy/MM/dd"),
                        weekday = g.Key.Weekday,
                        courses = g.Select(c => new
                        {
                            course_id = c.CourseId,
                            course_name = c.CourseName,
                            periods = string.Join(" ", c.Periods)
                        }).ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "已取得該時段課程",
                    academic_year = academicInfo.Item1,
                    semester = academicInfo.Item2,
                    data = coursesByDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得課程時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        #region 私有方法

        private async Task<int> CalculateLeaveHours(LeaveRecord leaveRequest)
        {
            int totalHours = 0;

            try
            {
                _logger.LogDebug($"開始計算學生 {leaveRequest.StudentId} 從 {leaveRequest.StartDate:yyyy-MM-dd} 到 {leaveRequest.EndDate:yyyy-MM-dd} 的請假時數");
                _logger.LogDebug($"請假節次：從第 {leaveRequest.StartPeriod} 節到第 {leaveRequest.EndPeriod ?? 12} 節");

                // 取得學生在該學期修的所有課程
                var studentCourses = await _context.CourseRecord
                    .Where(cr => cr.StudentId == leaveRequest.StudentId.Trim() &&
                           cr.AcademicYear == leaveRequest.AcademicYear &&
                           cr.Semester == leaveRequest.Semester)
                    .Join(_context.Course, // 連接課程表以獲取課程時間
                          cr => cr.CourseId,
                          c => c.CourseId,
                          (cr, c) => c)
                    .Where(c => !string.IsNullOrEmpty(c.CTime))
                    .ToListAsync();

                _logger.LogDebug($"學生本學期共修 {studentCourses.Count} 門課");
                foreach (var course in studentCourses)
                {
                    _logger.LogDebug($"課程：{course.CourseId} {course.CName} - 上課時間：{course.CTime}");
                }

                if (studentCourses.Count == 0)
                {
                    _logger.LogDebug("警告：該學生本學期沒有任何課程或課程時間為空");
                    return 0;
                }

                // 請假起始和結束日期
                DateTime startDate = leaveRequest.StartDate.Date;
                DateTime endDate = leaveRequest.EndDate.Date;
                int startPeriod = leaveRequest.StartPeriod;
                int endPeriod = leaveRequest.EndPeriod ?? 12;

                // 建立一個日期範圍
                List<DateTime> leaveDates = new List<DateTime>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    leaveDates.Add(date);
                }

                _logger.LogDebug($"請假日期範圍：{string.Join(", ", leaveDates.Select(d => d.ToString("yyyy-MM-dd")))}");

                // 對每門課程計算請假時段
                foreach (var course in studentCourses)
                {
                    // 解析課程時間
                    var courseSchedule = ParseCourseTime(course.CTime);

                    if (courseSchedule.Count == 0)
                    {
                        _logger.LogDebug($"警告：無法解析課程 {course.CourseId} {course.CName} 的時間：{course.CTime}");
                        continue;
                    }

                    // 遍歷請假期間的每一天
                    foreach (var date in leaveDates)
                    {
                        if (!courseSchedule.TryGetValue(date.DayOfWeek, out List<int>? periods) || periods.Count == 0)
                        {
                            // 該天沒有這門課
                            continue;
                        }

                        // 確定當天的請假起始和結束節次
                        int dayStartPeriod, dayEndPeriod;

                        if (date == startDate && date == endDate)
                        {
                            // 如果請假只有一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = endPeriod;
                        }
                        else if (date == startDate)
                        {
                            // 請假的第一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = 12;
                        }
                        else if (date == endDate)
                        {
                            // 請假的最後一天
                            dayStartPeriod = 1;
                            dayEndPeriod = endPeriod;
                        }
                        else
                        {
                            // 請假的中間日期，全天請假
                            dayStartPeriod = 1;
                            dayEndPeriod = 12;
                        }

                        // 計算當天有多少節這門課在請假範圍內
                        int classesToday = periods.Count(p => p >= dayStartPeriod && p <= dayEndPeriod);

                        if (classesToday > 0)
                        {
                            totalHours += classesToday;
                            _logger.LogDebug($"日期 {date:yyyy-MM-dd} ({date.DayOfWeek}): 課程 {course.CourseId} {course.CName} 有 {classesToday} 節在請假範圍內");
                            _logger.LogDebug($"課程節次：{string.Join(",", periods.Where(p => p >= dayStartPeriod && p <= dayEndPeriod))}");
                        }
                    }
                }

                _logger.LogDebug($"最終計算結果：總請假時數 = {totalHours}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算請假時數時發生錯誤");
            }

            return totalHours;
        }

        //獲取請假期間影響的所有課程
        private async Task<List<LeaveRecord>> GetAffectedCourses(LeaveRecord leaveRequest)
        {
            var affectedCourses = new List<LeaveRecord>();
            var courseTracker = new Dictionary<string, Dictionary<DateTime, List<int>>>();

            try
            {
                // 取得學生在該學期修的所有課程
                var studentCourses = await _context.CourseRecord
                    .Where(cr => cr.StudentId == leaveRequest.StudentId.Trim() &&
                           cr.AcademicYear == leaveRequest.AcademicYear &&
                           cr.Semester == leaveRequest.Semester)
                    .Join(_context.Course, // 連接課程表以獲取課程時間
                          cr => cr.CourseId,
                          c => c.CourseId,
                          (cr, c) => c)
                    .Where(c => !string.IsNullOrEmpty(c.CTime))
                    .ToListAsync();

                // 請假起始和結束日期
                DateTime startDate = leaveRequest.StartDate.Date;
                DateTime endDate = leaveRequest.EndDate.Date;
                int startPeriod = leaveRequest.StartPeriod;
                int endPeriod = leaveRequest.EndPeriod ?? 12;

                // 建立一個日期範圍
                List<DateTime> leaveDates = new List<DateTime>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    leaveDates.Add(date);
                }

                // 對每門課程檢查請假時段，並收集所有受影響的節次
                foreach (var course in studentCourses)
                {
                    // 解析課程時間
                    var courseSchedule = ParseCourseTime(course.CTime);

                    if (courseSchedule.Count == 0)
                    {
                        continue;
                    }

                    // 遍歷請假期間的每一天
                    foreach (var date in leaveDates)
                    {
                        if (!courseSchedule.TryGetValue(date.DayOfWeek, out List<int>? periods) || periods.Count == 0)
                        {
                            // 該天沒有這門課
                            continue;
                        }

                        // 確定當天的請假起始和結束節次
                        int dayStartPeriod, dayEndPeriod;

                        if (date == startDate && date == endDate)
                        {
                            // 如果請假只有一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = endPeriod;
                        }
                        else if (date == startDate)
                        {
                            // 請假的第一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = 12;
                        }
                        else if (date == endDate)
                        {
                            // 請假的最後一天
                            dayStartPeriod = 1;
                            dayEndPeriod = endPeriod;
                        }
                        else
                        {
                            // 請假的中間日期，全天請假
                            dayStartPeriod = 1;
                            dayEndPeriod = 12;
                        }

                        // 檢查當天有哪些節次的這門課在請假範圍內
                        var affectedPeriods = periods.Where(p => p >= dayStartPeriod && p <= dayEndPeriod).ToList();

                        if (affectedPeriods.Any())
                        {
                            // 使用課程ID作為字典的key
                            string courseId = course.CourseId?.Trim() ?? string.Empty;

                            // 若該課程尚未在字典中，則新增
                            if (!courseTracker.ContainsKey(courseId))
                            {
                                courseTracker[courseId] = new Dictionary<DateTime, List<int>>();
                            }

                            // 若該日期尚未在該課程的字典中，則新增
                            if (!courseTracker[courseId].ContainsKey(date))
                            {
                                courseTracker[courseId][date] = new List<int>();
                            }

                            // 加入該日期該課程的所有受影響節次
                            courseTracker[courseId][date].AddRange(affectedPeriods);
                        }
                    }
                }

                // 根據收集的資訊建立 LeaveRecord 記錄
                foreach (var coursePair in courseTracker)
                {
                    string courseId = coursePair.Key;
                    var datePeriods = coursePair.Value;

                    foreach (var datePair in datePeriods)
                    {
                        DateTime date = datePair.Key;
                        List<int> periods = datePair.Value.Distinct().OrderBy(p => p).ToList();

                        if (periods.Count > 0)
                        {
                            // 為每個課程的每天創建一條記錄，包含最小開始節次和最大結束節次
                            var leaveRecord = new LeaveRecord
                            {
                                CourseId = courseId,
                                StudentId = leaveRequest.StudentId,
                                StartDate = date,
                                EndDate = date,
                                StartPeriod = periods.Min(), // 最小節次作為開始
                                EndPeriod = periods.Max(),   // 最大節次作為結束
                                AcademicYear = leaveRequest.AcademicYear,
                                Semester = leaveRequest.Semester,
                                LType = leaveRequest.LType,
                                Reason = leaveRequest.Reason,
                                ApplyDate = leaveRequest.ApplyDate,
                                Progress = "學生送件",
                                PhoneNumber = leaveRequest.PhoneNumber
                            };
                            affectedCourses.Add(leaveRecord);

                            _logger.LogDebug($"Added LeaveRecord: CourseId={courseId}, StudentId={leaveRequest.StudentId}, LeaveDate={date:yyyy-MM-dd}, Periods={string.Join(",", periods)}, StartPeriod={leaveRecord.StartPeriod}, EndPeriod={leaveRecord.EndPeriod}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得受影響課程時發生錯誤");
            }

            return affectedCourses;
        }

        // 解析課程時間字符串，返回每個星期各節次的映射
        private Dictionary<DayOfWeek, List<int>> ParseCourseTime(string? cTimeStr)
        {
            var result = new Dictionary<DayOfWeek, List<int>>();

            if (string.IsNullOrEmpty(cTimeStr))
            {
                _logger.LogDebug("警告：課程時間字串為空");
                return result;
            }

            try
            {
                // 星期對應表
                var dayMapping = new Dictionary<string, DayOfWeek>
                {
                    {"一", DayOfWeek.Monday},
                    {"二", DayOfWeek.Tuesday},
                    {"三", DayOfWeek.Wednesday},
                    {"四", DayOfWeek.Thursday},
                    {"五", DayOfWeek.Friday},
                    {"六", DayOfWeek.Saturday},
                    {"日", DayOfWeek.Sunday}
                };

                // 使用正則表達式解析課程時間格式
                string pattern = @"\(([一二三四五六日])\)([0-9]+)";
                var matches = Regex.Matches(cTimeStr, pattern);

                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        string day = match.Groups[1].Value;
                        string periodsStr = match.Groups[2].Value;

                        if (dayMapping.ContainsKey(day))
                        {
                            DayOfWeek dayOfWeek = dayMapping[day];

                            if (!result.ContainsKey(dayOfWeek))
                            {
                                result[dayOfWeek] = new List<int>();
                            }

                            // 解析每個節次
                            foreach (char c in periodsStr)
                            {
                                if (char.IsDigit(c))
                                {
                                    int period = int.Parse(c.ToString());
                                    if (!result[dayOfWeek].Contains(period))
                                    {
                                        result[dayOfWeek].Add(period);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogDebug($"無法使用正則表達式解析課程時間：{cTimeStr}，嘗試備用方法");

                    // 備用解析方法
                    string? currentDay = null;
                    bool inParentheses = false;

                    for (int i = 0; i < cTimeStr.Length; i++)
                    {
                        char c = cTimeStr[i];

                        if (c == '(')
                        {
                            inParentheses = true;
                        }
                        else if (c == ')')
                        {
                            inParentheses = false;
                        }
                        else if (inParentheses && dayMapping.ContainsKey(c.ToString()))
                        {
                            currentDay = c.ToString();
                        }
                        else if (!inParentheses && currentDay != null && char.IsDigit(c))
                        {
                            DayOfWeek dayOfWeek = dayMapping[currentDay];

                            if (!result.ContainsKey(dayOfWeek))
                            {
                                result[dayOfWeek] = new List<int>();
                            }

                            int period = int.Parse(c.ToString());
                            if (!result[dayOfWeek].Contains(period))
                            {
                                result[dayOfWeek].Add(period);
                            }
                        }
                    }
                }

                // 輸出解析結果
                foreach (var entry in result)
                {
                    _logger.LogDebug($"解析結果：星期{GetChineseDayOfWeek(entry.Key)} - 節次：{string.Join(",", entry.Value)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析課程時間時發生錯誤");
            }

            return result;
        }

        // 將 DayOfWeek 轉換為中文
        private string GetChineseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "一",
                DayOfWeek.Tuesday => "二",
                DayOfWeek.Wednesday => "三",
                DayOfWeek.Thursday => "四",
                DayOfWeek.Friday => "五",
                DayOfWeek.Saturday => "六",
                DayOfWeek.Sunday => "日",
                _ => dayOfWeek.ToString()
            };
        }

        /// <summary>
        /// 根據日期計算學年和學期
        /// </summary>
        /// <param name="date">要計算的日期</param>
        /// <returns>Tuple(學年, 學期)</returns>
        private Tuple<int, int> CalculateAcademicYearAndSemester(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int academicYear;
            int semester;

            if (month >= 1 && month <= 6)
            {
                academicYear = year - 1912;
            }
            else
            {
                academicYear = year - 1911;
            }

            if (month >= 2 && month <= 8)
            {
                semester = 2;
            }
            else
            {
                semester = 1;
            }

            return new Tuple<int, int>(academicYear, semester);
        }

        /// <summary>
        /// 獲取學生入學後所有學年學期的記錄 - 修正重複資料問題，只顯示主記錄
        /// </summary>
        private async Task<IActionResult> GetAllSemesterRecords(Student student)
        {
            try
            {
                // 檢查入學日期
                if (!student.DateOfEnrollment.HasValue)
                {
                    return BadRequest("該學生沒有入學日期記錄");
                }

                // 計算入學年（轉換為民國年）
                DateTime enrollmentDate = student.DateOfEnrollment.Value;
                int enrollmentYear = enrollmentDate.Year - 1911;

                int enrollmentSemester = enrollmentDate.Month >= 9 ? 1 : 2;
                int enrollmentAcademicYear = enrollmentDate.Month >= 9 ? enrollmentYear : enrollmentYear - 1;

                // 獲取當前年份，假設最多查詢到當前學年（轉換為民國年）
                int currentYear = DateTime.Now.Year - 1911;
                int currentAcademicYear = DateTime.Now.Month >= 9 ? currentYear : currentYear - 1;

                // 生成所有學年學期組合
                var allSemesters = new List<object>();

                for (int year = enrollmentAcademicYear; year <= currentAcademicYear; year++)
                {
                    for (int sem = 1; sem <= 2; sem++)
                    {
                        // 如果是入學當年，需要檢查是否從入學學期開始
                        if (year == enrollmentAcademicYear && sem < enrollmentSemester)
                        {
                            continue;
                        }

                        allSemesters.Add(new
                        {
                            academic_year = year,
                            semester = sem
                        });
                    }
                }

                // 只獲取該學生的主請假記錄（CourseId為null的記錄）
                var mainLeaveRecords = await _context.LeaveRecord
                    .Where(l => l.StudentId == student.StudentId.Trim() &&
                               (l.CourseId == null || l.CourseId == string.Empty))
                    .ToListAsync();

                // 按學年學期分組
                var mainRecordsBySemester = mainLeaveRecords
                    .GroupBy(l => new { l.AcademicYear, l.Semester })
                    .ToDictionary(g => g.Key, g => g.ToList());

                var result = new List<object>();

                foreach (var semesterInfo in allSemesters)
                {
                    var semKey = new
                    {
                        AcademicYear = (int)semesterInfo.GetType().GetProperty("academic_year")!.GetValue(semesterInfo)!,
                        Semester = (int)semesterInfo.GetType().GetProperty("semester")!.GetValue(semesterInfo)!
                    };

                    var simplifiedRecords = new List<object>();

                    // 只處理主記錄
                    if (mainRecordsBySemester.ContainsKey(semKey))
                    {
                        var leaveRecords = mainRecordsBySemester[semKey];
                        foreach (var record in leaveRecords)
                        {
                            // 計算請假時數
                            int hours = await CalculateLeaveHours(record);

                            simplifiedRecords.Add(new
                            {
                                leave_id = record.LeaveId,
                                start_date = record.StartDate.ToString("yyyy/MM/dd"),
                                end_date = record.EndDate.ToString("yyyy/MM/dd"),
                                hours = hours,
                                type = record.LType?.Trim() ?? string.Empty
                            });
                        }
                    }

                    result.Add(new
                    {
                        academic_year = semKey.AcademicYear,
                        semester = semKey.Semester,
                        has_records = simplifiedRecords.Any(),
                        data = simplifiedRecords
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "獲取學生所有學期請假記錄成功",
                    enrollment_date = enrollmentDate.ToString("yyyy/MM/dd"),
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取所有學期記錄錯誤");
                return StatusCode(500, new { message = "獲取所有學期記錄錯誤" });
            }
        }

        /// <summary>
        /// 獲取特定學期的記錄 - 修正重複資料問題，只顯示主記錄
        /// </summary>
        private async Task<IActionResult> GetSpecificSemesterRecords(string student_id, int academic_year, int semester)
        {
            try
            {
                // 只獲取該學期的主記錄（CourseId為null的記錄）
                var mainRecords = await _context.LeaveRecord
                    .Where(l => l.StudentId == student_id.Trim() &&
                               l.AcademicYear == academic_year &&
                               l.Semester == semester &&
                               (l.CourseId == null || l.CourseId == string.Empty))
                    .ToListAsync();

                var result = new List<object>();

                // 處理主記錄
                foreach (var mainRecord in mainRecords)
                {
                    // 計算實際請假時數
                    int actualHours = await CalculateLeaveHours(mainRecord);

                    result.Add(new
                    {
                        leave_id = mainRecord.LeaveId,
                        start_date = mainRecord.StartDate.ToString("yyyy/MM/dd"),
                        end_date = mainRecord.EndDate.ToString("yyyy/MM/dd"),
                        hours = actualHours,
                        type = mainRecord.LType?.Trim() ?? string.Empty
                    });
                }

                return Ok(new
                {
                    success = true,
                    academic_year = academic_year,
                    semester = semester,
                    has_records = result.Any(),
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取特定學期記錄錯誤");
                return StatusCode(500, new { message = "獲取特定學期記錄錯誤" });
            }
        }

        // 獲取請假期間影響的所有課程詳細資訊
        private async Task<List<dynamic>> GetAffectedCoursesDetails(LeaveRecord leaveRequest)
        {
            var affectedCourses = new List<dynamic>();

            try
            {
                // 取得學生在該學期修的所有課程
                var studentCourses = await _context.CourseRecord
                    .Where(cr => cr.StudentId == leaveRequest.StudentId.Trim() &&
                           cr.AcademicYear == leaveRequest.AcademicYear &&
                           cr.Semester == leaveRequest.Semester)
                    .Join(_context.Course,
                          cr => cr.CourseId,
                          c => c.CourseId,
                          (cr, c) => c)
                    .Where(c => !string.IsNullOrEmpty(c.CTime))
                    .ToListAsync();

                // 請假起始和結束日期
                DateTime startDate = leaveRequest.StartDate.Date;
                DateTime endDate = leaveRequest.EndDate.Date;
                int startPeriod = leaveRequest.StartPeriod;
                int endPeriod = leaveRequest.EndPeriod ?? 12;

                // 建立一個日期範圍
                List<DateTime> leaveDates = new List<DateTime>();
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    leaveDates.Add(date);
                }

                // 對每門課程檢查請假時段
                foreach (var course in studentCourses)
                {
                    // 解析課程時間
                    var courseSchedule = ParseCourseTime(course.CTime);

                    if (courseSchedule.Count == 0)
                    {
                        continue;
                    }

                    // 遍歷請假期間的每一天
                    foreach (var date in leaveDates)
                    {
                        if (!courseSchedule.TryGetValue(date.DayOfWeek, out List<int>? periods) || periods.Count == 0)
                        {
                            // 該天沒有這門課
                            continue;
                        }

                        // 確定當天的請假起始和結束節次
                        int dayStartPeriod, dayEndPeriod;

                        if (date == startDate && date == endDate)
                        {
                            // 如果請假只有一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = endPeriod;
                        }
                        else if (date == startDate)
                        {
                            // 請假的第一天
                            dayStartPeriod = startPeriod;
                            dayEndPeriod = 12;
                        }
                        else if (date == endDate)
                        {
                            // 請假的最後一天
                            dayStartPeriod = 1;
                            dayEndPeriod = endPeriod;
                        }
                        else
                        {
                            // 請假的中間日期，全天請假
                            dayStartPeriod = 1;
                            dayEndPeriod = 12;
                        }

                        // 檢查當天有哪些節次的這門課在請假範圍內
                        var affectedPeriods = periods.Where(p => p >= dayStartPeriod && p <= dayEndPeriod).ToList();

                        if (affectedPeriods.Any())
                        {
                            // 使用匿名類型直接存儲課程信息
                            affectedCourses.Add(new
                            {
                                Date = date,
                                Weekday = GetChineseDayOfWeek(date.DayOfWeek),
                                CourseId = course.CourseId?.Trim() ?? string.Empty,
                                CourseName = course.CName?.Trim() ?? "未知課程",
                                Periods = affectedPeriods
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得課程詳細資訊時發生錯誤");
            }

            return affectedCourses;
        }

        #endregion

        public class LeaveRequestDto
        {
            [JsonPropertyName("Student_Id")]
            public string Student_Id { get; set; } = string.Empty;

            [JsonPropertyName("Start_Date")]
            public string Start_Date { get; set; } = string.Empty;

            [JsonPropertyName("End_Date")]
            public string End_Date { get; set; } = string.Empty;

            [JsonPropertyName("Start_Period")]
            public int Start_Period { get; set; }

            [JsonPropertyName("End_Period")]
            public int? End_Period { get; set; }

            [JsonPropertyName("LType")]
            public string LType { get; set; } = string.Empty;

            [JsonPropertyName("Reason")]
            public string? Reason { get; set; }

            [JsonPropertyName("Phone_Number")]
            public string? Phone_Number { get; set; }

            // 注意：JSON 不支持文件上傳，所以這個字段在 JSON 請求中不會使用
            public IFormFile? Certificate { get; set; }
        }
    }
}