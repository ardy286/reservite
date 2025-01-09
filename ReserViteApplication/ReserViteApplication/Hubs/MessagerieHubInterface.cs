namespace ReserViteApplication.Hubs
{
    public interface MessagerieHubInterface
    {
        // Méthode pour recevoir un message
        Task ReceiveMessage(string message);
        // Méthode pour recevoir une notification visuelle pour les utilisateurs
        Task Notify(string notification);
        // Méthode pour recevoir les utilisateurs du groupe
        Task ReceiveGroupUsers(string groupName, List<string> users);
    }
}
