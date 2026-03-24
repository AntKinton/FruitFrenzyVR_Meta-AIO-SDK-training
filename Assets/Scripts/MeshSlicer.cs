using System.Collections.Generic;
using UnityEngine;

public static class MeshSlicer
{
    public struct SliceResult
    {
        public Mesh meshA; // vértices en el lado positivo del plano
        public Mesh meshB; // vértices en el lado negativo
    }

    public static SliceResult Slice(Mesh original, Plane plane)
    {
        var vertsA = new List<Vector3>();
        var vertsB = new List<Vector3>();
        var trisA  = new List<int>();
        var trisB  = new List<int>();
        var uvsA   = new List<Vector2>();
        var uvsB   = new List<Vector2>();
        var capVerts = new List<Vector3>(); // puntos en el plano → tapa

        Vector3[] verts = original.vertices;
        int[]     tris  = original.triangles;
        Vector2[] uvs   = original.uv.Length == verts.Length
                          ? original.uv
                          : new Vector2[verts.Length];

        for (int i = 0; i < tris.Length; i += 3)
        {
            int i0 = tris[i], i1 = tris[i+1], i2 = tris[i+2];
            Vector3 v0 = verts[i0], v1 = verts[i1], v2 = verts[i2];
            Vector2 u0 = uvs[i0],   u1 = uvs[i1],   u2 = uvs[i2];

            bool s0 = plane.GetSide(v0);
            bool s1 = plane.GetSide(v1);
            bool s2 = plane.GetSide(v2);

            if (s0 == s1 && s1 == s2)
            {
                // Triángulo entero en un lado
                AddTriangle(s0 ? vertsA : vertsB,
                            s0 ? trisA  : trisB,
                            s0 ? uvsA   : uvsB,
                            v0, v1, v2, u0, u1, u2);
            }
            else
            {
                // Triángulo intersecta el plano → cortar
                SliceTriangle(plane,
                    v0, v1, v2, u0, u1, u2, s0, s1, s2,
                    vertsA, trisA, uvsA,
                    vertsB, trisB, uvsB,
                    capVerts);
            }
        }

        // Generar tapa (cap) para cada lado
        if (capVerts.Count >= 3)
        {
            AddCap(capVerts, plane.normal, vertsA, trisA, uvsA, flip: false);
            AddCap(capVerts, plane.normal, vertsB, trisB, uvsB, flip: true);
        }

        return new SliceResult
        {
            meshA = BuildMesh(vertsA, trisA, uvsA),
            meshB = BuildMesh(vertsB, trisB, uvsB)
        };
    }

    // ─── helpers ────────────────────────────────────────────────────────────

    static void SliceTriangle(
        Plane plane,
        Vector3 v0, Vector3 v1, Vector3 v2,
        Vector2 u0, Vector2 u1, Vector2 u2,
        bool s0, bool s1, bool s2,
        List<Vector3> vA, List<int> tA, List<Vector2> uA,
        List<Vector3> vB, List<int> tB, List<Vector2> uB,
        List<Vector3> cap)
    {
        // Ordenar: primero los vértices del lado "solitario"
        // Caso: s0 único
        if (s0 != s1 && s0 != s2)
            CutOne(plane, v0,v1,v2, u0,u1,u2, s0, vA,tA,uA, vB,tB,uB, cap);
        else if (s1 != s0 && s1 != s2)
            CutOne(plane, v1,v2,v0, u1,u2,u0, s1, vA,tA,uA, vB,tB,uB, cap);
        else
            CutOne(plane, v2,v0,v1, u2,u0,u1, s2, vA,tA,uA, vB,tB,uB, cap);
    }

    // Un vértice (va) está solo; los otros dos (vb, vc) están del mismo lado
    static void CutOne(
        Plane plane,
        Vector3 va, Vector3 vb, Vector3 vc,
        Vector2 ua, Vector2 ub, Vector2 uc,
        bool sideA,
        List<Vector3> vA, List<int> tA, List<Vector2> uA,
        List<Vector3> vB, List<int> tB, List<Vector2> uB,
        List<Vector3> cap)
    {
        float tAB = PlaneT(plane, va, vb);
        float tAC = PlaneT(plane, va, vc);
        Vector3 iAB = Vector3.Lerp(va, vb, tAB);
        Vector3 iAC = Vector3.Lerp(va, vc, tAC);
        Vector2 uvAB = Vector2.Lerp(ua, ub, tAB);
        Vector2 uvAC = Vector2.Lerp(ua, uc, tAC);

        cap.Add(iAB);
        cap.Add(iAC);

        // El "uno" va a su lado con un triángulo
        var (sv, st, su) = sideA ? (vA, tA, uA) : (vB, tB, uB);
        AddTriangle(sv, st, su, va, iAB, iAC, ua, uvAB, uvAC);

        // Los "dos" van al otro lado con un quad (dos triángulos)
        var (dv, dt, du) = sideA ? (vB, tB, uB) : (vA, tA, uA);
        AddTriangle(dv, dt, du, vb,  vc,  iAB, ub,  uc,  uvAB);
        AddTriangle(dv, dt, du, vc, iAC, iAB,  uc,  uvAC, uvAB);
    }

    static float PlaneT(Plane p, Vector3 a, Vector3 b)
    {
        float da = Vector3.Dot(p.normal, a) + p.distance;
        float db = Vector3.Dot(p.normal, b) + p.distance;
        return da / (da - db);
    }

    static void AddTriangle(
        List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        Vector3 a, Vector3 b, Vector3 c,
        Vector2 ua, Vector2 ub, Vector2 uc)
    {
        int idx = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        uvs.Add(ua);  uvs.Add(ub);  uvs.Add(uc);
        tris.Add(idx); tris.Add(idx+1); tris.Add(idx+2);
    }

    // Fan triangulation de la tapa (cap) proyectada en el plano de corte
    static void AddCap(
        List<Vector3> capVerts, Vector3 planeNormal,
        List<Vector3> verts, List<int> tris, List<Vector2> uvs,
        bool flip)
    {
        // Calcular centroide
        Vector3 center = Vector3.zero;
        foreach (var v in capVerts) center += v;
        center /= capVerts.Count;

        // Ordenar vértices en torno al centroide por ángulo en el plano de corte
        Vector3 refDir = (capVerts[0] - center).normalized;
        Vector3 perpDir = Vector3.Cross(planeNormal, refDir).normalized;

        capVerts.Sort((a, b) =>
        {
            Vector3 da = a - center, db = b - center;
            float angleA = Mathf.Atan2(Vector3.Dot(da, perpDir), Vector3.Dot(da, refDir));
            float angleB = Mathf.Atan2(Vector3.Dot(db, perpDir), Vector3.Dot(db, refDir));
            return angleA.CompareTo(angleB);
        });

        // Fan desde el centroide
        for (int i = 0; i < capVerts.Count; i++)
        {
            int next = (i + 1) % capVerts.Count;
            Vector3 a = center;
            Vector3 b = flip ? capVerts[next] : capVerts[i];
            Vector3 c = flip ? capVerts[i]    : capVerts[next];
            Vector2 uvCenter = new Vector2(0.5f, 0.5f);
            Vector2 uvB = new Vector2(
                0.5f + 0.5f * Vector3.Dot(b - center, refDir),
                0.5f + 0.5f * Vector3.Dot(b - center, perpDir));
            Vector2 uvC = new Vector2(
                0.5f + 0.5f * Vector3.Dot(c - center, refDir),
                0.5f + 0.5f * Vector3.Dot(c - center, perpDir));
            AddTriangle(verts, tris, uvs, a, b, c, uvCenter, uvB, uvC);
        }
    }

    static Mesh BuildMesh(List<Vector3> verts, List<int> tris, List<Vector2> uvs)
    {
        var m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices  = verts.ToArray();
        m.triangles = tris.ToArray();
        m.uv        = uvs.ToArray();
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
