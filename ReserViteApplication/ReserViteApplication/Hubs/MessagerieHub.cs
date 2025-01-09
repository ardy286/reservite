using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ReserViteApplication.Data;

using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ReserViteApplication.Services;
using System.Security.Claims;
using System.Data;

namespace ReserViteApplication.Hubs
{
    public class MessagerieHub : Hub<MessagerieHubInterface>
    {
        private readonly ISMSSenderService _smsSenderService;

        // Liste pour suivre les utilisateurs connectés et leur numéro
        private static readonly ConcurrentDictionary<string, (string UserName, string PhoneNumber)> ConnectedUsers = new();

        // Liste pour stocker l'historique des messages
        private static readonly ConcurrentDictionary<string, List<string>> MessageHistory = new();

        public MessagerieHub(ISMSSenderService smsSenderService)
        {
            _smsSenderService = smsSenderService;
        }

        // Méthode pour diffuser un message à tous les clients connectés
        public async Task BroadcastMessage(string message)
        {
            string userName = Context.User?.Identity?.Name ?? "Utilisateur inconnu";
            string formattedMessage = $"{userName}: {message}";

            // Ajouter le message à l'historique de chaque utilisateur
            foreach (var connectionId in ConnectedUsers.Keys)
            {
                if (!MessageHistory.ContainsKey(connectionId))
                {
                    MessageHistory[connectionId] = new List<string>();
                }
                MessageHistory[connectionId].Add(formattedMessage);
            }

            await Clients.All.ReceiveMessage(formattedMessage);
        }

        // Méthode appelée lors de la connexion
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User?.Identity?.Name ?? "Utilisateur inconnu";
            string role = Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "Role inconnu";
            string phoneNumber = Context.User?.FindFirst(ClaimTypes.MobilePhone)?.Value ?? "N/A";

            // Ajouter l'utilisateur à un groupe Administrateurs s'il en fait partie
            if (role == "Administrateur")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Administrateurs");
            }

            ConnectedUsers[Context.ConnectionId] = (userName, phoneNumber ?? "N/A");

            if (!MessageHistory.ContainsKey(Context.ConnectionId))
            {
                MessageHistory[Context.ConnectionId] = new List<string>();
            }

            await base.OnConnectedAsync();
            await Clients.Caller.Notify($"Bienvenue, {userName}! Vous êtes connecté(e).");
        }


        // Méthode appelée lors de la déconnexion
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.FindFirst(ClaimTypes.Role)?.Value == "Administrateur")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Administrateurs");
            }
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
            {
                // Préparer l'historique des messages pour cet utilisateur
                if (MessageHistory.TryRemove(Context.ConnectionId, out var history))
                {
                    string currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string messageHistoryText = string.Join("\n", history);
                    string messageToSend = $"Historique de votre discussion le {currentDate} :\n{messageHistoryText}";

                    // Envoyer l'historique des messages via Twilio
                    if (!string.IsNullOrEmpty(userInfo.PhoneNumber) && userInfo.PhoneNumber != "N/A")
                    {
                        await _smsSenderService.SendSmsAsync("+1"+userInfo.PhoneNumber, messageToSend);
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }



        // Méthode pour envoyer un message à un administrateur spécifique
        public async Task SendMessageToAdmin(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                await Clients.Caller.Notify("Le message ne peut pas être vide.");
                return;
            }

            string sender = Context.User?.Identity?.Name ?? "Utilisateur inconnu";
            string adminGroup = "Administrateurs";

            // Vérifie si le groupe existe et envoie le message
            await Clients.Group(adminGroup).ReceiveMessage($"{sender} (Administrateur) : {message}");
        }


        // Méthode pour envoyer un message à un utilisateur spécifique
        public async Task SendMessageToUser(string message, string connectionId)
        {
            await Clients.Client(connectionId).ReceiveMessage(GetMessageToSend(message));
        }


        // Méthode utilitaire pour formater un message
        private string GetMessageToSend(string originalMessage)
        {
            return $"(ID de connexion: {Context.ConnectionId}). Message diffusé : {originalMessage}";
        }
    }
}

