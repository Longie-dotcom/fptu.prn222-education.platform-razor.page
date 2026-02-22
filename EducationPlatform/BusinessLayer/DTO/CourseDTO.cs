using Domain.CourseManagement.Enum;

namespace BusinessLayer.DTO
{
    // View DTO
    public class CourseDTO
    {
        public Guid CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? ThumbnailName { get; set; }
        public DateTime? RejectedAt { get; set; } 
        public DateTime? PublishedAt { get; set; }
        public UserDTO Teacher { get; set; } = new UserDTO();
        public GradeDTO Grade { get; set; } = new GradeDTO();
        public SubjectDTO Subject { get; set; } = new SubjectDTO();
    }

    public class CourseDetailDTO
    {
        public Guid CourseID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? ThumbnailName { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? PublishedAt { get; set; }
        public UserDTO Teacher { get; set; } = new UserDTO();
        public GradeDTO Grade { get; set; } = new GradeDTO();
        public SubjectDTO Subject { get; set; } = new SubjectDTO();
        public List<ViolatedPolicyDTO> ViolatedPolicies { get; set; } = new List<ViolatedPolicyDTO>();
        public List<LessonDTO> Lessons { get; set; } = new List<LessonDTO>();
    }

    public class QueryCourseDTO
    {
        public string? Title { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? TeacherName { get; set; } = string.Empty;
        public string? GradeName { get; set; } = string.Empty;
        public string? SubjectName { get; set; } = string.Empty;
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 1;
    }

    public class ViolatedPolicyDTO
    {
        public Guid ViolatedPolicyID { get; set; }
        public Guid PolicyID { get; set; }
        public Guid CourseID { get; set; }
        public PolicyDTO Policy { get; set; } = new PolicyDTO();
    }

    public class LessonDTO
    {
        public Guid LessonID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsViolated { get; set; }
        public string? AdminNote { get; set; }
        public Guid CourseID { get; set; }
        public List<QuizDTO> Quizzes { get; set; } = new List<QuizDTO>();
    }

    public class QuizDTO
    {
        public Guid QuizID { get; set; }
        public string Question { get; set; } = string.Empty;
        public string? Note { get; set; }
        public Guid LessonID { get; set; }
        public QuizAnswerDTO Answer { get; set; } = new QuizAnswerDTO();
    }

    public class QuizAnswerDTO
    {
        public QuizType Type { get; set; }
        public List<string> CorrectAnswers { get; set; } = new List<string>();
        public List<string>? Options { get; set; } = new List<string>();
    }

    public class PolicyDTO
    {
        public Guid PolicyID { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<PolicyRuleDTO> PolicyRules { get; set; } = new List<PolicyRuleDTO>();
    }

    public class PolicyRuleDTO
    {
        public Guid PolicyRuleID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid PolicyID { get; set; }
    }

    // Create DTO
    public class CreateCourseDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public string? ThumbnailName { get; set; }
        public Guid GradeID { get; set; }
        public Guid SubjectID { get; set; }
        public List<CreateLessonDTO> Lessons { get; set; } = new List<CreateLessonDTO>();
    }

    public class CreateLessonDTO
    {
        public string Title { get; set; } = string.Empty;
        public string VideoUrl { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<CreateQuizDTO> Quizzes { get; set; } = new List<CreateQuizDTO>();
    }

    public class CreateQuizDTO
    {
        public string Question { get; set; } = string.Empty;
        public string? Note { get; set; }
        public CreateQuizAnswerDTO Answer { get; set; } = new CreateQuizAnswerDTO();
    }

    public class CreateQuizAnswerDTO
    {
        public int Type { get; set; } = 1;
        public List<string>? CorrectAnswers { get; set; } = new List<string>();
        public List<string>? Options { get; set; } = new List<string>();
        public bool? TrueOrFalse { get; set; } = false;
    }

    // Update DTO

    // Delete DTO

    // Review DTO
    public class ReviewCourseDTO
    {
        public Guid CourseID { get; set; }
        public List<Guid>? ViolatedPolicyIDs { get; set; } = new List<Guid>();
        public List<(Guid violatedLessonId, string adminNote)>? ViolatedLessons { get; set; } = new List<(Guid violatedLessonId, string adminNote)>();
        public string? AdminNote { get; set; } = string.Empty;
    }
}
