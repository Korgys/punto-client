using punto_client.Models;

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
        Console.Write("\nQuelle tuile voulez-vous jouer ? ");
        var valeurTuile = int.Parse(Console.ReadLine());

        Console.Write("\nEn quelle position X ? ");
        var positionX = int.Parse(Console.ReadLine());

        Console.Write("\nEn quelle position Y ? ");
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
