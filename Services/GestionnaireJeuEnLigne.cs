using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using punto_client.Models;

namespace punto_client.Services;

public class GestionnaireJeuEnLigne
{
    private HubConnection _connection;

    public Joueur _joueur;
    public Plateau _plateau;
    public string _etatJeu;
    public bool _doitJouer;
    public string _auTourDeJoueur = "";
    public List<int> _tuilesEnMain;
    public TaskCompletionSource<Joueur> _joueurTcs;
    public TaskCompletionSource<Plateau> _plateauTcs;
    public TaskCompletionSource<bool> _doitJouerTcs;

    public GestionnaireJeuEnLigne()
    {
        Console.WriteLine("Tentative de connexion en cours ...");

        // Configuration de la connexion SignalR
        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5000/punto")
            .Build();

        Console.WriteLine("Connecté !");

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
            _auTourDeJoueur = joueur;
        });

        _connection.On<string>("MettreAJourEtatJeu", (etatJeu) =>
        {
            _etatJeu = etatJeu;
            // Si le tour qui commence est celui du joueur, dans ce cas c'est à nous de jouer
            _doitJouer = EtatJeu.EnCours.ToString() == _etatJeu && _joueur?.Nom == _auTourDeJoueur;            
            _doitJouerTcs.TrySetResult(_doitJouer);
        });

        _connection.On<List<int>>("MettreAJourTuilesEnMain", (tuilesEnMain) =>
        {
            if (tuilesEnMain != null)
            {
                Console.WriteLine("Vos tuiles en main : " + string.Join(", ", tuilesEnMain));
                _tuilesEnMain = tuilesEnMain;
            }
        });

        _connection.On<string>("MettreAJourPlateau", (jsonPlateau) =>
        {
            // Désérialise directement le JSON en une liste de tuiles
            List<Tuile> tuiles = JsonConvert.DeserializeObject<List<Tuile>>(jsonPlateau);

            // Crée un plateau et y assigne les tuiles
            _plateau = new Plateau { TuilesPlacees = tuiles };
            if (_plateauTcs != null) _plateauTcs.TrySetResult(_plateau);

            // Affiche le plateau mis à jour
            AfficherPlateau(_plateau);
        });

        _connection.On<string>("MettreAJourJoueur", (jsonJoueur) =>
        {
            _joueur = JsonConvert.DeserializeObject<Joueur>(jsonJoueur);
            Console.WriteLine($"Joueur mis à jour : {_joueur.Nom}");
            // Complète la TaskCompletionSource avec le joueur reçu
            if (_joueurTcs != null) _joueurTcs.TrySetResult(_joueur);
        });

        _connection.On<string>("TerminerJeu", (vainqueur) =>
        {
            Console.WriteLine($"La partie est terminée. Le vainqueur est {vainqueur}.");
            _etatJeu = EtatJeu.Termine.ToString();
        });

        _connection.Closed += async (error) =>
        {
            Console.WriteLine($"Connexion fermée : {error?.Message}");
            await Reconnecter();
        };

        _connection.Reconnecting += (error) =>
        {
            Console.WriteLine($"Reconnexion en cours : {error?.Message}");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Console.WriteLine($"Reconnexion réussie. ConnectionId : {connectionId}");
            return Task.CompletedTask;
        };

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

    public async Task RejoindrePartie(string nomDuJoueur)
    {
        try
        {
            await _connection.InvokeAsync("RejoindrePartie", nomDuJoueur);
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

    private async Task Reconnecter()
    {
        int tentative = 0;
        int maxTentatives = 5;
        int delaiEntreTentatives = 5000; // 5 secondes

        while (tentative < maxTentatives)
        {
            tentative++;
            try
            {
                Console.WriteLine($"Tentative de reconnexion {tentative}/{maxTentatives}...");
                await _connection.StartAsync();
                Console.WriteLine("Reconnexion réussie.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Échec de la reconnexion : {ex.Message}");
                if (tentative == maxTentatives)
                {
                    Console.WriteLine("Nombre maximal de tentatives atteint. Reconnexion abandonnée.");
                    break;
                }
            }
            await Task.Delay(delaiEntreTentatives);
        }
    }

    public async Task<bool> ObtenirEtatJeu()
    {
        // Crée une nouvelle TaskCompletionSource pour attendre la réponse
        _doitJouerTcs = new TaskCompletionSource<bool>();

        try
        {
            await _connection.InvokeAsync("ObtenirEtatJeu");
            return await _doitJouerTcs.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à ObtenirEtatJeu : {ex.Message}");
            return false;
        }
    }


    public async Task<Plateau> ObtenirPlateau()
    {
        _plateauTcs = new TaskCompletionSource<Plateau>();

        try
        {
            await _connection.InvokeAsync("ObtenirPlateau");
            return await _plateauTcs.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à ObtenirPlateau : {ex.Message}");
            return null;
        }
    }

    public async Task<Joueur> ObtenirJoueur()
    {
        // Crée une nouvelle TaskCompletionSource pour attendre la réponse
        _joueurTcs = new TaskCompletionSource<Joueur>();

        try
        {
            await _connection.InvokeAsync("ObtenirJoueur");
            // Attend la réponse du serveur via l'événement "RecevoirJoueur"
            return await _joueurTcs.Task;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à ObtenirJoueur : {ex.Message}");
            return null;
        }
    }

    public async Task ObtenirMainJoueur()
    {
        try
        {
            await _connection.InvokeAsync("ObtenirMainJoueur");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'appel à ObtenirMainJoueur : {ex.Message}");
        }
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

