namespace SEP490_BE.DAL.DTOs.TestReDTO
{
    public class TestWorklistQueryDto
    {
        public DateOnly? VisitDate { get; set; }
        public string? PatientName { get; set; }
        public RequiredState RequiredState { get; set; } = RequiredState.Missing;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
