using BusinessLayer.DTO;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.Helper;

namespace PresentationLayer.Pages.Courses
{
    [Authorize]
    public class DetailCourseModel : PageModel
    {
        #region Attributes
        private readonly ICourseService courseService;
        #endregion

        #region Properties
        public CourseDetailDTO Course { get; set; } = new();
        #endregion

        public DetailCourseModel(ICourseService courseService)
        {
            this.courseService = courseService;
        }

        #region Methods
        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var (userId, role) = CheckClaimHelper.CheckClaim(User);
                Course = await courseService.GetCourseDetail(id, userId, role);

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return RedirectToPage("ListCourse");
            }
        }
        #endregion
    }
}