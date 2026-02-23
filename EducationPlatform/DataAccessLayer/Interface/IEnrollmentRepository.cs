using Domain.EnrollmentManagement.Aggregate;

namespace DataAccessLayer.Interface
{
    public interface IEnrollmentRepository :
        IGenericRepository<Enrollment>,
        IRepositoryBase
    {
        Task<IEnumerable<Enrollment>> GetStudentEnrollments(
            Guid studentId);

        Task<Enrollment?> GetEnrollmentDetailByID(
            Guid enrollmentId);

        Task<IEnumerable<Enrollment>> GetStudentStatistic(
            Guid studentId);

        Task<Enrollment?> GetEnrollmentForUpdate(
            Guid enrollmentId);

        Task UpsertLessonProgress(
            Guid enrollmentId, Guid lessonId, bool isCompleted);

        Task<(bool isCorrect, List<string> correctAnswers, string explanation)> UpsertQuizProgress(
            Guid enrollmentId, Guid lessonId, Guid quizId, List<string> submittedAnswers);
    }
}
