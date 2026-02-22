using System;
using System.Collections.Generic;
using Domain.CourseManagement.Entity;
using Domain.DomainExceptions;

namespace Domain.EnrollmentManagement.Entity
{
    public class LessonProgress
    {
        #region Attributes
        private readonly List<QuizProgress> quizProgresses = new();
        #endregion

        #region Properties
        public Guid LessonProgressID { get; private set; }
        public bool IsCompleted { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        public Guid CourseProgressID { get; private set; }
        public Guid LessonID { get; private set; }

        public IReadOnlyCollection<QuizProgress> QuizProgresses
        {
            get { return quizProgresses.AsReadOnly(); }
        }
        public Lesson Lesson { get; private set; }
        #endregion

        protected LessonProgress() { }

        public LessonProgress(
            Guid lessonProgressID,
            Guid courseProgressId,
            Guid lessonID)
        {
            if (lessonProgressID == Guid.Empty)
                throw new DomainException(
                    "Lesson progress ID cannot be empty");

            if (courseProgressId == Guid.Empty)
                throw new DomainException(
                    "Course progress ID cannot be empty");

            if (lessonID == Guid.Empty)
                throw new DomainException(
                    "Lesson ID cannot be empty");

            LessonProgressID = lessonProgressID;
            IsCompleted = false;
            CourseProgressID = courseProgressId;
            LessonID = lessonID;
        }

        #region Methods
        public void MarkCompleted()
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
        }

        public void RecalculateCompletion()
        {
            if (IsCompleted) return;

            if (CalculateCorrectQuizRate() >= 100m)
            {
                IsCompleted = true;
                CompletedAt = DateTime.UtcNow;
            }
        }

        public decimal CalculateCorrectQuizRate()
        {
            // No quizzes → lesson is fully complete
            if (!quizProgresses.Any())
                return 100m;

            var total = quizProgresses.Count;
            var correct = quizProgresses.Count(q => q.IsCorrect);

            return Math.Round((decimal)correct * 100 / total, 2);
        }
        #endregion
    }
}
