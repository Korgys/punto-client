//using Microsoft.AspNetCore.SignalR.Client;
using punto_client.IA;
using punto_client.Models;
using punto_client.Services;

public class Program
{
    public static void Main(string[] args)
    {
        GestionnaireJeu gestionnaireJeu = new GestionnaireJeu();

        // Crée une nouvelle partie
        gestionnaireJeu.CreerNouveauJeu();

        // Ajoute 2 joueurs
        while (gestionnaireJeu.ObtenirEtatJeu() == EtatJeu.EnAttenteDeJoueur)
        {
            gestionnaireJeu.AjouterUnJoueur();
        }

        // Commencer la partie
        while(gestionnaireJeu.ObtenirEtatJeu() == EtatJeu.EnCours)
        {
            // Affichage du plateau
            gestionnaireJeu.AfficherPlateau();

            // Récupère le joueur qui doit jouer
            var joueur = gestionnaireJeu.ObtenirJoueurDevantJouer();

            // Affichage des tuiles jouables
            joueur.TuilesEnMain.ForEach(t => Console.Write("{0}\t", t));

            // Récupère le prochain coup du joueur
            var tuile = GestionnaireStrategieJoueurManuel.ObtenirProchainCoup(
                gestionnaireJeu.ObtenirPlateau(),
                joueur);

            // Si le joueur peut placer sa tuile
            if (gestionnaireJeu.PeutPlacerTuile(tuile))
            {
                // On place la tuile
                gestionnaireJeu.PlacerTuile(tuile);
            }

            // Fin de l'itération, on passe automatiquement au tour suivant
        }

        // Fin de la partie, affichage du plateau et du vainqueur
        gestionnaireJeu.AfficherPlateau();
        var jeu = gestionnaireJeu.ObtenirJeu();
        if (jeu.Vainqueur != null)
        {
            Console.WriteLine($"\nLe joueur {jeu.Vainqueur.Nom} a gagné la partie");
        }
    }

    // TODO: Partie WebSocket à paufiner
    //static async Task Main(string[] args)
    //{
    //    // Création de la connexion au Hub SignalR
    //    var connection = new HubConnectionBuilder()
    //        .WithUrl("http://localhost:5000/gameHub")
    //        .Build();

    //    // Gestion des événements reçus du serveur
    //    connection.On<string, int, int, int>("RecevoirTuile", (joueur, x, y, valeur) =>
    //    {
    //        Console.WriteLine($"Tuile jouée par {joueur} à ({x}, {y}) avec la valeur {valeur}");
    //    });

    //    connection.On<string>("JoueurARejoint", (joueur) =>
    //    {
    //        Console.WriteLine($"{joueur} a rejoint la partie.");
    //    });

    //    // Tente de se connecter au serveur
    //    try
    //    {
    //        Console.WriteLine("Connexion en cours...");
    //        await connection.StartAsync();
    //        Console.WriteLine("Connecté au serveur SignalR.");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Erreur lors de la connexion : {ex.Message}");
    //        return;
    //    }

    //    // Interactions utilisateur
    //    string joueur = "Joueur1";
    //    while (true)
    //    {
    //        Console.WriteLine("Choisissez une action :\n1. Rejoindre une partie\n2. Jouer une tuile");
    //        var choix = Console.ReadLine();

    //        if (choix == "1")
    //        {
    //            // Rejoins une partie
    //            await connection.InvokeAsync("RejoindrePartie", joueur);
    //            Console.WriteLine($"{joueur} a rejoint la partie.");
    //        }
    //        else if (choix == "2")
    //        {
    //            // Joue une tuile
    //            Console.WriteLine("Entrez les coordonnées x et y de la tuile (ex : 2 2) :");
    //            var input = Console.ReadLine().Split(' ');
    //            int x = int.Parse(input[0]);
    //            int y = int.Parse(input[1]);

    //            Console.WriteLine("Entrez la valeur de la tuile :");
    //            int valeur = int.Parse(Console.ReadLine());

    //            await connection.InvokeAsync("JouerTuile", joueur, x, y, valeur);
    //            Console.WriteLine($"Tuile jouée par {joueur} à ({x}, {y}) avec la valeur {valeur}");
    //        }
    //    }
    //}
}
