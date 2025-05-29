namespace eCHU.Models
{
    public partial class Alert
    {
        public string StudentId { get; set; } = null!;
        public string CourseId { get; set; } = null!;
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public string? Alert1 { get; set; }
        public string? Reason { get; set; }
        public string? CurrentStatus { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }

    public partial class BuildingDescription
    {
        public string BuildingId { get; set; } = null!;
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public partial class Course
    {
        public Course()
        {
            Alert = new HashSet<Alert>();
            CourseRecord = new HashSet<CourseRecord>();
            LeaveRecord = new HashSet<LeaveRecord>();
        }

        public string CourseId { get; set; } = null!;
        public string? CName { get; set; }
        public string? CLanguage { get; set; }
        public string? CType { get; set; }
        public string? GCType { get; set; }
        public int? Credit { get; set; }
        public int? CHours { get; set; }
        public string? CTime { get; set; }
        public string? Classroom { get; set; }
        public string? InstructorId { get; set; }
        public int? CourseNumberOfPeople { get; set; }
        public int? AcademicYear { get; set; }
        public int? Semester { get; set; }
        public string? Note { get; set; }
        public string? DeptId { get; set; }

        public virtual ICollection<Alert> Alert { get; set; }
        public virtual Department? Department { get; set; }
        public virtual Instructor? Instructor { get; set; }
        public virtual ICollection<CourseRecord> CourseRecord { get; set; }
        public virtual ICollection<LeaveRecord> LeaveRecord { get; set; }
    }

    public partial class CourseRecord
    {
        public string StudentId { get; set; } = null!;
        public string CourseId { get; set; } = null!;
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public double? Grade { get; set; }
        public string? Pass { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }

    public partial class Department
    {
        public Department()
        {
            Course = new HashSet<Course>();
            Instructor = new HashSet<Instructor>();
            Student = new HashSet<Student>();
        }

        public string DeptId { get; set; } = null!;
        public string? DeptName { get; set; }
        public int? ElectiveCredits { get; set; }
        public int RequiredCredits { get; set; }

        public virtual ICollection<Course> Course { get; set; }
        public virtual ICollection<Instructor> Instructor { get; set; }
        public virtual ICollection<Student> Student { get; set; }
    }

    public partial class DepartmentRequirement
    {
        public string DeptId { get; set; } = null!;
        public string? DeptName { get; set; }
        public int? RequiredMajorElectiveCredit { get; set; }
    }

    public partial class GradeRecord
    {
        public string StudentId { get; set; } = null!;
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public double? AverageScore { get; set; }
        public double? ConductGrade { get; set; }
        public int? EarnedCredits { get; set; }

        public virtual Student Student { get; set; } = null!;
    }

    public partial class GraduationCommon
    {
        public string StudentId { get; set; } = null!;
        public string GName { get; set; } = null!;
        public int? Credit { get; set; }
        public bool? Pass { get; set; }

        public virtual Student Student { get; set; } = null!;
    }

    public partial class GraduationCredit
    {
        public string StudentId { get; set; } = null!;
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public int? Credit { get; set; }

        public virtual Student Student { get; set; } = null!;
    }

    public partial class GraduationSkill
    {
        public string StudentId { get; set; } = null!;
        public bool? Pass { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int SkillCode { get; set; }
        public string? Skill { get; set; }

        public virtual Student Student { get; set; } = null!;
    }

    public partial class GraduationVolunteer
    {
        public int VolunteerId { get; set; }
        public string StudentId { get; set; } = null!;
        public int? Hours { get; set; }
        public DateTime? Date { get; set; }
        public int? AcademicYear { get; set; }
        public int? Semester { get; set; }
        public int VolunteerCode { get; set; }
        public string? VolunteerType { get; set; }

        public virtual Student Student { get; set; } = null!;
    }

    public partial class Instructor
    {
        public Instructor()
        {
            Course = new HashSet<Course>();
            Student = new HashSet<Student>();
        }

        public string InstructorId { get; set; } = null!;
        public string? IName { get; set; }
        public string? IEmail { get; set; }
        public string? DeptId { get; set; }

        public virtual ICollection<Course> Course { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<Student> Student { get; set; }
    }

    public partial class LeaveRecord
    {
        public int LeaveId { get; set; }
        public string StudentId { get; set; } = null!;
        public string? CourseId { get; set; }
        public DateTime ApplyDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int StartPeriod { get; set; }
        public int? EndPeriod { get; set; }
        public string? PhoneNumber { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public string? Reason { get; set; }
        public int? Hours { get; set; }
        public string? LType { get; set; }
        public string? Progress { get; set; }
        public byte[]? Certificate { get; set; }

        public virtual Course? Course { get; set; }
        public virtual Student Student { get; set; } = null!;
    }

    public partial class PhoneExtension
    {
        public int ExtensionId { get; set; }
        public string? OrgName { get; set; }
        public string? SubunitName { get; set; }
        public string? Description { get; set; }
        public string? Extension { get; set; }
    }

    public partial class SAActivity
    {
        public SAActivity()
        {
            SAActivityRecord = new HashSet<SAActivityRecord>();
        }

        public int ActivityId { get; set; }
        public int AcademicYear { get; set; }
        public int Semester { get; set; }
        public string? AName { get; set; }
        public string? Location { get; set; }
        public DateTime? Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Outline { get; set; }
        public string? Remarks { get; set; }
        public int? Vacancy { get; set; }
        public int? NumberOfApplicants { get; set; }
        public string? Organizer { get; set; }
        public string? Fee { get; set; }
        public string? Meal { get; set; }
        public string? Status { get; set; }

        public virtual ICollection<SAActivityRecord> SAActivityRecord { get; set; }
    }

    public partial class SAActivityRecord
    {
        public int RegistrationId { get; set; }
        public string StudentId { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? RegistrationTime { get; set; }
        public string? Status { get; set; }
        public string? EmailStatus { get; set; }
        public int? ActivityId { get; set; }

        public virtual SAActivity? SAActivity { get; set; }
        public virtual Student Student { get; set; } = null!;
    }

    public partial class Student
    {
        public Student()
        {
            Alert = new HashSet<Alert>();
            CourseRecord = new HashSet<CourseRecord>();
            GradeRecord = new HashSet<GradeRecord>();
            GraduationCommon = new HashSet<GraduationCommon>();
            GraduationCredit = new HashSet<GraduationCredit>();
            GraduationSkill = new HashSet<GraduationSkill>();
            GraduationVolunteer = new HashSet<GraduationVolunteer>();
            LeaveRecord = new HashSet<LeaveRecord>();
            SAActivityRecord = new HashSet<SAActivityRecord>();
        }

        public string StudentId { get; set; } = null!;
        public string? SName { get; set; }
        public string? Class { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? SEmail { get; set; }
        public string? EnrollmentStatus { get; set; }
        public DateTime? DateOfEnrollment { get; set; }
        public int? ClassNumberOfPeople { get; set; }
        public string? Password1 { get; set; }
        public int? Room { get; set; }
        public string? NicId { get; set; }
        public string? DeptId { get; set; }
        public string? InstructorId { get; set; }

        public virtual ICollection<Alert> Alert { get; set; }
        public virtual ICollection<CourseRecord> CourseRecord { get; set; }
        public virtual Department? Department { get; set; }
        public virtual ICollection<GradeRecord> GradeRecord { get; set; }
        public virtual ICollection<GraduationCommon> GraduationCommon { get; set; }
        public virtual ICollection<GraduationCredit> GraduationCredit { get; set; }
        public virtual ICollection<GraduationSkill> GraduationSkill { get; set; }
        public virtual ICollection<GraduationVolunteer> GraduationVolunteer { get; set; }
        public virtual Instructor? Instructor { get; set; }
        public virtual ICollection<LeaveRecord> LeaveRecord { get; set; }
        public virtual ICollection<SAActivityRecord> SAActivityRecord { get; set; }
    }

    public partial class StudentVolunteerSummary
    {
        public string? StudentId { get; set; }
        public int? VolunteerType1Sum { get; set; }
        public int? VolunteerType2Sum { get; set; }
        public int? VolunteerType3Sum { get; set; }
        public int? TotalHours { get; set; }
        public string? SumPassorfail { get; set; }
    }

    public class LoginModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}