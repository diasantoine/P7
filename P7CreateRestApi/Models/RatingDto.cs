using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace P7CreateRestApi.Models
{
    public class RatingDto
    {
        // Chosen by the database: returned in the responses, ignored in the request bodies.
        [ReadOnly(true)]
        public int Id { get; set; }

        [Required(ErrorMessage = "La notation Moody's est obligatoire.")]
        public string MoodysRating { get; set; } = string.Empty;

        [Required(ErrorMessage = "La notation S&P est obligatoire.")]
        public string SandPRating { get; set; } = string.Empty;

        [Required(ErrorMessage = "La notation Fitch est obligatoire.")]
        public string FitchRating { get; set; } = string.Empty;

        // A byte: only whole numbers from 0 to 255 are accepted.
        [Required(ErrorMessage = "Le numéro d'ordre est obligatoire.")]
        public byte? OrderNumber { get; set; }
    }
}
