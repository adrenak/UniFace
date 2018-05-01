using UnityEngine;
using System.Text;
using System.Linq;
using UniPrep.Extensions;
using System.Collections.Generic;

public class FaceCluster {
    // ================================================
    // SUBCLASSES 
    // ================================================
    public class UVBounds {
        float m_MinX, m_MinY, m_MaxX, m_MaxY;
        
        public float GetWidth() {
            return m_MaxX - m_MinX;
        }

        public float GetHeight() {
            return m_MaxY - m_MinY;
        }

        public Vector2 GetCenter() {
            return new Vector2(
                (m_MinX + m_MaxX) / 2,
                (m_MinY + m_MaxY) / 2
            );
        }

        public UVBounds(float _minX, float _maxX, float _minY, float _maxY) {
            m_MinX = _minX; m_MinY = _minY;
            m_MaxX = _maxX; m_MaxY = _maxY;
        }
    }

    public class WorldBounds {
        float m_MinX, m_MinY, m_MinZ;
        float m_MaxX, m_MaxY, m_MaxZ;

        public float GetScaleX() {
            return m_MaxX - m_MinX + .01f;
        }

        public float GetScaleY() {
            return m_MaxY - m_MinY + .01f;
        }

        public float GetScaleZ() {
            return m_MaxZ - m_MinZ + .01f;
        }

        public Vector3 GetCenter() {
            return new Vector3(
                (m_MinX + m_MaxX) / 2,
                (m_MinY + m_MaxY) / 2,
                (m_MinZ + m_MaxZ) / 2
            );
        }

        public WorldBounds(float _minX, float _maxX, float _minY, float _maxY, float _minZ, float _maxZ) {
            m_MinX = _minX; m_MinY = _minY; m_MinZ = _minZ;
            m_MaxX = _maxX; m_MaxY = _maxY; m_MaxZ = _maxZ;
        }
    }

    // ================================================
    // FIELDS
    // ================================================
    bool m_IsClustered;

    List<Vector3> m_Verts = new List<Vector3>();
    List<Vector2> m_UVs = new List<Vector2>();
    int[] m_Triangles;

    List<FaceFragment> m_Fragments = new List<FaceFragment>();
    List<FaceCluster> m_Clusters = new List<FaceCluster>();

    UVBounds m_UVBounds;
    WorldBounds m_WorldBounds;
    
    // ================================================
    // GETTERS
    // ================================================
    public List<Vector3> GetVerts() {
        return m_Verts;
    }

    public List<Vector2> GetUVs() {
        return m_UVs;
    }

    public int[] GetTriangles() {
        return m_Triangles;
    }

    public UVBounds GetBounds() {
        return m_UVBounds;
    }

    // ================================================
    // PUBLIC METHODS
    // ================================================
    public void AddCluster(FaceCluster _cluster) {
        AddFragments(_cluster.GetFragments());
    }

    public void AddFragments(FaceFragment[] _fragments) {
        for(int i = 0; i < _fragments.Length; i++)
            AddFragment(_fragments[i]);
    }
    public void AddFragment(FaceFragment _fragment) {
        if (m_Fragments == null) m_Fragments = new List<FaceFragment>();
        m_Fragments.Add(_fragment);
    }

    public FaceFragment[] GetFragments() {
        // If the clustering has not been done yet
        if (!m_IsClustered) 
            return m_Fragments == null ? null : m_Fragments.ToArray();

        // If the clustering has been done and fragments was previously requested
        // (and therefore store) then return the stored array
        if (m_Fragments != null) return m_Fragments.ToArray();

        // If the clustering has been done, and fragments were never requested
        // then create, store and return recalculated fragments
        List<FaceFragment> fragments = new List<FaceFragment>();
        for(int i = 0; i < m_Triangles.Length; i = i + 3) {
            fragments.Add(new FaceFragment(
                new[] {
                    m_Verts[m_Triangles[i]],
                    m_Verts[m_Triangles[i + 1]],
                    m_Verts[m_Triangles[i + 2]]
                },
                new[] {
                    m_UVs[m_Triangles[i]],
                    m_UVs[m_Triangles[i + 1]],
                    m_UVs[m_Triangles[i + 2]]
                }
            ));
        }
        m_Fragments = fragments;
        return m_Fragments.ToArray();
    }

    public void Execute(bool _removeDuplicates = false) {
        // Can't do clustering twice
        if (m_IsClustered)
            return;

        List<Vector3> vertUnion;
        List<Vector2> uvUnion;
        List<int> mapping = new List<int>(); ;

        CreateUnions(out vertUnion, out uvUnion);

        if (!_removeDuplicates) {
            m_Verts = vertUnion;
            m_UVs = uvUnion;
            for (int i = 0; i < m_Verts.Count; i++)
                mapping.Add(i);
        }
        else
            RemoveDuplicates(vertUnion, uvUnion, mapping);
        
        m_Triangles = mapping.ToArray();

        CalculateUVBounds();
        CalculateWorldBounds();
        m_Fragments = null;
        m_IsClustered = true;
    }

    // ================================================
    // UV MANIPULATIONS
    // ================================================
    // FLIPPING/MIRRORING
    public void FlipUVsHorizontally() {
        var center = m_UVBounds.GetCenter();
        for(int i = 0; i < m_UVs.Count; i++) {
            var _offset = center.x - m_UVs[i].x;
            m_UVs[i] = m_UVs[i].SetX(m_UVs[i].x + 2 * _offset);
        }
        CalculateUVBounds();
    }

    public void FlipUVsVertically() {
        var center = m_UVBounds.GetCenter();
        for(int i = 0; i < m_UVs.Count; i++) { 
            var _offset = center.y - m_UVs[i].y;
            m_UVs[i] = m_UVs[i].SetY(m_UVs[i].y + 2 * _offset);
        }
        CalculateUVBounds();
    }
    
    // ROTATION
    public void TurnUVsCW(int _times = 1) {
        RotateUVs(-90 * _times);
    }

    public void TurnUVsCCW(int _times = 1) {
        RotateUVs(90 * _times);
    }

    public void RotateUVs(float _degrees) {
        for(int i = 0; i < m_UVs.Count; i++) {
            var center = m_UVBounds.GetCenter();
            var r = Vector2.Distance(center, m_UVs[i]);
            var x = m_UVs[i].x - center.x;
            var y = m_UVs[i].y - center.y;

            var iniDeg = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            var finDeg = iniDeg + _degrees;

            var finX = r * Mathf.Cos(finDeg * Mathf.Deg2Rad) + center.x;
            var finY = r * Mathf.Sin(finDeg * Mathf.Deg2Rad) + center.y;

            m_UVs[i] = new Vector2(finX, finY);
        }
        CalculateUVBounds();
    }

    // TRANSLATION
    public void TranslateUVsOnX(float _xAmount) {
        var amount = new Vector2(_xAmount, 0);
        TranslateUVs(amount);
    }

    public void TranslateUVsOnY(float _yAmount) {
        var amount = new Vector2(0, _yAmount);
        TranslateUVs(amount);
    }

    public void RepositionUVCenter(Vector2 _center) {
        var offset = _center - m_UVBounds.GetCenter();
        TranslateUVs(offset);
    }

    public void TranslateUVs(Vector2 _offset) {
        for(int i = 0; i < m_UVs.Count; i++)
            m_UVs[i] += _offset;
        CalculateUVBounds();
    }

    // SCALING
    public void SetUVSize(Vector2 _dim) {
        var xScaleFactor = _dim.x / m_UVBounds.GetWidth();
        var yScaleFactor = _dim.y / m_UVBounds.GetHeight();
        var center = m_UVBounds.GetCenter();

        for (int i = 0; i < m_UVs.Count; i++) {
            var _negativeXOffset = -1 * (center.x - m_UVs[i].x);
            m_UVs[i] = m_UVs[i].SetX(center.x + _negativeXOffset * xScaleFactor);

            var _negativeYOffset = -1 * (center.y - m_UVs[i].y);
            m_UVs[i] = m_UVs[i].SetY(center.y + _negativeYOffset * yScaleFactor);
        }
        CalculateUVBounds();
    }

    public void SetUVWidth(float _width) {
        SetUVSize(new Vector2(_width, m_UVBounds.GetHeight()));
    }

    public void SetUVHeight(float _height) {
        SetUVSize(new Vector2(m_UVBounds.GetWidth(), _height));
    }

    public void ScaleUVRelative(Vector2 _relativeScale) {
        SetUVSize(new Vector2(
            m_UVBounds.GetWidth() * _relativeScale.x,
            m_UVBounds.GetHeight() * _relativeScale.y
        ));
    }

    // ================================================
    // 3D MANIPULATIONS
    // ================================================
    // SCALING
    public void SetWorldSize(Vector3 _dim) {
        var xScaleFactor = _dim.x / m_WorldBounds.GetScaleX();
        var yScaleFactor = _dim.y / m_WorldBounds.GetScaleY();
        var zScaleFactor = _dim.z / m_WorldBounds.GetScaleZ();
        var center = m_WorldBounds.GetCenter();

        for (int i = 0; i < m_Verts.Count; i++) {
            var _negativeXOffset = -1 * (center.x - m_Verts[i].x);
            m_Verts[i] = m_Verts[i].SetX(center.x + _negativeXOffset * xScaleFactor);

            var _negativeYOffset = -1 * (center.y - m_Verts[i].y);
            m_Verts[i] = m_Verts[i].SetY(center.y + _negativeYOffset * yScaleFactor);

            var _negativeZOffset = -1 * (center.z - m_Verts[i].z);
            m_Verts[i] = m_Verts[i].SetZ(center.z + _negativeZOffset * zScaleFactor);
        }
        CalculateWorldBounds();
    }

    public void SetWorldX(float _x) {
        SetWorldSize(new Vector3(_x, m_WorldBounds.GetScaleY(), m_WorldBounds.GetScaleZ()));
    }

    public void SetWorldY(float _y) {
        SetWorldSize(new Vector3(m_WorldBounds.GetScaleX() + .01F, _y, m_WorldBounds.GetScaleZ()));
    }

    public void SetWorldZ(float _z) {
        SetWorldSize(new Vector3(m_WorldBounds.GetScaleX(), m_WorldBounds.GetScaleY(), _z));
    }

    public void ScaleWorldRelative(Vector3 _relativeScale) {
        SetWorldSize(new Vector3(
            m_WorldBounds.GetScaleX() * _relativeScale.x,
            m_WorldBounds.GetScaleY() * _relativeScale.y,
            m_WorldBounds.GetScaleZ() * _relativeScale.z
        ));
    }

    // ROTATION
    public void RotateAlongLocalX(float _degrees) {
        Rotate(new Vector3(_degrees, 0, 0));
    }

    public void RotateAlongLocalY(float _degrees) {
        Rotate(new Vector3(0, _degrees, 0));
    }

    public void RotateAlongLocalZ(float _degrees) {
        Rotate(new Vector3(0, 0, _degrees));
    }

    public void Rotate(Vector3 _angles) {
        for (int i = 0; i < m_UVs.Count; i++) {
            var center = m_WorldBounds.GetCenter();
            m_Verts[i] = RotatePointAroundPivot(m_Verts[i], center, _angles);
        }
        CalculateWorldBounds();
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles) {
        return Quaternion.Euler(angles) * (point - pivot) + pivot;
    }

    // TRANSLATION
    public void TranslateWorldPositionOnX(float _xAmount) {
        var amount = new Vector3(_xAmount, 0, 0);
        TranslateWorldPosition(amount);
    }

    public void TranslateWorldPositionOnY(float _yAmount) {
        var amount = new Vector3(0, _yAmount, 0);
        TranslateWorldPosition(amount);
    }

    public void TranslateWorldPositionOnZ(float _zAmount) {
        var amount = new Vector3(0, 0, _zAmount);
        TranslateWorldPosition(amount);
    }

    public void RepositionWorldCenter(Vector3 _center) {
        var offset = _center - m_WorldBounds.GetCenter();
        TranslateWorldPosition(offset);
    }

    public void TranslateWorldPosition(Vector3 _offset) {
        for (int i = 0; i < m_Verts.Count; i++)
            m_Verts[i] += _offset;
        CalculateWorldBounds();
    }

    // ================================================
    // INTERNAL
    // ================================================
    // This method makes a duplicated list of all the vertices and UV coordinates
    // from all the TriFragments and TriClusters that have been fed to this cluster.
    void CreateUnions(out List<Vector3> _vertUnion, out List<Vector2> _uvUnion) {
        _vertUnion = new List<Vector3>();
        _uvUnion = new List<Vector2>();

        for(int i = 0; i < m_Fragments.Count; i++) {
            _vertUnion.Add(m_Fragments[i].verts);
            _uvUnion.Add(m_Fragments[i].uvs);
        }
    }

    void RemoveDuplicates(List<Vector3> _verts, List<Vector2> _uvs, List<int> _indexMapping) {
        for(int i = 0; i < _uvs.Count; i++) {
            bool isDuplicate = false;
            for (int j = 0; j < m_UVs.Count; j++) {
                if (_uvs[i].Approximately(m_UVs[j]) && _verts[i].Approximately(m_Verts[j])) {
                    isDuplicate = true;
                    _indexMapping.Add(j);
                    break;
                }
            }
            if (!isDuplicate) {
                m_UVs.Add(_uvs[i]);
                m_Verts.Add(_verts[i]);
                _indexMapping.Add(m_UVs.Count - 1);
            }
        }
    }

    void CalculateUVBounds() {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;

        for(int i = 0; i < m_UVs.Count; i++) {
            var uv = m_UVs[i];
            if (uv.x < minX)
                minX = uv.x;

            if (uv.x > maxX) 
                maxX = uv.x;

            if (uv.y < minY)
                minY = uv.y;

            if (uv.y > maxY) 
                maxY = uv.y;
        }
        m_UVBounds = new UVBounds(minX, maxX, minY, maxY);
    }

    void CalculateWorldBounds() {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        for (int i = 0; i < m_Verts.Count; i++) {
            var vert = m_Verts[i];
            if (vert.x < minX)
                minX = vert.x;

            if (vert.x > maxX)
                maxX = vert.x;

            if (vert.y < minY)
                minY = vert.y;

            if (vert.y > maxY)
                maxY = vert.y;

            if (vert.z < minZ)
                minZ = vert.z;

            if (vert.z > maxZ)
                maxZ = vert.z;
        }
        m_WorldBounds = new WorldBounds(minX, maxX, minY, maxY, minZ, maxZ);
    }

    // ================================================
    // OVERRIDES 
    // ================================================
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.Append("Verts : ");
        foreach(var v in m_Verts) 
            sb.Append(v.ToString());

        sb.Append("\n");

        sb.Append("UVs : ");
        foreach (var u in m_UVs)
            sb.Append(u.ToString());

        sb.Append("\n");

        sb.Append("\nTriangles : ");
        foreach (var t in m_Triangles)
            sb.Append(t.ToString());

        return sb.ToString();
    }
}
