using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReserViteApplication.Models
{
    public class Paiement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaiementId { get; set; }

        public double Montant { get; set; }
        public DateTime DatePaiement { get; set; }

        [Required(ErrorMessage = "Le champ mode de paiement est obligatoire.")]
        public string ModeDePaiement { get; set; } // Carte de crédit, PayPal, etc.

        // Clé étrangère vers la réservation
        [ForeignKey("Reservation")]
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; }

        // Propriétés supplémentaires pour la vue de paiement
        [NotMapped]
        public int ChambreId { get; set; }

        [NotMapped]
        public int UtilisateurId { get; set; }
    }
}
