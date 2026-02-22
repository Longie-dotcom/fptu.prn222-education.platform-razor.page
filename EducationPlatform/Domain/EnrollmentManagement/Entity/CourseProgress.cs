using Domain.DomainExceptions;

namespace Domain.EnrollmentManagement.Entity
{
    public class CourseProgress
    {
        #region Attributes
        private readonly List<LessonProgress> lessonProgresses = new();
        #endregion

        #region Properties
        public Guid CourseProgressID { get; private set; }
        public decimal CompletionRate { get; private set; }
        public bool IsCompleted { get; private set; }

        public Guid EnrollmentID { get; private set; }

        public IReadOnlyCollection<LessonProgress> LessonProgresses
        {
            get { return lessonProgresses.AsReadOnly(); }
        }
        #endregion

        protected CourseProgress() { }

        public CourseProgress(
            Guid courseProgressId,
            Guid enrollmentId)
        {
            if (courseProgressId == Guid.Empty)
                throw new DomainException(
                    "Course progress ID cannot be empty");

            if (enrollmentId == Guid.Empty)
                throw new DomainException(
                    "Enrollment ID cannot be empty");

            CourseProgressID = courseProgressId;
            CompletionRate = 0;
            IsCompleted = false;
            EnrollmentID = enrollmentId;
        }

        #region Methods
        public void AddLessonProgress(Guid lessonId)
        {
            var lessonProgress = new LessonProgress(
                Guid.NewGuid(), 
                CourseProgressID, 
                lessonId);

            lessonProgresses.Add(lessonProgress);
        }

        public void RecalculateCompletion()
        {
            if (!lessonProgresses.Any())
            {
                CompletionRate = 0;
                IsCompleted = false;
                return;
            }

            foreach (var lesson in lessonProgresses)
                lesson.RecalculateCompletion();

            var completedLessons = lessonProgresses.Count(l => l.IsCompleted);

            CompletionRate = Math.Round(
                (decimal)completedLessons * 100 / lessonProgresses.Count,
                2
            );

            IsCompleted = completedLessons == lessonProgresses.Count;
        }
        #endregion
    }
}
