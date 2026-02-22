using AutoMapper;
using BusinessLayer.BusinessException;
using BusinessLayer.DTO;
using BusinessLayer.Interface;
using DataAccessLayer.Interface;

namespace BusinessLayer.Implementation
{
    public class EnrollmentService : IEnrollmentService
    {
        #region Attributes
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        #endregion

        #region Properties
        #endregion

        public EnrollmentService(
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        #region Methods
        public async Task<IEnumerable<EnrollmentDTO>> GetStudentEnrollments(
            Guid studentId)
        {
            // Validate enrollment list existence include restriction (ownership)
            var list = await unitOfWork
                .GetRepository<IEnrollmentRepository>()
                .GetStudentEnrollments(studentId);

            if (list == null || !list.Any())
                throw new NotFound(
                    "Enrollment list is not found or empty");

            return mapper.Map<IEnumerable<EnrollmentDTO>>(list);
        }

        public async Task<EnrollmentDetailDTO> GetEnrollmentDetail(
            Guid enrollmentId)
        {
            // Validate enrollment existence
            var enrollment = await unitOfWork
                .GetRepository<IEnrollmentRepository>()
                .GetEnrollmentDetailByID(enrollmentId);

            if (enrollment == null)
                throw new NotFound("Enrollment detail has not been found");

            // Mapping
            var dto = mapper.Map<EnrollmentDetailDTO>(enrollment);

            // Restriction: hide quiz answers
            if (dto.CourseProgress?.LessonProgresses != null)
            {
                foreach (var lesson in dto.CourseProgress.LessonProgresses)
                {
                    if (lesson.QuizProgresses == null)
                        continue;

                    foreach (var quizProgress in lesson.QuizProgresses)
                    {
                        if (quizProgress.Quiz != null && quizProgress.Quiz.Answer != null)
                        {
                            quizProgress.Quiz.Answer.CorrectAnswers = new List<string>();
                        }
                    }
                }
            }

            return dto;
        }

        public async Task UpdateLessonProgress(
            Guid enrollmentId, Guid lessonId, double playedSeconds, double duration, bool isCompleted, Guid callerId)
        {
            // Apply persistence
            await unitOfWork.BeginTransactionAsync();
            await unitOfWork
                .GetRepository<IEnrollmentRepository>()
                .UpsertLessonProgress(enrollmentId, lessonId, isCompleted);
            await unitOfWork.CommitAsync(callerId.ToString());
        }

        public async Task<(bool isCorrect, string explanation)> UpdateQuizProgress(
            Guid enrollmentId, Guid lessonId, Guid quizId, List<string> selectedAnswers, Guid callerId)
        {
            // Validate enrollment existence
            var enrollment = await unitOfWork
                .GetRepository<IEnrollmentRepository>()
                .GetEnrollmentForUpdate(enrollmentId);

            if (enrollment == null)
                throw new NotFound("Enrollment not found");

            // Authorize ownership
            if (enrollment.StudentID != callerId)
                throw new AuthenticateException("You are not the owner of this enrollment");

            // Apply persistence
            await unitOfWork.BeginTransactionAsync();
            var result = await unitOfWork
                .GetRepository<IEnrollmentRepository>()
                .UpsertQuizProgress(enrollmentId, lessonId, quizId, selectedAnswers);
            await unitOfWork.CommitAsync(callerId.ToString());

            return (result.isCorrect, result.explanation);
        }
        #endregion
    }
}
