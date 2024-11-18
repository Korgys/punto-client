using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

/// <summary>
/// Cherche la tuile plaçable qui permet d'aligner le plus de tuiles possibles.
/// </summary>
public class GestionnaireStrategieAgressif : IGestionnaireStrategie
{
    /// <summary>
    /// Cherche la tuile plaçable qui permet d'aligner le plus de tuiles possibles
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <returns></returns>
    public Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        // Liste toutes les positions possibles
        var positionsPossibles = GestionnaireRegles.ObtenirTousLesCoupsPossibles(plateau, joueur);

        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 4))
            {
                return position;
            }
        }

        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 3))
            {
                return position;
            }
        }

        foreach (Tuile position in positionsPossibles)
        {
            if (GestionnaireRegles.VerifierConditionsVictoireAvecTuile(plateau, joueur, position, 2))
            {
                return position;
            }
        }

        // Si aucune position favorable n'est trouvée,
        // renvoie la première position possible au hasard en prenant d'abord la plus forte
        var aleatoire = new Random();
        var maxValeurTuile = positionsPossibles.Any() ? positionsPossibles.Max(t => t.Valeur) : 0;
        return positionsPossibles
            .Where(t => t.Valeur == maxValeurTuile)
            .OrderBy(p => aleatoire)
            .FirstOrDefault();
    }
}