using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using System.Reflection;
using UnityEngine;

namespace EvenBetterImageOverlay {
    public class OptionsKeyMapping : UICustomControl
    {
        protected static readonly string keyBindingTemplate = "KeyBindingTemplate";
        
        protected SavedInputKey editingBinding;
        protected string editingBindingCategory;

        public static readonly SavedInputKey toggleOverlay = new SavedInputKey("toggleOverlay", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.KeypadEnter, false, false, false), true);
        
        public static readonly SavedInputKey cycleThroughImages = new SavedInputKey("cycleThroughImages", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.R, false, true, false), true);
        public static readonly SavedInputKey lockImage = new SavedInputKey("lockImage", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.V, false, true, false), true);
        public static readonly SavedInputKey autoFitImage = new SavedInputKey("autoFitImage", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.T, false, true, false), true);
        public static readonly SavedInputKey resetImage = new SavedInputKey("resetImage", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.B, false, true, false), true);

        public static readonly SavedInputKey rotateClockwise = new SavedInputKey("rotateClockwise", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.Keypad9, false, false, false), true);
        public static readonly SavedInputKey rotateCounterClockwise = new SavedInputKey("rotateCounterClockwise", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.Keypad7, false, false, false), true);

        public static readonly SavedInputKey rotate90DegreesClockwise = new SavedInputKey("rotateCounterClockwise", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.RightBracket, false, true, false), true);
        public static readonly SavedInputKey rotate90DegreesCounterClockwise = new SavedInputKey("rotate90DegreesCounterClockwise", EvenBetterImageOverlay.keyBindingsSettingsFileName, SavedInputKey.Encode(KeyCode.LeftBracket, false, true, false), true);

        protected int count = 0;

        protected void AddKeyMapping(string label, SavedInputKey savedInputKey)
        {
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject(keyBindingTemplate)) as UIPanel;
            if (count++ % 2 == 1) uIPanel.backgroundSprite = null;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += new KeyPressHandler(this.OnBindingKeyDown);
            uIButton.eventMouseDown += new MouseEventHandler(this.OnBindingMouseDown);

            uILabel.text = label;
            uIButton.text = savedInputKey.ToLocalizedString("KEYNAME");
            uIButton.objectUserData = savedInputKey;
        }

        protected void OnEnable()
        {
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        protected void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        protected void OnLocaleChanged()
        {
            this.RefreshBindableInputs();
        }

        protected bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        protected bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        protected bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        protected bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        protected bool IsUnbindableMouseButton(UIMouseButton code)
        {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        protected KeyCode ButtonToKeycode(UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
            {
                return KeyCode.Mouse0;
            }
            if (button == UIMouseButton.Right)
            {
                return KeyCode.Mouse1;
            }
            if (button == UIMouseButton.Middle)
            {
                return KeyCode.Mouse2;
            }
            if (button == UIMouseButton.Special0)
            {
                return KeyCode.Mouse3;
            }
            if (button == UIMouseButton.Special1)
            {
                return KeyCode.Mouse4;
            }
            if (button == UIMouseButton.Special2)
            {
                return KeyCode.Mouse5;
            }
            if (button == UIMouseButton.Special3)
            {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        protected void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (this.editingBinding!= null && !this.IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey inputKey = (p.keycode == KeyCode.Escape) ? this.editingBinding.value : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace)
                {
                    inputKey = SavedInputKey.Empty;
                }
                this.editingBinding.value = inputKey;
                UITextComponent uITextComponent = p.source as UITextComponent;
                uITextComponent.text = this.editingBinding.ToLocalizedString("KEYNAME");
                this.editingBinding= null;
                this.editingBindingCategory= string.Empty;
            }
        }

        protected void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (this.editingBinding== null)
            {
                p.Use();
                this.editingBinding= (SavedInputKey)p.source.objectUserData;
                this.editingBindingCategory= p.source.stringUserData;
                UIButton uIButton = p.source as UIButton;
                uIButton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uIButton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!this.IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                InputKey inputKey = SavedInputKey.Encode(this.ButtonToKeycode(p.buttons), this.IsControlDown(), this.IsShiftDown(), this.IsAltDown());

                this.editingBinding.value = inputKey;
                UIButton uIButton2 = p.source as UIButton;
                uIButton2.text = this.editingBinding.ToLocalizedString("KEYNAME");
                uIButton2.buttonsMask = UIMouseButton.Left;
                this.editingBinding= null;
                this.editingBindingCategory= string.Empty;
            }
        }

        protected void RefreshBindableInputs()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                if (uITextComponent != null)
                {
                    SavedInputKey savedInputKey = uITextComponent.objectUserData as SavedInputKey;
                    if (savedInputKey != null)
                    {
                        uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                    }
                }
                UILabel uILabel = current.Find<UILabel>("Name");
                if (uILabel != null)
                {
                    uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
                }
            }
        }

        protected InputKey GetDefaultEntry(string entryName)
        {
            FieldInfo field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return 0;
            }
            object value = field.GetValue(null);
            if (value is InputKey)
            {
                return (InputKey)value;
            }
            return 0;
        }

        protected void RefreshKeyMapping()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                SavedInputKey savedInputKey = (SavedInputKey)uITextComponent.objectUserData;
                if (this.editingBinding!= savedInputKey)
                {
                    uITextComponent.text = savedInputKey.ToLocalizedString("KEYNAME");
                }
            }
        }
    }
}
