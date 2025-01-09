using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReserViteApplication.Models
{
    public class Chambre
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChambreId { get; set; }

        [Required(ErrorMessage = "Il faut mettre un nom pour la nouvelle chambre")]
        public string NomChambre { get; set; }

        [Required(ErrorMessage = "Il faut mettre un type de chambre pour la nouvelle chambre")]
        public string Type { get; set; } // (Simple, Double, Suite, etc.)

        [Required(ErrorMessage = "Il faut mettre un prix pour la nouvelle chambre")]
        public double PrixParNuit { get; set; }

        [Required(ErrorMessage = "Il faut mettre un nombre maximum de personnes pour la nouvelle chambre")]
        public int NombreDePersonnes { get; set; }

        [Required(ErrorMessage = "Il faut mettre la description pour la nouvelle chambre")]
        public string Description { get; set; }

        public bool EstDisponible { get; set; } = true;

        [Url(ErrorMessage = "L'URL de la photo n'est pas valide")]
        public string? Photo1 { get; set; }

        [Url(ErrorMessage = "L'URL de la photo n'est pas valide")]
        public string? Photo2 { get; set; }

        [Url(ErrorMessage = "L'URL de la photo n'est pas valide")]
        public string? Photo3 { get; set; }

        [Url(ErrorMessage = "L'URL de la photo n'est pas valide")]
        public string? Photo4 { get; set; }
    }
}
