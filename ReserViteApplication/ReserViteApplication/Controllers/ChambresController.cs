using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;
using ReserViteApplication.Models;
using ReserViteApplication.Services;

namespace ReserViteApplication.Controllers
{
    public class ChambresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISMSSenderService _smsSenderService;
        private readonly IEmailSender _emailSender;

        public ChambresController(ApplicationDbContext context, ISMSSenderService smsSenderService, IEmailSender emailSender)
        {
            _context = context;
            _smsSenderService = smsSenderService;
            _emailSender = emailSender;
        }

        // GET: Chambres
        public async Task<IActionResult> Index()
        {
            return View(await _context.Chambres.ToListAsync());
        }

        public async Task<IActionResult> AfirmReservation(int idChambre, int idUt, string checkin, string checkout)
        {
            // Convertir les dates d'arrivée et de départ en DateTime
            DateTime dateArrivee = DateTime.Parse(checkin);
            DateTime dateDepart = DateTime.Parse(checkout);

            // Calculer le nombre de nuits
            int nbNuits = (dateDepart - dateArrivee).Days;

            // Vérifier que le nombre de nuits est positif
            if (nbNuits <= 0)
            {
                ModelState.AddModelError("", "La date de départ doit être après la date d'arrivée.");
                return View("Index"); // Afficher un message d'erreur et rediriger vers la vue Index
            }

            // Récupérer l'utilisateur et la chambre à partir de la base de données
            Utilisateur utilisateur = _context.Utilisateurs.FirstOrDefault(u => u.UtilisateurId == idUt);
            Chambre chambre = _context.Chambres.FirstOrDefault(c => c.ChambreId == idChambre);

            if (utilisateur == null || chambre == null)
            {
                ModelState.AddModelError("", "Utilisateur ou chambre non trouvé.");
                return View("Index"); // Afficher un message d'erreur et rediriger vers la vue Index
            }

            // Calculer le montant de base de la réservation sans les taxes
            double montantInitial = chambre.PrixParNuit * nbNuits;

            // Définir les pourcentages de taxes
            const double TPS = 0.05;   // 5% TPS
            const double TVQ = 0.09975; // 9,975% TVQ

            // Calculer les montants de taxes
            double montantTPS = montantInitial * TPS;
            double montantTVQ = montantInitial * TVQ;

            // Calculer le montant total avec les taxes
            double montantTotal = montantInitial + montantTPS + montantTVQ;

            // Créer un nouvel objet réservation avec le montant total incluant les taxes
            Reservation reservation = new Reservation
            {
                DateArrivee = dateArrivee,
                DateDepart = dateDepart,
                MontantTotal = montantTotal,
                Utilisateur = utilisateur,
                Chambre = chambre,
                Statut = "Confirmée" // Vous pouvez changer en fonction de votre logique de statut
            };

            // Ajouter la réservation à la base de données
            _context.Reservations.Add(reservation);


            // Sauvegarder les modifications dans la base de données
            _context.SaveChanges();

            // Envoyer un SMS de confirmation à l'utilisateur
            string message = $"Votre réservation pour la chambre {chambre.NomChambre} du {dateArrivee.ToShortDateString()} au {dateDepart.ToShortDateString()} a été confirmée sur Reservite. À bientôt!";
            await _smsSenderService.SendSmsAsync("+1"+utilisateur.Numero, message);

            // Envoyer un email d'annulation à l'utilisateur
            string emailSubject = "Annulation de réservation";
            string emailMessage = $"<p>Bonjour {reservation.Utilisateur.Prenom},</p><p>Votre réservation pour la chambre {reservation.Chambre.NomChambre} du {reservation.DateArrivee.ToShortDateString()} au {reservation.DateDepart.ToShortDateString()} a été annulée.</p>";
            await _emailSender.SendEmailAsync(reservation.Utilisateur.Email, emailSubject, emailMessage);

            // Rediriger vers l'index des chambres ou vers une vue de confirmation
            return RedirectToAction("Index", "Chambres");
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> DeleteReservation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Utilisateur)
                .Include(r => r.Chambre)
                .FirstOrDefaultAsync(m => m.ReservationId == id);
            if (reservation == null)
            {
                return NotFound();
            }

            // Supprimer la réservation de la base de données
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            // Envoyer un SMS d'annulation à l'utilisateur
            string message = $"Votre réservation pour la chambre {reservation.Chambre.NomChambre} du {reservation.DateArrivee.ToShortDateString()} au {reservation.DateDepart.ToShortDateString()} a été annulée sur le site de Reservite. On attend encore votre visite!";
            await _smsSenderService.SendSmsAsync("+1" + reservation.Utilisateur.Numero, message);

            // Envoyer un email d'annulation à l'utilisateur
            string emailSubject = "Annulation de réservation";
            string emailMessage = $"<p>Bonjour {reservation.Utilisateur.Prenom},</p><p>Votre réservation pour la chambre {reservation.Chambre.NomChambre} du {reservation.DateArrivee.ToShortDateString()} au {reservation.DateDepart.ToShortDateString()} a été annulée.</p>";
            await _emailSender.SendEmailAsync(reservation.Utilisateur.Email, emailSubject, emailMessage);

            // Rediriger vers l'index des chambres
            return RedirectToAction("Index", "Chambres");
        }


        // GET: Chambres/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservedDates = _context.Reservations
            .Where(r => r.Chambre.ChambreId == id)
            .Select(r => new { StartDate = r.DateArrivee, EndDate = r.DateDepart })
            .ToList();

            var chambre = await _context.Chambres
                .FirstOrDefaultAsync(m => m.ChambreId == id);
            if (chambre == null)
            {
                return NotFound();
            }

            ViewData["ReservedDates"] = reservedDates;

            return View(chambre);
        }

        // GET: Chambres/Details/5
        public async Task<IActionResult> DetailsAdm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chambre = await _context.Chambres
                .FirstOrDefaultAsync(m => m.ChambreId == id);
            if (chambre == null)
            {
                return NotFound();
            }

            return View(chambre);
        }


        // GET: Chambres/Create
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Messagerie()
        {
            return View();
        }

        // POST: Chambres/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChambreId,NomChambre,Type,PrixParNuit,NombreDePersonnes,Description,EstDisponible,Photo1,Photo2,Photo3,Photo4")] Chambre chambre)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chambre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chambre);
        }

        public IActionResult Rechercher(string chambreType, DateTime? dateArriv, DateTime? dateDepart, int? nombrePers)
        {
            // Récupérer toutes les chambres disponibles de base
            var chambres = _context.Chambres.AsQueryable();

            // Filtrer par type de chambre
            if (!string.IsNullOrEmpty(chambreType))
            {
                chambres = chambres.Where(c => c.Type == chambreType);
            }

            // Filtrer par nombre de personnes
            if (nombrePers.HasValue)
            {
                chambres = chambres.Where(c => c.NombreDePersonnes >= nombrePers.Value);
            }

            // Filtrer par dates pour éviter les chambres déjà réservées
            if (dateArriv.HasValue && dateDepart.HasValue)
            {
                chambres = chambres.Where(c =>
                    !_context.Reservations
                        .Where(r => r.Chambre.ChambreId == c.ChambreId
                                    && r.Statut == "Confirmée") // Vérifie les réservations confirmées
                        .Any(r => dateArriv < r.DateDepart && dateDepart > r.DateArrivee));
            }

            // Exécute la requête et renvoie le résultat
            var chambresDisponibles = chambres.ToList();

            // Optionnel : Gérer le cas où aucune chambre n'est trouvée
            if (!chambresDisponibles.Any())
            {
                ViewBag.Message = "Aucune chambre disponible pour les critères de recherche sélectionnés.";
            }

            return View("Index", chambresDisponibles);
        }

        // GET: Chambres/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chambre = await _context.Chambres.FindAsync(id);
            if (chambre == null)
            {
                return NotFound();
            }
            return View(chambre);
        }

        public async Task<IActionResult> MesReservations(int idUt)
        {
            var reservations = await _context.Reservations
                .Include(r => r.Chambre)
                .Where(r => r.Utilisateur.UtilisateurId == idUt)
                .ToListAsync();

            if (!reservations.Any())
            {
                ViewBag.Message = "Aucune réservation trouvée.";
            }

            return View(reservations); // Passez les réservations directement à la vue
        }

        public async Task<IActionResult> MesReservationsEnCours(int idUt)
        {

            // Récupération des réservations pour l'utilisateur connecté avec les critères définis
            var reservationsEnCours = await _context.Reservations
                .Where(r => r.Utilisateur.UtilisateurId == idUt
                            && r.DateArrivee <= DateTime.Today
                            && r.DateDepart >= DateTime.Today
                            && r.Statut == "Confirmée")
                .Include(r => r.Chambre) // Chargement de la chambre associée pour afficher les détails dans la vue
                .ToListAsync();

            ViewBag.titre = "Mes réservations en cours";

            // Passer les réservations filtrées à la vue
            return View(reservationsEnCours);
        }

        public async Task<IActionResult> MesReservationsAVenir(int idUt)
        {
            var reservationsAVenir = await _context.Reservations
                .Where(r => r.Utilisateur.UtilisateurId == idUt
                            && r.DateArrivee > DateTime.Today
                            && r.Statut == "Confirmée")
                .Include(r => r.Chambre) // Inclut les informations sur la chambre pour la vue
                .ToListAsync();

            ViewBag.titre = "Mes réservations à venir";

            return View(reservationsAVenir);
        }

        public async Task<IActionResult> MesReservationsAnciennes(int idUt)
        {
            var reservationsAnciennes = await _context.Reservations
                .Where(r => r.Utilisateur.UtilisateurId == idUt
                            && r.DateDepart < DateTime.Today
                            && r.Statut == "Confirmée")
                .Include(r => r.Chambre) // Inclut les informations sur la chambre pour la vue
                .ToListAsync();

            ViewBag.titre = "Mes anciennes réservations";

            return View(reservationsAnciennes);
        }


        // POST: Chambres/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChambreId,NomChambre,Type,PrixParNuit,NombreDePersonnes,Description,EstDisponible,Photo1,Photo2,Photo3,Photo4")] Chambre chambre)
        {
            if (id != chambre.ChambreId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chambre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChambreExists(chambre.ChambreId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(chambre);
        }

        // GET: Chambres/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chambre = await _context.Chambres
                .FirstOrDefaultAsync(m => m.ChambreId == id);
            if (chambre == null)
            {
                return NotFound();
            }

            return View(chambre);
        }

        // POST: Chambres/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chambre = await _context.Chambres.FindAsync(id);
            if (chambre != null)
            {
                _context.Chambres.Remove(chambre);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChambreExists(int id)
        {
            return _context.Chambres.Any(e => e.ChambreId == id);
        }
    }
}
