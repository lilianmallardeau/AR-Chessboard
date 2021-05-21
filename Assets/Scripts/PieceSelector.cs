using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Cryptography;
using Chess;
using Chess.Peices;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;

public class PieceSelector : MonoBehaviour
{
    private List<GameObject> _squareColliders = new List<GameObject>();
    private ChessGame _currentGame = ARChessGameLinker.game;
    [SerializeField] private ARChessGameLinker currentBoard;
    
    
    // Square hover
    [SerializeField] private GameObject squarePossibleMovementsPrefab;
    [SerializeField] private GameObject selectionSquarePrefab;
    
    private GameObject _squareSelection;
    private Point _currentHoverSquareCord;
    private bool _selectorInBound;
    
    
    // Selected piece
    [SerializeField] private Material selectedMaterial; 
    private bool _isPieceSelected = false;
    private GameObject _selectedPiece;
    private Material _prevMat;
    
    
    // Popup & button management
    [SerializeField] private GameObject popup;
    [SerializeField] private GameObject selectionButton;
    [SerializeField] private GameObject offerDrawButton;
    [SerializeField] private GameObject acceptDrawButton;
    [SerializeField] private GameObject declineDrawButton;
    private Text _popupText;
    private EndgamePopupBehavior _popupBehavior;
    private bool _drawOffered = false;

    // Convert real scene coordinates to chess array coordinates
    public Point ToPoint(float x, float z)
    {
        return new Point(4 + Mathf.FloorToInt(8f * z / currentBoard.boardSize), 4 + Mathf.FloorToInt(8f * x / currentBoard.boardSize));
    }
    
    // Reverse operation, returns (x, z) pair
    public Tuple<float, float> ToCord(Point loc)
    {
        return new Tuple<float, float>(currentBoard.boardSize * (loc.Y - 3.5f) / 8f, currentBoard.boardSize * (loc.X - 3.5f) / 8f);
    }

    private void Start()
    {
        // Initiate hover square UI elements
        _squareSelection = Instantiate(selectionSquarePrefab, currentBoard.transform);
        _squareSelection.transform.localScale = new Vector3(currentBoard.squareSize, 1, currentBoard.squareSize) / (float)10;

        // Register with events
        _currentGame.OnCheck += OnCheck;
        _currentGame.OnEndGame += OnEndGame;
        
        // Initiate popup elements
        _popupText = popup.GetComponent<Text>();
        _popupBehavior = popup.GetComponent<EndgamePopupBehavior>();
    }

    private void Update()
    {
        UpdateSelectionSquare();
        

        /*Destroy(_prevSquareSelection);
        if (Mathf.Abs(position.x) <= currentBoard.boardSize / 2 && Mathf.Abs(position.z) <= currentBoard.boardSize / 2)
        {
            _prevSquareSelection = _squareSelection;
            _squareSelection = Instantiate(selectionSquarePrefab, currentBoard.transform);
            Tuple<float, float> pos = ToCord(ToPoint(position.x, position.z));
            
            _squareSelection.transform.localPosition = new Vector3(pos.Item1, currentBoard.transform.position.y+0.1f, pos.Item2);
            _squareSelection.transform.localScale = new Vector3(currentBoard.squareSize, 1, currentBoard.squareSize) / 10;
            
        }*/
    }

    private void UpdateSelectionSquare()
    {
        Vector3 position = currentBoard.transform.InverseTransformPoint(transform.position);
        _currentHoverSquareCord = ToPoint(position.x, position.z);
        // selector is inbound of the board
        if (_currentHoverSquareCord.X >= 0 && _currentHoverSquareCord.X < 8 && _currentHoverSquareCord.Y >= 0 && _currentHoverSquareCord.Y < 8)
        {
            _squareSelection.SetActive(true);
            Tuple<float, float> pos = ToCord(ToPoint(position.x, position.z));
            _squareSelection.transform.localPosition = new Vector3(pos.Item1, currentBoard.transform.position.y+0.2f, pos.Item2);
            _selectorInBound = true;
        }
        // selector out of bound
        else
        {
            _squareSelection.SetActive(false);
            _selectorInBound = false;
        }
    }
    
    // Destroy all objects in _squareColliders
    void RemoveSquareColliders()
    {
        foreach (var colliders in _squareColliders)
        {
            Destroy(colliders);
        }
        _squareColliders.Clear();
    }

    public void SelectSquare()
    {
        if (!_selectorInBound) return;

        GameObject piece = GetPieceGameObject(_currentHoverSquareCord);
        // When no piece is currently selected
        if (!_isPieceSelected)
        {
            if (piece)
            {
                // Select piece (checks if selected piece is on player's side)
                if (_currentGame.SelectPeice(_currentHoverSquareCord))
                {
                    // Fetch and display possible moves
                    List<Point> possibleMoves = _currentGame.GetSelectedMoves();
                    // Check if selected piece has available moves, else unselect it
                    if (!possibleMoves.Any())
                    {
                        return;
                    }
                    foreach (Point move in possibleMoves)
                    {
                        // Create square colliders and add them to _squareColliders
                        Tuple<float, float> position = ToCord(move);
                        var p = Instantiate(squarePossibleMovementsPrefab, currentBoard.transform);
                        p.transform.localPosition = new Vector3(position.Item1, currentBoard.transform.position.y+0.1f, position.Item2);
                        p.transform.localScale *= 0.5f;
                        _squareColliders.Add(p);
                    }
                    
                    _selectedPiece = piece;
                    _prevMat = _selectedPiece.GetComponent<Renderer>().material;
                    _selectedPiece.GetComponent<Renderer>().material = selectedMaterial;
                    _isPieceSelected = true;
                }
            }
        }
        
        // When a piece is already selected
        else
        {
            // If reselecting same piece, deselect
            if (piece == _selectedPiece)
            {
                _selectedPiece.GetComponent<Renderer>().material = _prevMat;
                _selectedPiece = null;
                _isPieceSelected = false;
                RemoveSquareColliders();
                return;
            }
            else
            {
                // Check that we can make this move
                if (!_currentGame.GetSelectedMoves().Contains(_currentHoverSquareCord))
                    return;
                
                // If there's already a piece in the case we've to delete the corresponding game object
                if (piece)
                    Destroy(piece);
                _currentGame.MakeMove(_currentHoverSquareCord);
                var newCoord = ToCord(_currentHoverSquareCord);
                _selectedPiece.transform.localPosition = new Vector3(newCoord.Item1, 0, newCoord.Item2);
                _selectedPiece.GetComponent<Renderer>().material = _prevMat;
                
                _selectedPiece = null;
                _isPieceSelected = false;
                RemoveSquareColliders();
                
                if (_currentGame.CanDraw())
                {
                    if (!_drawOffered)
                    {
                        offerDrawButton.SetActive(true);
                        _drawOffered = true;
                    }
                }
                else offerDrawButton.SetActive(false);
                
                selectionButton.GetComponentInChildren<Text>().color = _currentGame.turn == PSide.Black ? Color.white : Color
                    .black;
                selectionButton.GetComponent<Image>().color =
                    _currentGame.turn == PSide.Black ? Color.black : Color.white;
            }
        }
    }
    
    private GameObject GetPieceGameObject(Point piecePosition)
    {
        var position = ToCord(piecePosition);
        Vector3 raycastStart = currentBoard.transform.TransformPoint(new Vector3(position.Item1, 100, position.Item2));
        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Pieces")))
            return hit.transform.gameObject;
        return null;
    }

    private void OnCheck(PSide side, ChessGame.EndGames endType)
    {
        // Display pop-up message "Check !"
        _popupText.text = (side == PSide.Black ? "Black" : "White") + " is checked!";
        _popupText.gameObject.SetActive(true);
        _popupBehavior.StartPhaseOut(); 
    }

    private void OnEndGame(PSide side, ChessGame.EndGames endType)
    {
        // Display message accordingly to end type
        switch (endType)
        {
            case ChessGame.EndGames.Checkmate:
                _popupText.text = (side == PSide.Black ? "Black" : "White") + " is mated!";
                GetPieceGameObject(ChessGame.getKing(_currentGame.Gameboard, side)).GetComponent<Animator>().SetTrigger("Mated");
                break;
            case ChessGame.EndGames.Draw:
                _popupText.text = "Players agreed to a draw!";
                break;
            case ChessGame.EndGames.Stalemate:
                _popupText.text = "It's a stalemate!";
                break;
            default:
                _popupText.text = "This shouldn't happen. WTF man...";
                break;
        }

        _popupText.gameObject.SetActive(true);
        _popupBehavior.StartPhaseOut();
    }

    public void OfferDraw()
    {
        acceptDrawButton.SetActive(true);
        declineDrawButton.SetActive(true);
        offerDrawButton.SetActive(false);
        _drawOffered = true;
    }
    
    public void AnswerDraw(bool accepted)
    {
        if (accepted) _currentGame.Draw();
        acceptDrawButton.SetActive(false);
        declineDrawButton.SetActive(false);
    }
}
