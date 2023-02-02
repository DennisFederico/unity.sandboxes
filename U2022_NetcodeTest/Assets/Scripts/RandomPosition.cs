using UnityEngine;

public class RandomPosition {
    
    // The idea of this code is to return a random position inside a regular volume
    // that can be constrained by knowing some "edge points" to calculate to sides and "mirror"
    // i.e with the camera at (0,2,-12.5) rotated (9, 0, 0)
    // Given (Z=5, X=10) and (X=-5, X=4.5)
    // (z – z₁) / (z₂ – z₁) = (x – x₁) / (x₂ – x₁)
    // x = 0.55z + 7.25 -> Given Z between 5 and -5, Max(x) can be calculated with the formula and Min(x) = -Max(x)
    // Min(Y)=0 since its the plane.. max(Y) will depend also on Z between [-5,5]
    // (z=-5, y=4) and (z=5,y=7.5)
    // (z – z₁) / (z₂ – z₁) = (y – y₁) / (y₂ – y₁)
    // (z+5)/(5+5)=(y–4)/(7.5–4) => 3.75z + 18.75 = 10y – 40 => y = 0.375z + 5.875
    public static Vector3 GetRandomPosition() {
        var z = Random.Range(-8f, 5f);
        var xRange = 0.55f * z + 7.25f;
        var yRange = 0.375f * z + 5.875f;
        return new Vector3(Random.Range(-xRange, xRange), Random.Range(0.75f, yRange), z);
    }

}