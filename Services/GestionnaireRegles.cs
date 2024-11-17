using punto_client.Models;

namespace punto_client.Services;

/// <summary>
/// Classe permettant de gérer les règles du jeu.
/// </summary>
public class GestionnaireRegles
{
    /// <summary>
    /// Renvoie VRAI si le joueur peut placer la tuile sur le plateau.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <param name="tuile"></param>
    /// <returns></returns>
    public static bool PeutPlacerTuile(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        if (tuile == null) return false;

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

    /// <summary>
    /// Vérifie l'impact de la tuile placée sur les conditions de victoire du joueur.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <param name="tuileAPoser"></param>
    /// <param name="tuilesAligneesPourGagner"></param>
    /// <returns></returns>
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
        var positionsPossibles = new List<Tuile>();

        // Pour chaque tuile distincte dans la main
        foreach (int valeurTuile in joueur.TuilesDansLaMain.Distinct())
        {
            positionsPossibles.AddRange(ObtenirPositionsPossibles(plateau, joueur, valeurTuile));
        }

        return positionsPossibles;
    }

    /// <summary>
    /// Obtenir toutes les positions de tuile possibles pour une tuile donnée.
    /// </summary>
    public static List<Tuile> ObtenirPositionsPossibles(Plateau plateau, Joueur joueur, int valeurTuile)
    {
        // Récupère les positions déjà jouées par l'adversaire avec une tuile plus faible (superposer)
        List<Tuile> positionsSuperposables = new List<Tuile>(
            plateau.TuilesPlacees
                .Where(t => t.Proprietaire.Nom != joueur.Nom && t.Valeur < valeurTuile)
                .Select(t => new Tuile(joueur, valeurTuile, t.PositionX, t.PositionY)));

        // Récupère les positions adjacentes à des tuiles jouées qui restent dans la grille (juxtaposer)
        List<Tuile> positionsAdjacentes = new List<Tuile>(
            plateau.TuilesPlacees.SelectMany(t =>
                new List<Tuile>
                {
                    new Tuile(joueur, valeurTuile, t.PositionX - 1, t.PositionY),     // Gauche
                    new Tuile(joueur, valeurTuile, t.PositionX + 1, t.PositionY),     // Droite
                    new Tuile(joueur, valeurTuile, t.PositionX, t.PositionY - 1),     // Haut
                    new Tuile(joueur, valeurTuile, t.PositionX, t.PositionY + 1),     // Bas
                    new Tuile(joueur, valeurTuile, t.PositionX - 1, t.PositionY - 1), // Diagonale haut-gauche
                    new Tuile(joueur, valeurTuile, t.PositionX + 1, t.PositionY - 1), // Diagonale haut-droite
                    new Tuile(joueur, valeurTuile, t.PositionX - 1, t.PositionY + 1), // Diagonale bas-gauche
                    new Tuile(joueur, valeurTuile, t.PositionX + 1, t.PositionY + 1)  // Diagonale bas-droite
                }))
            .Distinct()                         // Elimine les positions doublons
            .Where(p => !plateau.TuilesPlacees  // Ne tiens pas compte de ses propres positions déjà couvertes
                .Any(t => t.Proprietaire == joueur
                    && t.PositionX == p.PositionX && t.PositionY == p.PositionY))
            .ToList();

        var positionsPossibles = new List<Tuile>();
        positionsPossibles.AddRange(positionsSuperposables);
        positionsPossibles.AddRange(positionsAdjacentes);

        return positionsPossibles
            .Where(t => PeutPlacerTuile(plateau, joueur, t)) // Uniquement les tuiles dans la grille et posable
            .OrderBy(p => p.PositionX) // Tri par position X
            .ThenBy(p => p.PositionY) // Puis pas position Y
            .ToList();
    }

    public static bool SuperposeUneTuileAdverseExistante(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        return plateau.TuilesPlacees
            .Any(t => t.Proprietaire != joueur
                && t.PositionX == tuile.PositionX
                && t.PositionY == tuile.PositionY
                && t.Valeur < tuile.Valeur);
    }

    public static int CompterTuilesAdjacentesDuJoueur(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        // Définir les positions adjacentes (8 directions possibles autour d'une tuile)
        var directions = new (int dx, int dy)[]
        {
            (-1, -1), (-1, 0), (-1, 1),
            (0, -1),          (0, 1),
            (1, -1),  (1, 0), (1, 1)
        };

        // Compter les tuiles du joueur autour de la tuile donnée
        return plateau.TuilesPlacees
            .Count(t => t.Proprietaire == joueur &&
                directions.Any(d => t.PositionX == tuile.PositionX + d.dx && t.PositionY == tuile.PositionY + d.dy));
    }

}
