using Domain.CourseManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.IdentityManagement.ValueObject;

namespace DataAccessLayer.Interface
{
    public interface ICourseRepository :
        IGenericRepository<Course>,
        IRepositoryBase
    {
        Task<IEnumerable<Course>> GetAllCourses(
            string? title,
            decimal? price,
            string? teacherName,
            string? gradeName,
            string? subjectName,
            int pageIndex,
            int pageSize,
            Guid? teacherId,
            Role callerRole);

        Task<Course?> GetCourseDetailByID(
            Guid courseId);

        void ReplaceViolatedPolicies(
            Guid courseId,
            IEnumerable<ViolatedPolicy> newViolatedPolicies);

        void AddLessons(
            IEnumerable<Lesson> lessons);

        void AddQuizzes(
            IEnumerable<Quiz> quizzes);
    }
}
