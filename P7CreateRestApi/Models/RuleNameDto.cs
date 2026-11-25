using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace P7CreateRestApi.Models
{
    public class RuleNameDto
    {
        // Chosen by the database: returned in the responses, ignored in the request bodies.
        [ReadOnly(true)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom de la règle est obligatoire.")]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string? Json { get; set; }
        public string? Template { get; set; }
        public string? SqlStr { get; set; }
        public string? SqlPart { get; set; }
    }
}
