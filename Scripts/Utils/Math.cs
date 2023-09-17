namespace Zombies;

public class Math
{
    public static float RandomRange(float min, float max)
    {
        return (new Random().NextSingle() * (max - min)) + min;
    }
}
