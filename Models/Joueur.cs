namespace punto_client.Models;

public class Joueur
{
    public string Nom { get; set; }
    public bool EstUnOrdinateur { get; set; }
    public int OrdreDeJeu { get; set; }
    public int Penalite { get; set; } = 0;
    public List<int> TuilesDansLaMain { get; set; }
    public List<int> TuilesDansLaPioche { get; set; }
}
