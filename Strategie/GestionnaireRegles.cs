using punto_client.Models;

namespace punto_client.Strategie;

/// <summary>
/// Classe permettant de gérer les règles du jeu.
/// </summary>
public class GestionnaireRegles
{
    /// <summary>
    /// Permet de créer les tuiles dans la pioche du joueur. 
    /// Les tuiles sont mélangées au hasard.
    /// </summary>
    /// <returns></returns>
    public static List<int> CreerTuilesPourJoueur()
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

    public static bool PeutPlacerTuile(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        // Récupérer les positions des tuiles déjà placées
        var tuilesPlacees = plateau.TuilesPlacees;

        // Détermine les bornes dynamiques de la grille (min et max X et Y)
        int minX = tuilesPlacees.Min(t => t.PositionX);
        int maxX = tuilesPlacees.Max(t => t.PositionX);
        int minY = tuilesPlacees.Min(t => t.PositionY);
        int maxY = tuilesPlacees.Max(t => t.PositionY);

        // Calcule la taille actuelle de la grille
        int largeurGrille = maxX - minX + 1;
        int hauteurGrille = maxY - minY + 1;

        // Vérifie que la tuile peut être placée dans une grille 6x6
        bool estDansLaGrille = Math.Max(tuile.PositionX, maxX) - Math.Min(tuile.PositionX, minX) < 6
                            && Math.Max(tuile.PositionY, maxY) - Math.Min(tuile.PositionY, minY) < 6
                            && tuile.PositionX > -6 && tuile.PositionX < 6
                            && tuile.PositionY > -6 && tuile.PositionX < 6;

        // Vérifie si la tuile est adjacente à une tuile existante
        bool estAdjacent = tuilesPlacees.Any(t =>
            Math.Abs(t.PositionX - tuile.PositionX) <= 1 &&
            Math.Abs(t.PositionY - tuile.PositionY) <= 1);

        // Vérifie s'il existe déjà une tuile à cet emplacement
        Tuile tuileExistante = tuilesPlacees
            .FirstOrDefault(t => t.PositionX == tuile.PositionX && t.PositionY == tuile.PositionY);

        // Vérifie si la tuile placée est plus forte que celle existante (ou s'il n'y a pas de tuile)
        bool estPlusForteSiPoseeSurTuileExistante = tuileExistante == null || tuileExistante.Valeur < tuile.Valeur;

        // Vérifie si toutes les conditions de placement sont remplies
        var coupAutorise = joueur != null                       // Etre un joueur de la partie
            && joueur.TuilesDansLaMain.Contains(tuile.Valeur)   // Jouer une tuile de sa main
            && estAdjacent                                      // La tuile doit être adjacente à une tuile existante
            && estPlusForteSiPoseeSurTuileExistante             // La tuile doit être plus puissante si posée sur une tuile existante
            && 1 <= tuile.Valeur && tuile.Valeur <= 9           // La valeur de la tuile doit être comprise entre 1 et 9
            && estDansLaGrille;                                 // La tuile doit être placée dans une grille 6x6

        return coupAutorise;
    }

    /// <summary>
    /// Vérifie si le joueur a aligné 4 tuiles horizontalement, verticalement ou en diagonale
    /// </summary>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public static bool VerifierConditionsVictoire(Plateau plateau, Joueur joueur, int tuilesAligneesPourGagner = 4)
    {
        // Récupère les tuiles du joueur
        var tuilesJoueur = plateau.TuilesPlacees
                            .Where(t => t.Proprietaire.Nom == joueur.Nom)
                            .ToList();

        // Parcourir chaque tuile du joueur pour vérifier les alignements
        foreach (var tuile in tuilesJoueur)
        {
            // Vérification horizontale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 0, tuilesAligneesPourGagner)) return true;

            // Vérification verticale
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 0, 1, tuilesAligneesPourGagner)) return true;

            // Vérification diagonale gauche-droite (bas-droite)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, 1, tuilesAligneesPourGagner)) return true;

            // Vérification diagonale droite-gauche (bas-gauche)
            if (VerifierAlignementDirection(tuilesJoueur, tuile, 1, -1, tuilesAligneesPourGagner)) return true;
        }

        return false; // Aucun alignement trouvé
    }

    public static bool VerifierConditionsVictoireAvecTuile(Plateau plateau, Joueur joueur, Tuile tuileAPoser, int tuilesAligneesPourGagner = 4)
    {
        // Ajoute temporairement la tuile au plateau pour simuler le coup
        var plateauTemporaire = new Plateau
        {
            TuilesPlacees = new List<Tuile>(plateau.TuilesPlacees) { tuileAPoser }
        };

        // Corrige le cas où on place une tuile par dessus une tuile existante
        IEnumerable<Tuile> doublons = plateauTemporaire.TuilesPlacees
            .Where(t => t.PositionX == tuileAPoser.PositionX && t.PositionY == tuileAPoser.PositionY);
        if (doublons.Count() == 2)
        {
            plateauTemporaire.TuilesPlacees.Remove(doublons.OrderBy(t => t.Valeur).First());
        }

        // Utilise la méthode VerifierConditionsVictoire pour vérifier si ce coup entraîne une victoire
        return VerifierConditionsVictoire(plateauTemporaire, joueur, tuilesAligneesPourGagner);
    }

    /// <summary>
    /// Cette méthode vérifie si 4 tuiles sont alignées dans une direction spécifique
    /// </summary>
    /// <param name="tuilesJoueur"></param>
    /// <param name="tuile"></param>
    /// <param name="deltaX"></param>
    /// <param name="deltaY"></param>
    /// <returns></returns>
    public static bool VerifierAlignementDirection(List<Tuile> tuilesJoueur, Tuile tuile, int deltaX, int deltaY, int tuilesAligneesPourGagner = 4)
    {
        int count = 1; // Compte la tuile actuelle

        // Vérifie dans la direction positive (droite/bas)
        for (int i = 1; i < tuilesAligneesPourGagner; i++)
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
        for (int i = 1; i < tuilesAligneesPourGagner; i++)
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
        return count >= tuilesAligneesPourGagner;
    }

    /// <summary>
    /// Génère tous les coups possibles pour un joueur en fonction du plateau actuel.
    /// </summary>
    public static List<Tuile> ObtenirTousLesCoupsPossibles(Plateau plateau, Joueur joueur)
    {
        var coupsPossibles = new List<Tuile>();

        foreach (int valeurTuile in joueur.TuilesDansLaMain)
        {
            var tuile = new Tuile { Proprietaire = joueur, Valeur = valeurTuile };

            var positionsPossibles = ObtenirPositionsPossibles(plateau, tuile);

            foreach (var position in positionsPossibles)
            {
                var coup = new Tuile
                {
                    Proprietaire = joueur,
                    Valeur = valeurTuile,
                    PositionX = position.X,
                    PositionY = position.Y
                };

                // Si on peut placer la tuile et qu'elle n'est pas déjà référencée dans les coups possibles
                if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, coup)
                    && !coupsPossibles.Any(c => c.Valeur == coup.Valeur && c.PositionX == coup.PositionX && c.PositionY == coup.PositionY))
                {
                    coupsPossibles.Add(coup);
                }
            }
        }

        return coupsPossibles;
    }

    /// <summary>
    /// Obtenir les positions possibles pour une tuile donnée (méthode déjà existante).
    /// </summary>
    public static List<Position> ObtenirPositionsPossibles(Plateau plateau, Tuile tuile)
    {
        var positionsPossibles = new List<Position>();

        // Détermine les bornes dynamiques de la grille (min et max X et Y)
        int minX = plateau.TuilesPlacees.Min(t => t.PositionX);
        int maxX = plateau.TuilesPlacees.Max(t => t.PositionX);
        int minY = plateau.TuilesPlacees.Min(t => t.PositionY);
        int maxY = plateau.TuilesPlacees.Max(t => t.PositionY);

        // Positions déjà jouées par l'adversaire avec une tuile plus faible
        positionsPossibles.AddRange(
            plateau.TuilesPlacees
                .Where(t => t.Proprietaire.Nom != tuile.Proprietaire.Nom && t.Valeur < tuile.Valeur)
                .Select(t => new Position(t)));

        // Positions adjacentes à des tuiles jouées qui restent dans la grille
        foreach (var t in plateau.TuilesPlacees)
        {
            var positionsAdjacentes = new List<Position>
            {
                new Position(t.PositionX - 1, t.PositionY),     // Gauche
                new Position(t.PositionX + 1, t.PositionY),     // Droite
                new Position(t.PositionX, t.PositionY - 1),     // Haut
                new Position(t.PositionX, t.PositionY + 1),     // Bas
                new Position(t.PositionX - 1, t.PositionY - 1), // Diagonale haut-gauche
                new Position(t.PositionX + 1, t.PositionY - 1), // Diagonale haut-droite
                new Position(t.PositionX - 1, t.PositionY + 1), // Diagonale bas-gauche
                new Position(t.PositionX + 1, t.PositionY + 1)  // Diagonale bas-droite
            };

            // Ajoute les positions adjacentes si elles sont dans la grille 6x6 et respectent les règles
            foreach (var pos in positionsAdjacentes)
            {
                tuile.PositionX = pos.X;
                tuile.PositionY = pos.Y;

                if (GestionnaireRegles.PeutPlacerTuile(plateau, tuile.Proprietaire, tuile))
                {
                    positionsPossibles.Add(pos);
                }
            }
        }

        // Ne tiens pas compte de ses propres positions déjà couvertes
        // Ainsi que des positions en double
        positionsPossibles = positionsPossibles
            .Where(p => !plateau.TuilesPlacees
                .Any(t => t.Proprietaire == tuile.Proprietaire
                    && t.PositionX == p.X && t.PositionY == p.Y))
            .Distinct()
            .OrderBy(p => p.X)
            .ThenBy(p => p.Y)
            .ToList();

        return positionsPossibles;
    }
}
