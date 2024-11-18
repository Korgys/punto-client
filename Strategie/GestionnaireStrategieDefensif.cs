using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

/// <summary>
/// Cherche à empêcher l'adversaire de gagner avant de se concentrer sur son propre jeu.
/// </summary>
public class GestionnaireStrategieDefensif : IGestionnaireStrategie
{
    /// <summary>
    /// Cherche la tuile pour empêcher l'adversaire de gagner ou à défaut, cherche à aligner ses tuiles.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        // Liste toutes les positions possibles
        var aleatoire = new Random();
        var positionsPossibles = GestionnaireRegles.ObtenirTousLesCoupsPossibles(plateau, joueur)
            .OrderByDescending(t => GestionnaireRegles.SuperposeUneTuileAdverseExistante(plateau, joueur, t))
            .ThenBy(t => aleatoire)
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
            .OrderBy(j => (j.OrdreDeJeu + adversaires.Count) % joueur.OrdreDeJeu).ToList();
        foreach (Joueur adversaire in adversaires)
        {
            // Bloque l'adversaire si 3 de ses tuiles sont déjà alignées
            if (GestionnaireRegles.VerifierConditionsVictoire(plateau, joueur, 3))
            {
                foreach (Tuile position in positionsPossibles)
                {
                    // Si tuile permettant de le faire passer à 2 tuiles
                    if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, adversaire, position, 2))
                    {
                        return position;
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
}