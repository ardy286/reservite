using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReserViteApplication.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }
        [Required(ErrorMessage = "Le champ Date d'arrivée est obligatoire.")]
        public DateTime DateArrivee { get; set; }
        [Required(ErrorMessage = "Le champ Date de départ est obligatoire.")]
        public DateTime DateDepart { get; set; }
        public double MontantTotal { get; set; }
        public Utilisateur Utilisateur { get; set; }
        public Chambre Chambre { get; set; }
        public string Statut { get; set; } // Confirmée ou  Annulée
    }

}
