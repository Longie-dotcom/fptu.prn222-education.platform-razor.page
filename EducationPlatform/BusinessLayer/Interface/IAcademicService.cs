using BusinessLayer.DTO;

namespace BusinessLayer.Interface
{
    public interface IAcademicService
    {
        Task<IEnumerable<SubjectDTO>> GetSubjects();

        Task<IEnumerable<GradeDTO>> GetGrades();
    }
}
