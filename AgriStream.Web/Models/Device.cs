using System.ComponentModel.DataAnnotations;

namespace AgriStream.Web.Models
{
    public class Device
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string DeviceId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FiledName { get; set; } = string.Empty;

        [Required, StringLength(30)]
        public string Status { get; set; } = "Idle";

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
