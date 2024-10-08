﻿using punto_client.Models;
using punto_client.Services;
using punto_client.Strategie;

public class Program
{
    private static List<IGestionnaireStrategie> _strategiesIA = new List<IGestionnaireStrategie>(); 

    public static async Task Main(string[] args)
    {
        // Si le programme est lancé avec un argument
        if (args != null && args.Count() == 1)
        {
            if (args[0] == "online") await JouerEnLigne();
            else if (args[0] == "offline") JouerEnLocal();
        }
        else
        {
            Console.WriteLine("Quel mode de jeu souhaitez-vous ?");
            Console.WriteLine("1. En local");
            Console.WriteLine("2. En ligne");
            Console.Write("Votre choix : ");

            var choix = Console.ReadLine();
            if (choix == "1") // En local
            {
                JouerEnLocal();
            }
            else if (choix == "2") // En ligne
            {
                await JouerEnLigne();
            }
        }
    }

    public static void JouerEnLocal()
    {
        GestionnaireJeuLocal gestionnaireJeu = new GestionnaireJeuLocal();

        int nombreJoueurs = -1;
        do
        {
            Console.Write("Nombre de joueurs : ");
            int.TryParse(Console.ReadLine(), out nombreJoueurs);
        } while (nombreJoueurs < 2 && 4 < nombreJoueurs);

        // Crée une nouvelle partie
        gestionnaireJeu.CreerNouveauJeu(nombreJoueurs);

        // Gère les joueurs contrôlés par l'ordinateur
        _strategiesIA = gestionnaireJeu.DefinirIA();

        // Ajoute des joueurs tant que le lobby n'est pas plein
        int compteurJoueur = 1;
        while (gestionnaireJeu.ObtenirEtatJeu() == EtatJeu.EnAttenteDeJoueur)
        {
            gestionnaireJeu.AjouterUnJoueur(compteurJoueur);
            compteurJoueur++;
        }

        // Commence la partie
        while (gestionnaireJeu.ObtenirEtatJeu() == EtatJeu.EnCours)
        {
            // Affichage du plateau
            gestionnaireJeu.AfficherPlateau();

            // Récupère le joueur qui doit jouer
            var joueur = gestionnaireJeu.ObtenirJoueurDevantJouer();

            // Affichage des tuiles jouables
            // Change la couleur en fonction du joueur
            GestionnaireJeuLocal.AfficherMessageDeJoueur(joueur.OrdreDeJeu, $"Tuiles : [{string.Join(',', joueur.TuilesDansLaMain)}]");

            // En fonction du type de joueur
            Tuile tuile = null;
            if (joueur.EstUnOrdinateur) // Ordinateur
            {
                // Récupère le prochain coup de l'ordinateur
                tuile = _strategiesIA[joueur.OrdreDeJeu-1].ObtenirProchainCoup(
                    gestionnaireJeu.ObtenirPlateau(), 
                    joueur);
            }
            else // Joueur
            {
                // Récupère le prochain coup du joueur
                tuile = GestionnaireStrategieJoueurManuel.ObtenirProchainCoup(
                    gestionnaireJeu.ObtenirPlateau(),
                    joueur);
            }

            // Essaie de placer la tuile et passe au joueur suivant
            gestionnaireJeu.PlacerTuile(tuile);

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

    public static async Task JouerEnLigne()
    {
        var gestionnaireJeuEnLigne = new GestionnaireJeuEnLigne();

        // Connexion au serveur
        await gestionnaireJeuEnLigne.Connecter();

        Console.WriteLine("Entrez votre nom de joueur :");
        string nomDuJoueur = Console.ReadLine();

        Console.WriteLine("Voulez-vous rejoindre une équipe ? (Laisser vide pour créer votre propre équipe)");
        string equipe = Console.ReadLine();

        // Rejoindre la partie
        await gestionnaireJeuEnLigne.RejoindrePartie(nomDuJoueur, string.IsNullOrWhiteSpace(equipe) ? null : equipe);

        while (true)
        {
            Console.WriteLine("Que voulez-vous faire ?");
            Console.WriteLine("1. Jouer une tuile");
            Console.WriteLine("2. Quitter");

            var choix = Console.ReadLine();

            if (choix == "1")
            {
                Console.WriteLine("Entrez les coordonnées (x y) de la tuile :");
                var coordonnees = Console.ReadLine().Split(' ');
                int x = int.Parse(coordonnees[0]);
                int y = int.Parse(coordonnees[1]);

                Console.WriteLine("Entrez la valeur de la tuile à jouer :");
                int valeur = int.Parse(Console.ReadLine());

                // Jouer une tuile
                await gestionnaireJeuEnLigne.JouerTuile(nomDuJoueur, x, y, valeur);
            }
            else if (choix == "2")
            {
                // Déconnexion
                await gestionnaireJeuEnLigne.Deconnecter();
                Console.WriteLine("Vous vous êtes déconnecté.");
                break;
            }
            else
            {
                Console.WriteLine("Choix non valide, veuillez réessayer.");
            }
        }
    }
}
