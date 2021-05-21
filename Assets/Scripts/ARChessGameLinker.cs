using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Chess;
using Chess.Peices;
using JetBrains.Annotations;
using UnityEngine;

public class ARChessGameLinker : MonoBehaviour
{
    // White pieces
    [Header("White pieces")]
    public GameObject whiteRook;
    public GameObject whiteKnight;
    public GameObject whiteBishop;
    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whitePawn;

    // Black pieces
    [Header("Black pieces")]
    public GameObject blackRook;
    public GameObject blackKnight;
    public GameObject blackBishop;
    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackPawn;
    
    // Board parameters
    [Header("Board and pieces parameters")]
    public float boardSize = 48;
    [HideInInspector] public float squareSize;
    public float piecesScale = 1;
    [SerializeField] private int piecesLayer = 6;
    
    // Chess game
    public static ChessGame game = new ChessGame(2, false);
    private List<GameObject> pieces = new List<GameObject>();


    // Start is called before the first frame update
    void Awake()
    {
        squareSize = boardSize / 8;
        // Instantiate pieces when using the editor
        if (Application.isEditor)
            UpdateChessBoard();
    }

    // Convert real scene coordinates to chess array coordinates
    public Point ToPoint(float x, float z)
    {
        return new Point(4 + Mathf.FloorToInt(8f * z / boardSize), 4 + Mathf.FloorToInt(8f * x / boardSize));
    }
    
    // Reverse operation, returns (x, z) pair
    public Tuple<float, float> ToCord(Point loc)
    {
        return new Tuple<float, float>(boardSize * (loc.Y - 3.5f) / 8f, boardSize * (loc.X - 3.5f) / 8f);
    }

    private void ClearChessboard()
    {
        foreach (var piece in pieces)
        {
            Destroy(piece);
        }
        pieces.Clear();
    }

    public void UpdateChessBoard()
    {
        ClearChessboard();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                Peice piece = game.GetPeice(new Point(i, j));
                if (piece != null)
                {
                    var coordinates = ToCord(new Point(i, j));
                    var worldCoordinates = transform.TransformPoint(coordinates.Item1, 0, coordinates.Item2);
                    var rotation = piece.MySide == PSide.White
                        ? Quaternion.Euler(-90, 0, 0)
                        : Quaternion.Euler(-90, 180, 0);
                    GameObject p = Instantiate(GetPiecePrefab(piece), worldCoordinates, rotation, transform);
                    p.layer = piecesLayer;
                    pieces.Add(p);
                }
            }
        }
    }

    private GameObject GetPiecePrefab(Chess.Peices.Peice piece)
    {
        return piece.MySide switch
        {
            PSide.White => piece.MyType switch
            {
                PType.King => whiteKing,
                PType.Queen => whiteQueen,
                PType.Rook => whiteRook,
                PType.Bishop => whiteBishop,
                PType.Knight => whiteKnight,
                PType.Pawn => whitePawn,
                _ => throw new ArgumentOutOfRangeException()
            },
            PSide.Black => piece.MyType switch
            {
                PType.King => blackKing,
                PType.Queen => blackQueen,
                PType.Rook => blackRook,
                PType.Bishop => blackBishop,
                PType.Knight => blackKnight,
                PType.Pawn => blackPawn,
                _ => throw new ArgumentOutOfRangeException()
            },
            _ => throw new ArgumentOutOfRangeException()
        };
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
