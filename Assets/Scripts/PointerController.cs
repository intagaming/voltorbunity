using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerController : MonoBehaviour {
    public enum PointerType {
        Tile,
        MemoButton,
        QuitButton,
        MenuButton
    }

    public enum HighlightType {
        NotInMemoMode,
        InMemoMode
    }

    public GameController game;
    public Sprite tilePointerSprite;
    public Sprite memoPointerSprite;
    public Sprite quitPointerSprite;
    public Sprite menuPointerSprite;
    public float timeBetweenUpdates = 0.1667f;
    private float timestamp;

    private PointerType pointerType = PointerType.MenuButton;
    //private HighlightType highlightType = HighlightType.NotInMemoMode;
    private Vector2Int currentPosition = new Vector2Int(1, 1);

    public bool ignoreInput = false;
    
    void Start() {
        transform.localPosition = GetPointerPosition(1, 1);
    }

    private void Update() {
        UpdatePointer();
    }


    public void OnGUI_Pointer(Event e) {
        if (!isShowing() || ignoreInput) return;
        if (Time.time < timestamp) {
            return;
        }
        if (e.isKey && e.type == EventType.KeyDown) {
            if (pointerType == PointerType.MenuButton) {
                if (e.keyCode == KeyCode.W || e.keyCode == KeyCode.UpArrow) {
                    if (currentPosition.x == currentPosition.y) {
                        currentPosition.x = 1;
                    } else currentPosition.x++;
                } else if (e.keyCode == KeyCode.S || e.keyCode == KeyCode.DownArrow) {
                    if (currentPosition.x == 1) {
                        currentPosition.x = currentPosition.y;
                    } else currentPosition.x--;
                } else if(e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
                    Transform menuCanvas = GameObject.Find("Lower Screen/MenuCanvas").transform;
                    for (int i = 2; i < menuCanvas.childCount; i++) {
                        if(menuCanvas.GetChild(i).gameObject.activeSelf) {
                            menuCanvas.GetChild(i).GetChild(menuCanvas.GetChild(i).childCount - currentPosition.x).gameObject.GetComponent<Button>().onClick.Invoke();
                            break;
                        }
                    }
                }
                timestamp = Time.time + timeBetweenUpdates;
            } else if(pointerType == PointerType.Tile || pointerType == PointerType.MemoButton || pointerType == PointerType.QuitButton) {
                if (e.keyCode == KeyCode.D || e.keyCode == KeyCode.RightArrow) {
                    currentPosition.y += 1;
                    if (currentPosition.y == 6) {
                        if (currentPosition.x <= 3) {
                            pointerType = PointerType.MemoButton;
                        } else {
                            pointerType = PointerType.QuitButton;
                        }
                    } else if (currentPosition.y == 7) {
                        currentPosition.y = 1;
                        pointerType = PointerType.Tile;
                    }
                } else if (e.keyCode == KeyCode.A || e.keyCode == KeyCode.LeftArrow) {
                    currentPosition.y -= 1;
                    pointerType = PointerType.Tile;
                    if (currentPosition.y == 0) {
                        currentPosition.y = 6;
                        if (currentPosition.x <= 3) {
                            pointerType = PointerType.MemoButton;
                        } else {
                            pointerType = PointerType.QuitButton;
                        }
                    }
                } else if (e.keyCode == KeyCode.W || e.keyCode == KeyCode.UpArrow) {
                    if (currentPosition.y == 6) {
                        SwitchPointerPingPong();
                    } else {
                        currentPosition.x -= 1;
                        if (currentPosition.x == 0) {
                            currentPosition.x = 5;
                        }
                    }
                } else if (e.keyCode == KeyCode.S || e.keyCode == KeyCode.DownArrow) {
                    if (currentPosition.y == 6) {
                        SwitchPointerPingPong();
                    } else {
                        currentPosition.x += 1;
                        if (currentPosition.x == 6) {
                            currentPosition.x = 1;
                        }
                    }
                } else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) {
                    if(currentPosition.y == 6) {
                        
                    } else {
                        int row = currentPosition.x - 1;
                        int column = currentPosition.y - 1;
                        if (game.renderManager.flipStatus[row, column] == true) return;
                        ignoreInput = true;
                        game.FlipTile(row, column);
                    }
                }
                timestamp = Time.time + timeBetweenUpdates;
            }
        }

    }

    void UpdatePointer() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        switch(pointerType) {
            case PointerType.Tile:
                spriteRenderer.sprite = tilePointerSprite;
                break;
            case PointerType.MemoButton:
                spriteRenderer.sprite = memoPointerSprite;
                break;
            case PointerType.QuitButton:
                spriteRenderer.sprite = quitPointerSprite;
                break;
            case PointerType.MenuButton:
                spriteRenderer.sprite = menuPointerSprite;
                break;
        }
        transform.localPosition = GetPointerPosition(currentPosition.x, currentPosition.y);
        
    }

    void SwitchPointerPingPong() {
        if (pointerType == PointerType.MemoButton) {
            pointerType = PointerType.QuitButton;
        } else {
            pointerType = PointerType.MemoButton;
        }
    }

    Vector2 GetPointerPosition(int row, int column) {
        if (pointerType == PointerType.MenuButton) {
            // row 1 is the last item (bottom), with the max of 4 (4 options menu)
            return new Vector2(140, -117 + 24 * (row - 1));
        } else {
            if (column == 6) {
                if (pointerType == PointerType.MemoButton) {
                    return new Vector2(196, -8);
                } else {
                    return new Vector2(194, -163);
                }
            }
            return new Vector2(6 + 32 * (column - 1), -6 - 32 * (row - 1));
        }
    }

    public bool isShowing() {
        return gameObject.GetComponent<SpriteRenderer>().enabled;
    }

    public void showPointer() {
        currentPosition.x = 1;
        currentPosition.y = 1;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    // 'column' for menu pointer is the maximum row (3 or 4)
    public void showPointer(int row, int column) {
        currentPosition.x = row;
        currentPosition.y = column;
        gameObject.GetComponent<SpriteRenderer>().enabled = true;
    }

    public void hidePointer() {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void setPosition(int row, int column) {
        currentPosition.x = row;
        currentPosition.y = column;
    }

    public void setPosition(int row) {
        currentPosition.x = row;
    }

    public Vector2Int getPosition() {
        return currentPosition;
    }

    public void setPointerType(PointerType type) {
        pointerType = type;
    }

    

}
