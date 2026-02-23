using BusinessLayer.DTO;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.Helper;
using PresentationLayer.ViewModels;

namespace PresentationLayer.Pages.Courses
{
    [Authorize]
    public class ListCourseModel : PageModel
    {
        #region Attributes
        private readonly ICourseService courseService;
        private readonly IAcademicService academicService;
        #endregion

        #region Properties
        [BindProperty(SupportsGet = true)]
        public ListCourseViewModel VM { get; set; } = new();
        #endregion

        public ListCourseModel(
            ICourseService courseService,
            IAcademicService academicService)
        {
            this.courseService = courseService;
            this.academicService = academicService;
        }

        #region Methods
        public async Task OnGetAsync(int page = 1)
        {
            try
            {
                (Guid userId, string role) = CheckClaimHelper.CheckClaim(User);

                var query = new QueryCourseDTO
                {
                    Title = VM.Title,
                    GradeName = VM.GradeName,
                    SubjectName = VM.SubjectName,
                    PageIndex = page,
                    PageSize = 6
                };

                VM.Courses = await courseService.GetCourses(query, userId, role);
                VM.Grades = await academicService.GetGrades();
                VM.Subjects = await academicService.GetSubjects();
                VM.CurrentPage = page;
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
            }
        }
        #endregion
    }
}