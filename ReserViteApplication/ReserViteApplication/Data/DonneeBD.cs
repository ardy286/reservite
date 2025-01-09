using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReserViteApplication.Models;
using System;
using System.Linq;

namespace ReserViteApplication.Data
{
    public static class DonneeBD
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Vérifie si la base de données contient déjà des données
            if (context.Chambres.Any() || context.Utilisateurs.Any())
            {
                return; // La base de données est déjà initialisée
            }

            // Ajouter des chambres avec photos
            var chambre1 = new Chambre
            {
                NomChambre = "Suite Harry Potter",
                Type = "Suite",
                PrixParNuit = 250,
                NombreDePersonnes = 4,
                EstDisponible = true,
                Description = "Découvrez la magie dans cette suite inspirée par l'univers de Harry Potter ! Plongez dans un décor ensorcelé, où chaque détail évoque le monde des sorciers. Que vous soyez fan de Gryffondor, de Poufsouffle, de Serdaigle ou de Serpentard, cette suite est parfaite pour vous. Avec un espace pour quatre personnes, c'est l'option idéale pour une escapade en famille ou entre amis.",
                Photo1 = "/images/suite_harrypotter1.jpeg",
                Photo2 = "/images/suite_harrypotter2.jpeg",
                Photo3 = "/images/suite_harrypotter3.jpeg"
            };
            var chambre2 = new Chambre
            {
                NomChambre = "Chambre Double Harry Potter",
                Type = "Double",
                PrixParNuit = 180,
                NombreDePersonnes = 2,
                EstDisponible = true,
                Description = "Idéale pour les amateurs d'Harry Potter, cette chambre double est décorée pour évoquer l’atmosphère magique des dortoirs de Poudlard. Profitez de cette ambiance unique pour une escapade à deux au cœur du monde des sorciers.",
                Photo1 = "/images/chambre_double_harrypotter1.jpg",
                Photo2 = "/images/chambre_double_harrypotter2.jpg"
            };
            var chambre3 = new Chambre
            {
                NomChambre = "Chambre Simple Star Wars",
                Type = "Simple",
                PrixParNuit = 120,
                NombreDePersonnes = 1,
                EstDisponible = true,
                Description = "Que la force soit avec vous dans cette chambre simple inspirée de l’univers Star Wars. Parfaite pour les voyageurs solitaires, elle est équipée de décorations interstellaires et d’éléments rappelant les planètes lointaines de cette galaxie légendaire. Préparez-vous pour une nuit galactique !",
                Photo1 = "/images/chambre_simple_starwars.jpg"
            };
            var chambre4 = new Chambre
            {
                NomChambre = "Suite Avengers",
                Type = "Suite",
                PrixParNuit = 260,
                NombreDePersonnes = 4,
                EstDisponible = true,
                Description = "Partez à l'aventure dans cette suite inspirée par les Avengers ! Avec un décor évoquant les plus grands super-héros de l'univers Marvel, cette chambre spacieuse peut accueillir jusqu'à quatre personnes. Parfait pour les familles ou les groupes d'amis qui souhaitent vivre une expérience héroïque.",
                Photo1 = "/images/suite_avengers1.jpeg",
                Photo2 = "/images/suite_avengers2.jpeg",
                Photo3 = "/images/suite_avengers3.jpeg"
            };
            var chambre5 = new Chambre
            {
                NomChambre = "Chambre Double Avengers",
                Type = "Double",
                PrixParNuit = 190,
                NombreDePersonnes = 2,
                EstDisponible = true,
                Description = "Pour les fans de Marvel, cette chambre double est une invitation à rejoindre l'équipe des Avengers. Avec un décor inspiré des super-héros emblématiques, c'est le choix idéal pour un séjour énergique à deux.",
                Photo1 = "/images/chambre_double_avengers1.jpg"
            };

            // Ajout toutes les chambres
            context.Chambres.AddRange(chambre1, chambre2, chambre3, chambre4, chambre5);

            // Ajout des utilisateurs
            var utilisateur1 = new Utilisateur { Nom = "John", Prenom = "Doe", Numero = "5146906589", Email = "kindy@crosemont.qc.ca", MotDePasse = "root", EstAdmin = false };
            var utilisateur2 = new Utilisateur { Nom = "Jane", Prenom = "Smith", Numero = "5146906589", Email = "youssef@crosemont.qc.ca", MotDePasse = "root", EstAdmin = true };

            context.Utilisateurs.AddRange(utilisateur1, utilisateur2);

            // Ajout des réservations
            var reservation1 = new Reservation
            {
                DateArrivee = DateTime.Now.AddDays(2),
                DateDepart = DateTime.Now.AddDays(5),
                MontantTotal = chambre1.PrixParNuit * 3,
                Utilisateur = utilisateur1,
                Chambre = chambre1,
                Statut = "Confirmée"
            };

            var reservation2 = new Reservation
            {
                DateArrivee = DateTime.Now.AddDays(10),
                DateDepart = DateTime.Now.AddDays(12),
                MontantTotal = chambre4.PrixParNuit * 2,
                Utilisateur = utilisateur2,
                Chambre = chambre4,
                Statut = "Confirmée"
            };

            context.Reservations.AddRange(reservation1, reservation2);

            // Save dans la base de donnee
            context.SaveChanges();
        }
    }
}
