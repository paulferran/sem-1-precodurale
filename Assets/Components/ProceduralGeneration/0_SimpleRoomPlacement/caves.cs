using System;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Components.ProceduralGeneration;
using VTools.Grid;
using VTools.Utility;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;

[CreateAssetMenu(menuName = "Procedural Generation Method/caves")]
public class caves : ProceduralGenerationMethod
{
    [Header("Generation")]
    [Range(1, 1000)] public int maxSteps = 200;
    [Range(-10, 10)] public int offset;


    [Serializable]
    public class NoiseLayer
    {
        public string name = "Layer";
        public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        [Range(0f, 1f)] public float frequency = 0.04f;
        [Range(0f, 1f)] public float amplitude = 1f;

        public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.None;
        [Range(1, 10)] public int octaves = 2;
        [Range(0f, 5f)] public float lacunarity = 1.2f;
        [Range(-1f, 1f)] public float gain = 0.5f;
        public bool ceilling;

        public int GetOffset(int offset)
        {
            if (ceilling)
            {
                return (int)Math.Ceiling((double)offset / 2);
            }
            else
            {
                return -((int)Math.Floor((double)offset / 2));
            }
        }

    }

    [Header("Noise Layers")]
    public List<NoiseLayer> noiseLayers = new();

    [Header("Objects")]
    [SerializeField] private List<GameObject> objects = new();

    public FastNoiseLite InstantiateNoise(NoiseLayer layer)
    {
        var noise = new FastNoiseLite();
        noise.SetNoiseType(layer.noiseType);
        noise.SetFrequency(layer.frequency);
        noise.SetDomainWarpAmp(layer.amplitude);
        noise.SetFractalType(layer.fractalType);
        noise.SetFractalOctaves(layer.octaves);
        noise.SetFractalLacunarity(layer.lacunarity);
        noise.SetFractalGain(layer.gain);
        return noise;
    }

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        while (GameObject.Find("prefab(Clone)"))
        {
            DestroyImmediate(GameObject.Find("prefab(Clone)"));
        }

        cancellationToken.ThrowIfCancellationRequested();

        List<FastNoiseLite> noises = new();

        for (int i = 0; i < noiseLayers.Count(); i++)
        {
            noises.Add(InstantiateNoise(noiseLayers[i]));
        }

            for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                for (int layer = 0; layer < noiseLayers.Count(); layer++)
                {
                    FastNoiseLite noise = noises[layer];
                    float height = noise.GetNoise(x, y) + noiseLayers[layer].GetOffset(offset);
                    Instantiate(objects[0], new Vector3(x, (int)(height * 10) + 20, y), Quaternion.identity);
                }
            }
        }

        await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
    }
}
