using Domain.AcademicManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.CourseManagement.Enum;
using Domain.CourseManagement.ValueObject;
using Domain.DomainExceptions;
using Domain.IdentityManagement.Aggregate;

namespace Domain.CourseManagement.Aggregate
{
    public class Course
    {
        #region Attributes
        private readonly List<Lesson> lessons = new();
        private readonly List<ViolatedPolicy> violatedPolicies = new();
        #endregion
        
        #region Properties
        public Guid CourseID { get; private set; }
        public string Title { get; set; }
        public string Description { get; private set; }
        public CourseStatus Status { get; private set; }
        public CoursePrice Price { get; private set; }
        public string? ThumbnailName { get; private set; }
        public DateTime? RejectedAt { get; private set; }
        public string? AdminNote { get; private set; }
        public DateTime? PublishedAt { get; private set; }

        public Guid TeacherID { get; private set; }
        public Guid GradeID { get; private set; }
        public Guid SubjectID { get; private set; }

        public User Teacher { get; private set; } = null!;
        public Grade Grade { get; private set; } = null!;
        public Subject Subject { get; private set; } = null!;

        public IReadOnlyCollection<ViolatedPolicy> ViolatedPolicies
        {
            get { return violatedPolicies.AsReadOnly(); }
        }

        public IReadOnlyCollection<Lesson> Lessons
        {
            get { return lessons.AsReadOnly(); }
        }
        #endregion

        protected Course() { }

        public Course(
            Guid courseId,
            string title,
            string description,
            decimal? price,
            string? thumbnailName,
            Guid teacherId,
            Guid gradeId,
            Guid subjectId)
        {
            if (courseId == Guid.Empty)
                throw new DomainException(
                    "Course ID cannot be empty");

            if (teacherId == Guid.Empty)
                throw new DomainException(
                    "Teacher ID cannot be empty");

            if (gradeId == Guid.Empty)
                throw new DomainException(
                    "Grade ID cannot be empty");

            if (subjectId == Guid.Empty)
                throw new DomainException(
                    "Subject ID cannot be empty");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException(
                    "Title is required");

            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException(
                    "Description is required");

            CourseID = courseId;
            Title = title.Trim();
            Description = description.Trim();
            Status = CourseStatus.InReview;
            Price = price.HasValue ? CoursePrice.Paid(price.Value) : CoursePrice.Free();
            ThumbnailName = thumbnailName;
            TeacherID = teacherId;
            GradeID = gradeId;
            SubjectID = subjectId;
        }

        #region Methods
        public Lesson AddLesson(
            string title,
            string objectives,
            string description,
            string videoUrl,
            int order)
        {
            if (lessons.Any(l => l.Order == order))
                throw new DomainException(
                    $"Lesson order {order} already exists in this course");

            var lesson = new Lesson(
                Guid.NewGuid(),
                title,
                objectives,
                description,
                videoUrl,
                order,
                CourseID);

            lessons.Add(lesson);

            return lesson;
        }

        public List<ViolatedPolicy> ReviewCourse(
            List<Guid>? violatedPolicyIds,
            List<(Guid violatedLessonId, string adminNote)>? violatedLessonNotes,
            string? adminNote)
        {
            var hasViolatedLessons = violatedLessonNotes?.Any() == true;
            var hasViolatedPolicies = violatedPolicyIds?.Any() == true;

            if (hasViolatedLessons && !hasViolatedPolicies)
                throw new DomainException(
                    "A violated lesson must be associated with at least one violated policy.");

            AdminNote = adminNote;

            // -----------------------
            // Reset all lessons
            // -----------------------
            foreach (var lesson in lessons)
            {
                lesson.NoteAsNonViolated();
            }

            // -----------------------
            // Mark violated lessons
            // -----------------------
            if (hasViolatedLessons)
            {
                foreach (var (lessonId, note) in violatedLessonNotes!)
                {
                    var lesson = lessons.FirstOrDefault(l => l.LessonID == lessonId)
                                 ?? throw new DomainException(
                                     $"Violated lesson with ID: {lessonId} is not found");

                    lesson.NoteAsViolated(note);
                }
            }

            // -----------------------
            // Clear existing violated policies
            // -----------------------
            violatedPolicies.Clear();

            // -----------------------
            // Add new violated policies if any
            // -----------------------
            if (hasViolatedPolicies)
            {
                foreach (var policyId in violatedPolicyIds!)
                {
                    violatedPolicies.Add(new ViolatedPolicy(Guid.NewGuid(), policyId, CourseID));
                }

                RejectedAt = DateTime.Now;
                Status = CourseStatus.Rejected;
            }
            else
            {
                PublishedAt = DateTime.Now;
                Status = CourseStatus.Published;
            }

            return violatedPolicies;
        }
        #endregion
    }
}
