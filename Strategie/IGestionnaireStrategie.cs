using punto_client.Models;

namespace punto_client.Strategie;

public interface IGestionnaireStrategie
{
    Tuile ObtenirProchainCoup(Plateau plateau, Joueur joueur);
}
