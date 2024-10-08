﻿using punto_client.Models;
using punto_client.Strategie;

namespace punto_client.Services;

public class GestionnaireJeuLocal
{
    private Jeu Jeu;
    public Jeu ObtenirJeu() => Jeu;
    public EtatJeu ObtenirEtatJeu() => Jeu.EtatJeu;
    public void CreerNouveauJeu(int nombreJoueurs) => Jeu = new Jeu(nombreJoueurs);
    public Joueur ObtenirJoueurDevantJouer() => Jeu.AuTourDuJoueur;
    public Plateau ObtenirPlateau() => Jeu.Plateau;

    /// <summary>
    /// Ajoute un joueur à la partie (si c'est possible).
    /// </summary>
    /// <param name="numero"></param>
    /// <param name="ordinateur"></param>
    public void AjouterUnJoueur(int numero, bool ordinateur = false)
    {
        // On ajoute pas des joueurs si la partie n'est pas en attente de joueur
        if (Jeu.EtatJeu != EtatJeu.EnAttenteDeJoueur)
        {
            return;
        }

        // A ce stade, la partie est dans l'état EtatJeu.EnAttenteDeJoueur
        // et le nombre de joueurs max n'est pas atteint.
        // On peut alors ajouter un joueur.

        // Création du joueur et de sa pioche ou de l'ordinateur
        var message = ordinateur
            ? $"Création de l'ordinateur CPU {numero}.\n"
            : $"Joueur {numero}, entrez votre pseudo : ";
        AfficherMessageDeJoueur(numero, message);
        string nomJoueur = ordinateur
            ? $"CPU {numero}"
            : Console.ReadLine();

        var joueur = new Joueur
        {
            Nom = nomJoueur,
            EstUnOrdinateur = ordinateur,
            OrdreDeJeu = Jeu.Joueurs.Count + 1, // nombre de joueurs + 1
            TuilesDansLaPioche = CreerTuilesPourJoueur(),
            TuilesDansLaMain = new List<int>()
        };

        // Pioche 2 tuiles
        PiocherTuile(joueur);
        PiocherTuile(joueur);

        // Ajoute le joueur à la partie
        Jeu.Joueurs.Add(joueur);

        // Si c'est le 1er joueur, place la tuile au centre
        if (joueur.OrdreDeJeu == 1)
        {
            var tuile = new Tuile
            {
                Valeur = joueur.TuilesDansLaMain.First(),
                PositionX = 0,
                PositionY = 0,
                Proprietaire = joueur
            };
            Jeu.Plateau.TuilesPlacees.Add(tuile);
            Console.WriteLine($"{joueur.Nom} a placé une tuile de valeur {tuile.Valeur} en position ({tuile.PositionX}, {tuile.PositionY}).");
        }

        // Démarre la partie si le nombre maximum de joueurs est atteint
        if (Jeu.Joueurs.Count == Jeu.NombreMaxDeJoueurs && Jeu.EtatJeu == EtatJeu.EnAttenteDeJoueur)
        {
            Jeu.EtatJeu = EtatJeu.EnCours;
            Jeu.AuTourDuJoueur = Jeu.Joueurs[1]; // 2e joueur
            AfficherMessageDeJoueur(Jeu.AuTourDuJoueur.OrdreDeJeu, $"C'est au tour de {Jeu.AuTourDuJoueur.Nom} de jouer.\n");
        }
    }

    /// <summary>
    /// Affiche le plateau en console.
    /// </summary>
    public void AfficherPlateau()
    {
        int tailleGrille = 6 * 2 - 1; // 6 de rayon sur une grille de 11 de diametre
        var grille = new string[tailleGrille, tailleGrille];
        var tuilesPlacees = Jeu.Plateau.TuilesPlacees;

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
            else if (i >= 5) Console.Write($" {i-5}|"); // Affichage des nombres positifs avec un espace devant
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
    /// Permet d'ajouter des ordinateurs à la partie.
    /// </summary>
    /// <returns></returns>
    public List<IGestionnaireStrategie> DefinirIA()
    {
        var strategies = new List<IGestionnaireStrategie>();

        // Choix du nombre d'IA
        int maxCpu = Jeu.NombreMaxDeJoueurs - Jeu.Joueurs.Count;
        Console.Write($"Précisez le nombre de joueurs gérés par l'ordinateur (maximum {maxCpu}) : ");
        int choix = -1;
        do
        {
            int.TryParse(Console.ReadLine(), out choix);
        } while (choix < 0 && choix >= maxCpu);

        for (int i = 0; i < choix; i++)
        {
            AjouterUnJoueur(i+1, true); // Ajoute un ordinateur
            strategies.Add(new GestionnaireStrategieAleatoire()); // Définit la stratégie utilisée
        }

        return strategies;
    }

    /// <summary>
    /// Place une tuile sur le plateau si c'est possible.
    /// Si la tuile est placée et ne déclenche pas la fin du jeu, le jeu passe au joueur suivant.
    /// </summary>
    /// <param name="tuile"></param>
    public void PlacerTuile(Tuile tuile)
    {
        // Vérifie si c'est le tour du joueur actuel et si la tuile peut être placée
        var joueur = Jeu.AuTourDuJoueur;
        if (Jeu.EtatJeu == EtatJeu.EnCours                                      // Partie en cours
            && joueur != null                                                   // Joueur dans la partie
            && !GestionnaireRegles.PeutPlacerTuile(Jeu.Plateau, joueur, tuile)) // Règles du jeu
        {
            Console.WriteLine($"La tuile {tuile.Valeur} ne peut pas être placée en ({tuile.PositionX}, {tuile.PositionY}).");

            // Gestion des penalités
            joueur.Penalite++;
            Console.WriteLine($"Le joueur {joueur.Nom} a reçu une pénalité ({joueur.Penalite}/3).");
            if (joueur.Penalite >= 3) // Disqualifie le joueur après 3 pénalités
            {
                Console.WriteLine($"Le joueur {joueur.Nom} a été disqualifié.");
                Jeu.Joueurs.Remove(joueur);
                if (Jeu.Joueurs.Count == 1) // Il reste un seul joueur : il est désigné comme vainqueur
                {
                    Jeu.EtatJeu = EtatJeu.Termine;
                    Console.WriteLine($"{Jeu.Joueurs.Last().Nom} a gagné la partie !");
                    return;
                }
                else
                {
                    PasserAuJoueurSuivant();
                }
            }
            return;
        }

        // Vérifie et retire la tuile existante c'est un juxtaposition
        var tuileExistante = Jeu.Plateau.TuilesPlacees.FirstOrDefault(t => t.PositionX == tuile.PositionX && t.PositionY == tuile.PositionY);
        if (tuileExistante != null)
        {
            Jeu.Plateau.TuilesPlacees.Remove(tuileExistante);
        }

        // Ajoute la tuile sur le plateau
        Jeu.Plateau.TuilesPlacees.Add(tuile);

        // Retire la tuile de la main du joueur et pioche une nouvelle
        joueur.TuilesDansLaMain.Remove(tuile.Valeur);
        PiocherTuile(joueur);

        Console.WriteLine($"{joueur.Nom} a placé une tuile de valeur {tuile.Valeur} en position ({tuile.PositionX}, {tuile.PositionY}).");

        // Vérifier si le joueur a gagné
        if (VerifierConditionsVictoire(joueur))
        {
            Jeu.EtatJeu = EtatJeu.Termine;
            Console.WriteLine($"{joueur.Nom} a gagné la partie !");
            return;
        }

        // Passer au joueur suivant
        PasserAuJoueurSuivant();
    }

    /// <summary>
    /// Permet de passer au tour du joueur suivant.
    /// </summary>
    private void PasserAuJoueurSuivant()
    {
        var indexJoueurActuel = Jeu.Joueurs.IndexOf(Jeu.AuTourDuJoueur);
        var indexJoueurSuivant = (indexJoueurActuel + 1) % Jeu.Joueurs.Count;
        Jeu.AuTourDuJoueur = Jeu.Joueurs[indexJoueurSuivant];

        AfficherMessageDeJoueur(Jeu.AuTourDuJoueur.OrdreDeJeu, $"C'est au tour de {Jeu.AuTourDuJoueur.Nom} de jouer.\n");
    }

    /// <summary>
    /// Vérifie si le joueur a aligné 4 tuiles horizontalement, verticalement ou en diagonale
    /// </summary>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public bool VerifierConditionsVictoire(Joueur joueur)
    {
        // Récupère les tuiles du joueur
        var tuilesJoueur = Jeu.Plateau.TuilesPlacees
                            .Where(t => t.Proprietaire.Nom == joueur.Nom)
                            .ToList();

        // Parcourir chaque tuile du joueur pour vérifier les alignements
        foreach (var tuile in tuilesJoueur)
        {
            // Vérification horizontale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 0)) return true;

            // Vérification verticale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 0, 1)) return true;

            // Vérification diagonale gauche-droite (bas-droite)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 1)) return true;

            // Vérification diagonale droite-gauche (bas-gauche)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, -1)) return true;
        }

        return false; // Aucun alignement trouvé
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
            _ => ConsoleColor.White,// Autres joueurs ou cas par défaut
        };
        Console.Write(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Cette méthode vérifie si 4 tuiles sont alignées dans une direction spécifique
    /// </summary>
    /// <param name="tuilesJoueur"></param>
    /// <param name="tuile"></param>
    /// <param name="deltaX"></param>
    /// <param name="deltaY"></param>
    /// <returns></returns>
    private static bool VerifierAlignementDirection(List<Tuile> tuilesJoueur, Tuile tuile, int deltaX, int deltaY)
    {
        int count = 1; // Compte la tuile actuelle

        // Vérifie dans la direction positive (droite/bas)
        for (int i = 1; i < 4; i++)
        {
            var tuileSuivante = tuilesJoueur.FirstOrDefault(t =>
                t.PositionX == tuile.PositionX + i * deltaX &&
                t.PositionY == tuile.PositionY + i * deltaY);
            if (tuileSuivante != null)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        // Vérifie dans la direction négative (gauche/haut)
        for (int i = 1; i < 4; i++)
        {
            var tuilePrecedente = tuilesJoueur.FirstOrDefault(t =>
                t.PositionX == tuile.PositionX - i * deltaX &&
                t.PositionY == tuile.PositionY - i * deltaY);
            if (tuilePrecedente != null)
            {
                count++;
            }
            else
            {
                break;
            }
        }

        // Si on a trouvé 4 tuiles alignées
        return count >= 4;
    }

    /// <summary>
    /// Permet de créer les tuiles dans la pioche du joueur. 
    /// Les tuiles sont mélangées au hasard.
    /// </summary>
    /// <returns></returns>
    private static List<int> CreerTuilesPourJoueur()
    {
        // Mélange des tuiles
        var tuiles = new List<int>
        {
            1, 1,
            2, 2,
            3, 3,
            4, 4,
            5, 5,
            6, 6,
            7, 7,
            8, 8,
            9, 9
        };

        // Mélange des tuiles pour plus d'aléatoire
        var aleatoire = new Random();
        return tuiles.OrderBy(t => aleatoire.Next()).ToList();
    }

    /// <summary>
    /// Pioche la première tuile de la pioche du joueur.
    /// </summary>
    /// <param name="joueur"></param>
    private static void PiocherTuile(Joueur joueur)
    {
        // Ne fais rien si le joueur n'a plus de tuile
        if (!joueur.TuilesDansLaPioche.Any())
        {
            return;
        }

        // Pioche la première tuile
        var tuilePiochee = joueur.TuilesDansLaPioche.First();
        joueur.TuilesDansLaMain.Add(tuilePiochee);
        // Retire la tuile de la pioche
        joueur.TuilesDansLaPioche = joueur.TuilesDansLaPioche.Skip(1).ToList();
    }
}
