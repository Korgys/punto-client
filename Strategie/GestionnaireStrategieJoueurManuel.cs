using punto_client.Models;
using punto_client.Services;

namespace punto_client.Strategie;

public class GestionnaireStrategieJoueurManuel
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
    public static Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur)
    {
        var tuile = new Tuile
        {
            Valeur = int.MinValue,
            Proprietaire = joueur
        };

        Console.WriteLine(); // Pour l'affichage

        // Demande à l'utilisateur de renseigner la valeur et les positions de la tuile
        do // Boucle tant que le joueur ne sélectionne pas une tuile de sa main
        {
            Console.Write("Quelle tuile voulez-vous jouer ? ");
            tuile.Valeur = int.Parse(Console.ReadLine());
        } while (!joueur.TuilesDansLaMain.Contains(tuile.Valeur));

        bool positionsRenseignees = false;
        do
        {
            Console.Write("En quelle position X, Y ? ");
            var positions = Console.ReadLine().Split(',');
            if (positions.Length == 2 // x et y renseignés
                && int.TryParse(positions[0], out int positionX)
                && int.TryParse(positions[1], out int positionY)) 
            {
                tuile.PositionX = positionX;
                tuile.PositionY = positionY;
                positionsRenseignees = true;
            }
        } while (!positionsRenseignees && !GestionnaireRegles.PeutPlacerTuile(plateau, joueur, tuile)); // Tant que positions pas renseignées et tuile pas plaçable

        return tuile;
    }
}
