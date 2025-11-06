using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.CellularAutomata
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Terrain Automata")]
    public class TerrainAutomata : ProceduralGenerationMethod
    {
        [Header("Automata Settings")]
        [Range(0, 100)]
        [SerializeField] private int landDensity = 45;
        [SerializeField] private int smoothSteps = 5;


        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            InitializeTerrainNoise();

            for (int i = 0; i < smoothSteps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SmoothStep();

                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
            }
        }

        private void InitializeTerrainNoise()
        {
            var landTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
            var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(WATER_TILE_NAME);

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    bool isLand;

                    if (x == 0 || y == 0 || x == Grid.Width - 1 || y == Grid.Lenght - 1)
                        isLand = false;
                    else
                        isLand = RandomService.Range(0, 100) < landDensity;

                    var template = isLand ? landTemplate : waterTemplate;
                    GridGenerator.AddGridObjectToCell(cell, template, true);
                }
            }
        }

        private void SmoothStep()
        {
            bool[,] nextState = new bool[Grid.Width, Grid.Lenght];

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    bool isLand = IsLand(cell);
                    int landNeighbours = CountLandNeighbours(x, y);

                    if (landNeighbours >= 4)
                        nextState[x, y] = true;
                    else
                        nextState[x, y] = false;
                }
            }

            ApplyNextState(nextState);
        }

        private void ApplyNextState(bool[,] nextState)
        {
            var landTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
            var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(WATER_TILE_NAME);

            for (int x = 0; x < Grid.Width; x++)
            {
                for (int y = 0; y < Grid.Lenght; y++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                        continue;

                    var template = nextState[x, y] ? landTemplate : waterTemplate;
                    GridGenerator.AddGridObjectToCell(cell, template, true);
                }
            }
        }

        private bool IsLand(Cell cell)
        {
            if (cell?.GridObject?.Template == null)
                return false;

            return cell.GridObject.Template.Name == GRASS_TILE_NAME;
        }

        private int CountLandNeighbours(int gridX, int gridY)
        {
            int count = 0;

            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x == gridX && y == gridY) continue;

                    if (!Grid.TryGetCellByCoordinates(x, y, out Cell neighbour))
                    {
                        continue;
                    }

                    if (IsLand(neighbour))
                        count++;
                }
            }

            return count;
        }
    }
}
