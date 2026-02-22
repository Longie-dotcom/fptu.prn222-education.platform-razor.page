using DataAccessLayer.Interface;
using DataAccessLayer.Persistence;
using Domain.CourseManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.CourseManagement.Enum;
using Domain.IdentityManagement.ValueObject;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Implementation
{
    public class CourseRepository :
        GenericRepository<Course>,
        ICourseRepository
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public CourseRepository(EducationPlatformDBContext context) : base(context) { }

        #region Methods
        public async Task<IEnumerable<Course>> GetAllCourses(
            string? title,
            decimal? price,
            string? teacherName,
            string? gradeName,
            string? subjectName,
            int pageIndex,
            int pageSize,
            Guid? teacherId,
            Role callerRole)
        {
            // Safety guards
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            IQueryable<Course> query = context.Courses
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Grade)
                .Include(c => c.Subject);

            // ---------- Filters ----------
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Title, $"%{title}%"));
            }

            if (price.HasValue)
            {
                query = query.Where(c =>
                    c.Price.Amount == price.Value);
            }

            if (!string.IsNullOrWhiteSpace(teacherName))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Teacher.Name, $"%{teacherName}%"));
            }
            if (!string.IsNullOrWhiteSpace(gradeName))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Grade.Name, $"%{gradeName}%"));
            }

            if (!string.IsNullOrWhiteSpace(subjectName))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.Subject.Name, $"%{subjectName}%"));
            }

            if (teacherId.HasValue)
            {
                query = query.Where(c =>
                    c.TeacherID == teacherId);
            }

            if (callerRole == Role.Student)
            {
                query = query.Where(c =>
                    c.Status == CourseStatus.Published);
            }

            // ---------- Sorting (IMPORTANT for paging) ----------
            query = query.OrderByDescending(c => c.PublishedAt ?? DateTime.MinValue);

            // ---------- Paging ----------
            query = query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }


        public async Task<Course?> GetCourseDetailByID(
            Guid courseId)
        {
            return await context.Courses
                .AsSplitQuery()
                .Include(c => c.Teacher)
                .Include(c => c.Grade)
                .Include(c => c.Subject)
                .Include(c => c.ViolatedPolicies)
                    .ThenInclude(c => c.Policy)
                        .ThenInclude(c => c.PolicyRules)
                .Include(c => c.Lessons)
                    .ThenInclude(l => l.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseID == courseId);
        }

        public void ReplaceViolatedPolicies(
            Guid courseId,
            IEnumerable<ViolatedPolicy> newViolatedPolicies)
        {
            if (newViolatedPolicies == null)
                newViolatedPolicies = Enumerable.Empty<ViolatedPolicy>();

            // Remove existing policies for the course
            var existingPolicies = context.ViolatedPolicies
                                          .Where(vp => vp.CourseID == courseId)
                                          .ToList();

            if (existingPolicies.Any())
                context.ViolatedPolicies.RemoveRange(existingPolicies);

            // Add new policies
            if (newViolatedPolicies.Any())
                context.ViolatedPolicies.AddRange(newViolatedPolicies);
        }

        public void AddLessons(
            IEnumerable<Lesson> lessons)
        {
            if (lessons == null && !lessons.Any())
                return;

            context.Lessons.AddRange(lessons);
        }

        public void AddQuizzes(
            IEnumerable<Quiz> quizzes)
        {
            if (quizzes == null && !quizzes.Any())
                return;

            context.Quizzes.AddRange(quizzes);
        }
        #endregion
    }
}
