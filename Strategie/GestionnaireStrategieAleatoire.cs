using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

/// <summary>
/// Stratégie basée sur l'aléatoire parmi les coups possibles.
/// </summary>
public class GestionnaireStrategieAleatoire : IGestionnaireStrategie
{
    /// <summary>
    /// Obtient le prochain coup du joueur à qui s'est le tour de jouer.
    /// c'est ici qu'on met en place la logique de l'IA.
    /// Pour l'exemple, on va demander manuellement au joueur d'entrer la valeur de la tuile à jouer.
    /// Mais pour d'autres implémentations dans le dossier, ce sera fait automatiquement.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <returns>La tuile jouée</returns>
    public Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        var aleatoire = new Random();

        // Liste toutes les positions possibles
        var coupsPossibles = GestionnaireRegles.ObtenirTousLesCoupsPossibles(plateau, joueur);

        // Prends la première position jouable au hasard
        coupsPossibles = coupsPossibles.OrderBy(p => aleatoire.Next()).ToList();
        var coup = coupsPossibles.FirstOrDefault();

        return coup;
    }
}
