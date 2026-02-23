using DataAccessLayer.Interface;
using DataAccessLayer.Persistence;
using Domain.EnrollmentManagement.Aggregate;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Implementation
{
    public class EnrollmentRepository :
        GenericRepository<Enrollment>,
        IEnrollmentRepository
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public EnrollmentRepository(EducationPlatformDBContext context) : base(context) { }

        #region Methods
        public async Task<IEnumerable<Enrollment>> GetStudentEnrollments(
            Guid studentId)
        {
            IQueryable<Enrollment> query = context.Enrollments
                .AsNoTracking()
                .Include(e => e.Course)
                .Include(e => e.CourseProgress);

            // ---------- Filters ----------
            query = query.Where(c =>
                c.StudentID == studentId);

            // ---------- Sorting ----------
            query = query.OrderByDescending(c => c.EnrolledAt);

            return await query.ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentDetailByID(
            Guid enrollmentId)
        {
            return await context.Enrollments
                // read-only detail
                .AsNoTracking()
                .AsSplitQuery()
                // Course
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Grade)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Include(e => e.Course)
                    .ThenInclude(c => c.ViolatedPolicies)
                        .ThenInclude(c => c.Policy)
                            .ThenInclude(c => c.PolicyRules)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                        .ThenInclude(l => l.Quizzes)
                // Progress → LessonProgress → Lesson
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.Lesson)
                // Progress → LessonProgress → QuizProgress → Quiz
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.QuizProgresses)
                            .ThenInclude(qp => qp.Quiz)
                .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentId);
        }

        public async Task<IEnumerable<Enrollment>> GetStudentStatistic(
            Guid studentId)
        {
            return await context.Enrollments
                .AsNoTracking()
                .AsSplitQuery()
                // Course
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Grade)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Subject)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                        .ThenInclude(l => l.Quizzes)
                // Progress → LessonProgress → Lesson
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.Lesson)
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.QuizProgresses)
                            .ThenInclude(qp => qp.Quiz)
                .Where(e => e.StudentID == studentId)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<Enrollment?> GetEnrollmentForUpdate(Guid enrollmentId)
        {
            return await context.Enrollments
                // tracked entity for update
                .AsSplitQuery()
                .Include(e => e.Course)
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.Lesson)
                .Include(e => e.CourseProgress)
                    .ThenInclude(cp => cp.LessonProgresses)
                        .ThenInclude(lp => lp.QuizProgresses)
                            .ThenInclude(qp => qp.Quiz)
                .FirstOrDefaultAsync(e => e.EnrollmentID == enrollmentId);
        }

        public async Task UpsertLessonProgress(
            Guid enrollmentId, Guid lessonId, bool isCompleted)
        {
            // Load course progress with lesson progresses for this enrollment (tracked)
            var cp = await context.CourseProgresses
                .Include(x => x.LessonProgresses)
                .FirstOrDefaultAsync(x => x.EnrollmentID == enrollmentId);

            if (cp == null)
            {
                // ensure enrollment exists
                var enrollment = await context.Enrollments.FindAsync(enrollmentId);
                if (enrollment == null) throw new InvalidOperationException("Enrollment not found");

                cp = new Domain.EnrollmentManagement.Entity.CourseProgress(Guid.NewGuid(), enrollmentId);
                context.CourseProgresses.Add(cp);
            }

            var lp = cp.LessonProgresses.FirstOrDefault(x => x.LessonID == lessonId);
            if (lp == null)
            {
                lp = new Domain.EnrollmentManagement.Entity.LessonProgress(Guid.NewGuid(), cp.CourseProgressID, lessonId);
                cp.LessonProgresses.ToList().Add(lp);
                context.LessonProgresses.Add(lp);
            }

            if (isCompleted && !lp.IsCompleted)
            {
                lp.MarkCompleted();
            }

            // Recalculate completion on cp
            cp.RecalculateCompletion();

            // If course completed, set enrollment CompletedAt
            if (cp.IsCompleted)
            {
                var enrollment = await context.Enrollments.FindAsync(enrollmentId);
                if (enrollment != null && enrollment.CompletedAt == null)
                {
                    // set completed at
                    var prop = enrollment.GetType().GetProperty("CompletedAt");
                    if (prop != null)
                        prop.SetValue(enrollment, DateTime.UtcNow);
                }
            }
        }

        public async Task<(bool isCorrect, List<string> correctAnswers, string explanation)> UpsertQuizProgress(
            Guid enrollmentId, Guid lessonId, Guid quizId, List<string> submittedAnswers)
        {
            // Ensure lesson progress exists
            var cp = await context.CourseProgresses
                .Include(x => x.LessonProgresses)
                    .ThenInclude(lp => lp.QuizProgresses)
                .FirstOrDefaultAsync(x => x.EnrollmentID == enrollmentId);

            if (cp == null)
            {
                var enrollment = await context.Enrollments.FindAsync(enrollmentId);
                if (enrollment == null) throw new InvalidOperationException("Enrollment not found");
                cp = new Domain.EnrollmentManagement.Entity.CourseProgress(Guid.NewGuid(), enrollmentId);
                context.CourseProgresses.Add(cp);
            }

            var lp = cp.LessonProgresses.FirstOrDefault(x => x.LessonID == lessonId);
            if (lp == null)
            {
                lp = new Domain.EnrollmentManagement.Entity.LessonProgress(Guid.NewGuid(), cp.CourseProgressID, lessonId);
                cp.LessonProgresses.ToList().Add(lp);
                context.LessonProgresses.Add(lp);
            }

            // Load quiz to evaluate correct answers
            var quiz = await context.Quizzes.FindAsync(quizId);
            if (quiz == null) throw new InvalidOperationException("Quiz not found");

            // Normalize submitted answers
            var submitted = submittedAnswers?.Select(s => s?.Trim()).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s!).ToList() ?? new List<string>();

            // Determine correctness
            var correct = quiz.Answer.CorrectAnswers.Select(a => a.Trim()).ToList();
            bool isCorrect = false;
            if (quiz.Answer.Type == Domain.CourseManagement.Enum.QuizType.SingleChoice || quiz.Answer.Type == Domain.CourseManagement.Enum.QuizType.TrueFalse)
            {
                isCorrect = submitted.Count == 1 && correct.Any(c => string.Equals(c, submitted.First(), StringComparison.OrdinalIgnoreCase));
            }
            else // MultipleChoice
            {
                // 1. Flatten the correct answers in case they are stored as comma-separated strings
                var flattenedCorrect = correct
                    .SelectMany(c => c.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(c => c.Trim().ToLowerInvariant())
                    .ToList();

                // 2. Flatten the submitted answers just in case
                var flattenedSubmitted = submitted
                    .SelectMany(s => s.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim().ToLowerInvariant())
                    .ToList();

                var sset = new HashSet<string>(flattenedSubmitted);
                var cset = new HashSet<string>(flattenedCorrect);

                // 3. Ensure we actually have items to compare
                isCorrect = sset.Count > 0 && sset.SetEquals(cset);
            }

            // Upsert QuizProgress
            var qp = lp.QuizProgresses.FirstOrDefault(x => x.QuizID == quizId);
            if (qp == null)
            {
                qp = new Domain.EnrollmentManagement.Entity.QuizProgress(Guid.NewGuid(), lp.LessonProgressID, quizId);
                lp.QuizProgresses.ToList().Add(qp);
                context.QuizProgresses.Add(qp);
            }

            // update fields
            var attemptCountProp = qp.GetType().GetProperty("AttemptCount");
            if (attemptCountProp != null)
            {
                var current = (int)(attemptCountProp.GetValue(qp) ?? 0);
                attemptCountProp.SetValue(qp, current + 1);
            }
            var isCorrectProp = qp.GetType().GetProperty("IsCorrect");
            isCorrectProp?.SetValue(qp, isCorrect);
            var lastAttemptProp = qp.GetType().GetProperty("LastAttemptAt");
            lastAttemptProp?.SetValue(qp, DateTime.UtcNow);

            // Persist will be done by caller
            // 1. If the quiz is correct, check if all quizzes for this lesson are now passed
            if (isCorrect)
            {
                // Load all quiz IDs associated with this lesson to verify total completion
                var allLessonQuizIds = await context.Quizzes
                    .Where(q => q.LessonID == lessonId)
                    .Select(q => q.QuizID)
                    .ToListAsync();

                var passedQuizIds = lp.QuizProgresses
                    .Where(x => x.IsCorrect)
                    .Select(x => x.QuizID)
                    .ToList();

                // 2. If all quizzes are passed, mark the lesson as completed
                if (allLessonQuizIds.All(id => passedQuizIds.Contains(id)))
                {
                    if (!lp.IsCompleted)
                    {
                        lp.MarkCompleted(); // This sets lp.IsCompleted = true
                    }
                }
            }

            // 3. Recalculate Course Completion % based on the updated lesson status
            cp.RecalculateCompletion();

            // 4. Update the Enrollment 'CompletedAt' if the course just reached 100%
            if (cp.IsCompleted)
            {
                var enrollment = await context.Enrollments.FindAsync(enrollmentId);
                if (enrollment != null && enrollment.CompletedAt == null)
                {
                    var prop = enrollment.GetType().GetProperty("CompletedAt");
                    prop?.SetValue(enrollment, DateTime.UtcNow);
                }
            }

            string explanation = quiz.Note ?? "No explanation provided.";

            return (isCorrect, correct, explanation);
        }
        #endregion
    }
}
