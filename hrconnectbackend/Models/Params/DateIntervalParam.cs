public class DateIntervalParam
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public DateIntervalParam(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}