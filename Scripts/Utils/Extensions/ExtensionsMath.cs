namespace Zombies;

public static class ExtensionsMath
{
	public static float ToRadians(this float degrees) => degrees * (Mathf.Pi / 180);
	public static float ToAngles(this float radians) => radians * (180 / Mathf.Pi); 
}
