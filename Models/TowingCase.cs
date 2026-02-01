
using System.ComponentModel.DataAnnotations;
namespace Proffessional.Models;
public class TowingCase
{

    [Key]
    [Required]
    public string CaseId { get; set; } = null!;
    public string? CustomerName { get; set; }
    public string? VehicleBrand { get; set; }
    public string? Model { get; set; }
    public string? RegistrationNo { get; set; }
    public string? ChassisNo { get; set; }
    public string? CustomerContactNumber { get; set; }
    public string? CustomerCallbackNumber { get; set; }
    public string? IncidentReason { get; set; }
    public string? IncidentPlace { get; set; }
    public string? DropLocation { get; set; }
    public string? AssignedVendorName { get; set; }
    public string? VendorContactNumber { get; set; }
    public string? RequestType { get; set; }
    public string? TowingType { get; set; }
    public string? TollBorderCharges { get; set; }
    public string? TollFreeNumber { get; set; }
    public int? CreatedBy { get; set; }
    public int? AssignedStaffId { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public Staff AssignedStaff { get; set; }

    //public DateTime? AssignedDate { get; set; }
    //public DateTime? ClosedDate { get; set; }

    //public int SLAHours { get; set; } = 4; // Example SLA: 4 hours


    public ICollection<TowingCaseImage> Images { get; set; }
    = new List<TowingCaseImage>();
}