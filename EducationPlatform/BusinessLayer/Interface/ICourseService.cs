using BusinessLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface ICourseService
    {
        Task<IEnumerable<PolicyDTO>> GetPolicies();

        Task<IEnumerable<CourseDTO>> GetCourses(
            QueryCourseDTO dto,
            Guid callerId, 
            string callerRole);

        Task<CourseDetailDTO> GetCourseDetail(
            Guid courseId,
            Guid callerId,
            string callerRole);

        Task CreateCourse(
            CreateCourseDTO dto, 
            Guid callerId, 
            string callerRole);

        Task ReviewCourse(
            ReviewCourseDTO dto,
            Guid callerId,
            string callerRole);
    }
}
