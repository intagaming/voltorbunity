using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RenderManager : MonoBehaviour {
    public GameController game;
    public Tilemap digitsTilemap;
    public Tilemap tilesTilemap;
    public Tilemap flipEffectsTilemap;

    public Tile[] digitTiles;
    public Tile[] tilesTiles;
    public Tile[] flipEffectsTiles;

    [HideInInspector]
    public List<List<int>> horizontalValues = new List<List<int>>();
    [HideInInspector]
    public List<List<int>> verticalValues = new List<List<int>>();

    public Vector3Int[] horizontalTilePositions;
    public Vector3Int[] verticalTilePositions;

    public bool[,] flipStatus = new bool[5, 5];

    // Use this for initialization
    void Start() {
        for (int i = 0; i < 5; i++) {
            horizontalValues.Add(new List<int>() { 0, 0, 0 }); // {firstDigit, secondDigit, totalBombs}
            verticalValues.Add(new List<int>() { 0, 0, 0 });
        }
    }

    void Update() {
        //for(int i = 0; i < 5; i++) {
        //    for(int j = 0; j < 5; j++) {

        //    }
        //}
        for(int i = 0; i < 5; i++) {
            digitsTilemap.SetTile(horizontalTilePositions[i], digitTiles[horizontalValues[i][0]]);
            digitsTilemap.SetTile(horizontalTilePositions[i] + new Vector3Int(8, 0, 0), digitTiles[horizontalValues[i][1]]);
            digitsTilemap.SetTile(horizontalTilePositions[i] + new Vector3Int(8, -13, 0), digitTiles[horizontalValues[i][2]]);
            digitsTilemap.SetTile(verticalTilePositions[i], digitTiles[verticalValues[i][0]]);
            digitsTilemap.SetTile(verticalTilePositions[i] + new Vector3Int(8, 0, 0), digitTiles[verticalValues[i][1]]);
            digitsTilemap.SetTile(verticalTilePositions[i] + new Vector3Int(8, -13, 0), digitTiles[verticalValues[i][2]]);
        }
    }

    public void UpdateValues() {
        for (int i = 0; i < 5; i++) { // Row (Vertical indicator tiles)
            int totalValue = 0;
            int totalBomb = 0;
            for (int j = 0; j < 5; j++) {
                int value = game.values[i, j];
                if (value == 0) {
                    totalBomb++;
                } else {
                    totalValue += value;
                }
            }
            int tmp = totalValue;
            verticalValues[i][1] = tmp % 10;
            tmp = tmp / 10;
            verticalValues[i][0] = tmp % 10;
            verticalValues[i][2] = totalBomb;
        }
        for (int i = 0; i < 5; i++) { // Column (Horizontal indicator tiles)
            int totalValue = 0;
            int totalBomb = 0;
            for (int j = 0; j < 5; j++) {
                int value = game.values[j, i];
                if (value == 0) {
                    totalBomb++;
                } else {
                    totalValue += value;
                }
            }
            int tmp = totalValue;
            horizontalValues[i][1] = tmp % 10;
            tmp = tmp / 10;
            horizontalValues[i][0] = tmp % 10;
            horizontalValues[i][2] = totalBomb;
        }

    }

    void ResetFlipStatus() {
        for(int i = 0; i < 5; i++) {
            for(int j = 0; j < 5; j++) {
                flipStatus[i, j] = false;
                tilesTilemap.SetTile(new Vector3Int(i, -j, 0), null);
            }
        }
    }

}
