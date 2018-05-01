using UnityEngine;

public class FaceFragment {
    public Vector3[] verts;
    public Vector2[] uvs;

    public FaceFragment(Vector3[] pVerts, Vector2[] pUVs) {
        verts = pVerts;
        uvs = pUVs;
    }
        
    public Vector3 GetVerticeCenter() {
        var sum = Vector3.zero;
        foreach (var v in verts)
            sum += v;
        return sum / verts.Length;
    }

    public Vector2 GetUVCenter() {
        var sum = Vector2.zero;
        foreach (var uv in uvs)
            sum += uv;
        return sum / uvs.Length;
    }

    public Vector3 GetWorldCenter() {
        var sum = Vector3.zero;
        foreach (var vert in verts)
            sum += vert;
        return sum / uvs.Length;
    }
}
