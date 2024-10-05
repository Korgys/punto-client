using punto_client.Models;

namespace punto_client.Strategie;

/// <summary>
/// Classe permettant de gérer les règles du jeu.
/// </summary>
public class GestionnaireRegles
{
    public static bool PeutPlacerTuile(Plateau plateau, Joueur joueur, Tuile tuile)
    {
        // Récupérer les positions des tuiles déjà placées
        var tuilesPlacees = plateau.TuilesPlacees;

        // Déterminer les bornes dynamiques de la grille (min et max X et Y)
        int minX = tuilesPlacees.Min(t => t.PositionX);
        int maxX = tuilesPlacees.Max(t => t.PositionX);
        int minY = tuilesPlacees.Min(t => t.PositionY);
        int maxY = tuilesPlacees.Max(t => t.PositionY);

        // Calculer la taille actuelle de la grille
        int largeurGrille = maxX - minX + 1;
        int hauteurGrille = maxY - minY + 1;

        // Vérifier que la tuile peut être placée dans une grille 6x6
        bool estDansLaGrille = (largeurGrille <= 6 && hauteurGrille <= 6)
            && tuile.PositionX >= minX - 1 && tuile.PositionX <= maxX + 1
            && tuile.PositionY >= minY - 1 && tuile.PositionY <= maxY + 1;

        // Vérifier si la tuile est adjacente à une tuile existante
        bool estAdjacent = tuilesPlacees.Any(t =>
            Math.Abs(t.PositionX - tuile.PositionX) <= 1 &&
            Math.Abs(t.PositionY - tuile.PositionY) <= 1);

        // Vérifier s'il existe déjà une tuile à cet emplacement
        Tuile tuileExistante = tuilesPlacees
            .FirstOrDefault(t => t.PositionX == tuile.PositionX && t.PositionY == tuile.PositionY);

        // Vérifier si la tuile placée est plus forte que celle existante (ou s'il n'y a pas de tuile)
        bool estPlusForteSiPoseeSurTuileExistante = tuileExistante == null || tuileExistante.Valeur < tuile.Valeur;

        // Vérifier si toutes les conditions de placement sont remplies
        var coupAutorise = joueur != null                       // Etre un joueur de la partie
            && joueur.TuilesDansLaMain.Contains(tuile.Valeur)   // Jouer une tuile de sa main
            && estAdjacent                                      // La tuile doit être adjacente à une tuile existante
            && estPlusForteSiPoseeSurTuileExistante             // La tuile doit être plus puissante si posée sur une tuile existante
            && 1 <= tuile.Valeur && tuile.Valeur <= 9           // La valeur de la tuile doit être comprise entre 1 et 9
            && estDansLaGrille;                                 // La tuile doit être placée dans une grille 6x6

        return coupAutorise;
    }
}
