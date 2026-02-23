using AutoMapper;
using BusinessLayer.BusinessException;
using BusinessLayer.DTO;
using BusinessLayer.Interface;
using DataAccessLayer.Interface;
using Domain.CourseManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.CourseManagement.Enum;
using Domain.IdentityManagement.ValueObject;

namespace BusinessLayer.Implementation
{
    public class CourseService : ICourseService
    {
        #region Attributes
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        #endregion

        #region Properties
        #endregion

        public CourseService(
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        #region Methods
        public async Task<IEnumerable<PolicyDTO>> GetPolicies()
        {
            // Validate policies list existence
            var list = await unitOfWork
                .GetRepository<IPolicyRepository>()
                .GetDetailPolicies();

            if (list == null || !list.Any())
                throw new NotFound(
                    "The policy list is not found or empty");

            return mapper.Map<IEnumerable<PolicyDTO>>(list);
        }

        public async Task<IEnumerable<CourseDTO>> GetCourses(
             QueryCourseDTO dto,
             Guid callerId,
             string callerRole)
        {
            // ---- Role parsing ----
            if (!Enum.TryParse<Role>(callerRole, true, out var role))
                throw new AuthenticateException(
                    "Invalid role");

            // ---- Teacher scoping ----
            Guid? teacherId = role == Role.Teacher ? callerId : null;

            var list = await unitOfWork
                .GetRepository<ICourseRepository>()
                .GetAllCourses(
                    dto.Title,
                    dto.Price,
                    dto.TeacherName,
                    dto.GradeName,
                    dto.SubjectName,
                    dto.PageIndex,
                    dto.PageSize,
                    teacherId,
                    role);

            if (list == null || !list.Any())
                throw new NotFound(
                    "Course list is not found or empty");

            return mapper.Map<IEnumerable<CourseDTO>>(list);
        }

        public async Task<CourseDetailDTO> GetCourseDetail(
            Guid courseId,
            Guid callerId,
            string callerRole)
        {
            // Validate course list existence
            var course = await unitOfWork
                .GetRepository<ICourseRepository>()
                .GetCourseDetailByID(courseId);

            if (course == null)
                throw new NotFound(
                    $"Course with ID: {courseId} is not found");

            // Mapping
            var dto = mapper.Map<CourseDetailDTO>(course);

            // Restriction: Hide lessons from student users
            if (!Enum.TryParse<Role>(callerRole, true, out var role))
                throw new AuthenticateException(
                    "Invalid role");

            if (role == Role.Student)
                dto.Lessons = new List<LessonDTO>();

            return dto;
        }

        public async Task CreateCourse(
            CreateCourseDTO dto,
            Guid callerId,
            string callerRole)
        {
            // Apply domain
            // Restore course
            var course = new Course(
                Guid.NewGuid(),
                dto.Title,
                dto.Description,
                dto.Price,
                dto.ThumbnailName,
                callerId,
                dto.GradeID,
                dto.SubjectID);
            var lessons = new List<Lesson>();
            var quizzes = new List<Quiz>();

            foreach (var lessonDto in dto.Lessons)
            {
                var lesson = course.AddLesson(
                    lessonDto.Title,
                    lessonDto.Objectives,
                    lessonDto.Description,
                    lessonDto.VideoUrl,
                    lessonDto.Order);

                foreach (var quizDto in lessonDto.Quizzes)
                {
                    var quiz = lesson.AddQuiz(
                        quizDto.Question,
                        quizDto.Note);

                    quiz.AddAnswer(
                        (QuizType)quizDto.Answer.Type,
                        quizDto.Answer.CorrectAnswers,
                        quizDto.Answer.Options,
                        quizDto.Answer.TrueOrFalse);

                    // Restore quiz
                    quizzes.Add(quiz);
                }

                // Restore lesson
                lessons.Add(lesson);
            }

            // Apply persistence
            await unitOfWork.BeginTransactionAsync();
            unitOfWork
                .GetRepository<ICourseRepository>()
                .Add(course);
            unitOfWork
                .GetRepository<ICourseRepository>()
                .AddLessons(lessons);
            unitOfWork
                .GetRepository<ICourseRepository>()
                .AddQuizzes(quizzes);
            await unitOfWork.CommitAsync(callerId.ToString());
        }

        public async Task ReviewCourse(
            ReviewCourseDTO dto,
            Guid callerId,
            string callerRole)
        {
            // Validate course existence
            var course = await unitOfWork
                .GetRepository<ICourseRepository>()
                .GetCourseDetailByID(dto.CourseID);

            if (course == null)
                throw new NotFound(
                    $"Course with ID: {dto.CourseID} is not found");

            // Apply domain
            var violatedPolicies = course.ReviewCourse(
                dto.ViolatedPolicyIDs,
                dto.ViolatedLessons,
                dto.AdminNote);

            // Apply persistence
            await unitOfWork.BeginTransactionAsync();
            unitOfWork
                .GetRepository<ICourseRepository>()
                .Update(course.CourseID, course);
            unitOfWork
                .GetRepository<ICourseRepository>()
                .ReplaceViolatedPolicies(course.CourseID, violatedPolicies);
            await unitOfWork.CommitAsync(callerId.ToString());
        }
        #endregion
    }
}
