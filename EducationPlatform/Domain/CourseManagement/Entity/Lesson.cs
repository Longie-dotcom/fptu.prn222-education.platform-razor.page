using Domain.DomainExceptions;

namespace Domain.CourseManagement.Entity
{
    public class Lesson
    {
        #region Attributes
        private readonly List<Quiz> quizzes = new();
        #endregion

        #region Properties
        public Guid LessonID { get; private set; }
        public string Title { get; private set; }
        public string Objectives { get; private set; }
        public string Description { get; private set; }
        public string VideoUrl { get; private set; }
        public int Order { get; private set; }
        public bool IsViolated { get; private set; }
        public string? AdminNote { get; private set; }

        public Guid CourseID { get; private set; }

        public IReadOnlyCollection<Quiz> Quizzes
        {
            get { return quizzes.AsReadOnly(); }
        }
        #endregion

        protected Lesson() { }

        public Lesson(
            Guid lessonId,
            string title,
            string objectives,
            string description,
            string videoUrl,
            int order,
            Guid courseId)
        {
            if (lessonId == Guid.Empty)
                throw new DomainException(
                    "Lesson ID cannot be empty");

            if (courseId == Guid.Empty)
                throw new DomainException(
                    "Course ID cannot be empty");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException(
                    "Lesson title is required");

            if (string.IsNullOrWhiteSpace(objectives))
                throw new DomainException(
                    "Lesson objectives is required");

            if (string.IsNullOrWhiteSpace(videoUrl))
                throw new DomainException(
                    "Lesson video URL is required");

            if (order <= 0)
                throw new DomainException(
                    "Lesson order must be greater than zero");

            LessonID = lessonId;
            Title = title.Trim();
            Objectives = objectives.Trim();
            Description = description.Trim();
            VideoUrl = videoUrl.Trim();
            Order = order;
            CourseID = courseId;
            IsViolated = false;
        }

        #region Methods
        public Quiz AddQuiz(
            string question,
            string? note)
        {
            var quiz = new Quiz(
                Guid.NewGuid(),
                question,
                note,
                LessonID);

            quizzes.Add(quiz);

            return quiz;
        }

        public void NoteAsViolated(string adminNote)
        {
            if (string.IsNullOrEmpty(adminNote))
                throw new DomainException($"All violated lesson must contain a note");

            IsViolated = true;
            AdminNote = adminNote.Trim();
        }

        public void NoteAsNonViolated()
        {
            IsViolated = false;
            AdminNote = string.Empty;
        }
        #endregion
    }
}
