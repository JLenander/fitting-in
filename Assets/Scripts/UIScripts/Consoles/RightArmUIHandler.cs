using UnityEngine;
using UnityEngine.UIElements;

public class RightArmUIHandler : TerminalUIHandler
{
    private VisualElement _handElem;
    
    public static RightArmUIHandler Instance;
    public void Awake()
    {
        Instance = this;
    }
    
    protected override void Start()
    {
        base.Start();
        if (root != null)
        {
            _handElem = root.Q<VisualElement>("HandImage");
        }
    }
    
    public override void ShowUI(GameObject player)
    {
        if (_handElem != null)
        {
            string imagePath = "UI/Terminals/r_hand";
            var background = new StyleBackground(Resources.Load<Texture2D>(imagePath));
            _handElem.style.backgroundImage = background;
        }

        base.ShowUI(player);
    }
}
