using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

/// <summary>
/// Cherche à s'étendre en priorisant les tuiles autour des siennes.
/// </summary>
public class GestionnaireStrategieParDiffusion : IGestionnaireStrategie
{
    /// <summary>
    /// Cherche à s'étendre en priorisant les tuiles autour des siennes.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        // Liste toutes les positions possibles
        var positionsPossibles = GestionnaireRegles.ObtenirTousLesCoupsPossibles(plateau, joueur)
            .OrderByDescending(t => GestionnaireRegles.SuperposeUneTuileAdverseExistante(plateau, joueur, t))
            .ThenByDescending(t => GestionnaireRegles.CompterTuilesAdjacentesDuJoueur(plateau, joueur, t))
            .ThenBy(t => CompterTuilesAdjacentesA9(plateau, joueur, t))
            .ToList();

        // Si possibilité d'aligner 4 tuiles et de gagner, gagne la partie
        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 4))
            {
                return position;
            }
        }

        // Pour chaque adversaire, cherche à le bloquer si nécessaire (3 tuiles alignées)
        List<Joueur> adversaires = plateau.TuilesPlacees.Select(t => t.Proprietaire).Distinct().ToList();
        // Ordonne les joueurs par ordre de jeu après le tour du joueur en cours
        adversaires = adversaires
            .Where(j => j != joueur)
            .OrderBy(j => j.OrdreDeJeu % adversaires.Count).ToList();
        foreach (Joueur adversaire in adversaires)
        {
            // Bloque l'adversaire si 3 de ses tuiles sont déjà alignées
            if (GestionnaireRegles.VerifierConditionsVictoire(plateau, adversaire, 3))
            {
                foreach (Tuile position in positionsPossibles)
                {
                    // Vérifie si la tuile se trouve au milieu d'un alignement de 3
                    if (EstAuMilieuDeTrois(plateau, adversaire, position))
                    {
                        return position;
                    }
                }

                // Essaie de poser de manière à casser l'alignement de 3
                foreach (Tuile position in positionsPossibles)
                {
                    if (!GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, adversaire, position, 3))
                    {
                        return position;
                    }
                }

                // Sinon pose la tuile la plus forte sur la continuité de l'alignement de l'adversaire
                foreach (Tuile position in positionsPossibles)
                {
                    var meilleureTuile = joueur.TuilesDansLaMain.Max();
                    if (VerifierAlignementEtContinuer(plateau, adversaire, position))
                    {
                        // Place la tuile la plus forte sur la continuité de l'alignement
                        return new Tuile(joueur, meilleureTuile, position.PositionX, position.PositionY);
                    }
                }
            }
        }

        // Cherche à aligner 3 tuiles
        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 3))
            {
                return position;
            }
        }

        // Cherche à aligner 2 tuiles
        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 2))
            {
                return position;
            }
        }

        // Si aucune position favorable n'est trouvée,
        // renvoie la première position possible au hasard en prenant d'abord la plus forte
        // et celle qui permet de se superposer sur l'adversaire
        var maxValeurTuile = positionsPossibles.Any() ? positionsPossibles.Max(t => t.Valeur) : 0;
        return positionsPossibles
            .Where(t => t.Valeur == maxValeurTuile)
            .FirstOrDefault();
    }

    public static int CompterTuilesAdjacentesA9(Plateau plateau, Joueur joueur, Tuile tuile)
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
            .Count(t => t.Proprietaire != joueur 
                && t.Valeur == 9
                && directions.Any(d => t.PositionX == tuile.PositionX + d.dx && t.PositionY == tuile.PositionY + d.dy));
    }

    /// <summary>
    /// Méthode pour vérifier si une tuile est au milieu d'un alignement de 3 tuiles
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="adversaire"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool EstAuMilieuDeTrois(Plateau plateau, Joueur adversaire, Tuile position)
    {
        var directions = new (int dx, int dy)[]
        {
            (1, 0),   // Horizontal
            (0, 1),   // Vertical
            (1, 1),   // Diagonale gauche-droite
            (1, -1)   // Diagonale droite-gauche
        };

        foreach (var (dx, dy) in directions)
        {
            var tuileAvant = plateau.TuilesPlacees.FirstOrDefault(t =>
                t.Proprietaire == adversaire &&
                t.PositionX == position.PositionX - dx &&
                t.PositionY == position.PositionY - dy);

            var tuileApres = plateau.TuilesPlacees.FirstOrDefault(t =>
                t.Proprietaire == adversaire &&
                t.PositionX == position.PositionX + dx &&
                t.PositionY == position.PositionY + dy);

            // Si la tuile est entourée par deux tuiles adverses alignées, elle est au milieu
            if (tuileAvant != null && tuileApres != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Méthode pour vérifier si une position continue un alignement existant
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="adversaire"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    private bool VerifierAlignementEtContinuer(Plateau plateau, Joueur adversaire, Tuile position)
    {
        var directions = new (int dx, int dy)[]
        {
            (1, 0),   // Horizontal
            (0, 1),   // Vertical
            (1, 1),   // Diagonale gauche-droite
            (1, -1)   // Diagonale droite-gauche
        };

        foreach (var (dx, dy) in directions)
        {
            var tuilePrecedente = plateau.TuilesPlacees.FirstOrDefault(t =>
                t.Proprietaire == adversaire &&
                t.PositionX == position.PositionX - dx &&
                t.PositionY == position.PositionY - dy);

            var tuileSuivante = plateau.TuilesPlacees.FirstOrDefault(t =>
                t.Proprietaire == adversaire &&
                t.PositionX == position.PositionX + dx &&
                t.PositionY == position.PositionY + dy);

            // Si la position prolonge un alignement existant de l'adversaire
            if (tuilePrecedente != null || tuileSuivante != null)
            {
                return true;
            }
        }

        return false;
    }
}