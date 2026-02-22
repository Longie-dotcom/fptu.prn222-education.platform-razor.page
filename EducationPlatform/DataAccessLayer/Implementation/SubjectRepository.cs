using DataAccessLayer.Interface;
using DataAccessLayer.Persistence;
using Domain.AcademicManagement.Aggregate;

namespace DataAccessLayer.Implementation
{
    public class SubjectRepository :
        GenericRepository<Subject>,
        ISubjectRepository
    {
        #region Attributes
        #endregion

        #region Properties
        #endregion

        public SubjectRepository(EducationPlatformDBContext context) : base(context) { }

        #region Methods
        #endregion
    }
}
