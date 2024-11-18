using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

public class GestionnaireStrategieAgressifV2 : IGestionnaireStrategie
{
    public Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        var tuile = new Tuile
        {
            Proprietaire = joueur
        };

        // Pour chaque tuile dans la main du joueur
        foreach (int valeurTuile in joueur.TuilesDansLaMain)
        {
            tuile.Valeur = valeurTuile;

            // Liste toutes les positions possibles
            var positionsPossibles = ObtenirPositionsPossibles(plateau, tuile);

            // Cherche la tuile plaçable qui permet d'aligner le plus de tuiles possibles
            var meilleurePosition = TrouverMeilleurePosition(plateau, joueur, positionsPossibles, tuile);

            if (meilleurePosition != null && GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
            {
                tuile.PositionX = meilleurePosition.X;
                tuile.PositionY = meilleurePosition.Y;
                return tuile;
            }
        }

        // Si aucune position favorable n'est trouvée, renvoie la première position possible au hasard
        var aleatoire = new Random();
        tuile.Valeur = joueur.TuilesDansLaMain[aleatoire.Next(joueur.TuilesDansLaMain.Count)];
        var positions = ObtenirPositionsPossibles(plateau, tuile);

        do
        {
            if (!positions.Any()) break; // Si plus de positions disponibles, sortir de la boucle

            var position = positions.First();
            positions = positions.Skip(1).ToList(); // Retire la position

            tuile.PositionX = position.X;
            tuile.PositionY = position.Y;
        } while (!GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile) && positions.Any());

        return tuile;
    }

    /// <summary>
    /// Recherche la meilleure position pour aligner des tuiles.
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="joueur"></param>
    /// <param name="positionsPossibles"></param>
    /// <param name="tuile"></param>
    /// <returns>La meilleure position ou null</returns>
    private Position TrouverMeilleurePosition(Plateau plateau, Joueur joueur, List<Position> positionsPossibles, Tuile tuile)
    {
        foreach (var position in positionsPossibles)
        {
            tuile.PositionX = position.X;
            tuile.PositionY = position.Y;

            // Vérifie si la position est jouable selon les règles
            if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
            {
                // Vérifie si la position permet d'aligner 4 tuiles
                if (VerifierConditionsVictoireAvecTuile(plateau, joueur, tuile, 4))
                {
                    return position;
                }
            }
        }

        foreach (var position in positionsPossibles)
        {
            tuile.PositionX = position.X;
            tuile.PositionY = position.Y;

            if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
            {
                // Vérifie si la position permet d'aligner 3 tuiles
                if (VerifierConditionsVictoireAvecTuile(plateau, joueur, tuile, 3))
                {
                    return position;
                }
            }
        }

        foreach (var position in positionsPossibles)
        {
            tuile.PositionX = position.X;
            tuile.PositionY = position.Y;

            if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile))
            {
                // Vérifie si la position permet d'aligner 2 tuiles
                if (VerifierConditionsVictoireAvecTuile(plateau, joueur, tuile, 2))
                {
                    return position;
                }
            }
        }

        // Si aucune position ne permet d'aligner 2, 3 ou 4 tuiles, renvoie null
        return null;
    }

    /// <summary>
    /// Liste toutes les positions possibles
    /// </summary>
    /// <param name="plateau"></param>
    /// <param name="tuile"></param>
    /// <returns></returns>
    public List<Position> ObtenirPositionsPossibles(Plateau plateau, Tuile tuile)
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

        return positionsPossibles;
    }

    private bool VerifierConditionsVictoireAvecTuile(Plateau plateau, Joueur joueur, Tuile tuileAPoser, int tuilesAlignees)
    {
        // Simule la position sans ajouter la tuile si elle ne respecte pas les règles
        if (GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuileAPoser))
        {
            // Crée une copie temporaire du plateau
            var plateauAvecTuileAPoser = new Plateau
            {
                TuilesPlacees = new List<Tuile>(plateau.TuilesPlacees) { tuileAPoser }
            };
            return GestionnaireRegles.VerifierConditionsVictoire(plateauAvecTuileAPoser, joueur, tuilesAlignees);
        }

        return false;
    }
}