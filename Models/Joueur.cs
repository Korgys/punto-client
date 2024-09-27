namespace punto_client.Models;

public class Joueur
{
    public string Nom { get; set; }
    public int Ordre { get; set; }
    public List<int> TuilesEnMain { get; set; }
    public List<int> TuilesDansLaPioche { get; set; }
}
