namespace punto_client.Models;

public class Position
{
    public int X;
    public int Y;

    public Position(Tuile tuile)
    {
        X = tuile.PositionX;
        Y = tuile.PositionY;
    }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }
}
