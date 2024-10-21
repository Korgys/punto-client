using punto_client.Models;

namespace punto_client.Strategie;

/// <summary>
/// Stratégie basée sur l'aléatoire parmis les coups possibles.
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
        var tuile = new Tuile
        {
            Proprietaire = joueur,
            Valeur = joueur.TuilesDansLaMain[aleatoire.Next(0, joueur.TuilesDansLaMain.Count)] // Correction pour éviter l'erreur d'indexation
        };

        // Liste toutes les positions possibles
        var positionsPossibles = ObtenirPositionsPossibles(plateau, tuile);

        // Prends la première position jouable au hasard
        positionsPossibles = positionsPossibles.OrderBy(p => aleatoire.Next()).ToList();
        do
        {
            if (!positionsPossibles.Any()) break; // Si plus de positions disponibles, sortir de la boucle

            var position = positionsPossibles.First();
            positionsPossibles = positionsPossibles.Skip(1).ToList(); // Retire la position

            tuile.PositionX = position.X;
            tuile.PositionY = position.Y;
        } while (!GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile) && positionsPossibles.Any());

        //Thread.Sleep(400); // Ralentit la prise de décision pour pouvoir voir le déroulement du jeu

        return tuile;
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
            // Liste des positions adjacentes à la tuile actuelle
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

            // Ajoute les positions adjacentes si elles sont dans la grille 6x6 et non déjà occupées
            foreach (var pos in positionsAdjacentes)
            {
                // Vérifie que la position est bien dans la grille fixe 6x6
                bool estDansLaGrille = Math.Max(pos.X, maxX) - Math.Min(pos.X, minX) < 6
                                    && Math.Max(pos.Y, maxY) - Math.Min(pos.Y, minY) < 6
                                    && pos.X > -6 && pos.X < 6
                                    && pos.Y > -6 && pos.Y < 6;
                bool estOccupee = plateau.TuilesPlacees.Any(p => p.PositionX == pos.X && p.PositionY == pos.Y);

                if (estDansLaGrille && !estOccupee)
                {
                    positionsPossibles.Add(pos);
                }
            }
        }

        return positionsPossibles;
    }
}
