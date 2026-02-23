using BusinessLayer.DTO;

namespace PresentationLayer.ViewModels
{
    public class ReviewCourseViewModel
    {
        public CourseDetailDTO Course { get; set; } = new CourseDetailDTO();
        public IEnumerable<PolicyDTO> Policies { get; set; } = new List<PolicyDTO>();
        public List<Guid> SelectedViolatedPolicyIDs { get; set; } = new List<Guid>();
        public List<LessonViolationViewModel> ViolatedLessons { get; set; } = new List<LessonViolationViewModel>();
        public string? AdminNote { get; set; } = string.Empty;
        public ReviewCourseDTO ToDTO()
        {
            return new ReviewCourseDTO
            {
                CourseID = this.Course.CourseID,
                ViolatedPolicyIDs = this.SelectedViolatedPolicyIDs,
                AdminNote = this.AdminNote,
                ViolatedLessons = this.ViolatedLessons
                    .Select(v => (v.LessonID, v.AdminNote))
                    .ToList()
            };
        }
    }

    public class LessonViolationViewModel
    {
        public Guid LessonID { get; set; }
        public string AdminNote { get; set; } = string.Empty;
    }
}