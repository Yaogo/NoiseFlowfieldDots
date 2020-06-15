using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseFlowField : MonoBehaviour
{
    FastNoise _fastNoise;
    public Vector3Int _gridSize;
    public float _cellSize;
    public Vector3[,,] _flowfieldDirection;
    public float _increment;
    public Vector3 _offset, offsetSpeed;

    public GameObject _particlePrefab;
    public int _amountOfParticles;
    [HideInInspector]
    public List<FlowFieldParticle> _particles;
    public List<MeshRenderer> _particleMeshRender;
    public float _particleScale, _particleMoveSpeed, _particleRotateSpeed;
    public float _spawnRadius;
    bool _particleSpawnValidation(Vector3 position)
    {
        bool valid = true;
        foreach (FlowFieldParticle particle in _particles)
        {
            if (Vector3.Distance(position, particle.transform.position) < _spawnRadius)
            {
                valid = false;
                break;
            }
        }
        if (valid)
            return true;
        else
            return false;
    }
    void Awake()
    {
        _flowfieldDirection = new Vector3[_gridSize.x, _gridSize.y, _gridSize.z];
        _fastNoise = new FastNoise();
        _particles = new List<FlowFieldParticle>();
        _particleMeshRender = new List<MeshRenderer>();
        for (int i = 0; i < _amountOfParticles; i++)
        {
            int attempt = 0;
            while (attempt < 100)
            {
                Vector3 randomPos = new Vector3(Random.Range(transform.position.x, transform.position.x + _gridSize.x * _cellSize),
                                Random.Range(transform.position.y, transform.position.y + _gridSize.y * _cellSize),
                                Random.Range(transform.position.z, transform.position.z + _gridSize.z * _cellSize));
                bool isValid = _particleSpawnValidation(randomPos);
                if (isValid)
                {
                    GameObject particleInstance = Instantiate(_particlePrefab, transform);
                    particleInstance.transform.position = randomPos;
                    particleInstance.transform.localScale = new Vector3(_particleScale, _particleScale, _particleScale);
                    _particles.Add(particleInstance.GetComponent<FlowFieldParticle>());
                    _particleMeshRender.Add(particleInstance.GetComponent<MeshRenderer>());
                    break;
                }
                if (!isValid)
                {
                    attempt++;
                }
            }
          //  Debug.Log(_particles.Count);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CalculateFlowfieldDirections();
        ParticleBehaviour();
    }
    void CalculateFlowfieldDirections()
    {
        //_fastNoise = new FastNoise();
        _offset = new Vector3(_offset.x + (offsetSpeed.x * Time.deltaTime), _offset.y + (offsetSpeed.y * Time.deltaTime), _offset.z + (offsetSpeed.z * Time.deltaTime));
        float xOff = 0f;
        for (int x = 0; x < _gridSize.x; x++)
        {
            float yOff = 0.0f;
            for (int y = 0; y < _gridSize.y; y++)
            {
                float zOff = 0.0f;
                for (int z = 0; z < _gridSize.z; z++)
                {
                    float noise = _fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y + zOff + _offset.z) + 1;
                    Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
                    _flowfieldDirection[x, y, z] = Vector3.Normalize(noiseDirection);
                    //Gizmos.color = new Color(noiseDirection.normalized.x, noiseDirection.normalized.y, noiseDirection.normalized.z);
                    //Vector3 pos = new Vector3(x, y, z) + transform.position;
                    //Vector3 endPos = pos + Vector3.Normalize(noiseDirection);
                    //Gizmos.DrawLine(pos, endPos);
                    //Gizmos.DrawSphere(endPos, 0.1f);
                    zOff += _increment;

                }
                yOff += _increment;
            }
            xOff += _increment;
        }
    }

    void ParticleBehaviour()
    {
        foreach (FlowFieldParticle p in _particles)
        {
            //x edges
            if (p.transform.position.x > transform.position.x + (_gridSize.x * _cellSize))
            {
                p.transform.position = new Vector3(transform.position.x, p.transform.position.y, p.transform.position.z);
            }
            if (p.transform.position.x < transform.position.x )
            {
                p.transform.position = new Vector3(transform.position.x + (_gridSize.x * _cellSize), p.transform.position.y, p.transform.position.z);
            }
            //z edge
            if (p.transform.position.z > transform.position.z + (_gridSize.z * _cellSize))
            {
                p.transform.position = new Vector3( p.transform.position.x, p.transform.position.y, transform.position.z);
            }
            if (p.transform.position.z < transform.position.z)
            {
                p.transform.position = new Vector3( p.transform.position.x, p.transform.position.y, transform.position.z + (_gridSize.z * _cellSize));
            }
            //y edge
            if (p.transform.position.y > transform.position.y + (_gridSize.y * _cellSize))
            {
                p.transform.position = new Vector3( p.transform.position.x, transform.position.y, p.transform.position.z);
            }
            if (p.transform.position.y < transform.position.y)
            {
                p.transform.position = new Vector3( p.transform.position.x, transform.position.y + (_gridSize.y * _cellSize), p.transform.position.z);
            }
            Vector3Int particlePos = new Vector3Int(
       Mathf.FloorToInt(Mathf.Clamp((p.transform.position.x - transform.position.x) / _cellSize, 0, _gridSize.x - 1)),
       Mathf.FloorToInt(Mathf.Clamp((p.transform.position.y - transform.position.y) / _cellSize, 0, _gridSize.y - 1)),
       Mathf.FloorToInt(Mathf.Clamp((p.transform.position.z - transform.position.z) / _cellSize, 0, _gridSize.z - 1))
       );
            p.ApplyRotation(_flowfieldDirection[particlePos.x,particlePos.y,particlePos.z],_particleRotateSpeed);
            p._moveSpeed = _particleMoveSpeed;
            //p.transform.localScale = new Vector3(_particleScale, _particleScale, _particleScale);
        }
   
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + new Vector3((_gridSize.x * _cellSize) * 0.5f, (_gridSize.y * _cellSize) * 0.5f, (_gridSize.z * _cellSize) * 0.5f), new Vector3(_gridSize.x * _cellSize, _gridSize.y * _cellSize, _gridSize.z * _cellSize));

        //_fastNoise = new FastNoise();
        //float xOff = 0f;
        //for (int x = 0; x < _gridSize.x; x++)
        //{
        //    float yOff = 0.0f;
        //    for (int y = 0; y < _gridSize.y; y++)
        //    {
        //        float zOff = 0.0f;
        //        for (int z = 0; z < _gridSize.z; z++)
        //        {
        //            float noise = _fastNoise.GetSimplex(xOff + _offset.x, yOff + _offset.y + zOff + _offset.z) + 1;
        //            Vector3 noiseDirection = new Vector3(Mathf.Cos(noise * Mathf.PI), Mathf.Sin(noise * Mathf.PI), Mathf.Cos(noise * Mathf.PI));
        //            Gizmos.color = new Color(noiseDirection.normalized.x, noiseDirection.normalized.y, noiseDirection.normalized.z);
        //            Vector3 pos = new Vector3(x, y, z) + transform.position;
        //            Vector3 endPos = pos + Vector3.Normalize(noiseDirection);
        //            Gizmos.DrawLine(pos, endPos);
             
        //            zOff += _increment;

        //        }
        //        yOff += _increment;
        //    }
        //    xOff += _increment;
        //}
    }
}
