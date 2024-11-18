using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

/// <summary>
/// Cherche à empêcher l'adversaire de gagner avant de se concentrer sur son propre jeu.
/// </summary>
public class GestionnaireStrategieDefensifV2 : IGestionnaireStrategie
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
        int nombreJoueurs = adversaires.Count();
        // Ordonne les joueurs par ordre de jeu après le tour du joueur en cours
        adversaires = adversaires
            .Where(j => j != joueur)
            .OrderBy(a => (nombreJoueurs + a.OrdreDeJeu - joueur.OrdreDeJeu) % nombreJoueurs).ToList();
        foreach (Joueur adversaire in adversaires)
        {
            List<Tuile> plusGrandAlignement = GestionnaireRegles.ObtenirPlusGrandAlignement(plateau, adversaire);

            // Adversaire sur le point de gagner
            if (plusGrandAlignement.Count == 3)
            {
                // Essaie de prendre la tuile du milieu 
                Tuile tuileDuMilieu = plusGrandAlignement[1];

                // Cas où on peut couper son alignement par le milieu
                int valeurMaxTuileDansLaMain = joueur.TuilesDansLaMain.Max();
                if (valeurMaxTuileDansLaMain > tuileDuMilieu.Valeur)
                {
                    return new Tuile
                    {
                        Valeur = valeurMaxTuileDansLaMain,
                        PositionX = tuileDuMilieu.PositionX,
                        PositionY = tuileDuMilieu.PositionY,
                        Proprietaire = joueur
                    };
                }
                // Cas où on peut mettre une tuile sur l'un des bouts
                else if (valeurMaxTuileDansLaMain > plusGrandAlignement[0].Valeur)
                {
                    return new Tuile
                    {
                        Valeur = valeurMaxTuileDansLaMain,
                        PositionX = plusGrandAlignement[0].PositionX,
                        PositionY = plusGrandAlignement[0].PositionY,
                        Proprietaire = joueur
                    };
                }
                else if (valeurMaxTuileDansLaMain > plusGrandAlignement[2].Valeur)
                {
                    return new Tuile
                    {
                        Valeur = valeurMaxTuileDansLaMain,
                        PositionX = plusGrandAlignement[1].PositionX,
                        PositionY = plusGrandAlignement[1].PositionY,
                        Proprietaire = joueur
                    };
                }
                else // Cas où on peut "bloquer" en mettant sur la continuité de l'alignement
                {
                    // Obtient les tuiles dans la continuité de l'alignement
                    var tuilesContinuantAlignement = GestionnaireRegles.ObtenirTuilesContinuantAlignement(plateau, plusGrandAlignement);
                    if (tuilesContinuantAlignement.Count == 2)
                    {
                        if (tuilesContinuantAlignement[0].Valeur < valeurMaxTuileDansLaMain)
                        {
                            var tuile = new Tuile
                            {
                                Valeur = valeurMaxTuileDansLaMain,
                                PositionX = tuilesContinuantAlignement[0].PositionX,
                                PositionY = tuilesContinuantAlignement[0].PositionY,
                                Proprietaire = joueur
                            };
                            if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
                            {
                                return new Tuile
                                {
                                    Valeur = valeurMaxTuileDansLaMain,
                                    PositionX = tuilesContinuantAlignement[0].PositionX,
                                    PositionY = tuilesContinuantAlignement[0].PositionY,
                                    Proprietaire = joueur
                                };
                            }
                        }
                        else if (tuilesContinuantAlignement[1].Valeur < valeurMaxTuileDansLaMain)
                        {
                            var tuile = new Tuile
                            {
                                Valeur = valeurMaxTuileDansLaMain,
                                PositionX = tuilesContinuantAlignement[1].PositionX,
                                PositionY = tuilesContinuantAlignement[1].PositionY,
                                Proprietaire = joueur
                            };
                            if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
                            {
                                return new Tuile
                                {
                                    Valeur = valeurMaxTuileDansLaMain,
                                    PositionX = tuilesContinuantAlignement[1].PositionX,
                                    PositionY = tuilesContinuantAlignement[1].PositionY,
                                    Proprietaire = joueur
                                };
                            }
                        }
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