using punto_client.Models;
using punto_client.Services;
using punto_client.Strategie;

public class Program
{
    private static IGestionnaireStrategie[] _strategiesIA; 

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

    public static void SimulerPartieLocales(int simulation, bool afficherDetailsParties = true)
    {
        List<int> victoiresCpu = new List<int> { 0, 0 };

        for (int i = 1; i <= simulation; i++)
        {
            GestionnaireJeuLocal gestionnaireJeu = new GestionnaireJeuLocal();

            // Crée une nouvelle partie
            int nombreJoueurs = 2;
            gestionnaireJeu.CreerNouveauJeu(nombreJoueurs);
            _strategiesIA = gestionnaireJeu.DefinirIA(
                new GestionnaireStrategieAleatoire(),
                new GestionnaireStrategieAleatoire());

            // Commence la partie
            while (gestionnaireJeu.ObtenirEtatJeu() == EtatJeu.EnCours)
            {
                if (afficherDetailsParties) gestionnaireJeu.AfficherPlateau();
                var joueur = gestionnaireJeu.ObtenirJoueurDevantJouer();
                if (afficherDetailsParties)
                {
                    GestionnaireJeuLocal.AfficherMessageDeJoueur(joueur.OrdreDeJeu, $"Tuiles : [{string.Join(',', joueur.TuilesDansLaMain)}]\n");
                }

                var tuile = _strategiesIA[joueur.OrdreDeJeu - 1].ObtenirProchainCoup(gestionnaireJeu.ObtenirPlateau(), joueur);
                gestionnaireJeu.PlacerTuile(tuile);
            }

            // Fin de la partie, affichage du plateau et du vainqueur
            if (afficherDetailsParties) gestionnaireJeu.AfficherPlateau();
            var jeu = gestionnaireJeu.ObtenirJeu();
            if (jeu.Vainqueur != null)
            {
                Console.WriteLine($"Le joueur {jeu.Vainqueur.Nom} a gagné la partie");
                victoiresCpu[jeu.Vainqueur.OrdreDeJeu - 1]++;
            }
            else
            {
                Console.WriteLine($"C'est une égalité !");
            }
        }

        Console.WriteLine();
        for (int i = 1; i <= victoiresCpu.Count; i++)
        {
            Console.WriteLine($"Victoire CPU {i} : {victoiresCpu[i - 1]}");
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
        else
        {
            Console.WriteLine("C'est une égalité !");
        }
    }

    public static async Task JouerEnLigne()
    {
        var gestionnaireJeuEnLigne = new GestionnaireJeuEnLigne();
        var strategie = new GestionnaireStrategieAgressifV2();
        
        // Connexion au serveur
        await gestionnaireJeuEnLigne.Connecter();

        Console.WriteLine("Entrez votre nom de joueur :");
        string nomDuJoueur = Console.ReadLine();

        // Rejoindre la partie
        await gestionnaireJeuEnLigne.RejoindrePartie(nomDuJoueur);

        // Récupère le joueur
        await gestionnaireJeuEnLigne.ObtenirJoueur();
        Joueur joueur = gestionnaireJeuEnLigne._joueur;

        // Tant que la partie n'est pas terminée
        while (gestionnaireJeuEnLigne._etatJeu != EtatJeu.Termine.ToString())
        {
            // Récupère l'état du jeu
            bool doitJouer = false;

            // Attends tant que c'est pas à nous de jouer
            do
            {
                await gestionnaireJeuEnLigne.ObtenirEtatJeu();
                doitJouer = gestionnaireJeuEnLigne._doitJouer;
                Thread.Sleep(2000);

                if (gestionnaireJeuEnLigne._etatJeu == EtatJeu.Termine.ToString())
                {
                    Console.WriteLine("La partie est finie.");
                    return;
                }
            } while (!doitJouer);

            // Récupère le plateau
            await gestionnaireJeuEnLigne.ObtenirPlateau();
            Plateau plateau = gestionnaireJeuEnLigne._plateau;

            // Récupère les tuiles en main
            do
            {
                await gestionnaireJeuEnLigne.ObtenirMainJoueur();
                joueur.TuilesDansLaMain = gestionnaireJeuEnLigne._tuilesEnMain;
            } while (joueur.TuilesDansLaMain == null);
            
            // Choix des informations de la tuile
            Tuile tuile = strategie.ObtenirProchainCoup(plateau, joueur);

            // Jouer une tuile
            await gestionnaireJeuEnLigne.JouerTuile(joueur.Nom, tuile.PositionX, tuile.PositionY, tuile.Valeur);
        }

        Console.WriteLine("La partie est finie.");
    }
}
