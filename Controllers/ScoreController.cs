using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eCHU.Models;

namespace eCHU.Controllers
{
    [ApiController]
    [Route("")]
    public class ScoreController : ControllerBase
    {
        private readonly WebAPIContext _context;
        private readonly ILogger<ScoreController> _logger;

        public ScoreController(WebAPIContext context, ILogger<ScoreController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("score")]
        public async Task<IActionResult> ScoreGet(string? academic_year = null, string? semester = null, string? student_id = null)
        {
            if (string.IsNullOrEmpty(student_id))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var query = _context.CourseRecord
                    .Where(cr => cr.StudentId == student_id);

                if (!string.IsNullOrEmpty(academic_year))
                {
                    if (!int.TryParse(academic_year, out int academicYearInt))
                    {
                        return BadRequest("學年格式不正確");
                    }
                    query = query.Where(cr => cr.AcademicYear == academicYearInt);

                    if (!string.IsNullOrEmpty(semester))
                    {
                        if (!int.TryParse(semester, out int semesterInt))
                        {
                            return BadRequest("學期格式不正確");
                        }
                        query = query.Where(cr => cr.Semester == semesterInt);
                    }
                }

                var courseRecords = await query
                    .Include(cr => cr.Course)
                    .Include(cr => cr.Student)
                    .ToListAsync();

                if (!courseRecords.Any())
                {
                    return Ok(new { success = false, message = "找不到該學生的成績記錄" });
                }

                var groupedRecords = courseRecords
                    .GroupBy(cr => new { cr.AcademicYear, cr.Semester })
                    .Select(group =>
                    {
                        var gradeRecord = _context.GradeRecord
                            .FirstOrDefault(gr =>
                                gr.AcademicYear == group.Key.AcademicYear &&
                                gr.Semester == group.Key.Semester &&
                                gr.StudentId == student_id);

                        decimal totalCredits = group
                            .Where(cr => cr.Course.Credit.HasValue)
                            .Sum(cr => (decimal)cr.Course.Credit.Value);

                        return new
                        {
                            academic_year = group.Key.AcademicYear,
                            semester = group.Key.Semester,
                            student_id = student_id,
                            student_name = group.First().Student.SName?.Trim(),
                            average_score = gradeRecord?.AverageScore,
                            total_credits = totalCredits,
                            earned_credits = gradeRecord?.EarnedCredits ?? 0,
                            conduct_grade = gradeRecord?.ConductGrade,
                            course_records = group.Select(cr => new
                            {
                                course_name = cr.Course.CName?.Trim(),
                                grade = cr.Grade,
                                credit = cr.Course.Credit,
                                ctype = cr.Course.CType?.Trim(),
                            }).ToList()
                        };
                    })
                    .OrderByDescending(r => r.academic_year)
                    .ThenByDescending(r => r.semester)
                    .ToList();

                var result = new
                {
                    success = true,
                    message = "找到對應的成績",
                    data = groupedRecords
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得成績資料時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpGet("BasicCompetenceIndicators")]
        public async Task<IActionResult> BasicCompetenceIndicatorsGet(string student_id)
        {
            if (string.IsNullOrEmpty(student_id))
            {
                return BadRequest("Student ID cannot be empty.");
            }

            try
            {
                var student = await _context.Student
                    .FirstOrDefaultAsync(s => s.StudentId == student_id);

                if (student == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生"
                    });
                }

                var graduationSkills = await _context.GraduationSkill
                    .Where(gs => gs.StudentId == student_id)
                    .ToListAsync();

                var graduationSkillsResult = graduationSkills.Select(gs => new
                {
                    skill = TransformSkillName(gs.Skill?.Trim()),
                    pass = gs.Pass
                }).ToList();

                var volunteerData = await _context.GraduationVolunteer
                    .Where(v => v.StudentId == student_id)
                    .ToListAsync();

                var volunteerDataResult = volunteerData.Select(v => new
                {
                    volunteer_type = TransformVolunteerType(v.VolunteerType?.Trim()),
                    hours = v.Hours,
                    pass = EvaluateVolunteerPass(v.VolunteerType, v.Hours)
                }).ToList();

                if (!graduationSkillsResult.Any() && !volunteerDataResult.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生的基本能力指標"
                    });
                }

                var loveSchoolHours = volunteerDataResult.Where(v => v.volunteer_type == "愛校學習課程").Sum(v => v.hours ?? 0);
                var loveDeptHours = volunteerDataResult.Where(v => v.volunteer_type == "愛系服務").Sum(v => v.hours ?? 0);
                var serviceHours = volunteerDataResult.Where(v => v.volunteer_type == "服務學習活動").Sum(v => v.hours ?? 0);

                bool overallPass = loveSchoolHours + loveDeptHours + serviceHours >= 18;

                var result = new
                {
                    success = true,
                    message = "成功取得基本能力指標",
                    student_id = student.StudentId.Trim(),
                    student_name = student.SName?.Trim(),
                    graduation_skills = graduationSkillsResult,
                    volunteer_data = volunteerDataResult,
                    overall_pass = overallPass
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得基本能力指標時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        private string? TransformVolunteerType(string? volunteerType)
        {
            if (string.IsNullOrEmpty(volunteerType))
                return volunteerType;

            return volunteerType.Trim() switch
            {
                "愛校" => "愛校學習課程",
                "愛系" => "愛系服務",
                "志工訓練課程" => "服務學習活動",
                _ => volunteerType
            };
        }

        private string? TransformSkillName(string? skillName)
        {
            if (string.IsNullOrEmpty(skillName))
                return skillName;

            return skillName.Trim() switch
            {
                "創新創意能力課程" => "課程",
                "創新創意能力競賽" => "競賽",
                _ => skillName
            };
        }

        private bool EvaluateVolunteerPass(string? volunteerType, int? hours)
        {
            if (string.IsNullOrEmpty(volunteerType) || !hours.HasValue)
                return false;

            return volunteerType.Trim() switch
            {
                "愛校" => hours >= 6,
                "愛系" or "志工訓練課程" => hours >= 2,
                _ => false
            };
        }

        [HttpGet("CommonRequiredGeneralEducation")]
        public async Task<IActionResult> CommonRequiredGeneralEducationGet(string student_id)
        {
            if (string.IsNullOrEmpty(student_id))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var student = await _context.Student
                    .FirstOrDefaultAsync(s => s.StudentId == student_id);

                if (student == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生"
                    });
                }

                // 取得學生課程記錄
                var allStudentCourseRecords = await _context.CourseRecord
                    .Where(cr => cr.StudentId == student_id)
                    .Join(_context.Course,
                        cr => cr.CourseId,
                        c => c.CourseId,
                        (cr, c) => new
                        {
                            CourseId = c.CourseId,
                            CourseName = c.CName,
                            CourseType = c.CType,
                            GeneralCourseType = c.GCType,
                            Credit = c.Credit ?? 0,
                            PassStatus = cr.Pass,
                            IsPass = cr.Pass == "Y" || cr.Pass == "Yes",
                            AcademicYear = cr.AcademicYear,
                            Semester = cr.Semester
                        })
                    .ToListAsync();

                // 處理字串修剪
                var processedRecords = allStudentCourseRecords.Select(c => new
                {
                    c.CourseId,
                    CourseName = c.CourseName?.Trim() ?? string.Empty,
                    CourseType = c.CourseType?.Trim() ?? string.Empty,
                    GeneralCourseType = c.GeneralCourseType?.Trim() ?? string.Empty,
                    c.Credit,
                    c.PassStatus,
                    c.IsPass,
                    c.AcademicYear,
                    c.Semester
                }).ToList();

                // 計算英文課程
                var englishCourses = processedRecords
                    .Where(c => c.GeneralCourseType == "英文" && c.CourseType == "必" && c.IsPass)
                    .ToList();

                var distinctEnglishLevels = new HashSet<string>();
                var validEnglishCourses = new List<object>();
                int totalEnglishCredits = 0;

                foreach (var course in englishCourses)
                {
                    string courseName = course.CourseName;
                    string? englishLevel = null;

                    if (courseName.Contains("英文一"))
                        englishLevel = "英文一";
                    else if (courseName.Contains("英文二"))
                        englishLevel = "英文二";
                    else if (courseName.Contains("英文三"))
                        englishLevel = "英文三";
                    else if (courseName.Contains("英文四"))
                        englishLevel = "英文四";

                    if (englishLevel != null && !distinctEnglishLevels.Contains(englishLevel))
                    {
                        distinctEnglishLevels.Add(englishLevel);
                        totalEnglishCredits += course.Credit;
                        validEnglishCourses.Add(new
                        {
                            course_name = course.CourseName,
                            level = englishLevel,
                            credit = course.Credit,
                            academic_year = course.AcademicYear,
                            semester = course.Semester
                        });
                    }
                }

                bool englishRequirementMet = totalEnglishCredits >= 6;

                // 檢查體育課程
                var peCourses = processedRecords
                    .Where(c => c.GeneralCourseType == "體育" && c.CourseType == "必")
                    .ToList();

                var pe1Courses = peCourses
                    .Where(c => c.CourseName.Contains("體育（一）") || c.CourseName.Contains("體育(一)"))
                    .OrderByDescending(c => c.AcademicYear)
                    .ThenByDescending(c => c.Semester)
                    .ToList();

                var pe2Courses = peCourses
                    .Where(c => c.CourseName.Contains("體育（二）") || c.CourseName.Contains("體育(二)"))
                    .OrderByDescending(c => c.AcademicYear)
                    .ThenByDescending(c => c.Semester)
                    .ToList();

                bool pe1Passed = pe1Courses.Count > 0 && pe1Courses.First().IsPass;
                bool pe2Passed = pe2Courses.Count > 0 && pe2Courses.First().IsPass;
                bool peRequirementMet = pe1Passed && pe2Passed;

                var graduationCommons = await _context.GraduationCommon
                    .Where(gc => gc.StudentId == student_id)
                    .ToListAsync();

                if (!graduationCommons.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生的通識畢業門檻資訊"
                    });
                }

                int pePassedCount = 0;
                if (pe1Passed) pePassedCount++;
                if (pe2Passed) pePassedCount++;

                var updatedGraduationCommons = new List<object>();

                foreach (var gc in graduationCommons)
                {
                    string gname = gc.GName.Trim();

                    if (gname == "英文")
                    {
                        updatedGraduationCommons.Add(new
                        {
                            gname = gname,
                            credit = totalEnglishCredits,
                            pass = englishRequirementMet
                        });
                    }
                    else if (gname == "體育")
                    {
                        updatedGraduationCommons.Add(new
                        {
                            gname = gname,
                            credit = pePassedCount,
                            pass = peRequirementMet
                        });
                    }
                    else
                    {
                        updatedGraduationCommons.Add(new
                        {
                            gname = gname,
                            credit = gc.Credit,
                            pass = gc.Pass
                        });
                    }
                }

                var result = new
                {
                    success = true,
                    message = "成功取得通識畢業門檻資訊",
                    student_id = student.StudentId.Trim(),
                    student_name = student.SName?.Trim() ?? string.Empty,
                    graduation_commons = updatedGraduationCommons,
                    Required_elective_credit = 10
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得通識畢業門檻資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpGet("RequiredCourses")]
        public async Task<IActionResult> GetRequiredCourses(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var student = await _context.Student.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生資訊"
                    });
                }

                var department = await _context.Department.FirstOrDefaultAsync(d => d.DeptId == student.DeptId);
                int requiredCredits = department?.RequiredCredits ?? 0;

                var requiredCourses = await (from cr in _context.CourseRecord
                                             join c in _context.Course on cr.CourseId equals c.CourseId
                                             where cr.StudentId == studentId && c.CType == "必"
                                             select new
                                             {
                                                 CourseName = c.CName!.Trim(),
                                                 AcademicYear = cr.AcademicYear,
                                                 Semester = cr.Semester,
                                                 Pass = cr.Pass!.Trim(),
                                                 Credit = c.Credit ?? 0
                                             })
                                        .OrderBy(x => x.AcademicYear)
                                        .ThenBy(x => x.Semester)
                                        .ToListAsync();

                if (!requiredCourses.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生的必修課程資訊"
                    });
                }

                int completedRequiredCredits = requiredCourses
                    .Where(c => c.Pass == "Yes")
                    .Sum(c => c.Credit);

                return Ok(new
                {
                    success = true,
                    message = "成功取得必修課程資訊",
                    data = requiredCourses,
                    totalRequiredCredits = requiredCredits,
                    completedRequiredCredits = completedRequiredCredits
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得必修課程資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpGet("ElectiveCourses")]
        public async Task<IActionResult> GetElectiveCourses(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                var student = await _context.Student
                    .Include(s => s.Department)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生資訊"
                    });
                }

                string? departmentId = student.DeptId?.Trim();
                _logger.LogInformation($"學生科系ID: '{departmentId}'");

                int requiredElectiveCredits = student.Department?.ElectiveCredits ?? 0;

                var electiveCourses = await (from cr in _context.CourseRecord
                                             join c in _context.Course on cr.CourseId equals c.CourseId
                                             where cr.StudentId == studentId && c.CType == "選"
                                             select new
                                             {
                                                 CourseName = c.CName!.Trim(),
                                                 AcademicYear = cr.AcademicYear,
                                                 Semester = cr.Semester,
                                                 Credits = c.Credit ?? 0,
                                                 Pass = cr.Pass!.Trim()
                                             })
                                         .ToListAsync();

                _logger.LogInformation($"找到 {electiveCourses.Count} 個選修課程記錄");

                if (!electiveCourses.Any())
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生的選修課程資訊"
                    });
                }

                var totalPassedCredits = electiveCourses
                    .Where(c => c.Pass.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                    .Sum(c => c.Credits);

                _logger.LogInformation($"已通過選修課程總學分: {totalPassedCredits}");

                var orderedCourses = electiveCourses
                    .OrderBy(x => x.AcademicYear)
                    .ThenBy(x => x.Semester)
                    .Select(c => new
                    {
                        CourseName = c.CourseName,
                        AcademicYear = c.AcademicYear,
                        Semester = c.Semester,
                        Credits = c.Credits,
                        Pass = c.Pass
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    message = "成功取得選修課程資訊",
                    data = new
                    {
                        Courses = orderedCourses,
                        TotalElectiveCredits = totalPassedCredits,
                        RequiredElectiveCredits = requiredElectiveCredits
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得選修課程資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        [HttpGet("MidtermAlert")]
        public async Task<IActionResult> GetMidtermAlert(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                return BadRequest("學號不能為空");
            }

            try
            {
                DetermineCurrentAcademicYearAndSemester(out int academicYear, out int semester);

                var student = await _context.Student.FirstOrDefaultAsync(s => s.StudentId == studentId);
                if (student == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "找不到該學生"
                    });
                }

                string departmentName = "未知";
                if (!string.IsNullOrEmpty(student.DeptId))
                {
                    var department = await _context.Department
                        .FirstOrDefaultAsync(d => d.DeptId!.Trim() == student.DeptId.Trim());
                    departmentName = department?.DeptName?.Trim() ?? "未知";
                }

                var alerts = await (from a in _context.Alert
                                    join c in _context.Course on a.CourseId equals c.CourseId
                                    join i in _context.Instructor on c.InstructorId equals i.InstructorId
                                    where a.StudentId == studentId
                                    && a.AcademicYear == academicYear
                                    && a.Semester == semester
                                    && a.Alert1 == "1"
                                    select new
                                    {
                                        Course_Id = c.CourseId.Trim(),
                                        CourseName = c.CName.Trim(),
                                        Credits = c.Credit ?? 0,
                                        IName = i.IName.Trim(),
                                        Reason = a.Reason == "1" ? "期中成績不及格" :
                                                a.Reason == "2" ? "出席率過低" : null,
                                        Current_Status = a.CurrentStatus != null ? a.CurrentStatus.Trim() : null
                                    })
                                .ToListAsync();

                int alertedCourseCount = alerts.Count;

                if (alertedCourseCount > 0)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "成功取得期中預警資訊",
                        academicYear = academicYear,
                        semester = semester,
                        Student_Id = studentId.Trim(),
                        SName = student.SName?.Trim(),
                        Department = departmentName,
                        alertedCourseCount = alertedCourseCount,
                        data = alerts
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = true,
                        message = "該學生本學期沒有預警課程",
                        academicYear = academicYear,
                        semester = semester,
                        Student_Id = studentId.Trim(),
                        SName = student.SName?.Trim(),
                        Department = departmentName,
                        alertedCourseCount = 0
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得期中預警資訊時發生錯誤");
                return StatusCode(500, new { message = "內部服務器錯誤" });
            }
        }

        private void DetermineCurrentAcademicYearAndSemester(out int academicYear, out int semester)
        {
            DateTime currentDate = DateTime.Now;
            int currentMonth = currentDate.Month;
            int westernYear = currentDate.Year;
            int rocYear = westernYear - 1911;

            if (currentMonth >= 2 && currentMonth <= 7)
            {
                semester = 2;
                academicYear = rocYear - 1;
            }
            else
            {
                semester = 1;
                academicYear = currentMonth == 1 ? rocYear - 1 : rocYear;
            }
        }
    }
}