namespace punto_client.Models;

public class Jeu
{
    public Jeu (int nombreJoueurs)
    {
        NombreMaxDeJoueurs = nombreJoueurs;
    }

    public EtatJeu EtatJeu { get; set; } = EtatJeu.EnAttenteDeJoueur;
    public int NombreMaxDeJoueurs { get; } // joueurs max
    public Joueur AuTourDuJoueur { get; set; }
    public Joueur Vainqueur { get; set; }
    public Plateau Plateau { get; set; } = new Plateau();
    public List<Joueur> Joueurs { get; set; } = new List<Joueur>();
}
