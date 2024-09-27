
using punto_client.Models;

namespace punto_client.Services;

public class GestionnaireJeu
{
    private Jeu Jeu;

    public Jeu ObtenirJeu()
    {
        return Jeu;
    }

    public EtatJeu ObtenirEtatJeu()
    {
        return Jeu.EtatJeu;
    }

    public void CreerNouveauJeu()
    {
        Jeu = new Jeu();
    }

    public void AjouterUnJoueur()
    {
        // TODO :  Ajouter un joueur à la partie (si c'est possible)
        // TODO : Démarrer la partie si le nombre de joueurs max est atteint
    }

    public void AfficherPlateau()
    {
        // TODO : Afficher le plateau en console
    }

    public Joueur ObtenirJoueurDevantJouer()
    {
        return Jeu.AuTourDuJoueur;
    }

    public Plateau ObtenirPlateau()
    {
        return Jeu.Plateau;
    }

    public bool PeutPlacerTuile(Tuile tuile)
    {
        // TODO: Ecrire les conditions pour placer une tuile
        return false;
    }

    public void PlacerTuile(Tuile tuile)
    {
        // TODO: Ecrire le process pour placer une tuile,
        // TODO: vérifier les conditions de victoire
        // TODO: passer au tour suivant
    }
}
