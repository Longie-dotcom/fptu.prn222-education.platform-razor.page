using Domain.AcademicManagement.Aggregate;
using Domain.CourseManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.IdentityManagement.Aggregate;
using Domain.IdentityManagement.ValueObject;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Persistence
{
    public static class Seeder
    {
        public static async Task SeedAsync(EducationPlatformDBContext context)
        {
            // ====================
            // Seed Grades (Grade 1 → 12)
            // ====================
            if (!await context.Grades.AnyAsync())
            {
                var grades = new List<Grade>();
                for (int i = 1; i <= 12; i++)
                {
                    grades.Add(new Grade(Guid.NewGuid(), $"Grade {i}"));
                }

                context.Grades.AddRange(grades);
            }

            // ====================
            // Seed Subjects
            // ====================
            if (!await context.Subjects.AnyAsync())
            {
                var subjects = new List<Subject>
                {
                    new Subject(Guid.NewGuid(), "MATH", "Mathematics", Guid.Empty),
                    new Subject(Guid.NewGuid(), "VIET", "Vietnamese", Guid.Empty),
                    new Subject(Guid.NewGuid(), "ENG", "English", Guid.Empty),
                    new Subject(Guid.NewGuid(), "SCI", "Science", Guid.Empty),
                    new Subject(Guid.NewGuid(), "HIS", "History", Guid.Empty),
                    new Subject(Guid.NewGuid(), "GEO", "Geography", Guid.Empty),
                    new Subject(Guid.NewGuid(), "PHYS", "Physics", Guid.Empty),
                    new Subject(Guid.NewGuid(), "CHEM", "Chemistry", Guid.Empty),
                    new Subject(Guid.NewGuid(), "BIO", "Biology", Guid.Empty),
                    new Subject(Guid.NewGuid(), "TECH", "Technology", Guid.Empty),
                    new Subject(Guid.NewGuid(), "ART", "Art", Guid.Empty),
                    new Subject(Guid.NewGuid(), "PE", "Physical Education", Guid.Empty),
                    new Subject(Guid.NewGuid(), "CIT", "Civic Education", Guid.Empty)
                };

                context.Subjects.AddRange(subjects);
            }

            // ====================
            // Seed Admin User
            // ====================
            if (!await context.Users.AnyAsync(u => u.Role == Role.Admin))
            {
                var adminUser = new User(
                    userId: Guid.NewGuid(),
                    email: "longdong32120@gmail.com",
                    plainPassword: "28012005",
                    phone: "0349331141",
                    name: "Dong Xuan Bao Long",
                    bio: "Platform Administrator",
                    role: Role.Admin,
                    true
                );

                context.Users.Add(adminUser);
            }

            // ====================
            // Seed Teacher Course Review Policies (Admin Enforcement)
            // ====================
            if (!await context.Policies.AnyAsync())
            {
                var policies = new List<Policy>();
                var policyRules = new List<PolicyRule>();

                void AddPolicy(
                    string policyName,
                    (string code, string description)[] rules)
                {
                    var policyId = Guid.NewGuid();

                    policies.Add(new Policy(policyId, policyName));

                    foreach (var (code, description) in rules)
                    {
                        policyRules.Add(new PolicyRule(
                            Guid.NewGuid(),
                            code,
                            description,
                            policyId
                        ));
                    }
                }

                AddPolicy(
                    "Content Quality Policy",
                    new[]
                    {
                        ("CLEAR_DESCRIPTION", "Course description must clearly describe learning objectives."),
                        ("STRUCTURED_CONTENT", "Course content must be well-structured and organized.")
                    });

                AddPolicy(
                    "Academic Integrity Policy",
                    new[]
                    {
                        ("NO_PLAGIARISM", "Course content must not be plagiarized."),
                        ("PROPER_CITATION", "External materials must be properly cited.")
                    });

                AddPolicy(
                    "Assessment Policy",
                    new[]
                    {
                        ("ASSESSMENT_REQUIRED", "Course must include at least one assessment."),
                        ("ASSESSMENT_RELEVANT", "Assessments must align with lesson content.")
                    });

                AddPolicy(
                    "Content Safety Policy",
                    new[]
                    {
                        ("NO_OFFENSIVE_LANGUAGE", "Course must not contain offensive or abusive language."),
                        ("AGE_APPROPRIATE", "Content must be appropriate for the target age group.")
                    });

                AddPolicy(
                    "Curriculum Alignment Policy",
                    new[]
                    {
                        ("GRADE_MATCH", "Course content must match the selected grade."),
                        ("SUBJECT_MATCH", "Course content must align with the selected subject.")
                    });

                AddPolicy(
                    "Technical Quality Policy",
                    new[]
                    {
                        ("MEDIA_QUALITY", "Videos and media must meet minimum quality standards."),
                        ("NO_BROKEN_LINKS", "Course must not contain broken or missing resources.")
                    });

                AddPolicy(
                    "Pricing Policy",
                    new[]
                    {
                        ("FAIR_PRICING", "Course pricing must be reasonable."),
                        ("NO_HIDDEN_FEES", "Course must not contain hidden charges.")
                    });

                context.Policies.AddRange(policies);
                await context.SaveChangesAsync();
            }

            await context.SaveChangesAsync();
        }
    }
}
