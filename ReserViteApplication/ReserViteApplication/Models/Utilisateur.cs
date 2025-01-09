using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ReserViteApplication.Models
{
    public class Utilisateur
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UtilisateurId { get; set; }

        [Required(ErrorMessage = "Le champ Nom est obligatoire.")]
        public string Nom { get; set; }
        [Required(ErrorMessage = "Le champ Prénom est obligatoire.")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le champ Numéro est obligatoire.")]
        public string Numero { get; set; }

        [Required(ErrorMessage = "Le champ Email est obligatoire.")]
        [EmailAddress(ErrorMessage = "L'adresse email n'est pas valide.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le champ Mot de passe est obligatoire.")]
        public string MotDePasse { get; set; }

        public bool EstAdmin { get; set; }
    }

}
