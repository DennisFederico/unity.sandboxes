using UnityEngine;

public class Interpolate : MonoBehaviour {

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform pointC;
    [SerializeField] private Transform pointD;
    
    [SerializeField] private Transform pointAB;
    [SerializeField] private Transform pointBC;
    [SerializeField] private Transform pointCD;
    
    [SerializeField] private Transform pointAB_BC;
    [SerializeField] private Transform pointBC_CD;
    
    [SerializeField] private Transform pointABCD;
    
    private float _interpolateAmount;
    // Update is called once per frame
    void Update() {
        _interpolateAmount = (_interpolateAmount + Time.deltaTime) % 1f;
        
        // pointAB.position = Vector3.Lerp(pointA.position, pointB.position, _interpolateAmount);
        // pointBC.position = Vector3.Lerp(pointB.position, pointC.position, _interpolateAmount);
        // pointCD.position = Vector3.Lerp(pointC.position, pointD.position, _interpolateAmount);
        //
        // pointAB_BC.position = Vector3.Lerp(pointAB.position, pointBC.position, _interpolateAmount);
        // pointBC_CD.position = Vector3.Lerp(pointBC.position, pointCD.position, _interpolateAmount);
        //
        // pointABCD.position = Vector3.Lerp(pointAB_BC.position, pointBC_CD.position, _interpolateAmount);

        pointABCD.position = CubicLerp(pointA.position, pointB.position, pointC.position, pointD.position, _interpolateAmount);
    }

    private Vector3 QuadraticLerp(Vector3 a, Vector3 b, Vector3 c, float t) {
        var vectorAb = Vector3.Lerp(a, b, t);
        var vectorBc = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(vectorAb, vectorBc, t);
    }

    private Vector3 CubicLerp(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t) {
        var vectorAbc = QuadraticLerp(a, b, c, t);
        var vectorBcd = QuadraticLerp(b, c, d, t);

        return Vector3.Lerp(vectorAbc, vectorBcd, t);
    }
}