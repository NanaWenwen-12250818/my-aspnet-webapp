using Microsoft.EntityFrameworkCore;

namespace eCHU.Models
{
    public partial class WebAPIContext : DbContext
    {
        public WebAPIContext(DbContextOptions<WebAPIContext> options) : base(options)
        {
        }

        public virtual DbSet<Alert> Alert { get; set; } = null!;
        public virtual DbSet<BuildingDescription> BuildingDescription { get; set; } = null!;
        public virtual DbSet<Course> Course { get; set; } = null!;
        public virtual DbSet<CourseRecord> CourseRecord { get; set; } = null!;
        public virtual DbSet<Department> Department { get; set; } = null!;
        public virtual DbSet<DepartmentRequirement> DepartmentRequirement { get; set; } = null!;
        public virtual DbSet<GradeRecord> GradeRecord { get; set; } = null!;
        public virtual DbSet<GraduationCommon> GraduationCommon { get; set; } = null!;
        public virtual DbSet<GraduationCredit> GraduationCredit { get; set; } = null!;
        public virtual DbSet<GraduationSkill> GraduationSkill { get; set; } = null!;
        public virtual DbSet<GraduationVolunteer> GraduationVolunteer { get; set; } = null!;
        public virtual DbSet<Instructor> Instructor { get; set; } = null!;
        public virtual DbSet<LeaveRecord> LeaveRecord { get; set; } = null!;
        public virtual DbSet<PhoneExtension> PhoneExtension { get; set; } = null!;
        public virtual DbSet<SAActivity> SAActivity { get; set; } = null!;
        public virtual DbSet<SAActivityRecord> SAActivityRecord { get; set; } = null!;
        public virtual DbSet<Student> Student { get; set; } = null!;
        public virtual DbSet<StudentVolunteerSummary> StudentVolunteerSummary { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseId, e.AcademicYear, e.Semester });

                entity.ToTable("Alert");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.CourseId)
                    .HasMaxLength(20)
                    .HasColumnName("Course_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.Alert1)
                    .HasMaxLength(5)
                    .HasColumnName("Alert");

                entity.Property(e => e.CurrentStatus)
                    .HasMaxLength(50)
                    .HasColumnName("Current_Status");

                entity.Property(e => e.Reason).HasMaxLength(5);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Alert)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Alert_Course");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Alert)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Alert_Student");
            });

            modelBuilder.Entity<BuildingDescription>(entity =>
            {
                entity.HasKey(e => e.BuildingId);

                entity.ToTable("Building_Description");

                entity.Property(e => e.BuildingId)
                    .HasMaxLength(10)
                    .HasColumnName("Building_Id");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.Property(e => e.CourseId)
                    .HasMaxLength(20)
                    .HasColumnName("Course_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.CHours).HasColumnName("CHours");

                entity.Property(e => e.CLanguage)
                    .HasMaxLength(10)
                    .HasColumnName("CLanguage");

                entity.Property(e => e.CName)
                    .HasMaxLength(100)
                    .HasColumnName("CName");

                entity.Property(e => e.CTime)
                    .HasMaxLength(50)
                    .HasColumnName("CTime");

                entity.Property(e => e.CType)
                    .HasMaxLength(10)
                    .HasColumnName("CType");

                entity.Property(e => e.Classroom).HasMaxLength(20);

                entity.Property(e => e.CourseNumberOfPeople).HasColumnName("Course_Number_of_people");

                entity.Property(e => e.DeptId)
                    .HasMaxLength(10)
                    .HasColumnName("Dept_Id");

                entity.Property(e => e.GCType)
                    .HasMaxLength(10)
                    .HasColumnName("GCType");

                entity.Property(e => e.InstructorId)
                    .HasMaxLength(20)
                    .HasColumnName("Instructor_Id");

                entity.Property(e => e.Note).HasMaxLength(200);

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Course)
                    .HasForeignKey(d => d.DeptId)
                    .HasConstraintName("FK_Course_Department");

                entity.HasOne(d => d.Instructor)
                    .WithMany(p => p.Course)
                    .HasForeignKey(d => d.InstructorId)
                    .HasConstraintName("FK_Course_Instructor");
            });

            modelBuilder.Entity<CourseRecord>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseId, e.AcademicYear, e.Semester });

                entity.ToTable("Course_Record");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.CourseId)
                    .HasMaxLength(20)
                    .HasColumnName("Course_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.Pass).HasMaxLength(10);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.CourseRecord)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Course_Record_Course");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.CourseRecord)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Course_Record_Student");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.DeptId);

                entity.ToTable("Department");

                entity.Property(e => e.DeptId)
                    .HasMaxLength(10)
                    .HasColumnName("Dept_Id");

                entity.Property(e => e.DeptName)
                    .HasMaxLength(50)
                    .HasColumnName("Dept_Name");

                entity.Property(e => e.ElectiveCredits).HasColumnName("Elective_Credits");

                entity.Property(e => e.RequiredCredits).HasColumnName("Required_Credits");
            });

            modelBuilder.Entity<DepartmentRequirement>(entity =>
            {
                entity.HasKey(e => e.DeptId);

                entity.ToTable("Department_Requirement");

                entity.Property(e => e.DeptId)
                    .HasMaxLength(10)
                    .HasColumnName("Dept_Id");

                entity.Property(e => e.DeptName)
                    .HasMaxLength(50)
                    .HasColumnName("Dept_Name");

                entity.Property(e => e.RequiredMajorElectiveCredit).HasColumnName("Required_Major_Elective_Credit");
            });

            modelBuilder.Entity<GradeRecord>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.AcademicYear, e.Semester });

                entity.ToTable("Grade_Record");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.AverageScore).HasColumnName("Average_Score");

                entity.Property(e => e.ConductGrade).HasColumnName("Conduct_Grade");

                entity.Property(e => e.EarnedCredits).HasColumnName("Earned_Credits");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.GradeRecord)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Grade_Record_Student");
            });

            modelBuilder.Entity<GraduationCommon>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.GName });

                entity.ToTable("Graduation_Common");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.GName).HasMaxLength(50);

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.GraduationCommon)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Graduation_Common_Student");
            });

            modelBuilder.Entity<GraduationCredit>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.AcademicYear, e.Semester });

                entity.ToTable("Graduation_Credit");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.GraduationCredit)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Graduation_Credit_Student");
            });

            modelBuilder.Entity<GraduationSkill>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.SkillCode });

                entity.ToTable("Graduation_Skill");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.SkillCode).HasColumnName("Skill_Code");

                entity.Property(e => e.LastUpdated).HasColumnName("Last_Updated");

                entity.Property(e => e.Skill).HasMaxLength(50);

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.GraduationSkill)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Graduation_Skill_Student");
            });

            modelBuilder.Entity<GraduationVolunteer>(entity =>
            {
                entity.HasKey(e => e.VolunteerId);

                entity.ToTable("Graduation_Volunteer");

                entity.Property(e => e.VolunteerId).HasColumnName("Volunteer_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.VolunteerCode).HasColumnName("Volunteer_Code");

                entity.Property(e => e.VolunteerType)
                    .HasMaxLength(50)
                    .HasColumnName("Volunteer_Type");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.GraduationVolunteer)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Graduation_Volunteer_Student");
            });

            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.ToTable("Instructor");

                entity.Property(e => e.InstructorId)
                    .HasMaxLength(20)
                    .HasColumnName("Instructor_id");

                entity.Property(e => e.DeptId)
                    .HasMaxLength(10)
                    .HasColumnName("Dept_Id");

                entity.Property(e => e.IEmail)
                    .HasMaxLength(100)
                    .HasColumnName("IEmail");

                entity.Property(e => e.IName)
                    .HasMaxLength(50)
                    .HasColumnName("IName");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Instructor)
                    .HasForeignKey(d => d.DeptId)
                    .HasConstraintName("FK_Instructor_Department");
            });

            modelBuilder.Entity<LeaveRecord>(entity =>
            {
                entity.HasKey(e => e.LeaveId);

                entity.ToTable("Leave_Record");

                entity.Property(e => e.LeaveId).HasColumnName("Leave_Id");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.ApplyDate).HasColumnName("Apply_Date");

                entity.Property(e => e.Certificate).HasColumnType("image");

                entity.Property(e => e.CourseId)
                    .HasMaxLength(20)
                    .HasColumnName("Course_Id");

                entity.Property(e => e.EndDate).HasColumnName("End_Date");

                entity.Property(e => e.EndPeriod).HasColumnName("End_Period");

                // 關鍵修正：配置 Hours 為計算欄位
                entity.Property(e => e.Hours)
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.LType)
                    .HasMaxLength(20)
                    .HasColumnName("LType");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("Phone_Number");

                entity.Property(e => e.Progress).HasMaxLength(20);

                entity.Property(e => e.Reason).HasMaxLength(200);

                entity.Property(e => e.StartDate).HasColumnName("Start_Date");

                entity.Property(e => e.StartPeriod).HasColumnName("Start_Period");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.LeaveRecord)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_Leave_Record_Course");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.LeaveRecord)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Leave_Record_Student");
            });

            modelBuilder.Entity<PhoneExtension>(entity =>
            {
                entity.HasKey(e => e.ExtensionId);

                entity.ToTable("Phone_Extension");

                entity.Property(e => e.ExtensionId).HasColumnName("Extension_Id");

                entity.Property(e => e.Description).HasMaxLength(100);

                entity.Property(e => e.Extension).HasMaxLength(10);

                entity.Property(e => e.OrgName)
                    .HasMaxLength(100)
                    .HasColumnName("Org_Name");

                entity.Property(e => e.SubunitName)
                    .HasMaxLength(100)
                    .HasColumnName("Subunit_Name");
            });

            modelBuilder.Entity<SAActivity>(entity =>
            {
                entity.HasKey(e => e.ActivityId);

                entity.ToTable("SA_Activity");

                entity.Property(e => e.ActivityId).HasColumnName("Activity_Id");

                entity.Property(e => e.AName)
                    .HasMaxLength(100)
                    .HasColumnName("AName");

                entity.Property(e => e.AcademicYear).HasColumnName("Academic_Year");

                entity.Property(e => e.Date).HasColumnType("date");

                entity.Property(e => e.EndTime).HasColumnName("End_Time");

                entity.Property(e => e.Fee).HasMaxLength(50);

                entity.Property(e => e.Location).HasMaxLength(100);

                entity.Property(e => e.Meal).HasMaxLength(50);

                entity.Property(e => e.NumberOfApplicants).HasColumnName("Number_of_Applicants");

                entity.Property(e => e.Organizer).HasMaxLength(100);

                entity.Property(e => e.Outline).HasMaxLength(500);

                entity.Property(e => e.Remarks).HasMaxLength(200);

                entity.Property(e => e.StartTime).HasColumnName("Start_Time");

                entity.Property(e => e.Status).HasMaxLength(20);
            });

            modelBuilder.Entity<SAActivityRecord>(entity =>
            {
                entity.HasKey(e => e.RegistrationId);

                entity.ToTable("SA_Activity_Record");

                entity.Property(e => e.RegistrationId).HasColumnName("Registration_Id");

                entity.Property(e => e.ActivityId).HasColumnName("Activity_Id");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.EmailStatus)
                    .HasMaxLength(20)
                    .HasColumnName("Email_Status");

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("Phone_Number");

                entity.Property(e => e.RegistrationTime).HasColumnName("Registration_Time");

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.HasOne(d => d.SAActivity)
                    .WithMany(p => p.SAActivityRecord)
                    .HasForeignKey(d => d.ActivityId)
                    .HasConstraintName("FK_SA_Activity_Record_SA_Activity");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.SAActivityRecord)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SA_Activity_Record_Student");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.Class).HasMaxLength(10);

                entity.Property(e => e.ClassNumberOfPeople).HasColumnName("Class_Number_of_people");

                entity.Property(e => e.DateOfBirth)
                    .HasColumnType("date")
                    .HasColumnName("Date_Of_Birth");

                entity.Property(e => e.DateOfEnrollment)
                    .HasColumnType("date")
                    .HasColumnName("Date_Of_Enrollment");

                entity.Property(e => e.DeptId)
                    .HasMaxLength(10)
                    .HasColumnName("Dept_Id");

                entity.Property(e => e.EnrollmentStatus)
                    .HasMaxLength(20)
                    .HasColumnName("Enrollment_Status");

                entity.Property(e => e.Gender).HasMaxLength(10);

                entity.Property(e => e.InstructorId)
                    .HasMaxLength(20)
                    .HasColumnName("Instructor_Id");

                entity.Property(e => e.NicId)
                    .HasMaxLength(20)
                    .HasColumnName("NIC_ID");

                entity.Property(e => e.Password1).HasMaxLength(50);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(20)
                    .HasColumnName("Phone_Number");

                entity.Property(e => e.SEmail)
                    .HasMaxLength(100)
                    .HasColumnName("SEmail");

                entity.Property(e => e.SName)
                    .HasMaxLength(50)
                    .HasColumnName("SName");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.Student)
                    .HasForeignKey(d => d.DeptId)
                    .HasConstraintName("FK_Student_Department");

                entity.HasOne(d => d.Instructor)
                    .WithMany(p => p.Student)
                    .HasForeignKey(d => d.InstructorId)
                    .HasConstraintName("FK_Student_Instructor");
            });

            modelBuilder.Entity<StudentVolunteerSummary>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("Student_Volunteer_Summary");

                entity.Property(e => e.StudentId)
                    .HasMaxLength(20)
                    .HasColumnName("Student_Id");

                entity.Property(e => e.SumPassorfail)
                    .HasMaxLength(10)
                    .HasColumnName("Sum_Passorfail");

                entity.Property(e => e.TotalHours).HasColumnName("Total_Hours");

                entity.Property(e => e.VolunteerType1Sum).HasColumnName("Volunteer_Type1_Sum");

                entity.Property(e => e.VolunteerType2Sum).HasColumnName("Volunteer_Type2_Sum");

                entity.Property(e => e.VolunteerType3Sum).HasColumnName("Volunteer_Type3_Sum");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}