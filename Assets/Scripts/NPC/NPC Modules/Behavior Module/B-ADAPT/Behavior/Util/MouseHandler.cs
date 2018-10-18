using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A class to handle mouse actions.
/// </summary>
public class MouseHandler : MonoBehaviour 
{

    private static MouseHandler instance;

    /// <summary>
    /// Self-generating singleton.
    /// </summary>
    public static MouseHandler GetInstance()
    {
        if (instance == null)
        {
            instance = new GameObject("MouseHandler").AddComponent<MouseHandler>();
            instance.Init();
        }
        return instance;
    }

    public delegate void MouseSelectHandler(RaycastHit hit);

    public delegate void DragAction(Vector3 mousePosition);

    public class ClickActions
    {
        public MouseSelectHandler onLeftClick;

        public MouseSelectHandler onRightClick;

        public ClickActions(MouseSelectHandler onLeftClick, MouseSelectHandler onRightClick)
        {
            this.onLeftClick = onLeftClick;
            this.onRightClick = onRightClick;
        }
    }

    public class DragActions
    {
        public DragAction onStartDrag;

        public DragAction onKeepDrag;

        public DragAction onStopDrag;

        public DragActions(DragAction onStartDrag, DragAction onKeepDrag, DragAction onStopDrag)
        {
            this.onStartDrag = onStartDrag;
            this.onKeepDrag = onKeepDrag;
            this.onStopDrag = onStopDrag;
        }
    }

    private HashSet<ClickActions> clickActions;

    private HashSet<DragActions> dragActions;

    private List<MouseSelectHandler> onLeftClick;

    private List<MouseSelectHandler> onRightClick;

    private List<DragAction> onStartDrag;

    private List<DragAction> onKeepDrag;

    private List<DragAction> onStopDrag;

    private bool initialized;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        if (!initialized)
        {
            Init();
        }
    }

    void Init()
    {
        clickActions = new HashSet<ClickActions>();
        dragActions = new HashSet<DragActions>();
        initialized = true;
    }

    void Update()
    {
        foreach (ClickActions action in new List<ClickActions>(clickActions))
        {
            HandleWorldClicks(action.onLeftClick, action.onRightClick);
        }
        foreach (DragActions action in new List<DragActions>(dragActions))
        {
            HandleWorldDrag(action.onStartDrag, action.onKeepDrag, action.onStopDrag);
        }
    }

    public void RegisterClickEvents(ClickActions clickAction)
    {
        this.clickActions.Add(clickAction);
    }

    public void DeregisterClickEvents(ClickActions clickAction)
    {
        this.clickActions.Remove(clickAction);
    }

    public void RegisterDragEvents(DragActions dragAction)
    {
        this.dragActions.Add(dragAction);
    }

    public void DeregisterDragEvents(DragActions dragAction)
    {
        this.dragActions.Remove(dragAction);
    }

    /// <summary>
    /// Handle clicks within the game world.
    /// </summary>
    /// <param name="onLeftClick">The method to be executed on a left click.</param>
    /// <param name="onRightClick">The method to be executed on a right click.</param>
    private void HandleWorldClicks(MouseSelectHandler onLeftClick, MouseSelectHandler onRightClick)
    {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);
        if (leftClick || rightClick)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                if (leftClick)
                {
                    onLeftClick.Invoke(hit);
                }
                else if (rightClick)
                {
                    onRightClick.Invoke(hit);
                }
            }
        }
    }

    /// <summary>
    /// Handle dragging the mouse within the game world.
    /// </summary>
    /// <param name="onStart">The method to be executed when mouse button is clicked first. Gets the mouse position as argument.</param>
    /// <param name="onDrag">The method to be executed when the mouse button remains clicked. Gets the mouse position as argument.</param>
    /// <param name="onStop">The method to be executed when the mouse button is released. Gets the mouse position as argument.</param>
    private void HandleWorldDrag(DragAction onStart, DragAction onDrag, DragAction onStop)
    {
        bool dragStart = Input.GetMouseButtonDown(0);
        bool dragContinued = Input.GetMouseButton(0);
        bool dragStopped = Input.GetMouseButtonUp(0);
        if (dragStart)
        {
            onStart.Invoke(Input.mousePosition);
        }
        if (dragContinued)
        {
            onDrag.Invoke(Input.mousePosition);
        }
        if (dragStopped)
        {
            onStop.Invoke(Input.mousePosition);
        }
    }
}
