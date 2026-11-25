using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace P7CreateRestApi.Models
{
    // Same idea as the bid screens: only the essential fields travel through the API.
    public class TradeDto
    {
        // Chosen by the database: returned in the responses, ignored in the request bodies.
        [ReadOnly(true)]
        public int TradeId { get; set; }

        [Required(ErrorMessage = "Le compte est obligatoire.")]
        public string Account { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de compte est obligatoire.")]
        public string AccountType { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "La quantité achetée doit être un nombre positif.")]
        public double? BuyQuantity { get; set; }
    }
}
