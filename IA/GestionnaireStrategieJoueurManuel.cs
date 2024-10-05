using punto_client.Models;
using System.Linq;

namespace punto_client.IA;

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
        // Demande à l'utilisateur de renseigner la valeur et les positions de la tuile
        int valeurTuile;
        Console.WriteLine();
        do
        {
            Console.Write("Quelle tuile voulez-vous jouer ? ");
            valeurTuile = int.Parse(Console.ReadLine());
        } while (!joueur.TuilesDansLaMain.Contains(valeurTuile)); // Boucle tant que le joueur ne sélectionne pas une tuile de sa main

        Console.Write("En quelle position X ? ");
        var positionX = int.Parse(Console.ReadLine());

        Console.Write("En quelle position Y ? ");
        var positionY = int.Parse(Console.ReadLine());

        return new Tuile
        {
            Valeur = valeurTuile,
            PositionX = positionX,
            PositionY = positionY,
            Proprietaire = joueur
        };
    }
}
