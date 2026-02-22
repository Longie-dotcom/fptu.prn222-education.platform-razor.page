using Domain.AcademicManagement.Aggregate;

namespace DataAccessLayer.Interface
{
    public interface ISubjectRepository :
        IGenericRepository<Subject>,
        IRepositoryBase
    {
    }
}
