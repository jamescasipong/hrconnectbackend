using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class UpdateAttendanceDTO
{
    public string DateToday { get; set; }  // Store as string
    public string ClockIn { get; set; }      // Store as string
    public string ClockOut { get; set; }     // Store as string
}

}
