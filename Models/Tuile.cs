namespace punto_client.Models;

public class Tuile
{
    public Tuile() { }

    public Tuile (Joueur proprietaire, int valeur, int positionX, int positionY)
    {
        Valeur = valeur;
        Proprietaire = proprietaire;
        PositionX = positionX;
        PositionY = positionY;
    }

    public int Valeur { get; set; }
    public Joueur Proprietaire { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}
