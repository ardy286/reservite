using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Models;

namespace ReserViteApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Chambre> Chambres { get; set; }
        public DbSet<Utilisateur> Utilisateurs { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Paiement> Paiement { get; set; } = default!;

    }
}
