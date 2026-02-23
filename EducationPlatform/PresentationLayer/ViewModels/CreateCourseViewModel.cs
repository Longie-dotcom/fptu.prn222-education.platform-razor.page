using BusinessLayer.DTO;

namespace PresentationLayer.ViewModels
{
    public class CreateLessonViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Objectives { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? VideoUrl { get; set; }
        public List<CreateQuizDTO> Quizzes { get; set; } = new();

        public CreateLessonDTO ToDTO()
        {
            return new CreateLessonDTO
            {
                Title = Title,
                Objectives = Objectives,
                Description = Description,
                Order = Order,
                VideoUrl = VideoUrl,
                Quizzes = Quizzes
            };
        }
    }

    public class CreateCourseViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public Guid GradeID { get; set; }
        public Guid SubjectID { get; set; }
        public IFormFile? Image { get; set; }
        public List<CreateLessonViewModel> Lessons { get; set; } = new();
        public IEnumerable<GradeDTO> Grades { get; set; } = new List<GradeDTO>();
        public IEnumerable<SubjectDTO> Subjects { get; set; } = new List<SubjectDTO>();

        public CreateCourseDTO ToDTO(
            string? thumbnailUrl,
            List<CreateLessonDTO> lessons)
        {
            return new CreateCourseDTO
            {
                Title = Title,
                Description = Description,
                Price = Price,
                ThumbnailName = thumbnailUrl,
                GradeID = GradeID,
                SubjectID = SubjectID,
                Lessons = lessons
            };
        }
    }
}
