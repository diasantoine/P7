using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace P7CreateRestApi.Models
{
    // Only the fields shown on the Findexium screens travel through the API.
    public class BidListDto
    {
        // Chosen by the database: returned in the responses, ignored in the request bodies.
        [ReadOnly(true)]
        public int BidListId { get; set; }

        [Required(ErrorMessage = "Le compte est obligatoire.")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de l'offre est obligatoire.")]
        public string BidType { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "La quantité de l'offre doit être un nombre positif.")]
        public double? BidQuantity { get; set; }
    }
}
