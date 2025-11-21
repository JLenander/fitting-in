using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LeftArmUIHandler : TerminalUIHandler
{
    private VisualElement _handElem;
    
    public static LeftArmUIHandler Instance;
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
            string imagePath = "UI/Terminals/l_hand";
            var background = new StyleBackground(Resources.Load<Texture2D>(imagePath));
            _handElem.style.backgroundImage = background;
        }

        base.ShowUI(player);
    }
}
