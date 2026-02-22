namespace BusinessLayer.DTO
{
    public class GradeDTO
    {
        public Guid GradeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class SubjectDTO
    {
        public Guid SubjectID { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
