using BusinessLayer.DTO;
using BusinessLayer.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using Presentation.Helper;
using System.Text;

namespace PresentationLayer.Pages.AISupports
{
    [Authorize(Roles = "Student")]
    public class AISupportModel : PageModel
    {
        #region Attributes
        private readonly IEnrollmentService enrollmentService;
        private readonly HttpClient httpClient;
        private readonly IConfiguration config;
        #endregion

        #region Properties
        public List<StudentWeaknessDTO> Weaknesses { get; set; } = new();

        public string? GeneratedQuiz { get; set; }
        #endregion

        public AISupportModel(
            IEnrollmentService enrollmentService,
            IConfiguration config,
            IHttpClientFactory factory)
        {
            this.enrollmentService = enrollmentService;
            this.config = config;
            this.httpClient = factory.CreateClient();
        }

        #region Methods

        public async Task OnGet()
        {
            try
            {
                var (userId, role) = CheckClaimHelper.CheckClaim(User);

                var result = await enrollmentService.GetStudentWeakness(userId);

                Weaknesses = result.ToList();
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
            }
        }

        public async Task<IActionResult> OnPostGenerateQuiz()
        {
            try
            {
                var (userId, role) = CheckClaimHelper.CheckClaim(User);

                var weaknesses = await enrollmentService.GetStudentWeakness(userId);

                if (!weaknesses.Any())
                {
                    TempData["ToastMessage"] = "No weak topics found.";
                    TempData["ToastType"] = "warning";
                    return Page();
                }

                var apiKey = config["AI:HuggingFaceKey"];

                var weakTopics = string.Join(", ", weaknesses.Select(x => x.LessonTitle));

                var prompt = $$"""
You are a Vietnamese teacher.

Create 5 multiple choice questions for a student based on these topics:
{{weakTopics}}

Return ONLY valid JSON.

Format:
[
  {
    "question": "text",
    "options": ["A","B","C","D"],
    "answer": "A",
    "explanation": "text"
  }
]
""";

                var body = new
                {
                    model = "mistralai/Mistral-7B-Instruct-v0.2:together",
                    messages = new[]
                    {
                new
                {
                    role = "user",
                    content = prompt
                }
            },
                    temperature = 0.3
                };

                var json = System.Text.Json.JsonSerializer.Serialize(body);

                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    "https://router.huggingface.co/v1/chat/completions");

                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                request.Headers.Add("Accept", "application/json");

                request.Content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await httpClient.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception(responseString);

                var parsed = JObject.Parse(responseString);

                var aiText = parsed["choices"]?[0]?["message"]?["content"]?.ToString() ?? "";

                // ---------- CLEAN AI RESPONSE ----------
                aiText = aiText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                // Extract only the JSON array
                var start = aiText.IndexOf('[');
                var end = aiText.LastIndexOf(']');

                if (start != -1 && end != -1)
                    aiText = aiText.Substring(start, end - start + 1);

                // Fix common invalid JSON from LLM
                aiText = System.Text.RegularExpressions.Regex.Replace(
                    aiText,
                    @"(\{|,)\s*(\w+)\s*:",
                    "$1\"$2\":"
                );

                // Validate JSON
                var quizArray = JArray.Parse(aiText);

                GeneratedQuiz = quizArray.ToString(Newtonsoft.Json.Formatting.None);

                Weaknesses = weaknesses.ToList();

                return Page();
            }
            catch (Exception ex)
            {
                TempData["ToastMessage"] = ex.Message;
                TempData["ToastType"] = "danger";
                return Page();
            }
        }
        #endregion
    }
}