using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using punto_client.Models;

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

        _connection.On<string>("MettreAJourPlateau", (jsonPlateau) =>
        {
            // Désérialise directement le JSON en une liste de tuiles
            List<Tuile> tuiles = JsonConvert.DeserializeObject<List<Tuile>>(jsonPlateau);

            // Crée un plateau et y assigne les tuiles
            Plateau plateau = new Plateau { TuilesPlacees = tuiles };

            // Affiche le plateau mis à jour
            AfficherPlateau(plateau);
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

    /// <summary>
    /// Affiche le plateau en console.
    /// </summary>
    public void AfficherPlateau(Plateau plateau)
    {
        int tailleGrille = 6 * 2 - 1; // 6 de rayon sur une grille de 11 de diametre
        var grille = new string[tailleGrille, tailleGrille];
        var tuilesPlacees = plateau.TuilesPlacees;

        // Initialise la grille avec des "." pour les emplacements vides
        for (int i = 0; i < tailleGrille; i++)
        {
            for (int j = 0; j < tailleGrille; j++)
            {
                grille[i, j] = " ";
            }
        }

        // Remplit la grille avec les tuiles placées (sans affichage de couleur ici)
        foreach (var tuile in tuilesPlacees)
        {
            if (tuile.PositionX > -6 && tuile.PositionX < 6 && tuile.PositionY > -6 && tuile.PositionY < 6)
            {
                grille[tuile.PositionY + 5, tuile.PositionX + 5] = tuile.Valeur.ToString();
            }
        }

        // Affiche la grille dans la console avec des couleurs
        Console.WriteLine("Plateau :");
        Console.WriteLine(" X 5 4 3 2 1 0 1 2 3 4 5 6");
        Console.WriteLine("  ________________________");
        for (int i = 0; i < tailleGrille; i++)
        {
            if (i == 0) Console.Write(" Y|");
            else if (i >= 5) Console.Write($" {i - 5}|"); // Affichage des nombres positifs avec un espace devant
            else Console.Write($"{i - 5}|");

            for (int j = 0; j < tailleGrille; j++)
            {
                // Cherche la tuile à cette position
                var tuile = tuilesPlacees.FirstOrDefault(t => t.PositionX == j - 5 && t.PositionY == i - 5);
                if (tuile != null)
                {
                    // Change la couleur en fonction du joueur
                    AfficherMessageDeJoueur(tuile.Proprietaire.OrdreDeJeu, grille[i, j] + " ");
                }
                else
                {
                    // Affiche les emplacements vides avec la couleur par défaut
                    Console.Write(grille[i, j] + " ");
                }
            }
            Console.WriteLine(); // Retour à la ligne après chaque rangée
        }

        // Réinitialise la couleur à la fin
        Console.ResetColor();
    }

    /// <summary>
    /// Affiche un message en couleur dans la console en fonction de l'index du joueur.
    /// </summary>
    /// <param name="numeroJoueur"></param>
    /// <param name="message"></param>
    public static void AfficherMessageDeJoueur(int numeroJoueur, string message)
    {
        Console.ForegroundColor = numeroJoueur switch
        {
            1 => ConsoleColor.Magenta,// Joueur 1 - Magenta
            2 => ConsoleColor.Cyan,// Joueur 2 - Cyan
            3 => ConsoleColor.Green,// Joueur 3 - Vert
            4 => ConsoleColor.Yellow,// Joueur 4 - Jaune
            5 => ConsoleColor.Gray,// Joueur 5 - Gris
            6 => ConsoleColor.Red,// Joueur 6 - Rouge
            7 => ConsoleColor.DarkYellow,// Joueur 7 - Jaune foncé
            8 => ConsoleColor.DarkRed,// Joueur 8 - rouge foncé
            9 => ConsoleColor.DarkGray,// Joueur 9 - gris foncé
            10 => ConsoleColor.DarkCyan,// Joueur 10 - cyan foncé
            _ => ConsoleColor.White,// Autres joueurs ou cas par défaut
        };
        Console.Write(message);
        Console.ResetColor();
    }
}

