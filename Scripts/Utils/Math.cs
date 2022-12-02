namespace Zombies;

public class Math
{
	public static float RandomRange(float min, float max) => new Random().NextSingle() * (max - min) + min;
}
