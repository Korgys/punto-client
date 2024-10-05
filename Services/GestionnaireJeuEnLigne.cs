using Microsoft.AspNetCore.SignalR.Client;

namespace punto_client.Services;

public class GestionnaireJeuEnLigne
{
    private HubConnection _connection;

    public GestionnaireJeuEnLigne()
    {
        // Configuration de la connexion SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/gameHub") 
            .Build();

        // Gestion des événements envoyés par le serveur
        _connection.On<string>("RejoindrePartie", (joueur) =>
        {
            Console.WriteLine($"{joueur} a rejoint la partie.");
        });

        _connection.On<string, int, int, int>("JouerTuile", (joueur, x, y, valeur) =>
        {
            Console.WriteLine($"{joueur} a joué une tuile ({valeur}) en position ({x}, {y}).");
        });

        _connection.On<string>("CommencerTour", (joueur) =>
        {
            Console.WriteLine($"C'est au tour de {joueur} de jouer.");
        });

        _connection.On<List<int>>("MettreAJourTuilesEnMain", (tuilesEnMain) =>
        {
            Console.WriteLine("Vos tuiles en main : " + string.Join(", ", tuilesEnMain));
        });

        _connection.On<string>("TerminerJeu", (vainqueur) =>
        {
            Console.WriteLine($"La partie est terminée. Le vainqueur est {vainqueur}.");
        });
    }

    public async Task Connecter()
    {
        try
        {
            await _connection.StartAsync();
            Console.WriteLine("Connexion au serveur SignalR réussie.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la connexion : {ex.Message}");
        }
    }

    public async Task RejoindrePartie(string nomDuJoueur, string? equipe = null)
    {
        try
        {
            await _connection.InvokeAsync("RejoindrePartie", nomDuJoueur, equipe);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à RejoindrePartie : {ex.Message}");
        }
    }

    public async Task JouerTuile(string nomDuJoueur, int x, int y, int valeur)
    {
        try
        {
            await _connection.InvokeAsync("JouerTuile", nomDuJoueur, x, y, valeur);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à JouerTuile : {ex.Message}");
        }
    }

    public async Task Deconnecter()
    {
        await _connection.StopAsync();
    }
}

