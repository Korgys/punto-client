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
}
