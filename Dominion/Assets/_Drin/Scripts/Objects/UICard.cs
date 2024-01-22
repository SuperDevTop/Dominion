using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UIElements;

public class UICard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField]
    public CharacterData data;
    private TextMeshProUGUI cost;
    private UnityEngine.UI.Image image;
    private bool canInteract = false;
    private bool isDragging = false;
    private bool isReserve = false;
    private GameObject model;
    public RectTransform dashboardRect;
    private RectTransform myRect;
    private Vector2 ogPos;
    private Color costOGColor;
    public bool isSelected = false;
    private UIMgr mgr;
    private void Awake()
    {
        myRect = GetComponent<RectTransform>();
        cost = GetComponentInChildren<TextMeshProUGUI>();
        image = GetComponent<UnityEngine.UI.Image>();
        costOGColor = cost.color;
    }
    public void Setup(RectTransform dashboard, CharacterData data, UIMgr newMgr, bool isReserve = false)
    {
        this.dashboardRect = dashboard;
        this.data = data;
        image.sprite = data.sprite;
        cost.text = data.cost.ToString();
        this.isReserve = isReserve;
        this.mgr = newMgr;
        if (isReserve)
        {
            image.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            costOGColor.a = 0.2f;
            cost.color = costOGColor;
            return;
        }
        StartCoroutine(Run());
    }
    private Color clr = Color.white;
    private IEnumerator Run()
    {
        while (true)
        {
            canInteract = isReserve || DeckMgr.elixir >= data.cost;
            image.color = canInteract ? clr : new Color(0.2f, 0.2f, 0.2f, 1f);
            if (canInteract)
                costOGColor.a = 1f;
            else
                costOGColor.a = 0.2f;
            cost.color = costOGColor;

            if (isSelected && Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(dashboardRect, Input.mousePosition))
                {
                   Vector2 clampedPosition = new Vector2(
                        Mathf.Clamp(Input.mousePosition.x, 0, Screen.width),
                        Mathf.Clamp(Input.mousePosition.y, 0, Screen.height)
                    );

                    if (SpawnCharacterAtPosition(clampedPosition))
                    {
                        print("Spawned char + " + data.characterName);
                        isSelected = false;
                        clr.a = 1f;
                        mgr.lastSelected = null;
                        DeckMgr.Use(data);
                    }
                    
                }
            }

            yield return null;
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canInteract || isReserve || GameStateMgr.ended) return;

        if (mgr.lastSelected)
        {
            mgr.lastSelected.isSelected = false;
            mgr.lastSelected.clr.a = 1f;
        }
        mgr.lastSelected = this;
        isSelected = true;
        clr.a = 0.3f;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!canInteract || isReserve || GameStateMgr.ended) return;

        isDragging = true;
        ogPos = myRect.anchoredPosition;
        clr.a = 0.02f;
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!canInteract || !isDragging || isReserve || GameStateMgr.ended)
        {
            if (model != null)
            {
                Destroy(model);
                model = null;
            }

            return;
        }

        myRect.anchoredPosition += eventData.delta / UIMgr.CanvasScale();

        // Check if the mouse pointer is inside the dashboardRect
        if (RectTransformUtility.RectangleContainsScreenPoint(dashboardRect, Input.mousePosition))
        {
            // If the mouse is inside the dashboardRect, destroy the model
            if (model != null)
            {
                Destroy(model);
                model = null;
            }
        }
        else
        {
            // If the mouse is outside the dashboardRect and the model doesn't exist, instantiate it
            if (model == null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("WalkAble")))
                {
                    Vector3 groundPosition = hit.point;
                    groundPosition.y = -1f;
                    Vector3 pos = groundPosition; 
                    model = Instantiate(data.visualModel, pos, Quaternion.identity);
                }
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!canInteract || !isDragging || isReserve) return;

        isDragging = false;

        clr.a = 1f;
        myRect.anchoredPosition = ogPos;

        // If the model doesn't exist or the mouse is inside the dashboardRect, return
        if (model == null || RectTransformUtility.RectangleContainsScreenPoint(dashboardRect, Input.mousePosition)) return;

        Vector3 lastPos = model.transform.position;
        Destroy(model.gameObject);

        CharController cc = MulMgr.Spawn(data.model, lastPos).GetComponent<CharController>();
        //cc.SetDragging(false, MulMgr.GetPl(), true);

        DeckMgr.Use(data);
        model = null;
    }
    private bool SpawnCharacterAtPosition(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("WalkAble")))
        {
            Vector3 groundPosition = hit.point;
            groundPosition.y = -1f;

            string otherLayer = MulMgr.GetPl() == 1 ? "BoundaryBlue" : "BoundaryRed";

            if (!Physics.CheckSphere(groundPosition, 0.5f, LayerMask.GetMask(otherLayer, "Boundary")))
            {
                CharController cc = MulMgr.Spawn(data.model, groundPosition).GetComponent<CharController>();
                return true;
            }
        }
        return false;
    }
}