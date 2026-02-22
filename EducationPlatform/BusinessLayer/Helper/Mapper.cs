using AutoMapper;
using BusinessLayer.DTO;
using Domain.AcademicManagement.Aggregate;
using Domain.CourseManagement.Aggregate;
using Domain.CourseManagement.Entity;
using Domain.CourseManagement.ValueObject;
using Domain.EnrollmentManagement.Aggregate;
using Domain.EnrollmentManagement.Entity;
using Domain.IdentityManagement.Aggregate;
using Domain.OrderManagement.Aggregate;

namespace BusinessLayer.Helper
{
    public class Mapper : Profile
    {
        public Mapper()
        {
            // ----- Identity Domain -----
            CreateMap<User, UserDTO>();

            // ----- Academic Domain -----
            CreateMap<Subject, SubjectDTO>();
            CreateMap<Grade, GradeDTO>();

            // ----- Order Domain -----
            CreateMap<Order,  OrderDTO>();

            // ----- Course Domain -----
            CreateMap<PolicyRule,PolicyRuleDTO>();
            CreateMap<Policy, PolicyDTO>()
                .ForMember(d => d.PolicyRules, opt => opt.MapFrom(s => s.PolicyRules));

            CreateMap<ViolatedPolicy, ViolatedPolicyDTO>()
                .ForMember(d => d.Policy, opt => opt.MapFrom(s => s.Policy));
            CreateMap<QuizAnswer, QuizAnswerDTO>()
                .ForMember(d => d.CorrectAnswers, opt => opt.MapFrom(s => s.CorrectAnswers))
                .ForMember(d => d.Options, opt => opt.MapFrom(s => s.Options));
            CreateMap<Quiz, QuizDTO>()
                .ForMember(d => d.Answer, opt => opt.MapFrom(s => s.Answer));
            CreateMap<Lesson, LessonDTO>()
                .ForMember(d => d.Quizzes, opt => opt.MapFrom(s => s.Quizzes));
            CreateMap<Course, CourseDTO>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price.Amount))
                .ForMember(d => d.Teacher, opt => opt.MapFrom(s => s.Teacher))
                .ForMember(d => d.Grade, opt => opt.MapFrom(s => s.Grade))
                .ForMember(d => d.Subject, opt => opt.MapFrom(s => s.Subject));
            CreateMap<Course, CourseDetailDTO>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.Price.Amount))
                .ForMember(d => d.Teacher, opt => opt.MapFrom(s => s.Teacher))
                .ForMember(d => d.Grade, opt => opt.MapFrom(s => s.Grade))
                .ForMember(d => d.Subject, opt => opt.MapFrom(s => s.Subject))
                .ForMember(d => d.ViolatedPolicies, opt => opt.MapFrom(s => s.ViolatedPolicies))
                .ForMember(d => d.Lessons, opt => opt.MapFrom(s => s.Lessons));

            // ----- Enrollment Domain -----
            CreateMap<QuizProgress, QuizProgressDTO>()
                .ForMember(d => d.Quiz, opt => opt.MapFrom(s => s.Quiz));
            CreateMap<LessonProgress, LessonProgressDTO>()
                .ForMember(d => d.QuizProgresses, opt => opt.MapFrom(s => s.QuizProgresses))
                .ForMember(d => d.Lesson, opt => opt.MapFrom(s => s.Lesson));
            CreateMap<CourseProgress, CourseProgressDTO>()
                .ForMember(d => d.LessonProgresses, opt => opt.MapFrom(s => s.LessonProgresses));
            CreateMap<Enrollment, EnrollmentDTO>();
            CreateMap<Enrollment, EnrollmentDetailDTO>()
                .ForMember(d => d.CourseProgress, opt => opt.MapFrom(s => s.CourseProgress))
                .ForMember(d => d.Course, opt => opt.MapFrom(s => s.Course));
        }
    }
}
