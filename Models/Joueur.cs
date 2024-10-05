namespace punto_client.Models;

public class Joueur
{
    public string Nom { get; set; }
    public int OrdreDeJeu { get; set; }
    public List<int> TuilesDansLaMain { get; set; }
    public List<int> TuilesDansLaPioche { get; set; }
}
