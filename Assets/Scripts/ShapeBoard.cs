using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShapeBoard : MonoBehaviour
{
    [Header("Board Properties")]
    [SerializeField] private int width=6;
    [SerializeField] private int height = 8;
    [SerializeField] private float spacingX;
    [SerializeField] private float spacingY;

    [Header("Related to shapes")]
    [SerializeField] private GameObject shapePerfab;
    [SerializeField] private List<GameObject> squaresPrefab;
    [SerializeField] private List<Sprite> sprites;
    private List<GameObject> shapes=new List<GameObject>();
    private List<Shape> ShapesToRemove=new List<Shape>();
    private int currentSquare = 0;

    [Space]
    [SerializeField] private float TimeBetweenMatches;

    [Space]
    [SerializeField] private Shape selectedShape;

    [SerializeField] private GameObject effect;

    [System.NonSerialized] public static ShapeBoard instance;
    private Node[,] shapeBoard;
    private bool isProcesingMove;

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if(hit.collider!=null&&hit.collider.gameObject.GetComponent<Shape>())
            {
                if (isProcesingMove)
                    return;
                Shape shape = hit.collider.gameObject.GetComponent<Shape>();
                Debug.Log("Clicked on shape " + shape.gameObject);

                SelectShape(shape);
            }
        }
    }
    private void Start()
    {
        CreateShapes();
        InitializeBoard();
    }
    private void CreateShapes()
    {
        shapeBoard = new Node[width, height];

        spacingX = (float)(width - 1) / 2;
        spacingY = (float)(height - 1) / 2 + 1;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2 position = new Vector2(x - spacingX, y - spacingY);
                GameObject shape = Instantiate(shapePerfab, position, Quaternion.identity, transform);
                
                if(currentSquare==0)
                {
                    Instantiate(squaresPrefab[1], position, Quaternion.identity, transform);
                    currentSquare = 1;
                }
                else
                {
                    Instantiate(squaresPrefab[0], position, Quaternion.identity, transform);
                    currentSquare = 0;
                }

                shape.GetComponent<Shape>().SetShape(x, y);
                shapes.Add(shape);
                shapeBoard[x, y] = new Node(true, shape);
            }
        }
    }
    private void InitializeBoard()
    {
        foreach (GameObject shape in shapes)
        {
            int randIndex = Random.Range(0, sprites.Count);

            shape.GetComponent<Shape>().SetShapeType(randIndex,sprites[randIndex]);
        }
        
        if (CheckBoard())
        {
            Debug.Log("Recreate the Board");
            InitializeBoard();
        }
        else
        {
            Debug.Log("Start Game");
            shapes.Clear();
        }
    }
    public bool CheckBoard()
    {
        if (GameManager.Instance.isGameEnded)
            return false;

        Debug.Log("Cheking Board");
        bool hasMatched = false;

        ShapesToRemove.Clear();

        foreach(Node nodeShape in shapeBoard)//make sure all the board !ismatching on every start of check.
        {
            if(nodeShape.shape!=null)
            {
                nodeShape.shape.GetComponent<Shape>().isMatching = false;
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(shapeBoard[x,y].isUsable)
                {
                    Shape shape = shapeBoard[x, y].shape.GetComponent<Shape>();

                    if (!shape.isMatching)
                    {
                        MatchResult matchShapes = IsConnected(shape);

                        if(matchShapes.connectedShapes.Count>=3)
                        {
                            MatchResult superMatchedShapes = SuperMatch(matchShapes);

                            ShapesToRemove.AddRange(superMatchedShapes.connectedShapes);

                            foreach (Shape currentShape in superMatchedShapes.connectedShapes)
                                currentShape.isMatching = true;

                            hasMatched = true;
                        }
                    }
                }
            }
        }
        
        return hasMatched;
    }
    IEnumerator ProcessTurnOnMatchedBoard(bool _subtractMoves)
    {
        foreach (Shape shapeToRemove in ShapesToRemove)
        {
            shapeToRemove.isMatching = false;
        }

        RemoveAndRefill(ShapesToRemove);
        GameManager.Instance.ProcessTurn(ShapesToRemove.Count,_subtractMoves);//add point and subtract moves.

        yield return new WaitForSeconds(TimeBetweenMatches);

        if(CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(false));
        }
    }

    private MatchResult SuperMatch(MatchResult _matchedShapes)
    {
        if(_matchedShapes.direction==MatchDirection.Horizontal|| _matchedShapes.direction == MatchDirection.LongHorizontal)
        {
            foreach(Shape _shape in _matchedShapes.connectedShapes)
            {
                List<Shape> extraConnectedShapes = new List<Shape>();

                CheckDirection(_shape, Vector2Int.up, extraConnectedShapes);
                CheckDirection(_shape, Vector2Int.down, extraConnectedShapes);

                if(extraConnectedShapes.Count>=2)
                {
                    Debug.Log("Super Horizontal Match");
                    extraConnectedShapes.AddRange(_matchedShapes.connectedShapes);

                    return new MatchResult
                    {
                        connectedShapes = extraConnectedShapes,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedShapes = _matchedShapes.connectedShapes,
                direction = _matchedShapes.direction
            };
        }

        else if (_matchedShapes.direction == MatchDirection.Vertical || _matchedShapes.direction == MatchDirection.LongVertical)
        {
            foreach (Shape _shape in _matchedShapes.connectedShapes)
            {
                List<Shape> extraConnectedShapes = new List<Shape>();

                CheckDirection(_shape, Vector2Int.left, extraConnectedShapes);
                CheckDirection(_shape, Vector2Int.right, extraConnectedShapes);

                if (extraConnectedShapes.Count >= 2)
                {
                    Debug.Log("Super Vertical Match");
                    extraConnectedShapes.AddRange(_matchedShapes.connectedShapes);

                    return new MatchResult
                    {
                        connectedShapes = extraConnectedShapes,
                        direction = MatchDirection.Super
                    };
                }
            }
            return new MatchResult
            {
                connectedShapes = _matchedShapes.connectedShapes,
                direction = _matchedShapes.direction
            };
        }
        return null;
    }
    private MatchResult IsConnected(Shape shape)
    {
        List<Shape> connectedShapes = new List<Shape>();
        ShapeType shapeType = shape.shapeType;

        connectedShapes.Add(shape);

        //horizontal check
        CheckDirection(shape, Vector2Int.right, connectedShapes); //checking right.
        CheckDirection(shape, Vector2Int.left, connectedShapes); //checking left.

        if(connectedShapes.Count==3)//3 match horizontal?
        {
            Debug.Log("3 match horizontal, color match is: " + connectedShapes[0].shapeType);

            return new MatchResult
            {
                connectedShapes = connectedShapes,
                direction = MatchDirection.Horizontal
            };
        }
        else if(connectedShapes.Count >= 3)//more then 3 match horizontal?
        {
            Debug.Log("more then 3 match horizontal, color match is: " + connectedShapes[0].shapeType);

            return new MatchResult
            {
                connectedShapes = connectedShapes,
                direction = MatchDirection.LongHorizontal
            };
        }

        connectedShapes.Clear();
        connectedShapes.Add(shape);

        //vertical check
        CheckDirection(shape, Vector2Int.up, connectedShapes); //checking up.
        CheckDirection(shape, Vector2Int.down, connectedShapes); //checking down.

        if (connectedShapes.Count == 3)//3 match vertical?
        {
            Debug.Log("3 match vertical, color match is: " + connectedShapes[0].shapeType);

            return new MatchResult
            {
                connectedShapes = connectedShapes,
                direction = MatchDirection.Vertical
            };
        }
        else if (connectedShapes.Count >= 3)//more then 3 match vertical?
        {
            Debug.Log("more then 3 match vertical, color match is: " + connectedShapes[0].shapeType);

            return new MatchResult
            {
                connectedShapes = connectedShapes,
                direction = MatchDirection.LongVertical
            };
        }

        return new MatchResult
        {
            connectedShapes = connectedShapes,
            direction = MatchDirection.None
        };
    }
    private void CheckDirection(Shape shape,Vector2Int direction,List<Shape> connectedShapes)
    {
        ShapeType shapeType = shape.shapeType;
        int x = shape.xIndex + direction.x;
        int y = shape.yIndex + direction.y;

        //check boundaries.
        while(x>=0 && x<width && y>=0 && y<height)
        {
            if(shapeBoard[x,y].isUsable)
            {
                Shape neighborShape = shapeBoard[x, y].shape.GetComponent<Shape>();

                if(!neighborShape.isMatching && neighborShape.shapeType == shapeType)
                {
                    connectedShapes.Add(neighborShape);

                    x += direction.x;
                    y += direction.y;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }
    }

    #region Swapping Shapes

    public void SelectShape(Shape _shape)
    {
        if(selectedShape==null)
        {
            Debug.Log(_shape);
            selectedShape = _shape;
        }
        else if(selectedShape==_shape)
        {
            selectedShape = null;
        }
        else if(selectedShape!=_shape)
        {
            SwapShape(selectedShape, _shape);
            selectedShape = null;
        }
    }
    private void SwapShape(Shape _currentShape,Shape _targetShape)
    {
        if(!IsAdjacent(_currentShape,_targetShape))
        {
            return;
        }
        DoSwap(_currentShape, _targetShape);

        isProcesingMove = true;

        StartCoroutine(ProcessMatches(_currentShape, _targetShape));
    }
    private bool IsAdjacent(Shape _currentShape,Shape _targetShape)
    {
        return Mathf.Abs(_currentShape.xIndex - _targetShape.xIndex) + Mathf.Abs(_currentShape.yIndex - _targetShape.yIndex) == 1;
    }
    private void DoSwap(Shape _currentShape,Shape _targetShape)
    {
        GameObject temp = shapeBoard[_currentShape.xIndex, _currentShape.yIndex].shape;

        shapeBoard[_currentShape.xIndex, _currentShape.yIndex].shape = shapeBoard[_targetShape.xIndex, _targetShape.yIndex].shape;
        shapeBoard[_targetShape.xIndex, _targetShape.yIndex].shape = temp;

        int tempXIndex = _currentShape.xIndex;
        int tempYIndex = _currentShape.yIndex;
        _currentShape.xIndex = _targetShape.xIndex;
        _currentShape.yIndex = _targetShape.yIndex;

        _targetShape.xIndex = tempXIndex;
        _targetShape.yIndex = tempYIndex;

        _currentShape.MoveToTarget(shapeBoard[_targetShape.xIndex, _targetShape.yIndex].shape.transform.position);
        _targetShape.MoveToTarget(shapeBoard[_currentShape.xIndex, _currentShape.yIndex].shape.transform.position);
    }
    IEnumerator ProcessMatches(Shape _currentShape,Shape _targetShape)
    {
        yield return new WaitForSeconds(0.3f);

        if(CheckBoard())
        {
            StartCoroutine(ProcessTurnOnMatchedBoard(true));
        }
        else
        {
            DoSwap(_currentShape, _targetShape);
            GameManager.Instance.ProcessTurn(0, true);
        }

        isProcesingMove = false;
    }
    #endregion


    #region Cascading Shapes
    private void RemoveAndRefill(List<Shape> _shapesToRemove)
    {
        foreach (Shape shape in _shapesToRemove)
        {
            int _xIndex = shape.xIndex;
            int _yIndex = shape.yIndex;
            Instantiate(effect, shape.transform.position, Quaternion.identity);
            Destroy(shape.gameObject);

            shapeBoard[_xIndex, _yIndex] = new Node(true, null);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (shapeBoard[x, y].shape == null)
                {
                    Debug.Log("The location x: " + x + " y:" + y + "is empty");
                    RefillShape(x, y);
                }
            }
        }
    }

    private void RefillShape(int x, int y)
    {
        int yOffset = 1;

        while (y + yOffset < height && shapeBoard[x, y + yOffset].shape == null)
        {
            Debug.Log("The shape above me in null, but i'm not at the top of the board");
            yOffset++;
        }

        if (y + yOffset < height && shapeBoard[x, y + yOffset].shape != null)//found a shape
        {
            Shape shapeAbove = shapeBoard[x, y + yOffset].shape.GetComponent<Shape>();
            //move to correct position
            Vector3 targetPos = new Vector3(x - spacingX, y - spacingY, shapeAbove.transform.position.z);
            //move to position
            shapeAbove.MoveToTarget(targetPos);
            //update incidices
            shapeAbove.SetShape(x, y);
            //update board
            shapeBoard[x, y] = shapeBoard[x, y + yOffset];
            //set the position the shape came from
            shapeBoard[x, y + yOffset] = new Node(true, null);
        }
        if (y + yOffset == height)//top of the board
        {
            SpawnShapeAtTop(x);
        }
    }

    private void SpawnShapeAtTop(int x)
    {
        int index = FindIndexOfLowestNull(x);
        int posToMoveTo = height - index;
        int randIndex = Random.Range(0, sprites.Count);

        GameObject newShape = Instantiate(shapePerfab, new Vector2(x - spacingX, height - spacingY), Quaternion.identity,transform);
        Shape thisShape = newShape.GetComponent<Shape>();

        thisShape.SetShapeType(randIndex, sprites[randIndex]);
        thisShape.SetShape(x, index);

        //Debug.Log(index);
        shapeBoard[x, index] = new Node(true, newShape);

        Vector3 targetPos = new Vector3(newShape.transform.position.x, newShape.transform.position.y - posToMoveTo, newShape.transform.position.z);
        thisShape.MoveToTarget(targetPos);
    }

    private int FindIndexOfLowestNull(int x)
    {
        int lowestNull = 99;

        for (int y = height - 1; y >= 0; y--)
        {
            if (shapeBoard[x, y].shape == null)
            {
                lowestNull = y;
            }
        }
        return lowestNull;
    }
    #endregion
}

public class MatchResult
{
    public List<Shape> connectedShapes;
    public MatchDirection direction;
}
public enum MatchDirection
{
    Vertical,
    Horizontal,
    LongVertical,
    LongHorizontal,
    Super,
    None
}
