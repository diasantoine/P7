using System.ComponentModel.DataAnnotations;

namespace P7CreateRestApi.Models
{
    // Only the fields shown on the Findexium screens travel through the API.
    public class CurvePointDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ne doit pas être vide.")]
        public byte? CurveId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le délai doit être un nombre positif.")]
        public double? Term { get; set; }

        public double? CurvePointValue { get; set; }
    }
}
